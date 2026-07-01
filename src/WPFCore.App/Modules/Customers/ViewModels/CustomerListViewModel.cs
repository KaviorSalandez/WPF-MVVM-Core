using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WPFCore.App.Bootstrap;
using WPFCore.App.Shared.Dialogs;
using WPFCore.App.Shared.Navigation;
using WPFCore.App.Shared.ViewModels;
using WPFCore.App.Modules.Customers.Services;
using WPFCore.App.Modules.Customers.Dtos;
using WPFCore.App.Modules.Customers.Mappers;

namespace WPFCore.App.Modules.Customers.ViewModels;

public sealed partial class CustomerListViewModel : ViewModelBase
{
    private readonly ICustomerService _service;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private readonly ILogger<CustomerListViewModel> _logger;
    private readonly CustomerMapper _mapper;
    private CancellationTokenSource? _searchDebounceCts;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private int _pageSize = 20;

    public ObservableCollection<CustomerDto> Customers { get; } = new();

    public CustomerListViewModel(
        ICustomerService service,
        INavigationService navigation,
        IDialogService dialog,
        ILogger<CustomerListViewModel> logger,
        CustomerMapper mapper)
    {
        _service = service;
        _navigation = navigation;
        _dialog = dialog;
        _logger = logger;
        _mapper = mapper;
        Title = "Quản lý khách hàng";
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsBusy = true;
            var data = await _service.GetAllAsync(SearchText, cancellationToken).ConfigureAwait(true);
            Customers.Clear();
            int stt = 1;
            foreach (var c in data)
            {
                var dto = _mapper.ToDto(c) with { Stt = stt++ };
                Customers.Add(dto);
            }
            _logger.LogInformation("Loaded {Count} customers", Customers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load customers");
            await _dialog.ShowErrorAsync("Lỗi", "Không thể tải danh sách khách hàng.", ex).ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAsync(CancellationToken cancellationToken)
    {
        var result = await _dialog.ShowDialogAsync<CustomerEditViewModel>(new CustomerEditParameter(null)).ConfigureAwait(true);
        if (result == true)
        {
            await LoadAsync(cancellationToken).ConfigureAwait(true);
            await _dialog.ShowMessageAsync("Thông báo", "Thêm khách hàng thành công!").ConfigureAwait(true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync(CancellationToken cancellationToken)
    {
        if (SelectedCustomer is null) return;
        var result = await _dialog.ShowDialogAsync<CustomerEditViewModel>(new CustomerEditParameter(SelectedCustomer.Id)).ConfigureAwait(true);
        if (result == true)
        {
            await LoadAsync(cancellationToken).ConfigureAwait(true);
            await _dialog.ShowMessageAsync("Thông báo", "Cập nhật khách hàng thành công!").ConfigureAwait(true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (SelectedCustomer is null) return;

        var confirmed = await _dialog.ShowConfirmationAsync(
            "Xác nhận xóa",
            $"Bạn có chắc muốn xóa khách hàng '{SelectedCustomer.Name}' (mã {SelectedCustomer.Code})?").ConfigureAwait(true);
        if (!confirmed) return;

        try
        {
            IsBusy = true;
            await _service.DeleteAsync(SelectedCustomer.Id, cancellationToken).ConfigureAwait(true);
            await LoadAsync(cancellationToken).ConfigureAwait(true);
            await _dialog.ShowMessageAsync("Thông báo", "Xóa khách hàng thành công!").ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete customer {Id}", SelectedCustomer.Id);
            await _dialog.ShowErrorAsync("Lỗi", $"Không thể xóa khách hàng {SelectedCustomer.Code}.", ex).ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanEditOrDelete() => SelectedCustomer is not null;

    partial void OnSelectedCustomerChanged(CustomerDto? value)
    {
        EditCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTextChanged(string? value)
    {
        // Debounce 300ms
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        var token = _searchDebounceCts.Token;

        _ = DebounceAndLoadAsync(token);
    }

    private async Task DebounceAndLoadAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token).ConfigureAwait(true);
            if (token.IsCancellationRequested) return;
            await LoadAsync(token).ConfigureAwait(true);
        }
        catch (TaskCanceledException) { /* debounce */ }
    }

    public override async Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
    {
        await LoadAsync(cancellationToken).ConfigureAwait(true);
        await base.OnNavigatedToAsync(parameter, cancellationToken).ConfigureAwait(true);
    }
}

public sealed record CustomerEditParameter(int? CustomerId);
