using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Modules.Maps.ViewModels;
using WPFCore.App.Modules.Maps.Views;

namespace WPFCore.App.Modules.Maps;

public static class MapsServiceCollectionExtensions
{
    public static IServiceCollection AddMapsModule(this IServiceCollection services)
    {
        // Views
        services.AddTransient<ViewMapView>();

        // ViewModels
        services.AddTransient<ViewMapViewModel>();

        return services;
    }
}
