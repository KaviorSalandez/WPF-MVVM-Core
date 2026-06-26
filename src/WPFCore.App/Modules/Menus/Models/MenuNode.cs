namespace WPFCore.App.Modules.Menus.Models;

/// <summary>
/// Node của cây menu dùng để binding ra UI. Khác với <see cref="MenuItemEntity"/> (bản ghi phẳng
/// trong DB), <see cref="MenuNode"/> đã được dựng thành cây — mỗi node chứa sẵn danh sách
/// <see cref="Children"/>. XAML bind đệ quy theo property <see cref="Children"/> này.
/// </summary>
/// <remarks>
/// Đây là vai trò "DTO cho UI" — tách khỏi entity DB để View không phụ thuộc trực tiếp vào
/// cấu trúc bảng (giống quan hệ <c>Customer</c> ↔ <c>CustomerDto</c>).
/// </remarks>
public sealed class MenuNode
{
    public int Id { get; init; }

    /// <summary>Tiêu đề hiển thị.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Khoá hành vi khi bấm (null nếu là nhóm cha).</summary>
    public string? ActionKey { get; init; }

    /// <summary>Glyph icon tuỳ chọn.</summary>
    public string? Glyph { get; init; }

    /// <summary>Mục có đang được phép bấm hay không.</summary>
    public bool IsEnabled { get; init; } = true;

    public int SortOrder { get; init; }

    /// <summary>Danh sách mục con (rỗng nếu là mục lá).</summary>
    public IReadOnlyList<MenuNode> Children { get; init; } = Array.Empty<MenuNode>();

    /// <summary><c>true</c> nếu node có mục con (là nhóm cha, bấm vào chỉ mở submenu).</summary>
    public bool HasChildren => Children.Count > 0;
}
