namespace Mkx.Templates.Shared.DTOs.Users;

public class UpdateUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public List<string> Roles { get; set; } = [];
}
