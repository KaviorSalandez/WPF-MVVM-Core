using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Customers.Services;

namespace WPFCore.App.Modules.Customers.Data;

public sealed class CustomerSeedData
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CustomerSeedData> _logger;

    public CustomerSeedData(IServiceProvider services, ILogger<CustomerSeedData> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task SeedAsync(int count)
    {
        using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var rng = new Random(42); // deterministic seed
        var firstNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi" };
        var lastNames = new[] { "An", "Bình", "Cường", "Dũng", "Hà", "Khánh", "Linh", "Minh", "Nam", "Phong", "Quân", "Sơn" };
        var domains = new[] { "gmail.com", "yahoo.com", "outlook.com", "example.com" };
        var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng", "Hải Phòng", "Cần Thơ" };

        for (int i = 1; i <= count; i++)
        {
            var firstName = firstNames[rng.Next(firstNames.Length)];
            var lastName = lastNames[rng.Next(lastNames.Length)];
            var name = $"{firstName} {lastName}";
            var code = $"KH{i:000}";
            var domain = domains[rng.Next(domains.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@{domain}";
            var phone = $"09{rng.Next(10000000, 99999999)}";
            var city = cities[rng.Next(cities.Length)];

            var customer = new Customer
            {
                Code = code,
                Name = name,
                Email = email,
                Phone = phone,
                Address = $"Số {rng.Next(1, 999)}, đường ABC, {city}",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-rng.Next(20, 60)))
            };

            try
            {
                await service.SaveAsync(customer).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed customer {Code}", code);
            }
        }
    }
}
