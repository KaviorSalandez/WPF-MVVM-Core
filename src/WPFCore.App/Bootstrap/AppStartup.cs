using System.Reflection;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using WPFCore.App.Configuration;
using WPFCore.App.Data;
using WPFCore.App.Modules.Customers;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.Themes;
using WPFCore.App.Shell.Login;
using Esri.ArcGISRuntime;

namespace WPFCore.App.Bootstrap;

/// <summary>
/// Application startup coordinator: chạy sau khi <see cref="AppHostBuilder"/> đã build xong host.
/// <list type="bullet">
///   <item>Apply EF Core migrations (nếu <see cref="DatabaseOptions.AutoMigrate"/>).</item>
///   <item>Seed development data (nếu <see cref="SeedDataOptions.Enabled"/>).</item>
///   <item>Resolve MainWindow từ DI, áp theme, show window.</item>
///   <item>Navigate tới default ViewModel.</item>
/// </list>
/// </summary>
/// <remarks>
/// Singleton — phải register sau khi host start. Nếu MainWindow chưa được register (Shell module
/// chưa có), sẽ log error và gọi <see cref="IHostApplicationLifetime.StopApplication"/> để host
/// shutdown gracefully.
/// </remarks>
public sealed class AppStartup
{
    // Tên type cho Customer module (chưa tồn tại — dùng reflectively để Bootstrap build trước module).
    private const string CustomerEntityTypeName = "WPFCore.App.Modules.Customers.Models.Customer, WPFCore.App";
    private const string CustomerListViewModelTypeName = "WPFCore.App.Modules.Customers.ViewModels.CustomerListViewModel, WPFCore.App";
    private const string CustomerSeedDataTypeName = "WPFCore.App.Modules.Customers.Data.CustomerSeedData, WPFCore.App";

    private readonly IServiceProvider _services;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private readonly FluentLightThemeLoader _themeLoader;
    private readonly ILogger<AppStartup> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public AppStartup(
        IServiceProvider services,
        INavigationService navigation,
        IDialogService dialog,
        FluentLightThemeLoader themeLoader,
        ILogger<AppStartup> logger,
        IHostApplicationLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(themeLoader);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(lifetime);

        _services = services;
        _navigation = navigation;
        _dialog = dialog;
        _themeLoader = themeLoader;
        _logger = logger;
        _lifetime = lifetime;
    }

    /// <summary>
    /// Entry point: chạy migration → seed → hiện LoginWindow → (nếu đăng nhập thành công)
    /// show MainWindow → navigate. Bất kỳ exception nào cũng được log critical rồi stop host.
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            var config = _services.GetRequiredService<IConfiguration>();
            var arcGisApiKey = config["ArcGIS:ApiKey"];
            if (!string.IsNullOrWhiteSpace(arcGisApiKey))
            {
                ArcGISRuntimeEnvironment.ApiKey = arcGisApiKey;
            }

            await ApplyMigrationsAsync().ConfigureAwait(false);
            await SeedDevelopmentDataAsync().ConfigureAwait(false);

