using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WPFCore.App.Bootstrap;

/// <summary>
/// Helper tĩnh để wire Serilog với Microsoft.Extensions.Hosting. Đọc cấu hình từ section
/// <c>"Serilog"</c> trong <see cref="IConfiguration"/> (cú pháp Serilog.Settings.Configuration).
/// </summary>
/// <remarks>
/// Gọi qua <c>UseSerilog(SerilogConfigurator.Configure)</c> trong host builder, ví dụ:
/// <code>
/// Host.CreateDefaultBuilder()
///     .UseSerilog(SerilogConfigurator.Configure)
///     .Build();
/// </code>
/// </remarks>
public static class SerilogConfigurator
{
    /// <summary>
    /// Callback được truyền cho <c>UseSerilog(...)</c>. Áp dụng các cài đặt từ section
    /// <c>Serilog</c> trong <see cref="IConfiguration"/> và enrich từ DI container.
    /// </summary>
    /// <param name="context">Host builder context (chứa Configuration và Environment).</param>
    /// <param name="services">Service provider hiện tại của host.</param>
    /// <param name="config">Logger configuration đang được build.</param>
    public static void Configure(HostBuilderContext context, IServiceProvider services, LoggerConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services);
    }
}