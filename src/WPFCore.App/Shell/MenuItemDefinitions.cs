using WPFCore.App.Modules.Customers.ViewModels;
using WPFCore.App.Modules.Dashboard.ViewModels;

namespace WPFCore.App.Shell;

/// <summary>
/// Khai báo tĩnh cho một mục menu trong Shell. Mỗi item có thể chứa <see cref="Children"/>
/// (sub-menu), hoặc một trong hai hành vi: <see cref="NavigateToViewModel"/> (mở page qua
/// <c>INavigationService</c>) hoặc <see cref="ActionKey"/> (gửi tới handler trong ViewModel).
/// </summary>
/// <param name="Title">Tiêu đề hiển thị trên menu.</param>
/// <param name="Glyph">Glyph icon (Segoe MDL2 / Fluent) — tuỳ chọn, dùng cho menu lệnh mở rộng.</param>
/// <param name="Children">Sub-menu items — nếu có thì <paramref name="NavigateToViewModel"/> và <paramref name="ActionKey"/> bị bỏ qua.</param>
/// <param name="NavigateToViewModel">Type của ViewModel sẽ được <c>INavigationService.NavigateTo</c> resolve.</param>
/// <param name="ActionKey">Khoá hành vi nội bộ (vd. "About", "Exit") — ShellViewModel sẽ switch-case.</param>
public sealed record MenuItemDefinition(
    string Title,
    string? Glyph = null,
    MenuItemDefinition[]? Children = null,
    Type? NavigateToViewModel = null,
    string? ActionKey = null);

/// <summary>
/// Cấu hình menu tĩnh cho Shell hệ thống Kiểm tra Dữ liệu Bản đồ.
/// Khi module mới được thêm, chỉ cần bổ sung <see cref="MenuItemDefinition"/> tương ứng vào <see cref="MainMenu"/>.
/// </summary>
public static class MenuDefinitions
{
    public static MenuItemDefinition[] MainMenu { get; } = new[]
    {
        // ── THỐNG KÊ ─────────────────────────────────────────────────────────
        new MenuItemDefinition("Thống kê", NavigateToViewModel: typeof(DashboardViewModel)),

        // ── DANH MỤC ──────────────────────────────────────────────────────────
        new MenuItemDefinition("Danh mục", Children: new[]
        {
            new MenuItemDefinition("Quản lý lớp dữ liệu bản đồ",   ActionKey: "MapLayer"),
            new MenuItemDefinition("Quản lý đối tượng địa lý",      ActionKey: "GeoFeature"),
            new MenuItemDefinition("Quản lý nguồn dữ liệu",         ActionKey: "DataSource"),
            new MenuItemDefinition("Quản lý quy định kiểm tra",     ActionKey: "CheckRules"),
            new MenuItemDefinition("Quản lý loại bản đồ",           ActionKey: "MapType"),
            new MenuItemDefinition("Quản lý người dùng",            ActionKey: "Users"),
        }),

        // ── HỆ THỐNG ──────────────────────────────────────────────────────────
        new MenuItemDefinition("Hệ thống", Children: new[]
        {
            new MenuItemDefinition("Kiểm tra dữ liệu bản đồ",  ActionKey: "RunCheck"),
            new MenuItemDefinition("Xem kết quả kiểm tra",     ActionKey: "CheckResults"),
            new MenuItemDefinition("Báo cáo tổng hợp",         ActionKey: "SummaryReport"),
            new MenuItemDefinition("Xuất báo cáo (Excel/PDF)", ActionKey: "ExportReport"),
            new MenuItemDefinition("---"),   // separator placeholder
            new MenuItemDefinition("Quản lý khách hàng (CRUD mẫu)",
                NavigateToViewModel: typeof(CustomerListViewModel)),
        }),

        // ── TRỢ GIÚP ──────────────────────────────────────────────────────────
        new MenuItemDefinition("Trợ giúp", Children: new[]
        {
            new MenuItemDefinition("Giới thiệu", ActionKey: "About"),
            new MenuItemDefinition("Thoát",      ActionKey: "Exit"),
        }),
    };
}
