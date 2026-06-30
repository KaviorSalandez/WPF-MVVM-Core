using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Shared.Themes;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Shared.Dialogs;

/// <summary>
/// <see cref="IDialogService"/> mặc định dùng WPF <see cref="MessageBox"/>
/// và custom dialog window cho <see cref="ShowDialogAsync{TViewModel}"/>.
/// </summary>
public sealed class SyncfusionDialogService : IDialogService
{
    private readonly FluentLightThemeLoader _themeLoader;
    private readonly IServiceProvider _services;

    public SyncfusionDialogService(FluentLightThemeLoader themeLoader, IServiceProvider services)
    {
        _themeLoader = themeLoader;
        _services = services;
    }

    public Task ShowMessageAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var result = Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task ShowErrorAsync(string title, string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        var detail = exception is null ? string.Empty : $"\n\nChi tiết:\n{exception}";
        Show(message + detail, title, MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task ShowSnackbarAsync(string message, int durationMs = 3000, CancellationToken cancellationToken = default)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastWindow(message, durationMs);
            toast.Show();
        });
        
        return Task.CompletedTask;
    }

    public async Task<bool?> ShowDialogAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        var vm = _services.GetRequiredService<TViewModel>();
        var view = Navigation.ViewLocator.Locate(typeof(TViewModel), _services)
            ?? throw new InvalidOperationException(
                $"ViewLocator không tìm thấy View cho ViewModel '{typeof(TViewModel).Name}'.");

        view.DataContext = vm;

        // Tạo Window wrapper chứa View
        var window = new Window
        {
            Content = view,
            Width = 550,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
        };

        // Đặt tiêu đề dialog từ IDialogAware hoặc ViewModelBase.Title
        if (vm is IDialogAware dialogAware)
        {
            window.Title = dialogAware.DialogTitle ?? "Dialog";

            // Lắng nghe event RequestClose từ ViewModel để tự động đóng dialog
            dialogAware.RequestClose += dialogResult =>
            {
                window.DialogResult = dialogResult;
            };
        }
        else if (vm is ViewModelBase vmBase)
        {
            window.Title = vmBase.Title ?? "Dialog";
        }

        // Áp dụng theme Syncfusion cho dialog window
        _themeLoader.ApplyTo(window);

        // Gọi lifecycle hook OnNavigatedToAsync nếu ViewModel hỗ trợ
        if (vm is INavigationAware navigationAware)
        {
            await navigationAware.OnNavigatedToAsync(parameter, CancellationToken.None).ConfigureAwait(true);
        }

        // ShowDialog() là blocking (modal) — trả về DialogResult khi window đóng
        var result = window.ShowDialog();
        return result;
    }

    private static MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
    {
        var owner = Application.Current?.MainWindow;
        return owner is null
            ? MessageBox.Show(message, title, button, image)
            : MessageBox.Show(owner, message, title, button, image);
    }
}
