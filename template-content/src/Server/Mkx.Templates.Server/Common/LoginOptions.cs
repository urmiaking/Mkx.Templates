namespace Mkx.Templates.Server.Common;

public record LoginOptions
{
    public static LoginOptions Default => new();

    public bool AllowLocal { get; init; } = true;
    public bool AllowExternal { get; init; } = true;
    public bool ShowRememberMe { get; init; } = true;
    public bool LockoutOnFailure { get; init; } = false;
}
