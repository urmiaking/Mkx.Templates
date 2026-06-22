using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Services;
using Mkx.Templates.Client.Components;
using Mkx.Templates.Shared.Routes;
using System.Security.Claims;
using Mkx.Templates.Sdk.Shared.Extensions;

namespace Mkx.Templates.Client.Layout.Components;

public partial class Drawer : AppComponentBase
{
    [Parameter] public bool IsDrawerOpen { get; set; }
    [Parameter] public EventCallback<bool> IsDrawerOpenChanged { get; set; }
    [Inject] public ThemeService ThemeService { get; set; } = default!;

    protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    protected string DisplayName => User?.FindFirst(ClaimTypes.GivenName)?.Value ?? User?.Identity?.Name ?? "کاربر مهمان";
    protected string UserName => User?.FindFirst(ClaimTypes.Name)?.Value ?? "برای دسترسی کامل وارد شوید";
    protected string AvatarText => !string.IsNullOrEmpty(User?.FindFirst(ClaimTypes.GivenName)?.Value ?? User?.Identity?.Name) ? (User?.FindFirst(ClaimTypes.GivenName)?.Value ?? User?.Identity?.Name)![0].ToString().ToUpper() : "م";

    protected override void OnInitialized()
    {
        ThemeService.OnToggleMode += OnThemeToggled;
        ThemeService.OnPaletteChanged += OnPaletteChanged;
    }

    private void OnThemeToggled(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnPaletteChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void ToggleTheme()
    {
        ThemeService.ToggleMode();
    }

    private async Task SetPalette(string paletteName)
    {
        await ThemeService.SetPaletteAsync(paletteName);
    }

    private async Task CloseDrawer()
    {
        IsDrawerOpen = false;
        await IsDrawerOpenChanged.InvokeAsync(false);
    }

    private void Login()
    {
        var returnUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        var loginUrl = ClientRoutes.Accounts.Login.AppendQueryString(new { returnUrl });
        Navigation.NavigateTo(loginUrl, forceLoad: true);
    }

    private void Logout()
    {
        // Redirect to Login page after logout to prevent landing on an authorized page
        Navigation.NavigateTo(ApiUrls.Accounts.Logout(ClientRoutes.Accounts.Login), forceLoad: true);
    }

    public override async ValueTask DisposeAsync()
    {
        ThemeService.OnToggleMode -= OnThemeToggled;
        ThemeService.OnPaletteChanged -= OnPaletteChanged;
        await base.DisposeAsync();
    }
}