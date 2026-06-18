namespace WPFCore.App.Configuration;

public sealed class AppOptions
{
    public const string SectionName = "App";

    public string Name { get; set; } = "WPFCore";
    public string Version { get; set; } = "1.0.0";
    public string Environment { get; set; } = "Production";
    public SeedDataOptions SeedData { get; set; } = new();
    public PathOptions Paths { get; set; } = new();
}

public sealed class SeedDataOptions
{
    public bool Enabled { get; set; }
    public int CustomerCount { get; set; } = 10;
}

public sealed class PathOptions
{
    /// <summary>SQLite database file path. Relative to %LOCALAPPDATA% if not absolute.</summary>
    public string Database { get; set; } = "WPFCore/wpfcore.db";

    /// <summary>Log directory. Relative to %LOCALAPPDATA% if not absolute.</summary>
    public string Logs { get; set; } = "WPFCore/logs";
}
