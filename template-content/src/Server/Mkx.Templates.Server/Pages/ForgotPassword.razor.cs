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
using Mkx.Templates.Shared.Utilities;
using Mkx.Templates.Sdk.Shared.Extensions;

namespace Mkx.Templates.Server.Pages;

public partial class ForgotPassword
{
    private string? _errorMessage;
    private string? _infoMessage;
    private int _resendSeconds;
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
        if (Input.Step == 2) _resendSeconds = GetResendSeconds();

        base.OnInitialized();
    }

    private async Task HandleSubmit()
    {
        _errorMessage = null;
        _infoMessage = null;
        if (Input.Step == 2 && Input.Action == "resend")
        {
            if (GetResendSeconds() > 0) { _errorMessage = "برای ارسال مجدد، شمارش معکوس را به پایان برسانید."; return; }
            Input.Step = 1;
            Input.OtpCode = "";
        }
        Input.PhoneNumber = (Input.PhoneNumber ?? "").Trim().ToEnglishNumericsChars().Replace(" ", "");

        if (Input.Step == 1)
        {
            if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                _errorMessage = "لطفاً شماره تلفن همراه خود را وارد کنید.";
                return;
            }

            var cacheKey = $"phone:smsCooldown:{Input.PhoneNumber}";
            if (GetResendSeconds() > 0)
            {
                _errorMessage = "برای ارسال مجدد، شمارش معکوس را به پایان برسانید.";
                Input.Step = 2;
                _resendSeconds = GetResendSeconds();
                return;
            }

            var user = await AccountService.FindUserByPhoneNumberAsync(Input.PhoneNumber);
            if (user == null || !user.PhoneNumberConfirmed || await UserManager.IsLockedOutAsync(user))
            {
                _errorMessage = "بازیابی برای این حساب در حال حاضر امکان‌پذیر نیست.";
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

                Cache.Set(cacheKey, DateTimeOffset.UtcNow.Add(SmsCooldown), SmsCooldown);
                Logger.LogInformation("Password recovery code accepted by SMS provider.");
                Input.Step = 2;
                _resendSeconds = GetResendSeconds();
                if (Input.Action == "resend") _infoMessage = "درخواست ارسال مجدد کد ثبت شد.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error generating or sending password reset token.");
                _errorMessage = "خطایی در فرآیند بازیابی رمز عبور رخ داد.";
            }
        }
        else if (Input.Step == 2)
        {
            Input.OtpCode = OtpCode.Normalize(Input.OtpCode);
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

            var isValid = await VerifyRecoveryCodeAsync(user);
            if (!isValid)
            {
                _errorMessage = "کد تایید وارد شده نامعتبر یا منقضی شده است.";
                Input.OtpCode = "";
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
            var isValid = await VerifyRecoveryCodeAsync(user);
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

    private async Task<bool> VerifyRecoveryCodeAsync(AppUser user)
    {
        if (!user.PhoneNumberConfirmed || await UserManager.IsLockedOutAsync(user)) return false;
        if (await UserManager.VerifyTwoFactorTokenAsync(user, "Phone", Input.OtpCode)) return true;
        var failed = await UserManager.AccessFailedAsync(user);
        if (!failed.Succeeded) Logger.LogWarning("Could not record failed password recovery verification.");
        return false;
    }

    private int GetResendSeconds() => Cache.TryGetValue<DateTimeOffset>($"phone:smsCooldown:{Input.PhoneNumber}", out var until)
        ? Math.Max(0, (int)Math.Ceiling((until - DateTimeOffset.UtcNow).TotalSeconds)) : 0;
    private static string MaskPhone(string phone) => phone.Length == 11 ? phone[..4] + "•••" + phone[^4..] : phone;

    private sealed class InputModel
    {
        public int Step { get; set; } = 1;
        public string Action { get; set; } = "";

        [Phone]
        public string PhoneNumber { get; set; } = "";

        public string OtpCode { get; set; } = "";

        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }
}