            // Toàn bộ UI logic (login + show main window) phải chạy trong một
            // Dispatcher.InvokeAsync duy nhất — không để gap giữa LoginWindow đóng
            // và MainWindow mở, tránh WPF phát hiện “no open windows” và shutdown
            // sớn (hậu quả của ShutdownMode=OnLastWindowClose).
            await Application.Current.Dispatcher.InvokeAsync(RunUiStartup);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Application startup failed");
            _lifetime.StopApplication();
        }
    }

    /// <summary>
    /// Chạy trên UI thread: hiện login dialog rồi — ngay lập tức, không có khoảng trống —
    /// mở MainWindow. <see cref="Window.ShowDialog"/> block theo kiểu synchronous nhưng
    /// vẫn pump messages qua internal dispatcher frame nên UI không bị chết.
    /// </summary>
    private void RunUiStartup()
    {
        // Bước 1: Hiện LoginWindow
        var loginWindow = _services.GetRequiredService<LoginWindow>();
        _themeLoader.ApplyTo(loginWindow);

        if (loginWindow.ShowDialog() != true)
        {
            _logger.LogInformation("Người dùng huỷ đăng nhập. Ứng dụng sẽ thoát.");
            _lifetime.StopApplication();
            return;
        }

        // Bước 2: Người dùng đăng nhập thành công → mở MainWindow NGAY LẬP TỨC.
        // Không có gap ⇒ WPF không thể trigger “OnLastWindowClose” trước khi MainWindow mở.
        var mainWindow = _services.GetService<Window>();
        if (mainWindow is null)
        {
            const string message =
                "MainWindow chưa được register trong DI. Shell module phải đăng ký MainWindow " +
                "trước khi app start. Kiểm tra AddWPFCoreModules() trong ServiceRegistrationExtensions.";
            _logger.LogError(message);
            _lifetime.StopApplication();
            return;
        }

        _themeLoader.ApplyTo(mainWindow);

        // Khi user đóng MainWindow → dừng host → trigger Application.Shutdown()
        // (xem ApplicationStopping.Register trong App.xaml.cs)
        mainWindow.Closed += (_, _) => _lifetime.StopApplication();

        mainWindow.Show();
        NavigateToDefault();
    }

    private async Task ApplyMigrationsAsync()
    {
        var dbOptions = _services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        if (!dbOptions.AutoMigrate)
        {
            _logger.LogInformation("Database.AutoMigrate=false, skip migrations");
            return;
        }

        _logger.LogInformation("Applying EF Core migrations...");

        // DbContext register singleton — không cần tạo scope, nhưng giữ pattern cho an toàn
        // nếu sau này đổi sang scoped.
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync().ConfigureAwait(false);

        _logger.LogInformation("EF Core migrations applied");
    }

    private async Task SeedDevelopmentDataAsync()
    {
        var appOptions = _services.GetRequiredService<IOptions<AppOptions>>().Value;
        if (!appOptions.SeedData.Enabled)
        {
            _logger.LogInformation("App.SeedData.Enabled=false, skip seeding");
            return;
        }

        // Resolve Customer type reflectively — Customer module có thể chưa tồn tại ở wave này
        var customerType = Type.GetType(CustomerEntityTypeName, throwOnError: false);
        if (customerType is null)
        {
            _logger.LogInformation(
                "Customer entity chưa được đăng ký (assembly wave). Skip seeding.");
            return;
        }

        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await dbContext.Database.CanConnectAsync().ConfigureAwait(false))
        {
            _logger.LogWarning("Cannot connect to database. Skip seeding.");
            return;
        }

        if (await HasAnyRecordsAsync(dbContext, customerType).ConfigureAwait(false))
        {
            _logger.LogInformation("Database đã có data, skip seeding");
            return;
        }

        _logger.LogInformation(
            "Seeding {Count} sample {Entity}...",
            appOptions.SeedData.CustomerCount,
            customerType.Name);

        var seedType = Type.GetType(CustomerSeedDataTypeName, throwOnError: false);
        if (seedType is null)
        {
            _logger.LogWarning(
                "CustomerSeedData chưa được đăng ký. Skip seeding. " +
                "Tạo class CustomerSeedData trong module Customers để bật seed.");
            return;
        }

        var seedInstance = ActivatorUtilities.CreateInstance(_services, seedType);
        var seedMethod = seedType.GetMethod("SeedAsync", BindingFlags.Public | BindingFlags.Instance);
        if (seedMethod is null || seedMethod.GetParameters().Length != 1)
        {
            _logger.LogWarning("CustomerSeedData.SeedAsync(int) không tồn tại. Skip seeding.");
            return;
        }

        var task = (Task)seedMethod.Invoke(seedInstance, new object[] { appOptions.SeedData.CustomerCount })!;
        await task.ConfigureAwait(false);

        _logger.LogInformation("Seed data completed");
    }

    private static async Task<bool> HasAnyRecordsAsync(DbContext dbContext, Type entityType)
    {
        // Lấy table name từ EF Core model metadata (kể cả schema nếu có) — không dùng entityType.Name
        // vì convention có thể là "Customer" trong code nhưng "customers" trong DB.
        var entityMetadata = dbContext.Model.FindEntityType(entityType)
            ?? throw new InvalidOperationException($"Entity type '{entityType.FullName}' chưa được đăng ký trong DbContext model.");
        var tableName = entityMetadata.GetTableName()
            ?? throw new InvalidOperationException($"Entity '{entityType.FullName}' chưa được map sang table.");

        // Quote table name để tránh conflict với SQL keywords (SQLite sẽ pass-through).
        var quotedTableName = tableName.Replace("\"", "\"\"");

        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync().ConfigureAwait(false);
        }
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM \"{quotedTableName}\") THEN 1 ELSE 0 END";
        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        return Convert.ToInt32(result) == 1;
    }

    private void NavigateToDefault()
    {
        // Navigate to DashboardViewModel as the default screen
        var viewModelType = Type.GetType("WPFCore.App.Modules.Dashboard.ViewModels.DashboardViewModel, WPFCore.App", throwOnError: false);
        if (viewModelType is null)
        {
            _logger.LogInformation(
                "DashboardViewModel chưa được đăng ký. Skip default navigation. " +
                "App sẽ hiển thị MainWindow trống.");
            return;
        }

        _navigation.NavigateTo(viewModelType);
        _logger.LogInformation("Navigated to default view: {Type}", viewModelType.Name);
    }
}