using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Mkx.Templates.Sdk.Server.Api;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Server.Pages;
using Mkx.Templates.Shared.Routes;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Mkx.Templates.Shared.DTOs.UserAccounts;

namespace Mkx.Templates.Server.Controllers;

[Route(ApiRoutes.Accounts.Base)]
public class AccountsController(
    SignInManager<AppUser> signInManager, IAntiforgery antiforgery) : ApiControllerBase
{
    [HttpPost(ApiRoutes.Accounts.PerformExternalLogin)]
    public IActionResult PerformExternalLogin([FromForm] string provider,
        [FromForm] string? returnUrl,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<KeyValuePair<string, StringValues>> query = [
            new("ReturnUrl", returnUrl),
            new("Action", ExternalLogin.LoginCallbackAction)];

        var redirectUrl = UriHelper.BuildRelative(
            HttpContext.Request.PathBase,
            ClientRoutes.Accounts.ExternalLogin,
            QueryString.Create(query));

        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, provider);
    }

    [HttpPost(ApiRoutes.Accounts.Logout)]
    [Authorize]
    public async Task<IActionResult> LogoutAsync([FromQuery] string? returnUrl = null)
    {
        await signInManager.SignOutAsync();

        return NoContent();
    }

    [HttpGet(ApiRoutes.Accounts.Antiforgery)]
    [AllowAnonymous]
    public IActionResult GetAntiforgeryToken()
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(new AntiforgeryResponse(antiforgery.GetAndStoreTokens(HttpContext).RequestToken!));
    }

    /// <summary>
    /// Returns the current user's authentication claims for WASM client initialization.
    /// This endpoint serves as a fallback when prerendering is disabled and
    /// PersistentComponentState cannot transfer auth data to the client.
    /// </summary>
    [HttpGet(ApiRoutes.Accounts.AuthState)]
    public IActionResult GetAuthState()
    {
        Response.Headers.CacheControl = "no-store";
        var principal = HttpContext.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            return Ok(new UserInfo(principal.Claims));
        }

        return Ok(new UserInfo { UserClaims = [] });
    }
}
