using Microsoft.Extensions.Logging;
using Mkx.Templates.Application.Services.Abstractions;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Application.Seeders;

[ScopedService]
internal class UserSeeder(IAccountService accountService, ILogger<UserSeeder> logger) : IDbSeeder
{
    private const string UserName = "admin";

    public int Order => 20;
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var admin = await accountService.FindUserAsync(UserName);

        if (admin is null)
        {
            await accountService.CreateUserAsync(new AppUser("مدیر سامانه", UserName),
                UserName,
                [BuiltinRoles.Administrators],
                cancellationToken);

            logger.LogInformation($"{nameof(UserSeeder)}: Created user '{UserName}' with role '{BuiltinRoles.Administrators}'.");
        }
    }
}