namespace Mkx.Templates.Shared.DTOs.Users;

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
