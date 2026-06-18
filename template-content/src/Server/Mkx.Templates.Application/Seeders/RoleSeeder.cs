using Microsoft.AspNetCore.Authorization.Infrastructure;
using Mkx.Templates.Application.Services.Abstractions;
using Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Attributes;
using System.Security.Claims;

namespace Mkx.Templates.Application.Seeders;

[ScopedService]
internal class RoleSeeder(IAccountService accountService,
    IEnumerable<IApplicationPolicyProvider> policyProviders) : IDbSeeder
{
    public int Order => 10;
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in BuiltinRoles.Roles)
            await accountService.EnsureRoleAsync(item, cancellationToken);

        await PopulateAdministratorRoleClaimsAsync(policyProviders, accountService);
    }

    private static async Task PopulateAdministratorRoleClaimsAsync(IEnumerable<IApplicationPolicyProvider> policyProviders, IAccountService accountService)
    {
        var policies = policyProviders
            .SelectMany(x => x.GetPolicies())
            .SelectMany(x => x.Policies);

        foreach (var policy in policies)
        {
            var claimRequirements = policy.Requirements.OfType<ClaimsAuthorizationRequirement>();

            foreach (var claimRequirement in claimRequirements)
                if (claimRequirement.AllowedValues is null)
                    await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators,
                        new Claim(claimRequirement.ClaimType, string.Empty));
                else
                    foreach (var requiredValue in claimRequirement.AllowedValues)
                        await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators, new Claim(claimRequirement.ClaimType, requiredValue));
        }
    }
}