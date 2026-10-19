using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
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
    [Inject] private ThemeService Theme { get; set; } = default!;
    private bool _mobileMenuOpen, _notificationsOpen, _themeChanging;
    private ElementReference _sidebar, _themeButton;
    private IJSObjectReference? _module, _menu;
    private static string Today => DateTime.Now.ToString("dddd، yyyy/MM/dd", CultureInfo.GetCultureInfo("fa-IR"));

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += LocationChanged;
        Theme.OnToggleMode += AppearanceChanged;
    }
    private void AppearanceChanged(object? sender, EventArgs args) => InvokeAsync(StateHasChanged);
    private void LocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _mobileMenuOpen = false;
        _notificationsOpen = false;
        InvokeAsync(StateHasChanged);
    }
    private void OpenMenu() { _notificationsOpen = false; _mobileMenuOpen = true; }
    private void CloseMenu() => _mobileMenuOpen = false;
    private void OpenNotifications() { _mobileMenuOpen = false; _notificationsOpen = true; }
    private async Task ToggleThemeAsync()
    {
        if (_themeChanging) return;
        _themeChanging = true;
        try { await Theme.ToggleModeAsync(_themeButton); }
        finally { _themeChanging = false; }
    }
    private void MenuKeyDown(KeyboardEventArgs args) { if (args.Key == "Escape") CloseMenu(); }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/navigation.js");
            _menu = await _module.InvokeAsync<IJSObjectReference>("bind", _sidebar);
        }
        if (_menu is not null) await _menu.InvokeVoidAsync("update", _mobileMenuOpen);
    }
    public async ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= LocationChanged;
        Theme.OnToggleMode -= AppearanceChanged;
        try
        {
            if (_menu is not null) { await _menu.InvokeVoidAsync("dispose"); await _menu.DisposeAsync(); }
            if (_module is not null) await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
    }
}
