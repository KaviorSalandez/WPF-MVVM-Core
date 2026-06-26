using WPFCore.App.Modules.Menus.Models;

namespace WPFCore.App.Modules.Menus.Services;

/// <summary>
/// Nghiệp vụ menu: lấy dữ liệu phẳng từ repository rồi dựng thành cây cha-con
/// cho giao diện, và các thao tác CRUD.
/// </summary>
public interface IMenuService
{
    Task<IReadOnlyList<MenuNode>> GetMenuTreeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuDto>> GetAllFlatAsync(CancellationToken cancellationToken = default);
    Task<MenuDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(MenuDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
