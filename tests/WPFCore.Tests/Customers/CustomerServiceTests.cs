using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using WPFCore.App.Modules.Customers.Services;
using WPFCore.App.Modules.Customers.Repositories;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.Tests.Shared;

namespace WPFCore.Tests.Customers;

[TestFixture]
public sealed class CustomerServiceTests : TestBase
{
    private Mock<ICustomerRepository> _repositoryMock = null!;
    private Mock<IValidator<Customer>> _validatorMock = null!;
    private CustomerService _sut = null!;

    private static readonly Customer ValidCustomer = new()
    {
        Id = 0,
        Code = "KH001",
        Name = "Nguyễn Văn A",
        Email = "a@example.com"
    };

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = CreateMock<ICustomerRepository>();
        _validatorMock = CreateMock<IValidator<Customer>>();
        _sut = new CustomerService(_repositoryMock.Object, _validatorMock.Object, CreateLogger<CustomerService>());
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnRepositoryData()
    {
        // Arrange
        var customers = new List<Customer> { ValidCustomer };
        _repositoryMock.Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(customers);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCustomer()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidCustomer);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeSameAs(ValidCustomer);
    }

    [Test]
    public async Task SaveAsync_WithNewValidCustomer_ShouldInsert()
    {
        // Arrange
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.ExistsByCodeAsync("KH001", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.InsertAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        // Act
        await _sut.SaveAsync(ValidCustomer);

        // Assert
        _repositoryMock.Verify(r => r.InsertAsync(It.Is<Customer>(c => c.Code == "KH001"), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        ValidCustomer.Id.Should().Be(42);
    }

    [Test]
    public async Task SaveAsync_WithExistingCustomer_ShouldUpdate()
    {
        // Arrange
        var existing = new Customer { Id = 5, Code = "KH001", Name = "Updated" };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.ExistsByCodeAsync("KH001", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.SaveAsync(existing);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SaveAsync_WithDuplicateCode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.ExistsByCodeAsync("KH001", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.SaveAsync(ValidCustomer);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*KH001*đã tồn tại*");
    }

    [Test]
    public async Task SaveAsync_WithInvalidValidation_ShouldThrowValidationException()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("Code", "Required") };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        Func<Task> act = async () => await _sut.SaveAsync(ValidCustomer);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task DeleteAsync_WithExistingCustomer_ShouldCallRepository()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidCustomer);
        _repositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingCustomer_ShouldThrow()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Id=999*");
    }
}
