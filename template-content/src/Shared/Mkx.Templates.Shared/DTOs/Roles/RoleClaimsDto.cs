namespace Mkx.Templates.Shared.DTOs.Roles;

public class RoleClaimsDto
{
    public string RoleName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int ClaimsCount { get; set; }
    public bool IsBuiltin { get; set; }
}
