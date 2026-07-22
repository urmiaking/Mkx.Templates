using Mkx.Templates.Server.Services;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Application.Services.Abstractions;
using Mkx.Templates.Application.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Mkx.Templates.Shared.Routes;
using Microsoft.Extensions.Caching.Memory;

namespace Mkx.Templates.Server.Pages;

public partial class ForgotPassword
{
    private string? _errorMessage;
    private EditContext _editContext = default!;
    private static readonly TimeSpan SmsCooldown = TimeSpan.FromMinutes(2);

    [SupplyParameterFromForm] 
    private InputModel Input { get; set; } = default!;

    [CascadingParameter] 
    private HttpContext HttpContext { get; set; } = default!;

    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private UserManager<AppUser> UserManager { get; set; } = default!;
    [Inject] private ISmsSender SmsSender { get; set; } = default!;
    [Inject] private ILogger<ForgotPassword> Logger { get; set; } = default!;
    [Inject] private IdentityRedirectManager RedirectManager { get; set; } = default!;
    [Inject] private IMemoryCache Cache { get; set; } = default!;

    protected override void OnInitialized()
    {
        Input ??= new InputModel();
        _editContext = new EditContext(Input);

        base.OnInitialized();
    }

    private async Task HandleSubmit()
    {
        _errorMessage = null;

        if (Input.Step == 1)
        {
            if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                _errorMessage = "لطفاً شماره تلفن همراه خود را وارد کنید.";
                return;
            }

            var cacheKey = $"phone:smsCooldown:{Input.PhoneNumber}";
            if (Cache.TryGetValue(cacheKey, out _))
            {
                _errorMessage = "لطفاً برای ارسال مجدد کد تایید ۲ دقیقه صبر کنید.";
                return;
            }

            var user = await AccountService.FindUserByPhoneNumberAsync(Input.PhoneNumber);
            if (user == null)
            {
                _errorMessage = "کاربری با این شماره تلفن یافت نشد.";
                return;
            }

            try
            {
                var code = await UserManager.GenerateTwoFactorTokenAsync(user, "Phone");
                var smsSent = await SmsSender.SendAsync(user.PhoneNumber!, $"کد تایید بازیابی رمز عبور: {code}");
                
                if (!smsSent)
                {
                    _errorMessage = "خطا در ارسال پیامک تایید. لطفا مجدداً تلاش کنید.";
                    return;
                }

                Cache.Set(cacheKey, true, SmsCooldown);
                Logger.LogInformation($"Password reset code generated and sent to {Input.PhoneNumber}");
                Input.Step = 2;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error generating or sending password reset token.");
                _errorMessage = "خطایی در فرآیند بازیابی رمز عبور رخ داد.";
            }
        }
        else if (Input.Step == 2)
        {
            if (string.IsNullOrWhiteSpace(Input.OtpCode))
            {
                _errorMessage = "لطفاً کد تایید را وارد کنید.";
                return;
            }

            var user = await AccountService.FindUserByPhoneNumberAsync(Input.PhoneNumber);
            if (user == null)
            {
                _errorMessage = "خطا در بازیابی اطلاعات کاربر.";
                return;
            }

            var isValid = await UserManager.VerifyTwoFactorTokenAsync(user, "Phone", Input.OtpCode);
            if (!isValid)
            {
                _errorMessage = "کد تایید وارد شده نامعتبر یا منقضی شده است.";
                return;
            }

            Input.Step = 3;
        }
        else if (Input.Step == 3)
        {
            if (string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                _errorMessage = "لطفاً رمز عبور جدید را وارد کنید.";
                return;
            }

            if (Input.NewPassword != Input.ConfirmPassword)
            {
                _errorMessage = "رمز عبور جدید و تکرار آن با هم مطابقت ندارند.";
                return;
            }

            var user = await AccountService.FindUserByPhoneNumberAsync(Input.PhoneNumber);
            if (user == null)
            {
                _errorMessage = "خطا در بازیابی اطلاعات کاربر.";
                return;
            }

            // Verify OTP code again to make sure the request is secure and authorized
            var isValid = await UserManager.VerifyTwoFactorTokenAsync(user, "Phone", Input.OtpCode);
            if (!isValid)
            {
                _errorMessage = "اعتبار سنجی با خطا مواجه شد. لطفا دوباره فرآیند را آغاز کنید.";
                Input.Step = 1;
                return;
            }

            var success = await AccountService.SetPasswordAsync(user, Input.NewPassword);
            if (success)
            {
                Input.Step = 4;
            }
            else
            {
                _errorMessage = "خطا در ذخیره رمز عبور جدید. لطفا مطمئن شوید که رمز عبور جدید پیچیدگی‌های لازم را دارد.";
            }
        }
    }

    private sealed class InputModel
    {
        public int Step { get; set; } = 1;

        [Phone]
        public string PhoneNumber { get; set; } = "";

        public string OtpCode { get; set; } = "";

        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }
}
