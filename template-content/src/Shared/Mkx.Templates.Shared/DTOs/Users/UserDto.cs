namespace Mkx.Templates.Shared.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = [];
    public int DirectClaimsCount { get; set; }
}
