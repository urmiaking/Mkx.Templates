namespace Mkx.Templates.Shared.DTOs.Claims;

public class PolicyTreeNodeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public List<PolicyTreeNodeDto> Children { get; set; } = [];
}
