using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Authorization;
using Mkx.Templates.Client.Services;

namespace Mkx.Templates.Client.Layout;

public partial class MainLayout : IAsyncDisposable
{
#if (IncludeSample)
    private static bool IncludesSample => true;
#else
    private static bool IncludesSample => false;
#endif
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<MainLayout> Logger { get; set; } = default!;
    [Inject] private BrowserPushService BrowserPush { get; set; } = default!;
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }
    private bool _mobileMenuOpen, _notificationsOpen;
    private ElementReference _sidebar;
    private IJSObjectReference? _module, _menu;

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += LocationChanged;
    }
    private void LocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _mobileMenuOpen = false;
        _notificationsOpen = false;
        InvokeAsync(StateHasChanged);
    }
    private void OpenMenu() { _notificationsOpen = false; _mobileMenuOpen = true; }
    private void CloseMenu() => _mobileMenuOpen = false;
    private void OpenNotifications() { _mobileMenuOpen = false; _notificationsOpen = true; }
    private void MenuKeyDown(KeyboardEventArgs args) { if (args.Key == "Escape") CloseMenu(); }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/navigation.js");
                _menu = await _module.InvokeAsync<IJSObjectReference>("bind", _sidebar);
            }
            catch (JSException ex)
            {
                Logger.LogWarning(ex, "Navigation enhancements are unavailable; the native navigation remains usable.");
            }
            try
            {
                if (AuthenticationStateTask is not null &&
                    (await AuthenticationStateTask).User.Identity?.IsAuthenticated == true)
                    await BrowserPush.RestoreGrantedAsync();
            }
            catch (Exception ex) when (ex is HttpRequestException or JSException or JSDisconnectedException)
            {
                // Push is optional: registration must never interrupt layout rendering.
            }
        }
        if (_menu is not null)
        {
            try { await _menu.InvokeVoidAsync("update", _mobileMenuOpen); }
            catch (JSException ex) { Logger.LogWarning(ex, "Unable to synchronize mobile navigation focus."); }
        }
    }
    public async ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= LocationChanged;
        try
        {
            if (_menu is not null) { await _menu.InvokeVoidAsync("dispose"); await _menu.DisposeAsync(); }
            if (_module is not null) await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
    }
}
