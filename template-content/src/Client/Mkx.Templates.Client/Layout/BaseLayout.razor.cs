using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Services;
using MudBlazor;

namespace Mkx.Templates.Client.Layout;

public partial class BaseLayout : IDisposable
{
    private MudThemeProvider? _mudThemeProvider;
    private bool _disposed;
    public bool IsDarkMode { get; set; }
    [Inject] private ThemeService ThemeService { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    protected override void OnInitialized()
    {
        ThemeService.OnToggleMode += AppearanceChanged;
        ThemeService.OnPaletteChanged += AppearanceChanged;
    }
    private async void AppearanceChanged(object? sender, EventArgs args)
    {
        if (_disposed) return;
        try { await InvokeAsync(async () => { IsDarkMode = ThemeService.IsDarkMode; await ApplyAppearanceAsync(); StateHasChanged(); }); }
        catch (JSDisconnectedException) { }
        catch (Exception ex) { if (!_disposed) await DispatchExceptionAsync(ex); }
    }
    private ValueTask ApplyAppearanceAsync() => JSRuntime.InvokeVoidAsync("Mkx.setAppearance", ThemeService.IsDarkMode);
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _disposed || _mudThemeProvider is null) return;
        await ThemeService.LoadSettingsAsync();
        if (string.IsNullOrEmpty(await LocalStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode)))
        {
            await ThemeService.SetDarkModeAsync(await _mudThemeProvider.GetSystemDarkModeAsync(), false);
            await _mudThemeProvider.WatchSystemDarkModeAsync(async value =>
            {
                if (!_disposed && string.IsNullOrEmpty(await LocalStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode)))
                    await ThemeService.SetDarkModeAsync(value, false);
            });
        }
        IsDarkMode = ThemeService.IsDarkMode;
        await ApplyAppearanceAsync();
        await JSRuntime.InvokeVoidAsync("Mkx.removeSplash");
        StateHasChanged();
    }
    public void Dispose()
    {
        _disposed = true;
        ThemeService.OnToggleMode -= AppearanceChanged;
        ThemeService.OnPaletteChanged -= AppearanceChanged;
    }
}
