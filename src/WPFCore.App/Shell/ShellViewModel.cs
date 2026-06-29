using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.ViewModels;
using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Services;

namespace WPFCore.App.Shell;

/// <summary>
/// ViewModel chính cho Shell <c>MainWindow</c>. Quản lý <see cref="WindowTitle"/>, danh sách
/// menu động (<see cref="MenuItems"/>) nạp từ cơ sở dữ liệu, và phản hồi
/// <c>INavigationService.Navigated</c> để cập nhật tiêu đề khi điều hướng.
/// </summary>
public sealed partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private readonly ILogger<ShellViewModel> _logger;
    private readonly IServiceProvider _services;

    [ObservableProperty]
    private string _windowTitle = "WPFCore Desktop Boilerplate";

    [ObservableProperty]
    private object? _currentView;

    /// <summary>
    /// Cây menu được nạp động từ DB. Trống lúc khởi tạo; <see cref="InitializeAsync"/> sẽ đổ dữ liệu.
    /// Là <see cref="ObservableCollection{T}"/> nên UI tự cập nhật sau khi nạp xong.
    /// </summary>
    public ObservableCollection<MenuNode> MenuItems { get; } = new();

    /// <summary>
    /// Danh sách các node breadcrumb hiện tại để bind lên UI.
    /// </summary>
    public ObservableCollection<BreadcrumbItem> Breadcrumbs { get; } = new();

    private List<string> _currentMenuPath = new();

    public ShellViewModel(
        INavigationService navigation,
        IDialogService dialog,
        ILogger<ShellViewModel> logger,
        IServiceProvider services)
    {
        _navigation = navigation;
        _dialog = dialog;
        _logger = logger;
        _services = services;
        _navigation.Navigated += OnNavigated;
    }

    /// <summary>
    /// Nạp menu từ DB và đổ vào <see cref="MenuItems"/>. Gọi từ <c>MainWindow.Loaded</c>
    /// (sau khi cửa sổ đã hiện) để truy vấn DB không chặn lúc khởi tạo.
    /// </summary>
    /// <remarks>
    /// Tạo scope DI riêng để dùng <see cref="IMenuService"/> (Scoped) từ ShellViewModel (Singleton) —
    /// tránh "captive dependency", giống cách <c>AppStartup</c> tạo scope cho DbContext.
    /// </remarks>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _services.CreateScope();
            var menuService = scope.ServiceProvider.GetRequiredService<IMenuService>();
            var tree = await menuService.GetMenuTreeAsync(cancellationToken).ConfigureAwait(true);

            MenuItems.Clear();
            foreach (var node in tree)
            {
                MenuItems.Add(node);
            }

            _logger.LogInformation("Menu loaded from database: {Count} top-level item(s)", MenuItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load menu from database");
            await _dialog.ShowErrorAsync("Lỗi", "Không thể tải menu từ cơ sở dữ liệu.", ex).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Command bind cho mọi mục menu. Tham số là <see cref="MenuNode"/> đang được bấm.
    /// Nhóm cha (có mục con) chỉ mở submenu nên bỏ qua; mục lá thì dispatch theo <c>ActionKey</c>.
    /// </summary>
    [RelayCommand]
    private void Menu(MenuNode? node)
    {
        if (node is null || node.HasChildren)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(node.ActionKey))
        {
            return;
        }

        // Tìm đường dẫn menu (ví dụ: ["Hệ thống", "Quản lý khách hàng"])
        var path = FindNodePath(MenuItems, node, new List<string>());
        if (path != null && path.Count > 0)
        {
            // Bỏ phần tử cuối (mục lá) vì View/ViewModel sẽ tự có Title riêng
            _currentMenuPath = path.Take(path.Count - 1).ToList();
        }
        else
        {
            _currentMenuPath.Clear();
        }

        Dispatch(node.ActionKey);
    }

    private List<string>? FindNodePath(IEnumerable<MenuNode> nodes, MenuNode target, List<string> currentPath)
    {
        foreach (var node in nodes)
        {
            var newPath = new List<string>(currentPath) { node.Title };
            if (node == target)
            {
                return newPath;
            }
            if (node.HasChildren)
            {
                var result = FindNodePath(node.Children, target, newPath);
                if (result != null) return result;
            }
        }
        return null;
    }

    /// <summary>Phân luồng theo <c>ActionKey</c>: điều hướng tới ViewModel hoặc thực thi hành động.</summary>
    private void Dispatch(string actionKey)
    {
        // Khi user click menu chính, reset lại lịch sử điều hướng để bắt đầu breadcrumb mới.
        if (actionKey == "CustomerList" || actionKey == "Dashboard" || actionKey == "Menus")
        {
            _navigation.ClearHistory();
        }

        switch (actionKey)
        {
            // ── Điều hướng tới các màn hình đã có ──────────────────────
            case "CustomerList":
                _navigation.NavigateTo(typeof(WPFCore.App.Modules.Customers.ViewModels.CustomerListViewModel));
                break;
            case "Dashboard":
                _navigation.NavigateTo(typeof(WPFCore.App.Modules.Dashboard.ViewModels.DashboardViewModel));
                break;
            case "Menus":
                _navigation.NavigateTo(typeof(WPFCore.App.Modules.Menus.ViewModels.MenuListViewModel));
                break;
            case "ViewMap":
                _navigation.NavigateTo(typeof(WPFCore.App.Modules.Maps.ViewModels.ViewMapViewModel));
                break;

            // ── Trợ giúp ───────────────────────────────────────────────
            case "About":
                _ = _dialog.ShowMessageAsync(
                    "Giới thiệu",
                    "Hệ thống Kiểm tra Dữ liệu Bản đồ\n.NET 6 + Syncfusion WPF + CommunityToolkit.Mvvm\n\n© 2026");
                break;
            case "Exit":
                Application.Current.Shutdown();
                break;

            // ── Các chức năng chưa phát triển ──────────────────────────
            default:
                _ = _dialog.ShowMessageAsync("Thông báo",
                    $"Chức năng \"{actionKey}\" đang được phát triển.");
                break;
        }
    }

    private void OnNavigated(object? sender, NavigationEventArgs e)
    {
        CurrentView = e.ViewModel;
        if (e.ViewModel is ViewModelBase vm)
        {
            WindowTitle = $"WPFCore — {vm.Title ?? "Trang"}";
        }
        UpdateBreadcrumbs();
    }

    private void UpdateBreadcrumbs()
    {
        Breadcrumbs.Clear();

        // 1. Thêm các menu cha (ví dụ: "Hệ thống")
        foreach (var menuTitle in _currentMenuPath)
        {
            Breadcrumbs.Add(new BreadcrumbItem
            {
                Title = menuTitle,
                ViewModel = null!, 
                NavigateCommand = null! // Không cho click vào mục hệ thống
            });
        }

        // 2. Thêm lịch sử điều hướng (từ FrameNavigationService)
        foreach (var vmObj in _navigation.History)
        {
            if (vmObj is ViewModelBase vmBase)
            {
                Breadcrumbs.Add(new BreadcrumbItem
                {
                    Title = vmBase.Title ?? "Unknown",
                    ViewModel = vmObj,
                    NavigateCommand = new RelayCommand(() => NavigateToBreadcrumb(vmObj))
                });
            }
        }
    }

    private void NavigateToBreadcrumb(object targetViewModel)
    {
        if (_navigation.CurrentViewModel == targetViewModel)
        {
            return;
        }

        while (_navigation.CanNavigateBack && _navigation.CurrentViewModel != targetViewModel)
        {
            _navigation.NavigateBack();
        }
    }
}
