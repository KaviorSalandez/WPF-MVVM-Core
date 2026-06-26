using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Modules.Menus.Repositories;
using WPFCore.App.Modules.Menus.Services;
using WPFCore.App.Shell.Login;

namespace WPFCore.App.Shell;

/// <summary>
/// Đăng ký Shell services vào DI container. Được gọi từ
/// <c>Bootstrap.ServiceRegistrationExtensions.AddWPFCoreServices</c>.
/// </summary>
public static class ShellServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký <see cref="ShellViewModel"/> (singleton) và <see cref="MainWindow"/> (transient).
    /// Alias <c>Window</c> → <see cref="MainWindow"/> để <c>AppStartup</c> có thể resolve
    /// thông qua <c>IServiceProvider.GetService&lt;Window&gt;()</c>.
    /// Đồng thời đăng ký <see cref="LoginViewModel"/> và <see cref="LoginWindow"/> (transient).
    /// </summary>
    public static IServiceCollection AddShell(this IServiceCollection services)
    {
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<Window>(sp => sp.GetRequiredService<MainWindow>());

        // Login
        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginWindow>();

        return services;
    }
}

