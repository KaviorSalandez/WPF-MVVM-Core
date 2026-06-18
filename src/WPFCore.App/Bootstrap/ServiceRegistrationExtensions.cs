using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Riok.Mapperly.Abstractions;
using WPFCore.App.Configuration;
using WPFCore.App.Data;
using WPFCore.App.Modules.Customers;
using WPFCore.App.Shell;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.Themes;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Bootstrap;

/// <summary>
/// Extension methods đăng ký toàn bộ DI services cho ứng dụng. Tách thành 2 phần:
/// <list type="bullet">
///   <item><see cref="AddWPFCoreServices"/>: cross-cutting infra (Options, EF Core, SQLite, FluentValidation, Mapperly, shared services).</item>
///   <item><see cref="AddWPFCoreModules"/>: domain modules (Customers, Products, ...). Mỗi module tự định nghĩa extension <c>AddXxxModule</c>.</item>
/// </list>
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Đăng ký infrastructure services (Options, EF Core, SQLite, FluentValidation, Mapperly, Syncfusion, shared navigation/dialog/theme).
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Root configuration (đã có appsettings + env vars).</param>
    public static IServiceCollection AddWPFCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // ----- Options (bind to IOptions<T>) -----
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<SyncfusionOptions>(configuration.GetSection(SyncfusionOptions.SectionName));

        // ----- Database (EF Core — dùng cho migrations + schema) -----
        var dbOptionsConfig = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();
        var connectionString = configuration.GetConnectionString(dbOptionsConfig.ConnectionStringName)
            ?? configuration.GetConnectionString("Default")
            ?? "Data Source=wpfcore.db";

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connectionString), ServiceLifetime.Singleton);

        // ----- SQLite + Dapper (runtime queries, mỗi UoW một connection) -----
        services.AddSingleton<IDbConnectionFactory>(sp => new SqliteConnectionFactory(
            sp.GetRequiredService<IOptions<DatabaseOptions>>(),
            connectionString));

        // ----- Syncfusion -----
        services.AddSingleton<SyncfusionLicenseRegistrar>();
        services.AddSingleton<FluentLightThemeLoader>();

        // ----- Shared services (singletons — hold app-wide state) -----
        // Navigation: register concrete FrameNavigationService first, then alias INavigationService
        // tới cùng instance. MainWindow injects concrete type để gọi Attach(Frame); các consumer khác
        // inject interface INavigationService.
        services.AddSingleton<FrameNavigationService>();
        services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<FrameNavigationService>());
        services.AddSingleton<IDialogService, SyncfusionDialogService>();

        // ----- FluentValidation — scan assembly for IValidator<T> -----
        services.AddValidatorsFromAssemblyContaining<AppOptions>(ServiceLifetime.Transient);

        // ----- Mapperly -----
        // Mỗi module tự đăng ký mapper concrete class của nó trong AddXxxModule().
        // ViewModel inject trực tiếp concrete mapper (ví dụ: CustomerMapper) thay vì abstraction.

        return services;
    }

    /// <summary>
    /// Đăng ký domain modules. Mỗi module expose extension <c>AddXxxModule(IServiceCollection)</c>
    /// và được gọi ở đây. Khi thêm module mới, chỉ cần thêm 1 dòng gọi extension.
    /// </summary>
    public static IServiceCollection AddWPFCoreModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Domain modules
        services.AddCustomersModule();

        // Shell (MainWindow, NavigationView)
        services.AddShell();

        return services;
    }
}

// NOTE: IMapper abstraction removed in favor of concrete mapper classes injected directly.
// Mỗi ViewModel/Service inject mapper cụ thể (CustomerMapper, ProductMapper, v.v.)
// để code rõ ràng hơn và Mapperly source-gen hoạt động trực tiếp không qua reflection.

/// <summary>
/// Aggregate interface cho tất cả Mapperly-generated mapper. Cho phép resolve một instance duy nhất
/// từ DI thay vì phải inject từng mapper class cụ thể.
/// </summary>
public interface IMapper
{
    /// <summary>Map object source sang <typeparamref name="TDestination"/> dùng runtime type detection.</summary>
    TDestination Map<TDestination>(object source);

    /// <summary>Map strongly-typed source sang <typeparamref name="TDestination"/>.</summary>
    TDestination Map<TSource, TDestination>(TSource source);
}

/// <summary>
/// Reflection-based aggregator: quét tất cả mapper class được annotate bởi
/// <see cref="MapperAttribute"/> và build một dispatch table <c>(SourceType, DestType) → Delegate</c>.
/// </summary>
internal sealed class MapperAggregator : IMapper
{
    private readonly Dictionary<(Type Source, Type Destination), Delegate> _mapMethods = new();

    public MapperAggregator(IEnumerable<Type> mapperTypes)
    {
        ArgumentNullException.ThrowIfNull(mapperTypes);

        foreach (var mapperType in mapperTypes)
        {
            foreach (var method in mapperType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                // Chỉ nhận method có 1 parameter, return khác void
                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                    continue;
                if (method.ReturnType == typeof(void))
                    continue;

                var sourceType = parameters[0].ParameterType;
                var destType = method.ReturnType;
                var key = (sourceType, destType);

                try
                {
                    var delegateType = typeof(Func<,>).MakeGenericType(sourceType, destType);
                    _mapMethods[key] = Delegate.CreateDelegate(delegateType, method);
                }
                catch
                {
                    // Method overload không phù hợp để bind delegate — bỏ qua mapper này
                }
            }
        }
    }

    /// <inheritdoc />
    public TDestination Map<TDestination>(object source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return MapInternal<TDestination>(source.GetType(), source);
    }

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var key = (typeof(TSource), typeof(TDestination));
        if (_mapMethods.TryGetValue(key, out var del))
        {
            return ((Func<TSource, TDestination>)del)(source);
        }

        throw new InvalidOperationException(
            $"No Mapperly mapping registered for {typeof(TSource).Name} -> {typeof(TDestination).Name}");
    }

    private TDestination MapInternal<TDestination>(Type sourceType, object source)
    {
        var key = (sourceType, typeof(TDestination));
        if (_mapMethods.TryGetValue(key, out var del))
        {
            var result = del.DynamicInvoke(source);
            if (result is null)
            {
                throw new InvalidOperationException(
                    $"Mapperly mapping {sourceType.Name} -> {typeof(TDestination).Name} returned null.");
            }
            return (TDestination)result;
        }

        throw new InvalidOperationException(
            $"No Mapperly mapping registered for {sourceType.Name} -> {typeof(TDestination).Name}");
    }
}