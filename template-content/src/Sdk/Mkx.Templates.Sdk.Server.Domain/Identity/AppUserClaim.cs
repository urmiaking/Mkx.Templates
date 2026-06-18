using Microsoft.AspNetCore.Identity;

namespace Mkx.Templates.Sdk.Server.Domain.Identity;

public class AppUserClaim : IdentityUserClaim<Guid>
{
    public virtual AppUser User { get; set; } = default!;
}
