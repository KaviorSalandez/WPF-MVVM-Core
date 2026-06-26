using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Services;

namespace WPFCore.App.Modules.Menus.Repositories;

/// <summary>
/// Truy cập dữ liệu menu (đọc/ghi từ bảng <c>menu_items</c> bằng EF Core).
/// </summary>
public interface IMenuRepository
{
    Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MenuItemEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByActionKeyAsync(string actionKey, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(MenuItemEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(MenuItemEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
