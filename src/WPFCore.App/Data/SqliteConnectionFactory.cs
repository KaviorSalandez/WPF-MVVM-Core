using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WPFCore.App.Configuration;

namespace WPFCore.App.Data;

/// <summary>
/// <see cref="IDbConnectionFactory"/> cho SQLite. Đăng ký qua DI factory để truyền connection string
/// đã được resolve từ <c>appsettings.json</c> + biến môi trường.
/// </summary>
public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private static int _typeHandlersRegistered;
    private readonly DatabaseOptions _options;

    public string ConnectionString { get; }

    public SqliteConnectionFactory(IOptions<DatabaseOptions> options, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        _options = options.Value;
        ConnectionString = connectionString;

        // Register Dapper type handlers exactly once per AppDomain (Dapper lưu static mapping)
        if (Interlocked.Exchange(ref _typeHandlersRegistered, 1) == 0)
        {
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
        }
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (_options.EnableWal)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL;";
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        return connection;
    }
}
