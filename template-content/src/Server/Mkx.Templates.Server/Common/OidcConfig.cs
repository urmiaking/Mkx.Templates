namespace Mkx.Templates.Server.Common;

public class OidcConfig
{
    public string Scheme { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Authority { get; set; } = default!;
    public string RedirectUrl { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public List<string>? Scopes { get; set; }
    public string? Icon { get; set; }
}