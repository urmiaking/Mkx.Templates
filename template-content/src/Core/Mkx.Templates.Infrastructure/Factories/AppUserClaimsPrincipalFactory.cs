using Mkx.Templates.Sdk.Server.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Mkx.Templates.Infrastructure.Factories;

public class AppUserClaimsPrincipalFactory(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<AppUser, AppRole>(userManager, roleManager, optionsAccessor)
{
    public override async Task<ClaimsPrincipal> CreateAsync(AppUser user)
    {
        var principal = await base.CreateAsync(user);

        if (!principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
        {
            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Name));
            }
        }

        return principal;
    }
}
