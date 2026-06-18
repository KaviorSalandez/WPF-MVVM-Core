namespace WPFCore.App.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Connection string name (under ConnectionStrings section).</summary>
    public string ConnectionStringName { get; set; } = "Default";

    /// <summary>Enable Write-Ahead Logging for better concurrent read performance.</summary>
    public bool EnableWal { get; set; } = true;

    /// <summary>Auto-migrate on startup (apply pending EF migrations).</summary>
    public bool AutoMigrate { get; set; } = true;
}
