using FluentAssertions;
using Moq;
using NUnit.Framework;
using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Repositories;
using WPFCore.App.Modules.Menus.Services;
using WPFCore.Tests.Shared;

namespace WPFCore.Tests.Shell;

[TestFixture]
public sealed class MenuServiceTests : TestBase
{
    private static MenuService CreateService(IReadOnlyList<MenuItemEntity> rows, out Mock<IMenuRepository> repo)
    {
        repo = CreateMock<IMenuRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        return new MenuService(repo.Object, new MenuMapper(), CreateLogger<MenuService>());
    }

    [Test]
    public async Task GetMenuTreeAsync_BuildsParentChildHierarchy_SortedBySortOrder()
    {
        var service = CreateService(new List<MenuItemEntity>
        {
            new() { Id = 1, ParentId = null, Title = "Danh mục", ActionKey = null, SortOrder = 1 },
            new() { Id = 2, ParentId = 1, Title = "Con A", ActionKey = "A", SortOrder = 2 },
            new() { Id = 3, ParentId = 1, Title = "Con B", ActionKey = "B", SortOrder = 1 },
            new() { Id = 4, ParentId = null, Title = "Thoát", ActionKey = "Exit", SortOrder = 2 },
        }, out _);

        var tree = await service.GetMenuTreeAsync();

        tree.Should().HaveCount(2);
        tree[0].Title.Should().Be("Danh mục");
        tree[0].HasChildren.Should().BeTrue();
        tree[0].Children.Should().HaveCount(2);
        tree[0].Children[0].Title.Should().Be("Con B"); // SortOrder=1 đứng trước
        tree[0].Children[1].Title.Should().Be("Con A"); // SortOrder=2
        tree[1].Title.Should().Be("Thoát");
        tree[1].HasChildren.Should().BeFalse();
        tree[1].ActionKey.Should().Be("Exit");
    }

    [Test]
    public async Task GetMenuTreeAsync_ExcludesDisabledItems()
    {
        var service = CreateService(new List<MenuItemEntity>
        {
            new() { Id = 1, ParentId = null, Title = "Hiện", ActionKey = "X", SortOrder = 1, IsEnabled = true },
            new() { Id = 2, ParentId = null, Title = "Ẩn", ActionKey = "Y", SortOrder = 2, IsEnabled = false },
        }, out _);

        var tree = await service.GetMenuTreeAsync();

        tree.Should().ContainSingle();
        tree[0].Title.Should().Be("Hiện");
    }
}
