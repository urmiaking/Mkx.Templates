using Mkx.Templates.Client.Pages.UserAccounts.Components;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Client.Components.UI;
using Net.Codecrete.QrCodeGenerator;
using System.Globalization;
using Mkx.Templates.Shared.Utilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Mkx.Templates.Client.Pages.UserAccounts;

public partial class EnableAuthenticator
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}?secret={1}&issuer={0}&digits=6";

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: "home"),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: "settings"),
        new("رمز دو مرحله ای", href: ClientRoutes.UserAccounts.TwoFactorAuthentication, icon: "key"),
        new("فعالسازی", href: ClientRoutes.UserAccounts.EnableAuthenticator, icon: "check"),
    ];

    private GetAuthenticatorKeyResponse? _authenticatorKey;
    private string? _svgGraphicsPath;
    private string? _code;
    private string? _formattedKey;
    private int _qrSize = 40;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadSharedKeyAndQrCodeUriAsync();
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync()
    { 
        var result = await TryRequestAsync<IUserAccountService, GetAuthenticatorKeyResponse>(
            (s, ct) => s.GetAuthenticatorKeyAsync(ct));
        if (!result.Succeeded || result.Value is null) return;
        _authenticatorKey = result.Value;
        _formattedKey = FormatKey(_authenticatorKey.Key);
        var authenticatorUri = GenerateQrCodeUri(_authenticatorKey.Key);
        var qr = QrCode.EncodeText(authenticatorUri, QrCode.Ecc.Medium);
        _svgGraphicsPath = qr.ToGraphicsPath();
        _qrSize = qr.Size;
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string unformattedKey)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            UrlEncoder.Default.Encode("Mkx.Templates"),
            unformattedKey);
    }

    private async Task OnEnable()
    {
        _code = OtpCode.Normalize(_code);
        if (IsBusy || _authenticatorKey is null || _code.Length != 6)
            return;

        await SendRequestAsync<IUserAccountService, EnableTwoFactorAuthResponse>(
            action: (s, ct) => s.Enable2FaAsync(new EnableTwoFactorAuthRequest(_code), ct),
            afterSend: async response =>
            {
                var parameters = new DialogParameters<RecoveryCodeViewer>
                {
                    { x => x.RecoveryCodes, response.RecoveryCodes }
                };

                var dialog = await DialogService.ShowAsync<RecoveryCodeViewer>("کدهای بازیابی",
                    parameters,
                    new DialogOptions
                    {
                        CloseButton = false,
                        MaxWidth = MaxWidth.Small,
                        FullWidth = true,
                        BackdropClick = false
                    });

                await dialog.Result;

                Navigation.NavigateTo(ClientRoutes.UserAccounts.TwoFactorAuthentication);
            });
    }
}
