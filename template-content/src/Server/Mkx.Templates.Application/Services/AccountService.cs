using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Application.Services.Abstractions;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Attributes;
using System.Security.Claims;

namespace Mkx.Templates.Application.Services;

[ScopedService]
public class AccountService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<AccountService> logger) : IAccountService
{
    public async Task<IdentityResult> CreateUserAsync(AppUser user, string? password = null, string[]? roles = null, CancellationToken cancellationToken = default)
    {
        var result = await userManager.CreateAsync(user, password ?? GenerateRandomPassword(7));

        if (result.Succeeded)
        {
            if (roles != null)
            {
                foreach (var roleName in roles)
                {
                    var role = await roleManager.FindByNameAsync(roleName);

                    if (role == null)
                    {
                        role = new AppRole(roleName);
                        await roleManager.CreateAsync(role);
                    }

                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }

        return result;
    }

    public async Task<bool> EnsureRoleAsync(string username, string roleName, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return false;

        await EnsureRolesAsync(user, [roleName], cancellationToken);

        return true;
    }

    public async Task EnsureRolesAsync(AppUser user, string[] roles, CancellationToken cancellationToken)
    {
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                role = new AppRole(roleName);
                await roleManager.CreateAsync(role);
            }

            await userManager.AddToRoleAsync(user, roleName);
        }
    }

    public Task<AppUser?> FindUserAsync(string username)
        => userManager.FindByNameAsync(username);

    public Task<AppUser?> GetAsync(Guid id, CancellationToken cancellationToken)
        => userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public string GenerateRandomPassword(int length)
    {
        var rnd = new Random((int)DateTime.Now.Ticks);

        var min = 1;
        var max = 9;

        for (var i = 0; i < length - 1; i++)
        {
            min *= 10;
            max += min * 9;
        }

        return rnd.Next(min, max).ToString();
    }

