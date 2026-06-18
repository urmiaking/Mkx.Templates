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

    public void ToggleMode()
    {
        _isDarkMode = !_isDarkMode;

        if (OperatingSystem.IsBrowser())
        {
            _ = localStorage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, _isDarkMode.ToString());
        }

        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetPaletteAsync(string paletteName)
    {
        if (ColorPalettes.Palettes.ContainsKey(paletteName))
        {
            _currentPalette = paletteName;

            if (OperatingSystem.IsBrowser())
            {
                await localStorage.SetItemAsStringAsync(LocalStorageKeys.SelectedPalette, paletteName);
            }

            OnPaletteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task LoadSettingsAsync()
    {
        if (!OperatingSystem.IsBrowser())
        {
            _currentPalette = "EnterpriseBlue";
            _isDarkMode = false;
            return;
        }

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
}
