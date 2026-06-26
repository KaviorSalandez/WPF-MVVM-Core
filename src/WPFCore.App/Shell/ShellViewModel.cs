using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.ViewModels;
using WPFCore.App.Shell.Menu;

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

        Dispatch(node.ActionKey);
    }

    /// <summary>Phân luồng theo <c>ActionKey</c>: điều hướng tới ViewModel hoặc thực thi hành động.</summary>
    private void Dispatch(string actionKey)
    {
        switch (actionKey)
        {
            // ── Điều hướng tới các màn hình đã có ──────────────────────
            case "CustomerList":
                _navigation.NavigateTo(typeof(WPFCore.App.Modules.Customers.ViewModels.CustomerListViewModel));
                break;
            case "Dashboard":
                _navigation.NavigateTo(typeof(WPFCore.App.Modules.Dashboard.ViewModels.DashboardViewModel));
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
    }
}
