using FluentValidation;
using Microsoft.Extensions.Logging;
using WPFCore.App.Shared.Validation;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Customers.Repositories;

namespace WPFCore.App.Modules.Customers.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IValidator<Customer> _validator;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository repository, IValidator<Customer> validator, ILogger<CustomerService> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(searchTerm, cancellationToken);

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public async Task SaveAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(customer, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Customer validation failed: {Errors}", validation.ToSingleLineMessage());
            throw new ValidationException(validation.Errors);
        }

        var isDuplicate = await _repository.ExistsByCodeAsync(customer.Code, customer.Id == 0 ? null : customer.Id, cancellationToken)
            .ConfigureAwait(false);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"Mã khách hàng '{customer.Code}' đã tồn tại trong hệ thống.");
        }

        if (customer.Id == 0)
        {
            customer.Id = await _repository.InsertAsync(customer, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _repository.UpdateAsync(customer, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            throw new InvalidOperationException($"Không tìm thấy khách hàng Id={id} để xóa.");
        }
        await _repository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
    }
}
