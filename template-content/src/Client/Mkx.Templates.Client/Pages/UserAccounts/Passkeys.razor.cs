using Blazored.LocalStorage;

using Mkx.Templates.Client.Services;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Mkx.Templates.Client.Pages.UserAccounts;

public partial class Passkeys
{
    [Inject] public WebAuthnService WebAuthnService { get; set; } = default!;
    [Inject] public ILocalStorageService LocalStorage { get; set; } = default!;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.ManageAccounts),
        new("مدیریت کلیدهای عبور", href: ClientRoutes.UserAccounts.Passkeys, icon: Icons.Material.Filled.Fingerprint)
    ];

    private List<GetPasskeyResponse>? _passkeys;

    protected override async Task OnInitializedAsync()
    {
        await LoadPasskeysAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPasskeysAsync()
    {
        _passkeys = await SendRequestAsync<IUserAccountService, List<GetPasskeyResponse>>(
            action: (s, ct) => s.GetPasskeysAsync(ct));
    }

    private async Task AddNewPasskey()
    {
        var jsonOptions =
            await SendRequestAsync<IUserAccountService, string>(action: (s, ct) =>
                s.GetPasskeyCreationOptionsAsync(ct));

        if (string.IsNullOrEmpty(jsonOptions))
        {
            AddErrorToast("Failed to retrieve passkey creation options");
            return;
        }

        var credentialJson = await WebAuthnService.CreatePasskeyAsync(jsonOptions);

        if (string.IsNullOrEmpty(credentialJson))
        {
            AddErrorToast("عملیات لغو شد");
            return;
        }

        await SendRequestAsync<IUserAccountService>(action: (s, ct) =>
            s.AddPasskeyAsync(new CreatePasskeyRequest(credentialJson), ct),
            afterSend: async () =>
            {
                AddSuccessToast("کلید عبور با موفقیت افزوده شد");
                await LocalStorage.SetItemAsync("DeviceRegisteredPasskey", true);
                await LoadPasskeysAsync();
            });
    }

    private async Task RemovePasskey(GetPasskeyResponse passkey)
    {
        var result = await DialogService.ShowMessageBoxAsync("حذف کلید عبور",
            $"با حذف کلید عبور دستگاه {passkey.Name} دیگر نمی توانید بدون در دست داشتن رمز عبور وارد حساب کاربری خود شوید. آیا مطمئن هستید؟",
            "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(action: (s, ct) =>
                s.RemovePasskeyAsync(passkey.CredentialId, ct),
                afterSend: async () =>
                {
                    AddSuccessToast($"کلید عبور {passkey.Name} با موفقیت حذف شد");
                    await LocalStorage.SetItemAsync("DeviceRegisteredPasskey", false);
                    await LoadPasskeysAsync();
                });
        }
    }
}