using Dapper;
using Microsoft.Extensions.Logging;
using WPFCore.App.Data;
using WPFCore.App.Modules.Customers.Models;

namespace WPFCore.App.Modules.Customers.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(IDbConnectionFactory connectionFactory, ILogger<CustomerRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Code, Name, Email, Phone, Address, DateOfBirth, CreatedAt, UpdatedAt
            FROM customers
            WHERE (@Search IS NULL OR @Search = '' OR Code LIKE @Like OR Name LIKE @Like OR Email LIKE @Like OR Phone LIKE @Like)
            ORDER BY Code";

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var like = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm.Trim()}%";
        var rows = await conn.QueryAsync<Customer>(
            new CommandDefinition(sql, new { Search = searchTerm, Like = like }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        return rows.AsList();
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Code, Name, Email, Phone, Address, DateOfBirth, CreatedAt, UpdatedAt
            FROM customers WHERE Id = @Id";

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        return await conn.QuerySingleOrDefaultAsync<Customer>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1) FROM customers
            WHERE Code = @Code AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Code = code, ExcludeId = excludeId }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        return count > 0;
    }

    public async Task<int> InsertAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO customers (Code, Name, Email, Phone, Address, DateOfBirth, CreatedAt, UpdatedAt)
            VALUES (@Code, @Name, @Email, @Phone, @Address, @DateOfBirth, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();";

        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = customer.CreatedAt;

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var id = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, customer, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        _logger.LogInformation("Customer inserted: Id={Id}, Code={Code}", id, customer.Code);
        return id;
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE customers SET
                Code = @Code, Name = @Name, Email = @Email, Phone = @Phone,
                Address = @Address, DateOfBirth = @DateOfBirth, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        customer.UpdatedAt = DateTime.UtcNow;

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var affected = await conn.ExecuteAsync(
            new CommandDefinition(sql, customer, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        _logger.LogInformation("Customer updated: Id={Id}, Code={Code}, RowsAffected={Affected}",
            customer.Id, customer.Code, affected);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM customers WHERE Id = @Id";

        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var affected = await conn.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        _logger.LogInformation("Customer deleted: Id={Id}, RowsAffected={Affected}", id, affected);
    }
}
