using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Shared.Navigation;

/// <summary>
/// <see cref="INavigationService"/> dùng WPF <see cref="Frame"/>. <c>MainWindow</c> phải gọi
/// <see cref="Attach"/> sau <c>InitializeComponent</c> để gắn frame vật lý.
/// </summary>
public sealed class FrameNavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly Stack<object> _history = new();
    private Frame? _frame;

    public FrameNavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public bool CanNavigateBack => _history.Count > 1 && _frame is not null && _frame.CanGoBack;

    public object? CurrentViewModel => _frame?.DataContext;

    public event EventHandler<NavigationEventArgs>? Navigated;

    public IEnumerable<object> History => _history.Reverse();

    public void ClearHistory()
    {
        _history.Clear();
        // Cần xóa NavigationService journal của Frame nếu muốn tránh user nhấn Alt+Left để quay lại sau khi clear.
        if (_frame != null)
        {
            while (_frame.CanGoBack)
            {
                _frame.RemoveBackEntry();
            }
        }
    }

    /// <summary>Đăng ký Frame từ MainWindow (gọi trong code-behind MainWindow sau InitializeComponent).</summary>
    public void Attach(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _frame = frame;

        // Áp dụng theme trực tiếp cho Frame để lan truyền xuống tất cả các View con được load động
        var themeLoader = _services.GetRequiredService<Themes.FluentLightThemeLoader>();
        themeLoader.ApplyTo(_frame);

        // Lắng nghe sự kiện Navigated để áp dụng theme cho trang con sau khi Visual Tree đã được nạp xong vào Frame
        _frame.Navigated += (sender, args) =>
        {
            if (args.Content is System.Windows.DependencyObject content)
            {
                themeLoader.ApplyTo(content);
            }
        };
    }

    public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
        => NavigateTo(typeof(TViewModel), parameter);

    public void NavigateTo(Type viewModelType, object? parameter = null)
    {
        if (_frame is null)
        {
            throw new InvalidOperationException(
                "Navigation frame chưa được attach. Gọi Attach(frame) từ MainWindow sau InitializeComponent.");
        }

        var vm = _services.GetRequiredService(viewModelType);
        var view = ViewLocator.Locate(viewModelType, _services)
            ?? throw new InvalidOperationException(
                $"ViewLocator không tìm thấy View cho ViewModel '{viewModelType.Name}'.");

        // Frame.Navigate KHÔNG tự động set DataContext của view content.
        // Phải set thủ công để binding (ItemsSource, Command, ...) hoạt động.
        view.DataContext = vm;

        _history.Push(vm);
        _frame.Navigate(view, parameter);

        if (vm is INavigationAware aware)
        {
            // Fire and forget — lỗi của OnNavigatedToAsync sẽ được ghi log qua Serilog ở Bootstrap.
            _ = aware.OnNavigatedToAsync(parameter);
        }

        Navigated?.Invoke(this, new NavigationEventArgs { ViewModel = vm });
    }

    public void NavigateBack()
    {
        if (_frame is null || !_frame.CanGoBack)
        {
            return;
        }

        _frame.GoBack();

        // Sau GoBack: top-of-stack là back-target VM (Frame restored nó làm current view).
        // _history lưu VMs theo thứ tự navigation (top = current). Pop current, peek next = back-target.
        if (_history.Count > 0)
        {
            _history.Pop();
        }

        object? backTarget = _history.Count > 0 ? _history.Peek() : null;
        if (backTarget is INavigationAware aware)
        {
            // Fire OnNavigatedToAsync để back-target VM reload data
            // (vd. CustomerListViewModel sau khi SaveAsync trong CustomerEditViewModel).
            _ = aware.OnNavigatedToAsync(null);
        }
    }
}
