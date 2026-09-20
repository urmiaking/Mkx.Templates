using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.Authorization;
using Mkx.Templates.Shared.DTOs.Users;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Sdk.Server.Api;

namespace Mkx.Templates.Server.Controllers;

[Route(ApiRoutes.UserManagement.Base)]
[Authorize]
public class UserManagementController(IUserManagementService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.UserManagement.Enabled)]
    public async Task<IActionResult> IsUserManagementEnabledAsync(CancellationToken cancellationToken = default)
    {
        var enabled = await service.IsUserManagementEnabledAsync(cancellationToken);
        return Ok(enabled);
    }

    [HttpGet(ApiRoutes.UserManagement.GetUsers)]
    [Authorize(Policy = AppPolicies.Users.View)]
    public async Task<IActionResult> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await service.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet(ApiRoutes.UserManagement.GetUserById)]
    [Authorize(Policy = AppPolicies.Users.View)]
    public async Task<IActionResult> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await service.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost(ApiRoutes.UserManagement.CreateUser)]
    [Authorize(Policy = AppPolicies.Users.Manage)]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        await service.CreateUserAsync(dto, cancellationToken);
        return Ok(true);
    }

    [HttpPut(ApiRoutes.UserManagement.UpdateUser)]
    [Authorize(Policy = AppPolicies.Users.Manage)]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        await service.UpdateUserAsync(dto, cancellationToken);
        return Ok(true);
    }

    [HttpDelete(ApiRoutes.UserManagement.DeleteUser)]
    [Authorize(Policy = AppPolicies.Users.Manage)]
    public async Task<IActionResult> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteUserAsync(id, cancellationToken);
        return Ok(true);
    }

    [HttpGet(ApiRoutes.UserManagement.GetUserClaimsTree)]
    [Authorize(Policy = AppPolicies.Users.View)]
    public async Task<IActionResult> GetUserClaimsTreeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tree = await service.GetUserClaimsTreeAsync(userId, cancellationToken);
        return Ok(tree);
    }

    [HttpPut(ApiRoutes.UserManagement.UpdateUserClaims)]
    [Authorize(Policy = AppPolicies.Users.ManageClaims)]
    public async Task<IActionResult> UpdateUserClaimsAsync(Guid userId, [FromBody] List<string> grantedClaimNames, CancellationToken cancellationToken = default)
    {
        await service.UpdateUserClaimsAsync(userId, grantedClaimNames, cancellationToken);
        return Ok(true);
    }

    [HttpGet(ApiRoutes.UserManagement.GetRoles)]
    [Authorize(Policy = AppPolicies.Users.View)]
    public async Task<IActionResult> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await service.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet(ApiRoutes.UserManagement.GetRoleClaimsTree)]
    [Authorize(Policy = AppPolicies.Users.View)]
    public async Task<IActionResult> GetRoleClaimsTreeAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var tree = await service.GetRoleClaimsTreeAsync(roleName, cancellationToken);
        return Ok(tree);
    }

    [HttpPut(ApiRoutes.UserManagement.UpdateRoleClaims)]
    [Authorize(Policy = AppPolicies.Users.ManageClaims)]
    public async Task<IActionResult> UpdateRoleClaimsAsync(string roleName, [FromBody] List<string> grantedClaimNames, CancellationToken cancellationToken = default)
    {
        await service.UpdateRoleClaimsAsync(roleName, grantedClaimNames, cancellationToken);
        return Ok(true);
    }
}
