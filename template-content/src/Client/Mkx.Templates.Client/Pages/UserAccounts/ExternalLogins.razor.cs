using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Enums;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.UserAccounts;

public partial class ExternalLogins
{
    [SupplyParameterFromQuery(Name = "result")] public string? LinkResult { get; set; }

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: "home"),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: "settings"),
        new("احراز هویت خارجی", href: ClientRoutes.UserAccounts.ExternalLogins, icon: "link")
    ];

    private GetExternalProviderResponse? _externalLogins;
    private bool _buttonDisabled;

    protected override async Task OnInitializedAsync()
    {
        await LoadExternalProvidersAsync();
        await base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        CheckLinkResult();
        base.OnAfterRender(firstRender);
    }

    private void CheckLinkResult()
    {
        if (string.IsNullOrEmpty(LinkResult)) 
            return;

        var success = Enum.TryParse<ExternalLoginResult>(LinkResult, true, out var result);
        LinkResult = null;

        if (!success)
        {
            AddErrorToast("Conversion failed");
            return;
        }

        switch (result)
        {
            case ExternalLoginResult.NoInfo:
                AddErrorToast("خطا در ثبت سرویس");
                break;
            case ExternalLoginResult.Success:
                AddSuccessToast("حساب شما با موفقیت به سرویس احراز هویت متصل شد");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task LoadExternalProvidersAsync()
    {
        _externalLogins = await SendRequestAsync<IUserAccountService, GetExternalProviderResponse>(
            action: (s, ct) => s.GetExternalProvidersAsync(ct));
    }

    private static string GetProviderIcon(string provider)
    {
        return provider.ToLower() switch
        {
            "google" => "globe",
            "microsoft" => "globe",
            "github" => "globe",
            _ => "user"
        };
    }

    private Task StartExternalLogin(string provider)
    {
        Navigation.NavigateTo(ApiUrls.UserAccounts.ExternalLogin(provider), forceLoad: true);
        _buttonDisabled = true;
        return Task.CompletedTask;
    }
}
