using System.Collections;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.Validation;
using WPFCore.App.Shared.ViewModels;
using WPFCore.App.Modules.Customers.Models;
using WPFCore.App.Modules.Customers.Services;

namespace WPFCore.App.Modules.Customers.ViewModels;

public sealed partial class CustomerEditViewModel : ViewModelBase, INotifyDataErrorInfo, IDialogAware
{
    private readonly ICustomerService _service;
    private readonly IValidator<Customer> _validator;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private readonly ILogger<CustomerEditViewModel> _logger;
    private int _id;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private DateOnly? _dateOfBirth;

    [ObservableProperty]
    private bool _isNew = true;

    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    // IDialogAware implementation
    public string? DialogTitle => Title;
    public event Action<bool?>? RequestClose;

    public CustomerEditViewModel(
        ICustomerService service,
        IValidator<Customer> validator,
        INavigationService navigation,
        IDialogService dialog,
        ILogger<CustomerEditViewModel> logger)
    {
        _service = service;
        _validator = validator;
        _navigation = navigation;
        _dialog = dialog;
        _logger = logger;
        Title = "Khách hàng";
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(e => e);
        return _errors.TryGetValue(propertyName, out var list) ? list : Array.Empty<string>();
    }

    public override async Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
    {
        if (parameter is CustomerEditParameter p && p.CustomerId.HasValue)
        {
            try
            {
                IsBusy = true;
                var existing = await _service.GetByIdAsync(p.CustomerId.Value, cancellationToken).ConfigureAwait(true);
                if (existing is not null)
                {
                    _id = existing.Id;
                    Code = existing.Code;
                    Name = existing.Name;
                    Email = existing.Email;
                    Phone = existing.Phone;
                    Address = existing.Address;
                    DateOfBirth = existing.DateOfBirth;
                    IsNew = false;
                    Title = $"Sửa: {existing.Name}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load customer {Id}", p.CustomerId);
                await _dialog.ShowErrorAsync("Lỗi", "Không thể tải thông tin khách hàng.", ex).ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }
        else
        {
            IsNew = true;
            Title = "Thêm khách hàng mới";
        }
        await base.OnNavigatedToAsync(parameter, cancellationToken).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Id = _id,
            Code = Code?.Trim() ?? string.Empty,
            Name = Name?.Trim() ?? string.Empty,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            DateOfBirth = DateOfBirth
        };

        try
        {
            IsBusy = true;
            await _service.SaveAsync(customer, cancellationToken).ConfigureAwait(true);
            _logger.LogInformation("Customer saved: {Code}", customer.Code);
            CloseOrNavigateBack(true);
        }
        catch (ValidationException vex)
        {
            // Surface validation errors via INotifyDataErrorInfo
            _errors.Clear();
            foreach (var group in vex.Errors.GroupBy(e => e.PropertyName))
            {
                _errors[group.Key] = group.Select(e => e.ErrorMessage).ToList();
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(group.Key));
            }
            OnPropertyChanged(nameof(HasErrors));
            SaveCommand.NotifyCanExecuteChanged();
            _logger.LogWarning("Customer validation failed: {Errors}", vex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save customer");
            await _dialog.ShowErrorAsync("Lỗi", $"Không thể lưu khách hàng.", ex).ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseOrNavigateBack(false);
    }

    /// <summary>
    /// Nếu đang ở trong dialog → fire RequestClose để đóng popup.
    /// Nếu đang ở dạng page navigation → fallback về NavigateBack.
    /// </summary>
    private void CloseOrNavigateBack(bool? dialogResult)
    {
        if (RequestClose is not null)
        {
            RequestClose.Invoke(dialogResult);
        }
        else
        {
            _navigation.NavigateBack();
        }
    }

    private bool CanSave() => !HasErrors
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Code);

    partial void OnCodeChanged(string value) => ValidateProperty(nameof(Code), value);

    partial void OnNameChanged(string value) => ValidateProperty(nameof(Name), value);

    partial void OnEmailChanged(string? value) => ValidateProperty(nameof(Email), value);
    partial void OnPhoneChanged(string? value) => ValidateProperty(nameof(Phone), value);
    partial void OnAddressChanged(string? value) => ValidateProperty(nameof(Address), value);

    private void ValidateProperty(string propertyName, object? value)
    {
        var customer = new Customer
        {
            Id = _id,
            Code = Code,
            Name = Name,
            Email = Email,
            Phone = Phone,
            Address = Address,
            DateOfBirth = DateOfBirth
        };
        var result = _validator.Validate(customer, options => options.IncludeProperties(propertyName));
        var newErrors = result.Errors.Where(e => e.PropertyName == propertyName).Select(e => e.ErrorMessage).ToList();

        if (newErrors.Any())
            _errors[propertyName] = newErrors;
        else
            _errors.Remove(propertyName);

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
        SaveCommand.NotifyCanExecuteChanged();
    }
}
