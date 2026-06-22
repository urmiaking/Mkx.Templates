using Blazored.LocalStorage;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Layout.Themes;
using MudBlazor;

namespace Mkx.Templates.Client.Services;

public class ThemeService(ILocalStorageService localStorage)
{
    private string _currentPalette = "EnterpriseBlue";
    private bool _isDarkMode;

    public bool IsDarkMode => _isDarkMode;
    public string CurrentPalette => _currentPalette;
    public MudTheme CurrentTheme => ColorPalettes.Palettes[_currentPalette];

    public event EventHandler? OnToggleMode;
    public event EventHandler? OnPaletteChanged;

    public async Task ToggleModeAsync()
    {
        await SetDarkModeAsync(!_isDarkMode, saveToStorage: true);
    }

    public async Task SetDarkModeAsync(bool isDarkMode, bool saveToStorage = true)
    {
        _isDarkMode = isDarkMode;

        if (saveToStorage)
        {
            try
            {
                await localStorage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, _isDarkMode.ToString());
            }
            catch (InvalidOperationException)
            {
                // JSInterop not available (prerendering)
            }
        }

        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetPaletteAsync(string paletteName)
    {
        if (ColorPalettes.Palettes.ContainsKey(paletteName))
        {
            _currentPalette = paletteName;

            try
            {
                await localStorage.SetItemAsStringAsync(LocalStorageKeys.SelectedPalette, paletteName);
            }
            catch (InvalidOperationException)
            {
                // JSInterop not available (prerendering)
            }

            OnPaletteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            var savedPalette = await localStorage.GetItemAsStringAsync(LocalStorageKeys.SelectedPalette);
            if (!string.IsNullOrEmpty(savedPalette) && ColorPalettes.Palettes.ContainsKey(savedPalette))
            {
                _currentPalette = savedPalette;
            }

            var savedDarkMode = await localStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);
            if (!string.IsNullOrEmpty(savedDarkMode) && bool.TryParse(savedDarkMode, out var isDarkMode))
            {
                _isDarkMode = isDarkMode;
            }
        }
        catch (InvalidOperationException)
        {
            // JSInterop not available (prerendering), fallback to defaults
        }
    }
}
