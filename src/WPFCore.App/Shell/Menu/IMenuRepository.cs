namespace WPFCore.App.Shell.Menu;

/// <summary>
/// Truy cập dữ liệu menu (đọc từ bảng <c>menu_items</c>). Tầng repository — dùng Dapper,
/// trả về danh sách phẳng; việc dựng cây cha-con thuộc về <see cref="IMenuService"/>.
/// </summary>
public interface IMenuRepository
{
    /// <summary>Lấy toàn bộ mục menu (dạng phẳng), đã sắp theo cấp rồi tới thứ tự sắp xếp.</summary>
    Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}
