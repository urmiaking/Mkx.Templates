using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mkx.Templates.Client.Services;
namespace Mkx.Templates.Client.Layout;
public partial class BaseLayout : IAsyncDisposable
{
    [Inject] private ThemeService ThemeService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<BaseLayout> Logger { get; set; } = default!;
    private bool _disposed;
    private DotNetObjectReference<BaseLayout>? _receiver;
    protected override void OnInitialized()
    {
        ThemeService.OnToggleMode += AppearanceChanged;
    }
    private async void AppearanceChanged(object? sender, EventArgs args)
    {
        if (_disposed) return;
        try { await InvokeAsync(StateHasChanged); }
        catch (JSDisconnectedException) { }
        catch (Exception ex) { if (!_disposed) await DispatchExceptionAsync(ex); }
    }
    private async ValueTask ApplyAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("Mkx.setMobilePickerHidden", ThemeService.MobilePickerHidden);
            await JS.InvokeVoidAsync("Mkx.setPalette", ThemeService.Palette, false);
            await JS.InvokeVoidAsync("Mkx.setAppearance", ThemeService.IsDarkMode);
        }
        catch (JSException ex)
        {
            Logger.LogWarning(ex, "Unable to initialize browser appearance; saved preferences are retained.");
        }
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        var systemDark = false;
        try { systemDark = await JS.InvokeAsync<bool>("Mkx.systemDark"); }
        catch (JSException ex) { Logger.LogWarning(ex, "Unable to read system appearance preference."); }
        await ThemeService.LoadSettingsAsync(systemDark);

        _receiver = DotNetObjectReference.Create(this);
        try { await JS.InvokeVoidAsync("Mkx.watchSystemAppearance", _receiver); }
        catch (JSException ex)
        {
            Logger.LogWarning(ex, "Unable to watch system appearance changes.");
            _receiver.Dispose();
            _receiver = null;
        }

        await ApplyAsync();
        // Do not leave the splash screen visible if one of the optional JS initializers fails.
        try { await JS.InvokeVoidAsync("Mkx.removeSplash"); }
        catch (JSException ex) { Logger.LogWarning(ex, "Unable to remove the startup splash screen."); }
    }
    [JSInvokable]
    public Task SystemAppearanceChanged(bool dark) => _disposed ? Task.CompletedTask : ThemeService.SetDarkModeAsync(dark, false);
    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        ThemeService.OnToggleMode -= AppearanceChanged;
        if (_receiver is not null)
        {
            try { await JS.InvokeVoidAsync("Mkx.unwatchSystemAppearance"); }
            catch (JSDisconnectedException) { }
            _receiver.Dispose();
        }
    }
}
