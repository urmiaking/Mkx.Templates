using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Application.Services.Abstractions;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Application.Seeders;

[ScopedService]
internal class UserSeeder(IAccountService accountService, IConfiguration configuration, ILogger<UserSeeder> logger) : IDbSeeder
{
    public int Order => 20;
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("BootstrapAdmin:Enabled")) return;
        var userName = configuration["BootstrapAdmin:UserName"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("BootstrapAdmin requires a UserName and a Password supplied through secrets.");
        if (await accountService.FindUserAsync(userName) is not null) return;
        var result = await accountService.CreateUserAsync(
            new AppUser(configuration["BootstrapAdmin:DisplayName"] ?? "مدیر سامانه", userName),
            password, [BuiltinRoles.Administrators], cancellationToken);
        if (!result.Succeeded)
            throw new InvalidOperationException("Administrator bootstrap failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        logger.LogInformation("Administrator bootstrap completed. Disable BootstrapAdmin after provisioning.");
    }
}
