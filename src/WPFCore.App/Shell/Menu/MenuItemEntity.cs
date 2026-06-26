namespace WPFCore.App.Shell.Menu;

/// <summary>
/// Một bản ghi menu trong cơ sở dữ liệu (bảng <c>menu_items</c>).
/// Phân cấp cha-con bằng cách tự tham chiếu qua <see cref="ParentId"/>:
/// <list type="bullet">
///   <item><see cref="ParentId"/> = <c>null</c> → mục menu cấp cao nhất (top-level).</item>
///   <item><see cref="ParentId"/> có giá trị → là mục con của bản ghi có Id tương ứng.</item>
/// </list>
/// Đây là "model" theo đúng tầng dữ liệu của project (giống <c>Customer</c>).
/// </summary>
public sealed class MenuItemEntity
{
    /// <summary>Khoá chính (tự tăng).</summary>
    public int Id { get; set; }

    /// <summary>Id của mục cha. <c>null</c> nếu là mục cấp cao nhất.</summary>
    public int? ParentId { get; set; }

    /// <summary>Tiêu đề hiển thị trên menu.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Khoá hành vi khi bấm (vd <c>"CustomerList"</c>, <c>"About"</c>, <c>"Exit"</c>).
    /// <c>null</c> nếu mục này chỉ là nhóm cha (chứa mục con, không tự làm gì khi bấm).
    /// </summary>
    public string? ActionKey { get; set; }

    /// <summary>Thứ tự sắp xếp trong cùng một cấp (nhỏ hơn hiển thị trước).</summary>
    public int SortOrder { get; set; }

    /// <summary>Glyph icon (tuỳ chọn) — dành cho mở rộng sau.</summary>
    public string? Glyph { get; set; }

    /// <summary>Bật/tắt mục menu. Mục <c>false</c> sẽ không được dựng ra cây.</summary>
    public bool IsEnabled { get; set; } = true;
}
