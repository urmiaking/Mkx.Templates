using System.ComponentModel.DataAnnotations;

namespace Mkx.Templates.Shared.DTOs.Users;

public class CreateUserDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(256)]
    public string UserName { get; set; } = string.Empty;
    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }
    [Phone, StringLength(32)]
    public string? PhoneNumber { get; set; }
    [Required, StringLength(1024, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;
    [Required, MaxLength(2)]
    public List<string> Roles { get; set; } = [];
}
