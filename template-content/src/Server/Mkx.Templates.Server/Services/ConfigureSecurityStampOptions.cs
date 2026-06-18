using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mkx.Templates.Server.Services;

public class ConfigureSecurityStampOptions
    : IConfigureOptions<SecurityStampValidatorOptions>
{
    public void Configure(SecurityStampValidatorOptions options)
    {
        options.ValidationInterval = TimeSpan.FromMinutes(10); // Default interval is 30 minutes

        options.OnRefreshingPrincipal = refreshingPrincipal =>
        {
            var newIdentity = refreshingPrincipal.NewPrincipal?.Identities.First();
            var currentIdentity = refreshingPrincipal.CurrentPrincipal?.Identities.First();

            if (currentIdentity is not null && newIdentity is not null)
            {
                var currentClaimsNotInNewIdentity = currentIdentity.Claims.Where(c => !newIdentity.HasClaim(c.Type, c.Value));

                foreach (var claim in currentClaimsNotInNewIdentity)
                {
                    newIdentity.AddClaim(claim);
                }
            }

            return Task.CompletedTask;
        };
    }
}
