namespace WPFCore.App.Modules.Customers.Dtos;

public sealed class UpdateCustomerRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
