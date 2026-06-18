using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using MapIdeaHub.BirSign.SharedKernel.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Application.Seeders;

[ScopedService]
internal sealed class BirSignRoleSeeder(
    IdsService idsService,
    IConfiguration configuration,
    IEnumerable<IApplicationPolicyProvider> policyProviders,
    ILogger<BirSignRoleSeeder> logger) : IDbSeeder
{
    public int Order => 21;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!BirSignSettings.IsUseBirSign(configuration))
                return;

            var roles = new List<RoleInfo>
            {
                // 1. Built‑in roles
                new()
                {
                    SourcePrimaryKey = BuiltinRoles.Administrators,
                    Name = BuiltinRoles.Administrators,
                    IsPublicForAll = false,
                    IsPublicForOrganUsers = false,
                    Description = "مدیر سامانه"
                },
                new()
                {
                    SourcePrimaryKey = BuiltinRoles.Users,
                    Name = BuiltinRoles.Users,
                    IsPublicForAll = false,
                    IsPublicForOrganUsers = false,
                    Description = "کاربر سیستم"
                }
            };

            var allPolicies = policyProviders
                .SelectMany(pp => pp.GetPolicies())
                .SelectMany(p => p.Definitions)
                .DistinctBy(d => d.Name)
                .ToList();

            foreach (var policy in allPolicies)
            {
                var roleName = policy.Name;
                roles.Add(new RoleInfo
                {
                    SourcePrimaryKey = roleName,
                    Name = roleName,
                    IsPublicForAll = false,
                    IsPublicForOrganUsers = false,
                    Description = policy.Description
                });
            }

#if !DEBUG
            // 3.Send to BirSign
            var result = await idsService.SendRolesAsync(new RoleRequest { Roles = roles });

            if (result.IsSuccess)
                logger.LogInformation("BirSign roles synchronized successfully.");
            else
                logger.LogError("Failed to sync BirSign roles: {Errors}", result.Error);       
#endif
        }
        catch (Exception e)
        {
            logger.LogError("An exception occurred when syncing BirSign roles: {ex}", e.Message);
        }
    }
}