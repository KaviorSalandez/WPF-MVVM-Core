using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Data.SeedData;

namespace WPFCore.App.Modules.Menus.Data;

/// <summary>
/// Cấu hình EF Core cho bảng <c>menu_items</c>.
/// Được <c>AppDbContext.ApplyConfigurationsFromAssembly</c> tự động nhận diện (không cần khai báo DbSet).
/// </summary>
public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItemEntity>
{
    public void Configure(EntityTypeBuilder<MenuItemEntity> builder)
    {
        builder.ToTable("menu_items");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.ActionKey)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(m => m.Glyph)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(m => m.SortOrder).HasDefaultValue(0);
        builder.Property(m => m.IsEnabled).HasDefaultValue(true);

        // Quan hệ tự tham chiếu cha-con: ParentId trỏ tới Id của một MenuItemEntity khác.
        // OnDelete=Restrict: không cho xoá cha khi còn con (tránh mồ côi).
        builder.HasOne<MenuItemEntity>()
            .WithMany()
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.ParentId);

        // Nạp dữ liệu mặc định từ file SeedData riêng biệt
        builder.HasData(MenuItemSeed.GetMenuItems());
    }
}
