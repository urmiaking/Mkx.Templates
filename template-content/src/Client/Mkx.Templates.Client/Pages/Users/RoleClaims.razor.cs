using Mkx.Templates.Client.Extensions;
using Mkx.Templates.Client.Pages.Users.Components;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Claims;
using Mkx.Templates.Shared.DTOs.Roles;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using MudBlazor;

namespace Mkx.Templates.Client.Pages.Users;

public partial class RoleClaims
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت کاربران", href: ClientRoutes.Users.Index, icon: Icons.Material.Filled.People),
        new("دسترسی نقش‌های سیستم", href: ClientRoutes.Users.RoleClaims, icon: Icons.Material.Filled.AdminPanelSettings)
    ];

    private List<RoleClaimsDto> _roles = [];
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

        await LoadRolesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadRolesAsync()
    {
        await SendRequestAsync<IUserManagementService, List<RoleClaimsDto>>(
            (service, ct) => service.GetRolesAsync(ct),
            roles =>
            {
                _roles = roles ?? [];
                StateHasChanged();
            });
    }

    private async Task ManageRoleClaims(string roleName)
    {
        List<PolicyTreeNodeDto>? tree = null;

        await SendRequestAsync<IUserManagementService, List<PolicyTreeNodeDto>>(
            (service, ct) => service.GetRoleClaimsTreeAsync(roleName, ct),
            resTree => tree = resTree);

        StateHasChanged();

        if (tree == null) return;

        var displayRole = roleName.GetRoleName();


        var parameters = new DialogParameters<ClaimsTreeDialog>
        {
            { x => x.InitialTree, tree }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ClaimsTreeDialog>($"مدیریت دسترسی‌های نقش: {displayRole}", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<string> grantedClaims })
        {
            await SendRequestAsync<IUserManagementService>(
                (service, ct) => service.UpdateRoleClaimsAsync(roleName, grantedClaims, ct),
                async () =>
                {
                    AddSuccessToast($"دسترسی‌های نقش '{displayRole}' با موفقیت بروزرسانی شد.");
                    await LoadRolesAsync();
                });
        }
    }
}
