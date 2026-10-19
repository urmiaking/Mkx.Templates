using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mkx.Templates.Client.Common;

namespace Mkx.Templates.Client.Services;

public sealed class ThemeService(ILocalStorageService storage, IJSRuntime js)
{
    public static IReadOnlyList<(string Id, string Name)> Palettes { get; } =
        [("jade", "یشم"), ("ocean", "اقیانوس"), ("iris", "زنبق"), ("amber", "کهربا")];
    public string Palette { get; private set; } = "jade";
    public bool IsDarkMode { get; private set; }
    public event EventHandler? OnToggleMode;
    public async Task SetPaletteAsync(string palette, ElementReference source)
    {
        if (palette == Palette || !Palettes.Any(item => item.Id == palette)) return;
        Palette = palette;
        try { await storage.SetItemAsStringAsync(LocalStorageKeys.Palette, palette); }
        catch (JSException) { /* Keep appearance usable when storage is blocked. */ }
        if (Palette != palette) return;
        await js.InvokeVoidAsync("Mkx.setPalette", palette, true, source);
        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }
    public Task ToggleModeAsync(ElementReference source) => SetDarkModeAsync(!IsDarkMode, source: source);
    public async Task SetDarkModeAsync(bool value, bool saveToStorage = true, ElementReference? source = null)
    {
        IsDarkMode = value;
        if (saveToStorage)
        {
            try { await storage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, value.ToString()); }
            catch (JSException) { /* The selected appearance still works without browser storage. */ }
        }
        await js.InvokeVoidAsync("Mkx.setAppearance", value, source);
        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }
    public async Task LoadSettingsAsync(bool systemDark)
    {
        IsDarkMode = systemDark;
        try
        {
            var saved = await storage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);
            if (bool.TryParse(saved, out var dark)) IsDarkMode = dark;
        }
        catch (JSException) { }
        try
        {
            var saved = await storage.GetItemAsStringAsync(LocalStorageKeys.Palette);
            if (Palettes.Any(item => item.Id == saved)) Palette = saved!;
        }
        catch (JSException) { }
        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }
}
