using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Modules.Customers.Mappers;
using WPFCore.App.Modules.Customers.Repositories;
using WPFCore.App.Modules.Customers.Services;
using WPFCore.App.Modules.Customers.ViewModels;
using WPFCore.App.Modules.Customers.Views;
using WPFCore.App.Modules.Customers.Data;

namespace WPFCore.App.Modules.Customers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        services.AddSingleton<CustomerMapper>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerEditViewModel>();
        services.AddTransient<CustomerListView>();
        services.AddTransient<CustomerEditView>();

        services.AddTransient<CustomerSeedData>();

        return services;
    }
}
