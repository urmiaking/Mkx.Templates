using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Mkx.Templates.Client.Common;

namespace Mkx.Templates.Client.Services;

public sealed class ThemeService(ILocalStorageService storage)
{
    public bool IsDarkMode { get; private set; }
    public event EventHandler? OnToggleMode;
    public Task ToggleModeAsync() => SetDarkModeAsync(!IsDarkMode);
    public async Task SetDarkModeAsync(bool value, bool saveToStorage = true)
    {
        IsDarkMode = value;
        if (saveToStorage)
        {
            try { await storage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, value.ToString()); }
            catch (JSException) { /* The selected appearance still works without browser storage. */ }
        }
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
    }
}
