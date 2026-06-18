using FluentAssertions;
using NUnit.Framework;
using WPFCore.App.Modules.Customers.ViewModels;
using WPFCore.App.Shell;

namespace WPFCore.Tests.Shell;

[TestFixture]
public sealed class MenuDefinitionsTests
{
    [Test]
    public void MainMenu_ShouldInitializeWithoutThrowing()
    {
        // Act
        var action = () => MenuDefinitions.MainMenu;

        // Assert
        action.Should().NotThrow();
    }

    [Test]
    public void MainMenu_FirstItem_ShouldNavigateToCustomerListViewModel()
    {
        // Arrange & Act
        var mainMenu = MenuDefinitions.MainMenu;

        // Assert
        mainMenu.Should().NotBeNull();
        mainMenu.Should().NotBeEmpty();
        
        var customerItem = mainMenu[0];
        customerItem.Title.Should().Be("Khách hàng");
        customerItem.NavigateToViewModel.Should().Be(typeof(CustomerListViewModel));
    }
}
