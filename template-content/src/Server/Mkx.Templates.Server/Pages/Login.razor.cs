using Mkx.Templates.Server.Common;
using Mkx.Templates.Server.Services;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Server.Pages;

public partial class Login
{
    private string? _errorMessage;
    private EditContext _editContext = default!;
    private LoginOptions _options = LoginOptions.Default;

    [SupplyParameterFromForm] private InputModel Input { get; set; } = default!;
    [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    [Inject] private SignInManager<AppUser> SignInManager { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IdentityRedirectManager RedirectManager { get; set; } = default!;
    [Inject] private IOptions<LoginOptions> Options { get; set; } = default!;

    protected override void OnInitialized()
    {
        Input ??= new InputModel();
        _editContext = new EditContext(Input);
        _options = Options.Value;

        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    private async Task LoginUser()
    {
        if (!string.IsNullOrEmpty(Input.Passkey?.Error))
        {
            _errorMessage = $"خطا در احراز هویت: {Input.Passkey.Error}";
            return;
        }

        if (!string.IsNullOrEmpty(Input.Passkey?.CredentialJson))
        {
            Logger.LogInformation("Performing passkey sign-in...");
            var passkeySignInResult = await SignInManager.PasskeySignInAsync(Input.Passkey.CredentialJson);

            if (passkeySignInResult.Succeeded)
            {
                RedirectManager.RedirectTo(ReturnUrl);
                return;
            }
            else if (passkeySignInResult.RequiresTwoFactor)
            {
                RedirectManager.RedirectTo(
                    "Account/LoginWith2fa",
                    new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
                return;
            }
            else if (passkeySignInResult.IsLockedOut)
            {
                RedirectManager.RedirectTo(ClientRoutes.Accounts.Lockout);
                return;
            }
            else
            {
                _errorMessage = "خطا در احراز هویت اثر انگشت. لطفا مجدد تلاش کنید";
                return;
            }
        }

        if (_options.AllowLocal)
        {
            if (!_editContext.Validate())
                return;

            var result = await SignInManager.PasswordSignInAsync(
                Input.Username,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: _options.LockoutOnFailure);

            if (result.Succeeded)
            {
                RedirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                RedirectManager.RedirectTo(ClientRoutes.Accounts.Lockout);
            }
            else
            {
                _errorMessage = "خطا: اطلاعات ورودی معتبر نیست";
            }
        }
        else
        {
            _errorMessage = "خطا: ورود با حساب داخلی مجاز نیست";
        }
    }

    private sealed class InputModel
    {
        [Required] public string Username { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
        public PasskeyInputModel? Passkey { get; set; }
    }

    private sealed class PasskeyInputModel
    {
        public string? CredentialJson { get; set; }
        public string? Error { get; set; }
    }
}
