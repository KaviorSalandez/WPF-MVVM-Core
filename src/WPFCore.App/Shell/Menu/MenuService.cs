using Microsoft.Extensions.Logging;

namespace WPFCore.App.Shell.Menu;

/// <summary>
/// <see cref="IMenuService"/>: chuyển danh sách phẳng <see cref="MenuItemEntity"/> (có <c>ParentId</c>)
/// thành cây <see cref="MenuNode"/> bằng đệ quy. Loại bỏ các mục <c>IsEnabled = false</c>.
/// </summary>
public sealed class MenuService : IMenuService
{
    private readonly IMenuRepository _repository;
    private readonly ILogger<MenuService> _logger;

    public MenuService(IMenuRepository repository, ILogger<MenuService> logger)
    {
        _repository = repository;
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

        var tree = BuildChildren(RootKey, byParent);
        _logger.LogInformation("Built menu tree: {Count} top-level node(s)", tree.Count);
        return tree;
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
                Title = entity.Title,
                ActionKey = entity.ActionKey,
                Glyph = entity.Glyph,
                IsEnabled = entity.IsEnabled,
                Children = BuildChildren(entity.Id, byParent), // đệ quy xuống cấp dưới
            })
            .ToList();
    }
}
