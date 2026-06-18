namespace WPFCore.App.Configuration;

public sealed class SyncfusionOptions
{
    public const string SectionName = "Syncfusion";

    /// <summary>Syncfusion license key. Read from appsettings or SYNCFUSION_LICENSE_KEY env.</summary>
    public string? LicenseKey { get; set; }

    /// <summary>Theme name to apply at startup. Default: FluentLight.</summary>
    public string Theme { get; set; } = "FluentLight";
}
