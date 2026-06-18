using System.Data.Common;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using WPFCore.App.Data;
using WPFCore.App.Modules.Customers.Repositories;
using WPFCore.App.Modules.Customers.Models;

namespace WPFCore.Tests.Customers;

[TestFixture]
public sealed class CustomerRepositoryTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _dbContext = null!;
    private TestConnectionFactory _connectionFactory = null!;
    private CustomerRepository _sut = null!;

    /// <summary>
    /// Trả về connection MỚI mở tới cùng shared in-memory database.
    /// Dùng <c>Data Source=file::memory:?cache=shared</c> để nhiều connection
    /// thấy cùng schema &amp; data (khi ít nhất 1 connection giữ mở).
    /// </summary>
    private sealed class TestConnectionFactory : IDbConnectionFactory
    {
        private readonly DbConnection _shared;

        public TestConnectionFactory(DbConnection shared)
        {
            _shared = shared;
            ConnectionString = _shared.ConnectionString;
        }

        public string ConnectionString { get; }

        public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var c = new SqliteConnection(_shared.ConnectionString);
            await c.OpenAsync(cancellationToken).ConfigureAwait(false);
            return c;
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        // Shared in-memory connection giữ DB alive xuyên suốt test.
        // 'cache=shared' cho phép mọi connection mới cùng string thấy cùng database.
        _connection = new SqliteConnection("Data Source=file::memory:?cache=shared");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new AppDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _connectionFactory = new TestConnectionFactory(_connection);
        _sut = new CustomerRepository(_connectionFactory, NullLogger<CustomerRepository>.Instance);

        // Clean state — wipe all rows inserted by previous tests in this assembly
        // (in-memory shared DB persists across [SetUp]/[TearDown] within same AppDomain).
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM customers";
        await cmd.ExecuteNonQueryAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Test]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmpty()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task InsertAsync_ThenGetById_ShouldReturnInserted()
    {
        // Arrange
        var customer = new Customer
        {
            Code = "KH001",
            Name = "Nguyễn Văn A",
            Email = "a@example.com"
        };

        // Act
        var id = await _sut.InsertAsync(customer);
        var fetched = await _sut.GetByIdAsync(id);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Code.Should().Be("KH001");
        fetched.Name.Should().Be("Nguyễn Văn A");
        fetched.Email.Should().Be("a@example.com");
        fetched.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task ExistsByCodeAsync_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        var customer = new Customer { Code = "KH001", Name = "Test" };
        await _sut.InsertAsync(customer);

        // Act
        var exists = await _sut.ExistsByCodeAsync("KH001");

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsByCodeAsync_WithExcludeId_ShouldIgnoreThatRow()
    {
        // Arrange
        var customer = new Customer { Code = "KH001", Name = "Test" };
        var id = await _sut.InsertAsync(customer);

        // Act
        var existsWithoutExclude = await _sut.ExistsByCodeAsync("KH001");
        var existsWithExclude = await _sut.ExistsByCodeAsync("KH001", excludeId: id);

        // Assert
        existsWithoutExclude.Should().BeTrue();
        existsWithExclude.Should().BeFalse();
    }

    [Test]
    public async Task GetAllAsync_WithSearch_ShouldFilter()
    {
        // Arrange
        await _sut.InsertAsync(new Customer { Code = "KH001", Name = "Alpha" });
        await _sut.InsertAsync(new Customer { Code = "KH002", Name = "Beta" });
        await _sut.InsertAsync(new Customer { Code = "KH003", Name = "Gamma" });

        // Act
        var all = await _sut.GetAllAsync();
        var beta = await _sut.GetAllAsync("Beta");
        var kh002 = await _sut.GetAllAsync("KH002");

        // Assert
        all.Should().HaveCount(3);
        beta.Should().ContainSingle().Which.Name.Should().Be("Beta");
        kh002.Should().ContainSingle().Which.Code.Should().Be("KH002");
    }

    [Test]
    public async Task UpdateAsync_ShouldModifyRow()
    {
        // Arrange
        var customer = new Customer { Code = "KH001", Name = "Original" };
        var id = await _sut.InsertAsync(customer);

        // Act
        customer.Id = id;
        customer.Name = "Updated";
        customer.Email = "new@example.com";
        await _sut.UpdateAsync(customer);
        var fetched = await _sut.GetByIdAsync(id);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Updated");
        fetched.Email.Should().Be("new@example.com");
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveRow()
    {
        // Arrange
        var customer = new Customer { Code = "KH001", Name = "Test" };
        var id = await _sut.InsertAsync(customer);

        // Act
        await _sut.DeleteAsync(id);
        var fetched = await _sut.GetByIdAsync(id);

        // Assert
        fetched.Should().BeNull();
    }
}
