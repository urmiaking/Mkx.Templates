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

    private List<UserDto> _users = [];
    private string _searchQuery = string.Empty;
    private bool _isDisabled;

    protected override async Task OnInitializedAsync()
    {
        var enabled = await SendRequestAsync<IUserManagementService, bool>(
            (s, ct) => s.IsUserManagementEnabledAsync(ct));

        if (!enabled)
        {
            _isDisabled = true;
            return;
        }

        await LoadUsersAsync();
        await base.OnInitializedAsync();
    }

    private IEnumerable<UserDto> FilteredUsers
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
                return _users;

            var q = _searchQuery.Trim();
            return _users.Where(u =>
                u.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.UserName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (u.Email != null && u.Email.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }
    }

    private async Task LoadUsersAsync()
    {
        await SendRequestAsync<IUserManagementService, List<UserDto>>(
            (service, ct) => service.GetUsersAsync(ct),
            users =>
            {
                _users = users ?? [];
                StateHasChanged();
            });
    }

    private async Task OpenAddUserDialog()
    {
        var model = new UserDialog.UserModel();
        var parameters = new DialogParameters<UserDialog>
        {
            { x => x.IsEdit, false },
            { x => x.Model, model }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<UserDialog>("افزودن کاربر جدید", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data is UserDialog.UserModel resModel)
        {
            var createDto = new CreateUserDto
            {
                Name = resModel.Name,
                UserName = resModel.UserName,
                Email = resModel.Email,
                PhoneNumber = resModel.PhoneNumber,
                Password = resModel.Password ?? string.Empty,
                Roles = resModel.SelectedRoles
            };

            await SendRequestAsync<IUserManagementService>(
                (service, ct) => service.CreateUserAsync(createDto, ct),
                async () =>
                {
                    AddSuccessToast($"کاربر '{resModel.Name}' با موفقیت ایجاد شد.");
                    await LoadUsersAsync();
                });
        }
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

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<UserDialog>("ویرایش اطلاعات کاربر", parameters, options);
        var result = await dialog.Result;

        if (result != null && !result.Canceled && result.Data is UserDialog.UserModel resModel)
        {
            var updateDto = new UpdateUserDto
            {
                Id = resModel.Id,
                Name = resModel.Name,
                Email = resModel.Email,
                PhoneNumber = resModel.PhoneNumber,
                NewPassword = resModel.Password,
                Roles = resModel.SelectedRoles
            };

            await SendRequestAsync<IUserManagementService>(
                (service, ct) => service.UpdateUserAsync(updateDto, ct),
                async () =>
                {
                    AddSuccessToast($"اطلاعات کاربر '{resModel.Name}' با موفقیت بروزرسانی شد.");
                    await LoadUsersAsync();
                });
        }
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
