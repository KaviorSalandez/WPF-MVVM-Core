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
/// Cấu hình menu tĩnh cho Shell. Khi module mới được thêm, chỉ cần bổ sung
/// <see cref="MenuItemDefinition"/> tương ứng vào <see cref="MainMenu"/>.
/// </summary>
public static class MenuDefinitions
{
    public static IReadOnlyList<MenuItemDefinition> MainMenu { get; } = new[]
    {
        new MenuItemDefinition(
            "Khách hàng",
            NavigateToViewModel: Type.GetType("WPFCore.App.Modules.Customers.ViewModels.CustomerListViewModel, WPFCore.App")
                ?? throw new InvalidOperationException("CustomerListViewModel type không tìm được")),
        new MenuItemDefinition("Trợ giúp", Children: new[]
        {
            new MenuItemDefinition("Giới thiệu", ActionKey: "About"),
            new MenuItemDefinition("Thoát", ActionKey: "Exit"),
        }),
    };
}
