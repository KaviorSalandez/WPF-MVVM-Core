using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Services;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Modules.Menus.ViewModels;

public sealed partial class MenuListViewModel : ViewModelBase
{
    private readonly IMenuService _service;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private readonly ILogger<MenuListViewModel> _logger;

    public ObservableCollection<MenuNode> MenuItems { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private MenuNode? _selectedMenu;

    [ObservableProperty]
    private bool _isBusy;

    public MenuListViewModel(
        IMenuService service,
        INavigationService navigation,
        IDialogService dialog,
        ILogger<MenuListViewModel> logger)
    {
        _service = service;
        _navigation = navigation;
        _dialog = dialog;
        _logger = logger;
        Title = "Quản lý menu";
    }

    public override async Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
    {
        await LoadAsync(cancellationToken).ConfigureAwait(true);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsBusy = true;
            var data = await _service.GetMenuTreeAsync(cancellationToken).ConfigureAwait(true);
            MenuItems.Clear();
            foreach (var item in data)
            {
                MenuItems.Add(item);
            }
            _logger.LogInformation("Loaded menu tree");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load menus");
            await _dialog.ShowErrorAsync("Lỗi", "Không thể tải danh sách menu.", ex).ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        // Gợi ý ParentId nếu người dùng đang chọn 1 mục
        int? parentId = SelectedMenu?.Id;
        var result = await _dialog.ShowDialogAsync<MenuAddViewModel>(new MenuEditParameter(null, parentId)).ConfigureAwait(true);
        if (result == true)
        {
            await LoadAsync(CancellationToken.None).ConfigureAwait(true);
            await _dialog.ShowMessageAsync("Thông báo", "Thêm menu thành công!").ConfigureAwait(true);
        }
    }

    private bool CanEditOrDelete() => SelectedMenu is not null;

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedMenu is null) return;
        var result = await _dialog.ShowDialogAsync<MenuEditViewModel>(new MenuEditParameter(SelectedMenu.Id)).ConfigureAwait(true);
        if (result == true)
        {
            await LoadAsync(CancellationToken.None).ConfigureAwait(true);
            await _dialog.ShowMessageAsync("Thông báo", "Cập nhật menu thành công!").ConfigureAwait(true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedMenu is null) return;

        if (SelectedMenu.HasChildren)
        {
            await _dialog.ShowMessageAsync("Cảnh báo", "Không thể xóa menu đang có menu con. Vui lòng xóa menu con trước.").ConfigureAwait(true);
            return;
        }

        var confirm = await _dialog.ShowConfirmationAsync("Xác nhận", $"Bạn có chắc chắn muốn xóa menu '{SelectedMenu.Title}'?").ConfigureAwait(true);
        if (confirm)
        {
            try
            {
                IsBusy = true;
                await _service.DeleteAsync(SelectedMenu.Id, CancellationToken.None).ConfigureAwait(true);
                await LoadAsync(CancellationToken.None).ConfigureAwait(true);
                await _dialog.ShowMessageAsync("Thông báo", "Xóa menu thành công!").ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete menu {Id}", SelectedMenu.Id);
                await _dialog.ShowErrorAsync("Lỗi", "Không thể xóa menu.", ex).ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
