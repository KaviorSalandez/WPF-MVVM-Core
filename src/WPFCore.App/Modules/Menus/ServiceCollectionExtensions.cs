using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Modules.Menus.Repositories;
using WPFCore.App.Modules.Menus.Services;
using WPFCore.App.Modules.Menus.ViewModels;
using WPFCore.App.Modules.Menus.Views;

namespace WPFCore.App.Modules.Menus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMenusModule(this IServiceCollection services)
    {
        services.AddSingleton<WPFCore.App.Modules.Menus.Models.MenuMapper>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IMenuService, MenuService>();
        
        services.AddTransient<MenuListViewModel>();
        services.AddTransient<MenuListView>();
        
        services.AddTransient<MenuAddViewModel>();
        services.AddTransient<MenuAddView>();
        
        services.AddTransient<MenuEditViewModel>();
        services.AddTransient<MenuEditView>();

        return services;
    }
}
