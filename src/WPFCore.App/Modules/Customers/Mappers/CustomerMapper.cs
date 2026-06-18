using Riok.Mapperly.Abstractions;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Customers.Dtos;

namespace WPFCore.App.Modules.Customers.Mappers;

[Mapper]
public partial class CustomerMapper
{
    public partial CustomerDto ToDto(Customer customer);

    public partial Customer FromCreateRequest(CreateCustomerRequest request);

    public partial void ApplyUpdate(UpdateCustomerRequest request, Customer customer);
}
