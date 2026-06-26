using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WPFCore.App.Data;
using WPFCore.App.Modules.Menus.Models;

namespace WPFCore.App.Modules.Menus.Repositories;

/// <summary>
/// <see cref="IMenuRepository"/> dùng Entity Framework Core.
/// </summary>
public sealed class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<MenuRepository> _logger;

    public MenuRepository(AppDbContext context, ILogger<MenuRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .AsNoTracking()
            .OrderBy(m => m.ParentId)
            .ThenBy(m => m.SortOrder)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<MenuItemEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByActionKeyAsync(string actionKey, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems.Where(m => m.ActionKey == actionKey);
        
        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> InsertAsync(MenuItemEntity entity, CancellationToken cancellationToken = default)
    {
        _context.MenuItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Menu inserted: Id={Id}, Title={Title}", entity.Id, entity.Title);
        return entity.Id;
    }

    public async Task UpdateAsync(MenuItemEntity entity, CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();
        _context.MenuItems.Update(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Menu updated: Id={Id}, Title={Title}", entity.Id, entity.Title);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MenuItems.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (entity != null)
        {
            _context.MenuItems.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Menu deleted: Id={Id}", id);
        }
    }
}
