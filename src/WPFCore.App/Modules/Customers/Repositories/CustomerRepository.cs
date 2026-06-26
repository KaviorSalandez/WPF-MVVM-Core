using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WPFCore.App.Data;
using WPFCore.App.Modules.Customers.Models;

namespace WPFCore.App.Modules.Customers.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(AppDbContext context, ILogger<CustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim();
            // EF Core uses LIKE implicitly for .Contains()
            query = query.Where(c => c.Code.Contains(s) || 
                                     c.Name.Contains(s) || 
                                     (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(s)) || 
                                     (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(s)));
        }

        return await query.OrderBy(c => c.Code).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.Where(c => c.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> InsertAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = customer.CreatedAt;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        _logger.LogInformation("Customer inserted: Id={Id}, Code={Code}", customer.Id, customer.Code);
        return customer.Id;
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        customer.UpdatedAt = DateTime.UtcNow;

        // Xóa tracking cũ để tránh lỗi "already being tracked" khi ViewModel tạo mới instance
        _context.ChangeTracker.Clear();
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        _logger.LogInformation("Customer updated: Id={Id}, Code={Code}", customer.Id, customer.Code);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            var affected = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Customer deleted: Id={Id}, RowsAffected={Affected}", id, affected);
        }
    }
}
