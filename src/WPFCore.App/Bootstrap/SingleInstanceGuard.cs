using System.IO;
using System.IO.Pipes;
using System.Text;

namespace WPFCore.App.Bootstrap;

/// <summary>
/// Quản lý single-instance qua <see cref="Mutex"/> và forward <c>args</c> tới instance đang chạy
/// qua <see cref="NamedPipeServerStream"/>. Implement <see cref="IDisposable"/> để giải phóng tài nguyên
/// khi ứng dụng thoát.
/// </summary>
/// <remarks>
/// Luồng hoạt động:
/// <list type="number">
///   <item>Instance đầu tiên acquire mutex và start named pipe server.</item>
///   <item>Instance phụ thấy mutex đã bị chiếm → kết nối tới pipe và gửi args.</item>
///   <item>Instance chính nhận args qua event <see cref="ArgumentsReceived"/>.</item>
/// </list>
/// </remarks>
public sealed class SingleInstanceGuard : IDisposable
{
    private readonly string _mutexName;
    private readonly string _pipeName;
    private Mutex? _mutex;
    private CancellationTokenSource? _serverCts;
    private Task? _serverTask;
    private bool _disposed;

    /// <summary>
    /// Khởi tạo guard với tên mutex và named pipe.
    /// </summary>
    /// <param name="mutexName">Tên mutex toàn cục (ví dụ <c>"WPFCore.SingleInstance"</c>).</param>
    /// <param name="pipeName">Tên named pipe (ví dụ <c>"WPFCore.Ipc.Pipe"</c>).</param>
    public SingleInstanceGuard(string mutexName, string pipeName)
    {
        if (string.IsNullOrWhiteSpace(mutexName))
            throw new ArgumentException("Mutex name cannot be null or empty.", nameof(mutexName));
        if (string.IsNullOrWhiteSpace(pipeName))
            throw new ArgumentException("Pipe name cannot be null or empty.", nameof(pipeName));

        _mutexName = mutexName;
        _pipeName = pipeName;
    }

    /// <summary><c>true</c> khi process hiện tại là instance chính (đã acquire mutex).</summary>
    public bool IsPrimaryInstance { get; private set; }

    /// <summary>
    /// Event raised trên instance chính khi một instance phụ forward args tới.
    /// Payload là mảng args đã được tách bằng ký tự <c>'|'</c>.
    /// </summary>
    public event EventHandler<string[]>? ArgumentsReceived;

    /// <summary>
    /// Thử acquire single-instance lock. Nếu là instance phụ sẽ forward args tới instance chính
    /// qua named pipe rồi trả về <c>false</c>. Nếu thành công trả về <c>true</c> và bắt đầu lắng nghe pipe.
    /// </summary>
    /// <param name="args">Command-line arguments cần forward tới instance chính (có thể null).</param>
    public async Task<bool> TryAcquireOrForwardAsync(string[]? args)
    {
        _mutex = new Mutex(initiallyOwned: true, name: _mutexName, out var createdNew);
        if (!createdNew)
        {
            // Another instance owns the mutex — forward args and exit
            try
            {
                await ForwardArgsAsync(args ?? Array.Empty<string>()).ConfigureAwait(false);
            }
            catch
            {
                // Forward failed (pipe server chưa sẵn sàng, network issue...) nhưng vẫn không acquire được
                // → trả về false để caller thoát. Không retry để tránh block UI.
            }
            return false;
        }

        IsPrimaryInstance = true;
        _serverCts = new CancellationTokenSource();
        _serverTask = Task.Run(() => RunServerAsync(_serverCts.Token));
        return true;
    }

    private async Task ForwardArgsAsync(string[] args)
    {
        using var client = new NamedPipeClientStream(
            serverName: ".",
            pipeName: _pipeName,
            direction: PipeDirection.Out);

        await client.ConnectAsync(timeout: 2000).ConfigureAwait(false);

        var payload = Encoding.UTF8.GetBytes(string.Join('|', args));
        await client.WriteAsync(payload).ConfigureAwait(false);
        await client.FlushAsync().ConfigureAwait(false);
    }

    private async Task RunServerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                using var ms = new MemoryStream();
                await server.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                var raw = Encoding.UTF8.GetString(ms.ToArray());

                var forwardedArgs = string.IsNullOrWhiteSpace(raw)
                    ? Array.Empty<string>()
                    : raw.Split('|', StringSplitOptions.RemoveEmptyEntries);

                ArgumentsReceived?.Invoke(this, forwardedArgs);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown path
                break;
            }
            catch
            {
                // Server-side error — log ở caller qua ILogger nếu cần, hiện tại retry nhẹ
                try
                {
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _serverCts?.Cancel();
            _serverTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Server task đã bị cancel hoặc dispose — bỏ qua
        }

        _serverCts?.Dispose();

        if (_mutex is not null)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch
            {
                // Mutex không thuộc thread hiện tại hoặc đã dispose — bỏ qua
            }
            _mutex.Dispose();
            _mutex = null;
        }
    }
}