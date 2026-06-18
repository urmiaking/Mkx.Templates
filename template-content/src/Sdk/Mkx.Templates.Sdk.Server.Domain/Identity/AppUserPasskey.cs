using Microsoft.AspNetCore.Identity;

namespace Mkx.Templates.Sdk.Server.Domain.Identity;

public class AppUserPasskey : IdentityUserPasskey<Guid>
{
    public virtual AppUser User { get; set; } = default!;
}
