using Microsoft.EntityFrameworkCore;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Menus.Models;

namespace WPFCore.App.Data;

/// <summary>
/// EF Core DbContext của ứng dụng. Các module đăng ký entity qua
/// <see cref="IEntityTypeConfiguration{TEntity}"/> và được áp dụng tự động
/// qua extension method <c>ModelBuilder.ApplyConfigurationsFromAssembly</c>.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MenuItemEntity> MenuItems => Set<MenuItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
