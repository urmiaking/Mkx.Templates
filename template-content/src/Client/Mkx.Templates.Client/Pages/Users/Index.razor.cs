using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Client.Pages.Users.Components;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Claims;
using Mkx.Templates.Shared.DTOs.Users;
using Mkx.Templates.Shared.Routes;
using MudBlazor;

namespace Mkx.Templates.Client.Pages.Users;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت کاربران", href: ClientRoutes.Users.Index, icon: Icons.Material.Filled.People)
    ];

    private MudTable<UserDto>? _table;
    private bool _loadFailed;
    private string _searchQuery = string.Empty;
    private bool _isDisabled;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var enabled = await TryRequestAsync<IUserManagementService, bool>((s, ct) => s.IsUserManagementEnabledAsync(ct));
        _loadFailed = !enabled.Succeeded;
        _isDisabled = enabled.Succeeded && !enabled.Value;
    }
    private async Task<TableData<UserDto>> LoadPageAsync(TableState state, CancellationToken token)
    {
        var result = await TryRequestAsync<IUserManagementService, PagedList<UserDto>>((service, _) => service.GetUsersAsync(new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchQuery), token));
        _loadFailed = !result.Succeeded;
        return new() { Items = result.Value?.Data ?? [], TotalItems = result.Value?.Total ?? 0 };
    }
    private async Task SearchChanged(string value)
    {
        _searchQuery = value;
        if (_table is not null) { _table.NavigateTo(0); await _table.ReloadServerData(); }
    }
    private Task LoadUsersAsync() => _table?.ReloadServerData() ?? Task.CompletedTask;

    private async Task OpenAddUserDialog()
    {
        var model = new UserDialog.UserModel();
        var parameters = new DialogParameters<UserDialog>
        {
            { x => x.IsEdit, false },
            { x => x.Model, model }
        };

        var options = new DialogOptions { BackdropClick = false, CloseOnEscapeKey = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<UserDialog>("افزودن کاربر جدید", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false }) await LoadUsersAsync();
    }

    private async Task OpenEditUserDialog(UserDto user)
    {
        var model = new UserDialog.UserModel
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            SelectedRoles = [.. user.Roles]
        };

        var parameters = new DialogParameters<UserDialog>
        {
            { x => x.IsEdit, true },
            { x => x.Model, model }
        };

        var options = new DialogOptions { BackdropClick = false, CloseOnEscapeKey = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<UserDialog>("ویرایش اطلاعات کاربر", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false }) await LoadUsersAsync();
    }

    private async Task ManageUserClaims(UserDto user)
    {
        List<PolicyTreeNodeDto>? tree = null;

        await SendRequestAsync<IUserManagementService, List<PolicyTreeNodeDto>>(
            (service, ct) => service.GetUserClaimsTreeAsync(user.Id, ct),
            resTree => tree = resTree);

        StateHasChanged();

        if (tree == null) return;

        var parameters = new DialogParameters<ClaimsTreeDialog>
        {
            { x => x.InitialTree, tree }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ClaimsTreeDialog>($"مدیریت دسترسی‌های کاربر: {user.Name}", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<string> grantedClaims })
        {
            await SendRequestAsync<IUserManagementService>(
                (service, ct) => service.UpdateUserClaimsAsync(user.Id, grantedClaims, ct),
                async () =>
                {
                    AddSuccessToast($"دسترسی‌های کاربر '{user.Name}' با موفقیت بروزرسانی شد.");
                    await LoadUsersAsync();
                });
        }
    }

    private async Task ConfirmDeleteUser(UserDto user)
    {
        var confirm = await DialogService.ShowMessageBoxAsync(
            "تایید حذف کاربر",
            $"آیا از حذف کاربر '{user.Name}' (نام کاربری: {user.UserName}) اطمینان دارید؟ این عملیات قابل بازگشت نیست.",
            "بله، حذف شود",
            "انصراف");

        if (confirm == true)
        {
            await SendRequestAsync<IUserManagementService>(
                (service, ct) => service.DeleteUserAsync(user.Id, ct),
                async () =>
                {
                    AddSuccessToast($"کاربر '{user.Name}' با موفقیت حذف شد.");
                    await LoadUsersAsync();
                });
        }
    }
}
