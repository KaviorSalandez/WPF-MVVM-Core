using CommunityToolkit.Mvvm.ComponentModel;

namespace WPFCore.App.Shared.ViewModels;

/// <summary>
/// Base class cho ViewModel: cung cấp <see cref="IsBusy"/>, <see cref="Title"/>
/// và lifecycle hooks cho navigation.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _title;

    public virtual Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public virtual Task OnNavigatedFromAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

/// <summary>
/// Hook lifecycle cho ViewModel khi được navigate tới / rời đi bởi <c>INavigationService</c>.
/// </summary>
public interface INavigationAware
{
    Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default);

    Task OnNavigatedFromAsync(CancellationToken cancellationToken = default);
}
