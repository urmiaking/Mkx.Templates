using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mkx.Templates.Sdk.Server.Api;
using Mkx.Templates.Sdk.Server.Application.Abstractions;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Shared.Abstractions;

namespace Mkx.Templates.Server.Controllers;

[Route(ApiRoutes.UserAccounts.Base)]
[Authorize]
public class UserAccountsController(IUserAccountService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.UserAccounts.GetCurrentUser)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userInfo = await service.GetCurrentUserInfoAsync(cancellationToken);
        return Ok(userInfo);
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdateFullName)]
    public async Task<IActionResult> UpdateFullNameAsync([FromBody] UpdateUserFullNameRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserFullNameAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdatePhoneNumber)]
    public async Task<IActionResult> UpdatePhoneNumberAsync([FromBody] UpdateUserPhoneNumberRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserPhoneNumberAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost(ApiRoutes.UserAccounts.SendVerificationToken)]
    public async Task<IActionResult> SendVerificationTokenAsync(SendVerificationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.SendVerificationTokenAsync(request, cancellationToken);
        return Ok();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdateEmail)]
    public async Task<IActionResult> UpdateEmailAsync([FromBody] UpdateUserEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserEmailAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdatePassword)]
    public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdateUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpGet(ApiRoutes.UserAccounts.Get2FaStatus)]
    public async Task<IActionResult> Get2FaStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = await service.GetUser2FaStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost(ApiRoutes.UserAccounts.ForgetDevice)]
    public async Task<IActionResult> ForgetDeviceAsync(CancellationToken cancellationToken = default)
    {
        await service.ForgetDeviceAsync(cancellationToken);
        return Ok();
    }

    [HttpPost(ApiRoutes.UserAccounts.Disable2Fa)]
    public async Task<IActionResult> Disable2FaAsync(CancellationToken cancellationToken = default)
    {
        await service.Disable2FaAsync(cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.UserAccounts.GetAuthenticatorKey)]
    public async Task<IActionResult> GetAuthenticatorKeyAsync(CancellationToken cancellationToken = default)
    {
        var key = await service.GetAuthenticatorKeyAsync(cancellationToken);
        return Ok(key);
    }

    [HttpPost(ApiRoutes.UserAccounts.Enable2Fa)]
    public async Task<IActionResult> Enable2FaAsync(EnableTwoFactorAuthRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.Enable2FaAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.UserAccounts.GetExternalProviders)]
    public async Task<IActionResult> GetExternalProvidersAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetExternalProvidersAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.UserAccounts.GetPasskeys)]
    public async Task<IActionResult> GetPasskeysAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetPasskeysAsync(cancellationToken);
        return Ok(list);
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpGet(ApiRoutes.UserAccounts.GetPasskeyRequestOptions)]
    public async Task<IActionResult> GetPasskeyRequestOptionsAsync([FromQuery] string? username, CancellationToken cancellationToken = default)
    {
        var result = await service.GetPasskeyRequestOptionsAsync(username, cancellationToken);
        return Ok(result);
    }

    [IgnoreAntiforgeryToken]
    [HttpGet(ApiRoutes.UserAccounts.GetPasskeyCreationOptions)]
    public async Task<IActionResult> GetPasskeyCreationOptionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetPasskeyCreationOptionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.UserAccounts.AddPasskey)]
    public async Task<IActionResult> AddPasskeyAsync(CreatePasskeyRequest request, CancellationToken cancellationToken = default)
    {
        await service.AddPasskeyAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpDelete(ApiRoutes.UserAccounts.RemovePasskey)]
    public async Task<IActionResult> RemovePasskeyAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        await service.RemovePasskeyAsync(credentialId, cancellationToken);
        return NoContent();
    }

    [HttpGet(ApiRoutes.UserAccounts.GetAccountsList)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> GetAccountsListAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetAccountsListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPut(ApiRoutes.UserAccounts.LockUser)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> LockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.LockUserAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.UserAccounts.UnlockUser)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> UnlockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.UnlockUserAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost(ApiRoutes.UserAccounts.CreateAccount)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> CreateAccountAsync([FromBody] UserAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.CreateAccountAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdateAccount)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> UpdateAccountAsync(Guid id, [FromBody] UserAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await service.UpdateAccountAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.UserAccounts.DeleteAccount)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await service.DeleteAccountAsync(id, cancellationToken);
        return NoContent();
    }
}
