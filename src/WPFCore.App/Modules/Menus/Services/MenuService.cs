using Microsoft.Extensions.Logging;
using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Repositories;

namespace WPFCore.App.Modules.Menus.Services;

/// <summary>
/// <see cref="IMenuService"/>: chuyển danh sách phẳng <see cref="MenuItemEntity"/> (có <c>ParentId</c>)
/// thành cây <see cref="MenuNode"/> bằng đệ quy. Loại bỏ các mục <c>IsEnabled = false</c>.
/// </summary>
public sealed class MenuService : IMenuService
{
    private readonly IMenuRepository _repository;
    private readonly MenuMapper _mapper;
    private readonly ILogger<MenuService> _logger;

    public MenuService(IMenuRepository repository, MenuMapper mapper, ILogger<MenuService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    // Khoá đại diện cho "không có cha" (top-level). Id thật tự tăng từ 1 nên 0 không trùng.
    private const int RootKey = 0;

    public async Task<IReadOnlyList<MenuNode>> GetMenuTreeAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        // Gom các mục theo ParentId để tra cứu nhanh khi dựng cây.
        // Key = ParentId (RootKey cho top-level), Value = các mục con đã sắp theo SortOrder.
        // Dùng RootKey thay cho null vì Dictionary không nhận key kiểu nullable (int?).
        var byParent = new Dictionary<int, List<MenuItemEntity>>();
        foreach (var group in all.Where(m => m.IsEnabled).GroupBy(m => m.ParentId ?? RootKey))
        {
            byParent[group.Key] = group.OrderBy(m => m.SortOrder).ThenBy(m => m.Id).ToList();
        }

        var rootNodes = BuildChildren(RootKey, byParent);
        _logger.LogInformation("Built menu tree: {Count} top-level node(s)", rootNodes.Count);
        return rootNodes;
    }

    public async Task<IReadOnlyList<MenuDto>> GetAllFlatAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return entities.Select(_mapper.ToDto).ToList();
    }

    public async Task<MenuDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null ? null : _mapper.ToDto(entity);
    }

    public async Task<int> SaveAsync(MenuDto dto, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(dto.ActionKey))
        {
            var exists = await _repository.ExistsByActionKeyAsync(dto.ActionKey, dto.Id > 0 ? dto.Id : null, cancellationToken).ConfigureAwait(false);
            if (exists)
            {
                throw new InvalidOperationException($"ActionKey '{dto.ActionKey}' đã tồn tại.");
            }
        }

        var entity = _mapper.ToEntity(dto);

        if (entity.Id == 0)
        {
            return await _repository.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity.Id;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Dựng đệ quy danh sách con của <paramref name="parentKey"/>.</summary>
    private static IReadOnlyList<MenuNode> BuildChildren(
        int parentKey,
        IReadOnlyDictionary<int, List<MenuItemEntity>> byParent)
    {
        if (!byParent.TryGetValue(parentKey, out var items))
        {
            return Array.Empty<MenuNode>();
        }

        return items
            .Select(entity => new MenuNode
            {
                Id = entity.Id,
                Title = entity.Title,
                ActionKey = entity.ActionKey,
                SortOrder = entity.SortOrder,
                Glyph = entity.Glyph,
                IsEnabled = entity.IsEnabled,
                Children = BuildChildren(entity.Id, byParent), // đệ quy xuống cấp dưới
            })
            .ToList();
    }
}
