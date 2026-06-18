using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WPFCore.App.Shared.Navigation;

namespace WPFCore.App.Bootstrap;

/// <summary>
/// Composition root: build <see cref="IHost"/> với đầy đủ DI, configuration, Serilog.
/// Được gọi 1 lần trong <c>App.OnStartup</c> sau khi <see cref="SingleInstanceGuard"/> đã acquire.
/// </summary>
public static class AppHostBuilder
{
    /// <summary>
    /// UserSecrets ID cho <c>appsettings.Development.json</c>. Chỉ load khi <c>HostingEnvironment = Development</c>.
    /// </summary>
    public const string UserSecretsId = "wpfcore-app-secrets";

    /// <summary>
    /// Build host: configuration (JSON + env vars + user secrets), Serilog, DI services, modules.
    /// </summary>
    /// <param name="guard">
    /// Single-instance guard đã acquire thành công. Dùng để wire event
    /// <see cref="SingleInstanceGuard.ArgumentsReceived"/> tới logging + navigation forward.
    /// </param>
    /// <returns>Started <see cref="IHost"/> với license đã register.</returns>
    public static async Task<IHost> BuildAsync(SingleInstanceGuard guard)
    {
        ArgumentNullException.ThrowIfNull(guard);

        var host = Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Base config (load cả Production override để giữ behavior default an toàn)
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true);

                // Env vars có prefix WPFCORE_ (vd WPFCORE_App__Name)
                config.AddEnvironmentVariables(prefix: "WPFCORE_");

                // User secrets chỉ ở Development (sinh từ `dotnet user-secrets set ...`)
                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets(UserSecretsId, reloadOnChange: true);
                }
            })
            .UseSerilog(SerilogConfigurator.Configure)
            .ConfigureServices((context, services) =>
            {
                services.AddWPFCoreServices(context.Configuration);
                services.AddWPFCoreModules();

                // AppStartup singleton — owns MainWindow show/hide + migration lifecycle
                services.AddSingleton<AppStartup>();
            })
            .Build();

        // Trigger sync startup tasks (singleton instantiation, hosted services start) trước khi
        // các service khởi tạo phụ thuộc vào host.
        await host.StartAsync().ConfigureAwait(false);

        // Register Syncfusion license sau khi host start để ILogger hoạt động
        host.Services.GetRequiredService<SyncfusionLicenseRegistrar>().Register();

        // Wire single-instance forwarded args → log + để ShellViewModel xử lý foreground
        WireSingleInstanceForwarding(host, guard);

        return host;
    }

    private static void WireSingleInstanceForwarding(IHost host, SingleInstanceGuard guard)
    {
        var logger = host.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("WPFCore.App.SingleInstance");

        // Capture navigation service reference để tránh re-resolve trong callback
        var navigation = host.Services.GetRequiredService<INavigationService>();

        guard.ArgumentsReceived += (_, args) =>
        {
            logger.LogInformation(
                "Forwarded args from second instance: {Args}",
                string.Join(", ", args));

            // TODO: raise event để ShellViewModel bring existing MainWindow to foreground.
            // Hiện tại chỉ log — sẽ wire qua WeakEventMessenger hoặc IMediator ở Shell module.
            _ = navigation;
        };
    }
}