    public async Task<bool> SetPasswordAsync(AppUser user, string password, CancellationToken cancellationToken = default)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, password);

        return result.Succeeded;
    }

    public async Task<bool> SetPasswordAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return false;

        return await SetPasswordAsync(user, password, cancellationToken);
    }


    public async Task EnsureUserWithBirSignSsoAsync(
        ClaimsIdentity identity,
        CancellationToken cancellationToken = default)
    {
        // If there's no role don't create user
        var roleClaims = identity.FindAll("role").Select(c => c.Value).ToList();

        if (!roleClaims.Any())
            return;

        // ── 1. Extract user identity from claims ─────────────────
        var username = identity.FindFirst("preferred_username")?.Value
                    ?? identity.Name;
        if (string.IsNullOrEmpty(username))
        {
            logger.LogError("BirSign SSO: missing username / preferred_username claim.");
            return;
        }

        var firstName = identity.FindFirst("MPH_name")?.Value;
        var lastName = identity.FindFirst("MPH_family")?.Value;
        var displayName = $"{firstName} {lastName}".Trim();
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = username;

        var email = identity.FindFirst("MPH_email")?.Value
                 ?? identity.FindFirst(ClaimTypes.Email)?.Value;
        var phone = identity.FindFirst("MPH_phonenumber")?.Value;
        if (string.IsNullOrWhiteSpace(phone))
            phone = null;

        // ── 2. Find or create local user ─────────────────────────
        var user = await FindUserAsync(username);
        if (user == null)
        {
            user = new AppUser(displayName, username, email, phone);
            var result = await CreateUserAsync(user, cancellationToken: cancellationToken);
            if (!result.Succeeded)
            {
                logger.LogError(
                    "BirSign SSO: failed to create user {Username}. Errors: {Errors}",
                    username,
                    string.Join(", ", result.Errors));
                return;
            }
        }
        else
        {
            // Update personal info if changed
            var changed = false;
            if (user.Name != displayName) { user.SetName(displayName); changed = true; }
            if (user.Email != email) { user.Email = email; changed = true; }
            if (user.PhoneNumber != phone) { user.PhoneNumber = phone; changed = true; }
            if (changed)
                await userManager.UpdateAsync(user);
        }

        // ── 3. Clear existing roles and claims (fresh sync) ──────
        var existingRoles = await userManager.GetRolesAsync(user);
        if (existingRoles.Any())
            await userManager.RemoveFromRolesAsync(user, existingRoles);

        var existingClaims = await userManager.GetClaimsAsync(user);
        foreach (var claim in existingClaims)
            await userManager.RemoveClaimAsync(user, claim);

        // ── 4. Repopulate roles and claims from `role` claims ────
        foreach (var role in roleClaims)
        {
            switch (role)
            {
                case BuiltinRoles.Administrators:
                    await EnsureRoleAsync(BuiltinRoles.Administrators, cancellationToken);
                    await userManager.AddToRoleAsync(user, BuiltinRoles.Administrators);
                    break;
                case BuiltinRoles.Users:
                    await EnsureRoleAsync(BuiltinRoles.Users, cancellationToken);
                    await userManager.AddToRoleAsync(user, BuiltinRoles.Users);
                    break;
                default:
                    {
                        if (role.StartsWith(nameof(Mkx.Templates)))
                        {
                            await userManager.AddClaimAsync(user, new Claim(role, ""));
                        }
                        else
                        {
                            await EnsureRoleAsync(role, cancellationToken);
                            await userManager.AddToRoleAsync(user, role);
                        }

                        break;
                    }
            }
        }
    }

    public async Task EnsureRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            role = new AppRole(roleName);
            await roleManager.CreateAsync(role);

            logger.LogInformation($"{nameof(AccountService)}: Created role '{role}'.");
        }
    }

    public async Task<bool> AddRoleClaimAsync(string roleName, Claim claim, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var hasClaim = currentClaims.Any(x => x.Type == claim.Type && x.Value == claim.Value);

        if (!hasClaim)
        {
            var result = await roleManager.AddClaimAsync(role, claim);
            if (result.Succeeded)
            {
                logger.LogInformation($"{nameof(AccountService)}: Create claim '{claim.Type}' for role '{roleName}'");
                return result.Succeeded;
            }
        }

        return false;
    }

    public async Task<bool> RemoveRoleClaimAsync(string roleName, Claim claim, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var targetClaim = currentClaims.FirstOrDefault(x => x.Type == claim.Type && x.Value == claim.Value);

        if (targetClaim != null)
        {
            var result = await roleManager.RemoveClaimAsync(role, targetClaim);
            return result.Succeeded;
        }

        return false;
    }

    public async Task<IList<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return await userManager.GetRolesAsync(user);
    }

    public async Task<bool> RemoveRoleAsync(string username, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var result = await userManager.RemoveFromRoleAsync(user, roleName);

        return result.Succeeded;
    }

    public async Task<IList<Claim>> GetUserClaimsAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return await userManager.GetClaimsAsync(user);
    }

    public async Task<bool> AddUserClaimAsync(string username, Claim claim, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var currentClaims = await userManager.GetClaimsAsync(user);
        var hasClaim = currentClaims.Any(x => x.Type == claim.Type && x.Value == claim.Value);

        if (!hasClaim)
        {
            var result = await userManager.AddClaimAsync(user, claim);
            return result.Succeeded;
        }

        return false;
    }

    public async Task<bool> RemoveUserClaimAsync(string username, Claim claim, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var currentClaims = await userManager.GetClaimsAsync(user);
        var targetClaim = currentClaims.FirstOrDefault(x => x.Type == claim.Type && x.Value == claim.Value);

        if (targetClaim != null)
        {
            var result = await userManager.RemoveClaimAsync(user, targetClaim);
            return result.Succeeded;
        }

        return false;
    }

    public async Task<IEnumerable<AppRole>> GetRolesAsync(CancellationToken cancellationToken = default)
        => await roleManager.Roles.ToListAsync(cancellationToken);

    public async Task<bool> RenameRoleAsync(string roleName, string newName, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            return false;

        role.Name = newName;
        await roleManager.UpdateAsync(role);

        return true;
    }

    public async Task<bool> DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            return false;

        var result = await roleManager.DeleteAsync(role);

        return result.Succeeded;
    }

    public async Task<IEnumerable<AppUser>> GetRoleMembersAsync(string roleName, CancellationToken cancellationToken = default)
        => await userManager.Users
            .Where(x => x.UserRoles
                .Any(r => r.Role.Name == roleName))
            .ToListAsync(cancellationToken);

    public async Task<IList<Claim>> GetRoleClaimsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        return await roleManager.GetClaimsAsync(role);
    }

    public async Task SetMobileAsync(AppUser user, string mobile, CancellationToken cancellationToken = default)
    {
        await userManager.SetPhoneNumberAsync(user, mobile);
    }

    public async Task ConfirmPhoneNumberAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.PhoneNumber))
            return;

        var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
        await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, token);
    }
}