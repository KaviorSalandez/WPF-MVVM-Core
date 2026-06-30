using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WPFCore.App.Configuration;
using WPFCore.App.Data;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Customers.Services;

namespace WPFCore.App.Modules.Customers.Data;

public sealed class CustomerSeedData : IModuleSeeder
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CustomerSeedData> _logger;
    private readonly AppOptions _appOptions;

    public CustomerSeedData(IServiceProvider services, ILogger<CustomerSeedData> logger, IOptions<AppOptions> appOptions)
    {
        _services = services;
        _logger = logger;
        _appOptions = appOptions.Value;
    }

    public async Task SeedAsync()
    {
        int count = _appOptions.SeedData.CustomerCount;
        if (count <= 0) return;

        using var scope = _services.CreateScope();
        // Lấy AppDbContext để thao tác trực tiếp (vừa check Any, vừa Save nhanh)
        var dbContext = scope.ServiceProvider.GetRequiredService<WPFCore.App.Data.AppDbContext>();

        // Chỉ seed nếu bảng Customers chưa có dữ liệu
        if (dbContext.Customers.Any())
        {
            _logger.LogInformation("Bảng Customers đã có dữ liệu, bỏ qua bước Seed.");
            return;
        }

        _logger.LogInformation("Bắt đầu Seed {Count} dữ liệu Customer mẫu...", count);

        var rng = new Random(42); // deterministic seed
        var firstNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi" };
        var lastNames = new[] { "An", "Bình", "Cường", "Dũng", "Hà", "Khánh", "Linh", "Minh", "Nam", "Phong", "Quân", "Sơn" };
        var domains = new[] { "gmail.com", "yahoo.com", "outlook.com", "example.com" };
        var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng", "Hải Phòng", "Cần Thơ" };

        var newCustomers = new System.Collections.Generic.List<Customer>();

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

            newCustomers.Add(new Customer
            {
                Code = code,
                Name = name,
                Email = email,
                Phone = phone,
                Address = $"Số {rng.Next(1, 999)}, đường ABC, {city}",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-rng.Next(20, 60)))
            });
        }

        try
        {
            // Tối ưu Vấn đề 1: Dùng AddRange để Insert một lần (Bulk Insert) thay vì gọi vòng lặp
            dbContext.Customers.AddRange(newCustomers);
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Seed xong {Count} khách hàng thành công.", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi seed hàng loạt Customer.");
        }
    }
}
