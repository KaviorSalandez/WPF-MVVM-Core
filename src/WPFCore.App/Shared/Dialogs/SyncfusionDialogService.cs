using System.Windows;
using WPFCore.App.Shared.Themes;

namespace WPFCore.App.Shared.Dialogs;

/// <summary>
/// <see cref="IDialogService"/> mặc định dùng WPF <see cref="MessageBox"/>.
/// Theme loader được inject để chuẩn bị cho việc swap sang custom Syncfusion dialog host sau này.
/// </summary>
public sealed class SyncfusionDialogService : IDialogService
{
    private readonly FluentLightThemeLoader _themeLoader;

    public SyncfusionDialogService(FluentLightThemeLoader themeLoader)
    {
        _themeLoader = themeLoader;
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

    private static MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
    {
        var owner = Application.Current?.MainWindow;
        return owner is null
            ? MessageBox.Show(message, title, button, image)
            : MessageBox.Show(owner, message, title, button, image);
    }
}
