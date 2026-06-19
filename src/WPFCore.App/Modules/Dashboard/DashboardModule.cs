using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Modules.Dashboard.ViewModels;

namespace WPFCore.App.Modules.Dashboard;

public static class DashboardModule
{
    public static IServiceCollection AddDashboardModule(this IServiceCollection services)
    {
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<WPFCore.App.Modules.Dashboard.Views.DashboardView>();
        return services;
    }
}
