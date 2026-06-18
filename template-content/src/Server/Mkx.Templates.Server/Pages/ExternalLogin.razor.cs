using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Server.Services;
using Mkx.Templates.Shared.Routes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mkx.Templates.Server.Pages;

public partial class ExternalLogin
{
    public const string LoginCallbackAction = "LoginCallback";

    private string? _message;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? RemoteError { get; set; }

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    private string? Action { get; set; }

    [Inject] private SignInManager<AppUser> SignInManager { get; set; } = default!;
    [Inject] private UserManager<AppUser> UserManager { get; set; } = default!;
    [Inject] private RoleManager<AppRole> RoleManager { get; set; } = default!;
    [Inject] private IdentityRedirectManager RedirectManager { get; set; } = default!;
    [Inject] private ILogger<ExternalLogin> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (RemoteError is not null)
        {
            _message = $"خطا: {RemoteError}";
            return;
        }

        var info = await SignInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            _message = "خطا: مشکلی در بارگیری اطلاعات پیش آمد";
            return;
        }

        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            if (Action == LoginCallbackAction)
            {
                await OnLoginCallbackAsync(info);
                return;
            }

            RedirectManager.RedirectTo(ClientRoutes.Accounts.Login);
        }
    }

    private async Task OnLoginCallbackAsync(ExternalLoginInfo externalLoginInfo)
    {
        // Sign in the user with this external login provider if the user already has a login.
        var signinResult = await SigninAsync(externalLoginInfo);
        if (signinResult.Succeeded)
        {
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else if (signinResult.IsLockedOut)
        {
            RedirectManager.RedirectTo(ClientRoutes.Accounts.Lockout);
        }
        else
        {
            // If the user does not have an account, then create a new account...
            var createResult = await CreateAccountAsync(externalLoginInfo);
            if (createResult.Succeeded)
            {
                signinResult = await SigninAsync(externalLoginInfo);
                if (signinResult.Succeeded)
                {
                    RedirectManager.RedirectTo(ReturnUrl);
                }
                else if (signinResult.IsLockedOut)
                {
                    RedirectManager.RedirectTo(ClientRoutes.Accounts.Lockout);
                }
            }
        }
    }

    private async Task<IdentityResult> CreateAccountAsync(ExternalLoginInfo externalLoginInfo)
    {
        IdentityResult? result;
        var principal = externalLoginInfo.Principal;

        var username = GetUsername(principal);

        if (string.IsNullOrEmpty(username))
        {
            Logger.LogError(
                "{LoginProvider} provider did not provide a valid information for username.",
                externalLoginInfo.LoginProvider);

            _message = "خطا: ورود با مشکل مواجه شد لطفا دوباره امتحان کنید";
            return IdentityResult.Failed();
        }

        var currentUser = await UserManager.FindByNameAsync(username);

        if (currentUser is null)
        {
            var givenName = principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value;
            var familyName = principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;

            if (string.IsNullOrEmpty(givenName) || string.IsNullOrEmpty(familyName))
            {
                Logger.LogError(
                    "{LoginProvider} provider did not provide valid givenName and familyName claims.",
                    externalLoginInfo.LoginProvider);

                _message = "خطا: ورود با مشکل مواجه شد لطفا دوباره امتحان کنید";
                return IdentityResult.Failed();
            }

            currentUser = new AppUser($"{givenName} {familyName}", username);

            result = await UserManager.CreateAsync(currentUser);

            if (!result.Succeeded)
            {
                _message = $"خطا: {string.Join(",", result.Errors.Select(error => error.Description))}";
                return result;
            }
        }

        result = await UserManager.AddLoginAsync(currentUser, externalLoginInfo);

        return result;
    }

    private static string? GetUsername(ClaimsPrincipal principal)
    {
        var preferredUsername = principal.FindFirst(JwtRegisteredClaimNames.PreferredUsername)?.Value;
        if (!string.IsNullOrEmpty(preferredUsername))
            return preferredUsername;

        var phoneNumber = principal.FindFirst(JwtRegisteredClaimNames.PhoneNumber)?.Value;
        var phoneNumberVerified = principal.FindFirst(JwtRegisteredClaimNames.PhoneNumberVerified)?.Value;
        if (!string.IsNullOrEmpty(phoneNumber) && bool.TryParse(phoneNumberVerified, out var phoneVerified) && phoneVerified)
            return phoneNumber;

        var emailAddress = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var emailAddressVerified = principal.FindFirst(JwtRegisteredClaimNames.EmailVerified)?.Value;

        if (!string.IsNullOrEmpty(emailAddress) && bool.TryParse(emailAddressVerified, out var emailVerified) && emailVerified)
            return emailAddress;

        return null;
    }

    private async Task<SignInResult> SigninAsync(ExternalLoginInfo externalLoginInfo)
    {
        return await SignInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider,
                                                            externalLoginInfo.ProviderKey,
                                                            isPersistent: false,
                                                            bypassTwoFactor: true);
    }
}
