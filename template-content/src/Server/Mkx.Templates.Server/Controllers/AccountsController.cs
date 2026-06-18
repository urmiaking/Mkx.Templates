using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Mkx.Templates.Sdk.Server.Api;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Server.Pages;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Server.Controllers;

[Route(ApiRoutes.Accounts.Base)] 
public class AccountsController(
    SignInManager<AppUser> signInManager,
    IConfiguration configuration) : ApiControllerBase
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

    [HttpGet(ApiRoutes.Accounts.Logout)]
    public async Task<IActionResult> LogoutAsync([FromQuery] string? returnUrl = null)
    {
        if (BirSignSettings.IsUseBirSign(configuration))
        {
            // 1. Sign out the local application cookie
            await signInManager.SignOutAsync();

            // 2. Trigger SSO logout on BirSign.
            //    The OIDC handler will redirect to BirSign's end-session endpoint,
            //    then back to our PostLogoutRedirectUri, and finally to the provided RedirectUri.
            var redirectUrl = returnUrl ?? Url.Content("~/");
            return SignOut(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                BirSignConstants.AuthenticationType);
        }

        // Standard logout when BirSign is disabled
        await signInManager.SignOutAsync();
        return LocalRedirect($"~/{returnUrl ?? ""}");
    }
}