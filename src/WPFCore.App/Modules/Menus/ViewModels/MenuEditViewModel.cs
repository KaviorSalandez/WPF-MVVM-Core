using System.Collections;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WPFCore.App.Modules.Menus.Models;
using WPFCore.App.Modules.Menus.Services;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.Validation;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Modules.Menus.ViewModels;

public sealed partial class MenuEditViewModel : ViewModelBase, INotifyDataErrorInfo, IDialogAware
{
    private readonly IMenuService _service;
    private readonly IValidator<MenuDto> _validator;
    private readonly IDialogService _dialog;
    private readonly ILogger<MenuEditViewModel> _logger;
    private int _id;

    [ObservableProperty]
    private string _menuTitle = string.Empty;

    [ObservableProperty]
    private string? _actionKey;

    [ObservableProperty]
    private int? _parentId;

    [ObservableProperty]
    private int _sortOrder;

    [ObservableProperty]
    private string? _glyph;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isBusy;

    public IReadOnlyList<MenuDto> AvailableParents { get; private set; } = Array.Empty<MenuDto>();

    private readonly Dictionary<string, List<string>> _errors = new();
    public bool HasErrors => _errors.Count > 0;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public string? DialogTitle => Title;
    public event Action<bool?>? RequestClose;

    public MenuEditViewModel(
        IMenuService service,
        IValidator<MenuDto> validator,
        IDialogService dialog,
        ILogger<MenuEditViewModel> logger)
    {
        _service = service;
        _validator = validator;
        _dialog = dialog;
        _logger = logger;
        Title = "Sửa menu";
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(e => e);
        return _errors.TryGetValue(propertyName, out var list) ? list : Array.Empty<string>();
    }

    public override async Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
    {
        if (parameter is MenuEditParameter p && p.MenuId.HasValue)
        {
            try
            {
                IsBusy = true;
                
                // Load available parents (exclude self to avoid circular reference)
                var allMenus = await _service.GetAllFlatAsync(cancellationToken).ConfigureAwait(true);
                AvailableParents = allMenus.Where(m => m.Id != p.MenuId.Value).ToList();
                OnPropertyChanged(nameof(AvailableParents));

                var existing = await _service.GetByIdAsync(p.MenuId.Value, cancellationToken).ConfigureAwait(true);
                if (existing is not null)
                {
                    _id = existing.Id;
                    MenuTitle = existing.Title;
                    ActionKey = existing.ActionKey;
                    ParentId = existing.ParentId;
                    SortOrder = existing.SortOrder;
                    Glyph = existing.Glyph;
                    IsEnabled = existing.IsEnabled;
                    Title = $"Sửa: {existing.Title}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load menu {Id}", p.MenuId);
                await _dialog.ShowErrorAsync("Lỗi", "Không thể tải dữ liệu menu.", ex).ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsBusy = true;
            var dto = new MenuDto
            {
                Id = _id,
                Title = MenuTitle?.Trim() ?? string.Empty,
                ActionKey = string.IsNullOrWhiteSpace(ActionKey) ? null : ActionKey.Trim(),
                ParentId = ParentId,
                SortOrder = SortOrder,
                Glyph = string.IsNullOrWhiteSpace(Glyph) ? null : Glyph.Trim(),
                IsEnabled = IsEnabled
            };

            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                _errors.Clear();
                foreach (var error in validationResult.Errors)
                {
                    if (!_errors.ContainsKey(error.PropertyName))
                    {
                        _errors[error.PropertyName] = new List<string>();
                    }
                    _errors[error.PropertyName].Add(error.ErrorMessage);
                }
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(null));
                return;
            }

            await _service.SaveAsync(dto, cancellationToken).ConfigureAwait(true);
            RequestClose?.Invoke(true);
        }
        catch (InvalidOperationException ex)
        {
            await _dialog.ShowErrorAsync("Lỗi", ex.Message).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update menu");
            await _dialog.ShowErrorAsync("Lỗi", "Không thể lưu menu.", ex).ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}
