using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
