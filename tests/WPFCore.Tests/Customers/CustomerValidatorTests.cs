using FluentAssertions;
using NUnit.Framework;
using WPFCore.App.Modules.Customers.Validation;
using WPFCore.App.Modules.Customers.Models;

namespace WPFCore.Tests.Customers;

[TestFixture]
public sealed class CustomerValidatorTests
{
    private CustomerValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CustomerValidator();
    }

    [Test]
    public async Task ValidateAsync_WithValidCustomer_ShouldPass()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 0,
            Code = "KH001",
            Name = "Nguyễn Văn A",
            Email = "a@example.com",
            Phone = "0901234567",
            Address = "Hà Nội",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateAsync_WithEmptyCode_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "", Name = "Test" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Code));
    }

    [Test]
    public async Task ValidateAsync_WithLowercaseCode_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "kh001", Name = "Test" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Code)
            && e.ErrorMessage.Contains("in hoa"));
    }

    [Test]
    public async Task ValidateAsync_WithCodeTooShort_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "AB", Name = "Test" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Code));
    }

    [Test]
    public async Task ValidateAsync_WithCodeTooLong_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "ABCDEFGHIJK", Name = "Test" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Code));
    }

    [Test]
    public async Task ValidateAsync_WithEmptyName_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "KH001", Name = "" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Name));
    }

    [Test]
    public async Task ValidateAsync_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "KH001", Name = "Test", Email = "not-an-email" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Email));
    }

    [Test]
    public async Task ValidateAsync_WithNullEmail_ShouldPass()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "KH001", Name = "Test", Email = null };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task ValidateAsync_WithInvalidPhone_ShouldFail()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "KH001", Name = "Test", Phone = "abc-xyz" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Customer.Phone));
    }

    [Test]
    public async Task ValidateAsync_WithValidPhone_ShouldPass()
    {
        // Arrange
        var customer = new Customer { Id = 0, Code = "KH001", Name = "Test", Phone = "+84 901 234 567" };

        // Act
        var result = await _sut.ValidateAsync(customer);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
