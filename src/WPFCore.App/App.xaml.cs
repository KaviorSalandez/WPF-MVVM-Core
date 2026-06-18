using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WPFCore.App.Bootstrap;

namespace WPFCore.App;

public partial class App : Application
{
    private IHost? _host;
    private ILogger<App>? _logger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // ─── Global exception handlers (phải đăng ký trước khi làm bất cứ thứ gì) ───
        DispatcherUnhandledException    += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Cấu hình ngôn ngữ hiển thị mặc định toàn hệ thống là vi-VN (định dạng ngày giờ, số...)
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                System.Windows.Markup.XmlLanguage.GetLanguage("vi-VN")));

        base.OnStartup(e);

        // Acquire single-instance lock and forward args if another instance is running
        var guard = new SingleInstanceGuard("WPFCore.SingleInstance", "WPFCore.Ipc.Pipe");
        if (!await guard.TryAcquireOrForwardAsync(e.Args))
        {
            // Another instance is running and our args were forwarded; exit silently
            Shutdown(0);
            return;
        }

        // Build the host (DI, Serilog, configuration)
        _host = await AppHostBuilder.BuildAsync(guard);
        _logger = _host.Services.GetService<ILogger<App>>();

        // ShutdownMode=OnExplicitShutdown: WPF không tự tắt khi window đóng.
        // Ta tắt thủ công khi IHostApplicationLifetime báo stopping
        // (VD: _lifetime.StopApplication() được gọi từ AppStartup).
        _host.Services
             .GetRequiredService<IHostApplicationLifetime>()
             .ApplicationStopping
             .Register(() => Current.Dispatcher.Invoke(Current.Shutdown));

        // Run application startup (migrations, show main window)
        await _host.Services.GetRequiredService<AppStartup>().RunAsync();

    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    // ─── Bắt exception không được xử lý trên UI thread ───────────────────────
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.LogCritical(e.Exception,
            "Unhandled UI-thread exception (DispatcherUnhandledException)");

        // Đánh dấu đã xử lý để WPF không tự đóng app.
        // Hiện MessageBox để developer biết có lỗi.
        e.Handled = true;
        MessageBox.Show(
            $"Có lỗi không mong muốn xảy ra:\n\n{e.Exception.Message}\n\n" +
            $"Chi tiết:\n{e.Exception}",
            "Lỗi ứng dụng",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    // ─── Bắt exception từ background tasks bị bỏ qua (fire-and-forget) ──────
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception,
            "Unobserved task exception (background/fire-and-forget)");
        e.SetObserved(); // Ngăn process crash
    }
}
