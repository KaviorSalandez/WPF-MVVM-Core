using Dapper;
using WPFCore.App.Data;

namespace WPFCore.App.Shell.Menu;

/// <summary>
/// <see cref="IMenuRepository"/> dùng Dapper + SQL thuần qua <see cref="IDbConnectionFactory"/>,
/// giống hệt cách <c>CustomerRepository</c> truy vấn (mỗi thao tác một connection).
/// </summary>
public sealed class MenuRepository : IMenuRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MenuRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, ParentId, Title, ActionKey, SortOrder, Glyph, IsEnabled
            FROM menu_items
            ORDER BY ParentId, SortOrder, Id";

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var rows = await conn.QueryAsync<MenuItemEntity>(
            new CommandDefinition(sql, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        return rows.AsList();
    }
}
