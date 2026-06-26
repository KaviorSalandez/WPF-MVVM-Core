namespace WPFCore.App.Shell.Menu;

/// <summary>
/// Nghiệp vụ menu: lấy dữ liệu phẳng từ repository rồi dựng thành cây cha-con
/// (<see cref="MenuNode"/>) sẵn sàng cho UI binding.
/// </summary>
public interface IMenuService
{
    /// <summary>Trả về danh sách các node menu cấp cao nhất, mỗi node chứa sẵn cây con.</summary>
    Task<IReadOnlyList<MenuNode>> GetMenuTreeAsync(CancellationToken cancellationToken = default);
}
