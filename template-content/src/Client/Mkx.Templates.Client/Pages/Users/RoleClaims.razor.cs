using Mkx.Templates.Client.Extensions;
using Mkx.Templates.Client.Pages.Users.Components;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Claims;
using Mkx.Templates.Shared.DTOs.Roles;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.Users;

public partial class RoleClaims
{
    private List<RoleClaimsDto> _roles = [];
    private bool _isDisabled, _loadFailed, _loading = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _loadFailed = false;
        var enabled = await TryRequestAsync<IUserManagementService, bool>((service, token) => service.IsUserManagementEnabledAsync(token));
        if (!enabled.Succeeded) { _loading = false; _loadFailed = true; return; }
        _isDisabled = !enabled.Value;
        if (_isDisabled) { _loading = false; return; }
        var roles = await TryRequestAsync<IUserManagementService, List<RoleClaimsDto>>((service, token) => service.GetRolesAsync(token));
        if (roles.Succeeded) _roles = roles.Value ?? [];
        _loadFailed = !roles.Succeeded;
        _loading = false;
    }

    private async Task ManageRoleClaims(string roleName)
    {
        if (IsBusy) return;
        var tree = await TryRequestAsync<IUserManagementService, List<PolicyTreeNodeDto>>((service, token) => service.GetRoleClaimsTreeAsync(roleName, token));
        if (!tree.Succeeded) return;
        var displayRole = roleName.GetRoleName();
        var parameters = new DialogParameters<ClaimsTreeDialog>
        {
            { component => component.InitialTree, tree.Value ?? [] },
            { component => component.SaveChanges, (Func<IUserManagementService, List<string>, CancellationToken, Task>)((service, names, token) => service.UpdateRoleClaimsAsync(roleName, names, token)) }
        };
        var dialog = await DialogService.ShowAsync<ClaimsTreeDialog>($"مدیریت دسترسی‌های نقش: {displayRole}", parameters,
            new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true });
        if (await dialog.Result is { Canceled: false })
        {
            AddSuccessToast($"دسترسی‌های نقش «{displayRole}» به‌روز شد.");
            await LoadAsync();
        }
    }
}
