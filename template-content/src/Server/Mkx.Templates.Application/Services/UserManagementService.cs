using Mkx.Templates.Sdk.Server.Application.Abstractions;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Application.Services.Abstractions;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Claims;
using Mkx.Templates.Shared.DTOs.Roles;
using Mkx.Templates.Shared.DTOs.Users;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Server.Application.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mkx.Templates.Application.Services;

[ScopedService]
public class UserManagementService(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IAccountService accountService,
    ITransactionContext transactionContext,
    IEnumerable<IApplicationPolicyProvider> policyProviders) : IUserManagementService
{
    public Task<bool> IsUserManagementEnabledAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public async Task<PagedList<UserDto>> GetUsersAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        filter = filter.Normalize();
        var query = userManager.Users.AsNoTracking();
        if (filter.Search is { } search)
            query = query.Where(u => u.Name.Contains(search) || (u.UserName != null && u.UserName.Contains(search)) || (u.Email != null && u.Email.Contains(search)) || (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
        var total = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(u => u.UserName).ThenBy(u => u.Id).Skip(filter.Skip!.Value).Take(filter.Take!.Value)
            .Include(u => u.UserRoles!)
                .ThenInclude(ur => ur.Role!)
            .Include(u => u.Claims!)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedList<UserDto>
        {
            Total = total,
            Skip = filter.Skip.Value,
            Take = filter.Take.Value,
            Data = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = u.UserRoles?.Select(ur => ur.Role.Name).Where(r => !string.IsNullOrEmpty(r)).Cast<string>().ToList() ?? [],
                DirectClaimsCount = u.Claims?.Count ?? 0
            }).ToList()
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var u = await userManager.Users
            .Include(u => u.UserRoles!)
                .ThenInclude(ur => ur.Role!)
            .Include(u => u.Claims!)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (u == null) return null;

        return new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            UserName = u.UserName ?? string.Empty,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Roles = u.UserRoles?.Select(ur => ur.Role.Name).Where(r => !string.IsNullOrEmpty(r)).Cast<string>().ToList() ?? [],
            DirectClaimsCount = u.Claims?.Count ?? 0
        };
    }

    public async Task<bool> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        foreach (var role in dto.Roles) EnsureBuiltinRole(role);
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new BadRequestException("نام الزامی است.");
        var existing = await userManager.FindByNameAsync(dto.UserName);
        if (existing != null)
        {
            throw new BadRequestException($"کاربری با نام کاربری '{dto.UserName}' قبلاً ثبت شده است.");
        }

        var user = new AppUser(dto.Name, dto.UserName, dto.Email, dto.PhoneNumber);
        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"خطا در ایجاد کاربر: {errors}");
        }

        if (dto.Roles.Any())
        {
            foreach (var roleName in dto.Roles)
            {
                await accountService.EnsureRoleAsync(roleName, cancellationToken);
            }
            EnsureSucceeded(await userManager.AddToRolesAsync(user, dto.Roles));
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user == null)
        {
            throw new BadRequestException("کاربر مورد نظر یافت نشد.");
        }

        foreach (var role in dto.Roles) EnsureBuiltinRole(role);
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new BadRequestException("نام الزامی است.");
        if (user.Email != dto.Email) user.EmailConfirmed = false;
        if (user.PhoneNumber != dto.PhoneNumber) user.PhoneNumberConfirmed = false;
        user.SetName(dto.Name);
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new BadRequestException($"خطا در بروزرسانی اطلاعات کاربر: {errors}");
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
            if (!passResult.Succeeded)
            {
                var errors = string.Join(", ", passResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"خطا در تغییر کلمه عبور: {errors}");
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(BuiltinRoles.Administrators) && !dto.Roles.Contains(BuiltinRoles.Administrators))
            throw new BadRequestException("نقش مدیر را از این بخش نمی‌توان حذف کرد.");
        var rolesToRemove = currentRoles.Except(dto.Roles).ToList();
        var rolesToAdd = dto.Roles.Except(currentRoles).ToList();

        if (rolesToRemove.Any())
        {
            EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, rolesToRemove));
        }

        if (rolesToAdd.Any())
        {
            foreach (var roleName in rolesToAdd)
            {
                await accountService.EnsureRoleAsync(roleName, cancellationToken);
            }
            EnsureSucceeded(await userManager.AddToRolesAsync(user, rolesToAdd));
        }

        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user));
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new NotFoundException("کاربر مورد نظر یافت نشد.");
        }

        if (await userManager.IsInRoleAsync(user, BuiltinRoles.Administrators))
        {
            throw new BadRequestException("حذف حساب مدیر از این بخش مجاز نیست.");
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"خطا در حذف کاربر: {errors}");
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<List<PolicyTreeNodeDto>> GetUserClaimsTreeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new BadRequestException("کاربر مورد نظر یافت نشد.");
        }

        var existingClaims = await userManager.GetClaimsAsync(user);
        var grantedClaimTypes = existingClaims.Select(c => c.Type).ToHashSet();

        return BuildPolicyTree(grantedClaimTypes);
    }

    public async Task<bool> UpdateUserClaimsAsync(Guid userId, List<string> grantedClaimNames, CancellationToken cancellationToken = default)
    {
        await using var transaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new BadRequestException("کاربر مورد نظر یافت نشد.");
        }

        var currentClaims = await userManager.GetClaimsAsync(user);
        var targetClaimSet = ValidatePolicyClaims(grantedClaimNames);
        var policyNames = GetPolicyNames();

        foreach (var claim in currentClaims)
        {
            if (policyNames.Contains(claim.Type) && !targetClaimSet.Contains(claim.Type))
            {
                EnsureSucceeded(await userManager.RemoveClaimAsync(user, claim));
            }
        }

        var currentClaimTypes = currentClaims.Select(c => c.Type).ToHashSet();
        foreach (var claimName in targetClaimSet)
        {
            if (!currentClaimTypes.Contains(claimName))
            {
                EnsureSucceeded(await userManager.AddClaimAsync(user, new Claim(claimName, string.Empty)));
            }
        }

        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user));
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<List<RoleClaimsDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = new List<RoleClaimsDto>();

        foreach (var roleName in BuiltinRoles.Roles)
        {
            await accountService.EnsureRoleAsync(roleName, cancellationToken);
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var memberCount = await userManager.Users.CountAsync(user => user.UserRoles.Any(userRole => userRole.Role.Name == roleName), cancellationToken);
                var claims = await roleManager.GetClaimsAsync(role);
                roles.Add(new RoleClaimsDto
                {
                    RoleName = roleName,
                    MemberCount = memberCount,
                    ClaimsCount = claims.Count,
                    IsBuiltin = true
                });
            }
        }

        return roles;
    }

    public async Task<List<PolicyTreeNodeDto>> GetRoleClaimsTreeAsync(string roleName, CancellationToken cancellationToken = default)
    {
        EnsureBuiltinRole(roleName);
        await accountService.EnsureRoleAsync(roleName, cancellationToken);
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            throw new BadRequestException($"نقش '{roleName}' یافت نشد.");
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var grantedClaimTypes = existingClaims.Select(c => c.Type).ToHashSet();

        return BuildPolicyTree(grantedClaimTypes);
    }

    public async Task<bool> UpdateRoleClaimsAsync(string roleName, List<string> grantedClaimNames, CancellationToken cancellationToken = default)
    {
        await using var transaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        EnsureBuiltinRole(roleName);
        await accountService.EnsureRoleAsync(roleName, cancellationToken);
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            throw new BadRequestException($"نقش '{roleName}' یافت نشد.");
        }

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var targetClaimSet = ValidatePolicyClaims(grantedClaimNames);
        var policyNames = GetPolicyNames();

        foreach (var claim in currentClaims)
        {
            if (policyNames.Contains(claim.Type) && !targetClaimSet.Contains(claim.Type))
            {
                EnsureSucceeded(await roleManager.RemoveClaimAsync(role, claim));
            }
        }

        var currentClaimTypes = currentClaims.Select(c => c.Type).ToHashSet();
        foreach (var claimName in targetClaimSet)
        {
            if (!currentClaimTypes.Contains(claimName))
            {
                EnsureSucceeded(await roleManager.AddClaimAsync(role, new Claim(claimName, string.Empty)));
            }
        }

        foreach (var member in await userManager.GetUsersInRoleAsync(roleName))
            EnsureSucceeded(await userManager.UpdateSecurityStampAsync(member));
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private List<PolicyTreeNodeDto> BuildPolicyTree(HashSet<string> grantedClaimTypes)
    {
        var rootList = new List<PolicyTreeNodeDto>();

        foreach (var provider in policyProviders)
        {
            var categoryNode = new PolicyTreeNodeDto
            {
                Name = $"Category_{provider.Category}",
                Description = provider.Category,
                IsGranted = false,
                Children = []
            };

            foreach (var policyDef in provider.GetPolicies())
            {
                categoryNode.Children.Add(MapPolicyDefinitionToNode(policyDef, grantedClaimTypes));
            }

            categoryNode.IsGranted = categoryNode.Children.Any() && categoryNode.Children.All(c => c.IsGranted);

            rootList.Add(categoryNode);
        }

        return rootList;
    }

    private static PolicyTreeNodeDto MapPolicyDefinitionToNode(PolicyDefinition def, HashSet<string> grantedClaimTypes)
    {
        var node = new PolicyTreeNodeDto
        {
            Name = def.Name,
            Description = def.Description,
            IsGranted = grantedClaimTypes.Contains(def.Name),
            Children = []
        };

        if (def.ChildPolicies != null)
        {
            foreach (var childDef in def.ChildPolicies)
            {
                node.Children.Add(MapPolicyDefinitionToNode(childDef, grantedClaimTypes));
            }
        }

        return node;
    }

    private HashSet<string> GetPolicyNames() => policyProviders
        .SelectMany(provider => provider.GetPolicies())
        .SelectMany(definition => definition.Definitions)
        .Select(definition => definition.Name)
        .ToHashSet(StringComparer.Ordinal);

    private HashSet<string> ValidatePolicyClaims(List<string> names)
    {
        var requested = (names ?? []).ToHashSet(StringComparer.Ordinal);
        if (!requested.IsSubsetOf(GetPolicyNames()))
            throw new BadRequestException("دسترسی نامعتبر است.");
        return requested;
    }

    private static void EnsureBuiltinRole(string roleName)
    {
        if (!BuiltinRoles.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            throw new BadRequestException("نقش نامعتبر است.");
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(error => error.Description)));
    }
}
