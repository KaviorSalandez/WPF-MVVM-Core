using System.Data.Common;

namespace WPFCore.App.Data;

/// <summary>
/// Tạo SQLite <see cref="DbConnection"/> mở sẵn cho mỗi unit-of-work.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Mở một connection mới tới database đã cấu hình. Caller chịu trách nhiệm dispose.
    /// </summary>
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Connection string hiện tại (cho EF Core / migrations).</summary>
    string ConnectionString { get; }
}
