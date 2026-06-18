using WPFCore.App.Modules.Customers.Models;

namespace WPFCore.App.Modules.Customers.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
