using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Syncfusion.Licensing;
using WPFCore.App.Configuration;

namespace WPFCore.App.Bootstrap;

/// <summary>
/// Đăng ký Syncfusion license runtime. License key được đọc từ
/// <see cref="IOptions{TOptions}"/> của <see cref="SyncfusionOptions"/>, fallback sang
/// biến môi trường <c>SYNCFUSION_LICENSE_KEY</c>.
/// </summary>
/// <remarks>
/// Singleton — gọi <see cref="Register"/> một lần trong <c>AppHostBuilder</c> sau khi host
/// start (để <see cref="ILogger{TCategoryName}"/> hoạt động). Nếu key trống app sẽ chạy ở
/// trial mode (có banner watermark) — chỉ warning, không throw.
/// </remarks>
public sealed class SyncfusionLicenseRegistrar
{
    /// <summary>Tên biến môi trường fallback khi license key không có trong config.</summary>
    public const string EnvironmentVariableName = "SYNCFUSION_LICENSE_KEY";

    private readonly SyncfusionOptions _options;
    private readonly ILogger<SyncfusionLicenseRegistrar> _logger;

    /// <summary>
    /// Khởi tạo registrar với options và logger.
    /// </summary>
    public SyncfusionLicenseRegistrar(
        IOptions<SyncfusionOptions> options,
        ILogger<SyncfusionLicenseRegistrar> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký license với Syncfusion. Idempotent — gọi nhiều lần an toàn.
    /// </summary>
    public void Register()
    {
        var key = ResolveLicenseKey();

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning(
                "Syncfusion license key chưa được cấu hình. App sẽ chạy ở trial mode với banner nước. " +
                "Thêm Syncfusion:LicenseKey vào appsettings.Development.json hoặc set env {EnvVar}. " +
                "Xem docs/SETUP.md.",
                EnvironmentVariableName);
            return;
        }

        try
        {
            SyncfusionLicenseProvider.RegisterLicense(key);
            _logger.LogInformation(
                "Syncfusion license registered (Theme={Theme})",
                _options.Theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Syncfusion license registration failed. App sẽ chạy ở trial mode.");
        }
    }

    private string? ResolveLicenseKey()
    {
        // Ưu tiên config, fallback env var (hữu ích cho CI/CD, container)
        if (!string.IsNullOrWhiteSpace(_options.LicenseKey))
        {
            return _options.LicenseKey;
        }

        return Environment.GetEnvironmentVariable(EnvironmentVariableName);
    }
}