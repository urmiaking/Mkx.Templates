namespace Mkx.Templates.Server.Common;

public class OidcDescriptor
{
    public string Scheme { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Icon { get; init; }
}