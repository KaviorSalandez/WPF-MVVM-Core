using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WPFCore.App.Shell.Menu.Data;

/// <summary>
/// Cấu hình EF Core cho bảng <c>menu_items</c> + seed dữ liệu menu mặc định.
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

        // ── Seed dữ liệu menu mặc định (giữ nguyên cấu trúc menu cũ đang fix cứng) ──
        // Quy ước Id: 1 = top-level đơn, 10/20/30 = nhóm cha, 11.. = con của 10, v.v.
        builder.HasData(
            // Thống kê (top-level, mở Dashboard)
            new MenuItemEntity { Id = 1, ParentId = null, Title = "Thống kê", ActionKey = "Dashboard", SortOrder = 1 },

            // Danh mục (nhóm cha)
            new MenuItemEntity { Id = 10, ParentId = null, Title = "Danh mục", ActionKey = null, SortOrder = 2 },
            new MenuItemEntity { Id = 11, ParentId = 10, Title = "Quản lý lớp dữ liệu bản đồ", ActionKey = "MapLayer", SortOrder = 1 },
            new MenuItemEntity { Id = 12, ParentId = 10, Title = "Quản lý đối tượng địa lý", ActionKey = "GeoFeature", SortOrder = 2 },
            new MenuItemEntity { Id = 13, ParentId = 10, Title = "Quản lý nguồn dữ liệu", ActionKey = "DataSource", SortOrder = 3 },
            new MenuItemEntity { Id = 14, ParentId = 10, Title = "Quản lý quy định kiểm tra", ActionKey = "CheckRules", SortOrder = 4 },
            new MenuItemEntity { Id = 15, ParentId = 10, Title = "Quản lý loại bản đồ", ActionKey = "MapType", SortOrder = 5 },
            new MenuItemEntity { Id = 16, ParentId = 10, Title = "Quản lý người dùng", ActionKey = "Users", SortOrder = 6 },

            // Hệ thống (nhóm cha)
            new MenuItemEntity { Id = 20, ParentId = null, Title = "Hệ thống", ActionKey = null, SortOrder = 3 },
            new MenuItemEntity { Id = 21, ParentId = 20, Title = "Kiểm tra dữ liệu bản đồ", ActionKey = "RunCheck", SortOrder = 1 },
            new MenuItemEntity { Id = 22, ParentId = 20, Title = "Xem kết quả kiểm tra", ActionKey = "CheckResults", SortOrder = 2 },
            new MenuItemEntity { Id = 23, ParentId = 20, Title = "Báo cáo tổng hợp", ActionKey = "SummaryReport", SortOrder = 3 },
            new MenuItemEntity { Id = 24, ParentId = 20, Title = "Xuất báo cáo (Excel/PDF)", ActionKey = "ExportReport", SortOrder = 4 },
            new MenuItemEntity { Id = 25, ParentId = 20, Title = "Quản lý khách hàng (CRUD mẫu)", ActionKey = "CustomerList", SortOrder = 5 },

            // Trợ giúp (nhóm cha)
            new MenuItemEntity { Id = 30, ParentId = null, Title = "Trợ giúp", ActionKey = null, SortOrder = 4 },
            new MenuItemEntity { Id = 31, ParentId = 30, Title = "Giới thiệu", ActionKey = "About", SortOrder = 1 },
            new MenuItemEntity { Id = 32, ParentId = 30, Title = "Thoát", ActionKey = "Exit", SortOrder = 2 });
    }
}
