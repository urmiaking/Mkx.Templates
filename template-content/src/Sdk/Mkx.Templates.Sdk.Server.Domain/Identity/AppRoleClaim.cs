using Microsoft.AspNetCore.Identity;

namespace Mkx.Templates.Sdk.Server.Domain.Identity;

public class AppRoleClaim : IdentityRoleClaim<Guid>
{
    public virtual AppRole Role { get; set; } = default!;
}
