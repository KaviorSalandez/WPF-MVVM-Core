namespace WPFCore.App.Shared.Navigation;

/// <summary>
/// Navigation contract cho Shell MainWindow. ViewLocator sẽ map ViewModel → View.
/// </summary>
public interface INavigationService
{
    /// <summary>Có thể quay lại trang trước hay không.</summary>
    bool CanNavigateBack { get; }

    /// <summary>ViewModel hiện tại đang hiển thị.</summary>
    object? CurrentViewModel { get; }

    /// <summary>Event khi navigation hoàn tất (current view thay đổi).</summary>
    event EventHandler<NavigationEventArgs>? Navigated;

    /// <summary>Navigate tới ViewModel <typeparamref name="TViewModel"/> (đăng ký qua ViewLocator).</summary>
    void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class;

    /// <summary>Navigate tới ViewModel theo type runtime.</summary>
    void NavigateTo(Type viewModelType, object? parameter = null);

    /// <summary>Quay lại trang trước.</summary>
    void NavigateBack();

    /// <summary>Lấy lịch sử điều hướng hiện tại (từ gốc đến trang hiện tại).</summary>
    IEnumerable<object> History { get; }

    /// <summary>Xóa lịch sử điều hướng. Dùng khi chuyển sang một root menu mới.</summary>
    void ClearHistory();
}

/// <summary>Event payload khi <see cref="INavigationService.Navigated"/> được raise.</summary>
public sealed class NavigationEventArgs : EventArgs
{
    public object? ViewModel { get; init; }
}
