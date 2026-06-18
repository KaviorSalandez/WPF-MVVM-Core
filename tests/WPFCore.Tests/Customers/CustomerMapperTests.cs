using FluentAssertions;
using NUnit.Framework;
using WPFCore.App.Modules.Customers.Mappers;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Customers.Dtos;

namespace WPFCore.Tests.Customers;

[TestFixture]
public sealed class CustomerMapperTests
{
    private CustomerMapper _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CustomerMapper();
    }

    [Test]
    public void ToDto_ShouldMapAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var customer = new Customer
        {
            Id = 1,
            Code = "KH001",
            Name = "Nguyễn Văn A",
            Email = "a@example.com",
            Phone = "0901234567",
            Address = "Hà Nội",
            DateOfBirth = new DateOnly(1990, 1, 1),
            CreatedAt = now,
            UpdatedAt = now
        };

        // Act
        var dto = _sut.ToDto(customer);

        // Assert
        dto.Id.Should().Be(1);
        dto.Code.Should().Be("KH001");
        dto.Name.Should().Be("Nguyễn Văn A");
        dto.Email.Should().Be("a@example.com");
        dto.Phone.Should().Be("0901234567");
        dto.Address.Should().Be("Hà Nội");
        dto.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));
        dto.CreatedAt.Should().Be(now);
        dto.UpdatedAt.Should().Be(now);
    }

    [Test]
    public void FromCreateRequest_ShouldMapToCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Code = "KH002",
            Name = "Trần Văn B",
            Email = "b@example.com",
            Phone = "0909999999",
            Address = "TP.HCM",
            DateOfBirth = new DateOnly(1995, 5, 5)
        };

        // Act
        var customer = _sut.FromCreateRequest(request);

        // Assert
        customer.Id.Should().Be(0);
        customer.Code.Should().Be("KH002");
        customer.Name.Should().Be("Trần Văn B");
        customer.Email.Should().Be("b@example.com");
        customer.Phone.Should().Be("0909999999");
        customer.Address.Should().Be("TP.HCM");
        customer.DateOfBirth.Should().Be(new DateOnly(1995, 5, 5));
        customer.CreatedAt.Should().Be(default);
        customer.UpdatedAt.Should().Be(default);
    }

    [Test]
    public void ApplyUpdate_ShouldUpdateOnlyMutableFields()
    {
        // Arrange
        var original = new Customer
        {
            Id = 10,
            Code = "OLD01",
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        var update = new UpdateCustomerRequest
        {
            Id = 10,
            Code = "NEW01",
            Name = "New Name",
            Email = "new@example.com"
        };

        // Act
        _sut.ApplyUpdate(update, original);

        // Assert
        original.Id.Should().Be(10);
        original.Code.Should().Be("NEW01");
        original.Name.Should().Be("New Name");
        original.Email.Should().Be("new@example.com");
    }
}
