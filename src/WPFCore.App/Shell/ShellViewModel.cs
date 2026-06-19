using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Shell;

/// <summary>
/// ViewModel chính cho Shell <c>MainWindow</c>. Quản lý <see cref="WindowTitle"/>, danh sách
/// menu (<see cref="MenuItems"/>) và phản hồi <c>INavigationService.Navigated</c> để cập nhật
/// <see cref="CurrentView"/> khi người dùng điều hướng giữa các page.
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

    /// <summary>Danh sách menu tĩnh hiển thị trên thanh menu của MainWindow.</summary>
    public MenuItemDefinition[] MenuItems { get; } = MenuDefinitions.MainMenu;

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
    /// Command được binding từ menu. Phân luồng: nếu item có <c>NavigateToViewModel</c> thì
    /// điều hướng, nếu có <c>ActionKey</c> thì gọi <see cref="HandleAction"/>.
    /// </summary>
    [RelayCommand]
    private void Navigate(MenuItemDefinition? menuItem)
    {
        if (menuItem is null) return;

        if (menuItem.NavigateToViewModel is not null)
        {
            try
            {
                _navigation.NavigateTo(menuItem.NavigateToViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to navigate to {Vm}", menuItem.NavigateToViewModel.Name);
                _ = _dialog.ShowErrorAsync("Lỗi", $"Không thể mở '{menuItem.Title}'.", ex);
            }
        }
        else if (menuItem.ActionKey is not null)
        {
            HandleAction(menuItem.ActionKey);
        }
    }

    [RelayCommand]
    private void Action(string actionKey)
    {
        if (actionKey == "CustomerList")
        {
            _navigation.NavigateTo(typeof(WPFCore.App.Modules.Customers.ViewModels.CustomerListViewModel));
            return;
        }
        else if (actionKey == "Dashboard")
        {
            _navigation.NavigateTo(typeof(WPFCore.App.Modules.Dashboard.ViewModels.DashboardViewModel));
            return;
        }
        
        HandleAction(actionKey);
    }

    private void HandleAction(string actionKey)
    {
        switch (actionKey)
        {
            // ── Danh mục ──────────────────────────────────────────────
            case "MapLayer":
            case "GeoFeature":
            case "DataSource":
            case "CheckRules":
            case "MapType":
            case "Users":
                _ = _dialog.ShowMessageAsync("Thông báo",
                    $"Chức năng \"{actionKey}\" đang được phát triển.");
                break;

            // ── Hệ thống ──────────────────────────────────────────────
            case "RunCheck":
            case "CheckResults":
            case "SummaryReport":
            case "ExportReport":
                _ = _dialog.ShowMessageAsync("Thông báo",
                    $"Chức năng \"{actionKey}\" đang được phát triển.");
                break;

            // ── Trợ giúp ──────────────────────────────────────────────
            case "About":
                _ = _dialog.ShowMessageAsync(
                    "Giới thiệu",
                    "Hệ thống Kiểm tra Dữ liệu Bản đồ\n.NET 6 + Syncfusion WPF + CommunityToolkit.Mvvm\n\n© 2026");
                break;
            case "Exit":
                Application.Current.Shutdown();
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
    }
}
