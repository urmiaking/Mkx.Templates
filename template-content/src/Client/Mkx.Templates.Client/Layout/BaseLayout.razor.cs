using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mkx.Templates.Client.Services;
namespace Mkx.Templates.Client.Layout;
public partial class BaseLayout : IAsyncDisposable
{
    [Inject] private ThemeService ThemeService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private bool _disposed;
    private DotNetObjectReference<BaseLayout>? _receiver;
    protected override void OnInitialized()
    {
        ThemeService.OnToggleMode += AppearanceChanged;
    }
    private async void AppearanceChanged(object? sender, EventArgs args)
    {
        if (_disposed) return;
        try { await InvokeAsync(async () => { await ApplyAsync(); StateHasChanged(); }); }
        catch (JSDisconnectedException) { }
        catch (Exception ex) { if (!_disposed) await DispatchExceptionAsync(ex); }
    }
    private ValueTask ApplyAsync() => JS.InvokeVoidAsync("Mkx.setAppearance", ThemeService.IsDarkMode);
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await ThemeService.LoadSettingsAsync(await JS.InvokeAsync<bool>("Mkx.systemDark"));
        _receiver = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("Mkx.watchSystemAppearance", _receiver);
        await ApplyAsync();
        await JS.InvokeVoidAsync("Mkx.removeSplash");
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
