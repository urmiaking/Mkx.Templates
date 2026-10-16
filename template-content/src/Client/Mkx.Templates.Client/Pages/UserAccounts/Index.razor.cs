using Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.UserAccounts;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: "home"),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: "settings")
    ];

    private async Task ChangePassword()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small };

        var dialog = await DialogService.ShowAsync<PasswordEditor>("تغییر رمز عبور", dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("رمز عبور با موفقیت ویرایش شد.");

        }
    }
}
