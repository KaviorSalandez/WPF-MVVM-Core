namespace WPFCore.App.Modules.Customers.Dtos;

public sealed record CustomerDto
{
    public int Id { get; init; }
    public int Stt { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
