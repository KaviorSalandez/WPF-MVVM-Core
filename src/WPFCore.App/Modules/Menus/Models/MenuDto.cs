namespace WPFCore.App.Modules.Menus.Models;

public sealed record MenuDto
{
    public int Id { get; init; }
    
    public int? ParentId { get; init; }
    
    public string Title { get; init; } = string.Empty;
    
    public string? ActionKey { get; init; }
    
    public int SortOrder { get; init; }
    
    public string? Glyph { get; init; }
    
    public bool IsEnabled { get; init; } = true;
}
