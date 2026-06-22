using Blazored.LocalStorage;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Mkx.Templates.Client.Layout.Themes;

namespace Mkx.Templates.Client.Layout;

public partial class BaseLayout
{
    private MudThemeProvider? _mudThemeProvider;

    public bool IsDarkMode { get; set; }

    [Inject] private ThemeService? ThemeService { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private ISnackbar ToastService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (ThemeService is not null)
        {
            await ThemeService.LoadSettingsAsync();
            ThemeService.OnToggleMode += OnToggleMode;
            ThemeService.OnPaletteChanged += OnPaletteChanged;
            IsDarkMode = ThemeService.IsDarkMode;
        }

        await base.OnInitializedAsync();
    }

    public void OnPaletteChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }

    public void OnToggleMode(object? sender, EventArgs e)
    {
        IsDarkMode = ThemeService?.IsDarkMode ?? false;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadTheme();
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadTheme()
    {
        if (ThemeService is null || _mudThemeProvider is null) return;

        // 1. Try to load saved dark mode preference from local storage
        var savedDarkMode = await LocalStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);

        bool isDark;
        if (string.IsNullOrEmpty(savedDarkMode))
        {
            // No explicit user preference, load from OS preference
            isDark = await _mudThemeProvider.GetSystemDarkModeAsync();
            // Update ThemeService's state, but do NOT save it to local storage since it's just OS preference
            await ThemeService.SetDarkModeAsync(isDark, saveToStorage: false);

            // Subscribe to system preference changes dynamically (only when user has no saved preference)
            await _mudThemeProvider.WatchSystemDarkModeAsync(async (newValue) =>
            {
                // Only apply if user still hasn't explicitly set a preference
                var currentSaved = await LocalStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);
                if (string.IsNullOrEmpty(currentSaved))
                {
                    await ThemeService.SetDarkModeAsync(newValue, saveToStorage: false);
                    IsDarkMode = newValue;
                    StateHasChanged();
                }
            });
        }
        else
        {
            // Use saved preference
            bool.TryParse(savedDarkMode, out isDark);
            await ThemeService.SetDarkModeAsync(isDark, saveToStorage: true);
        }

        var savedPalette = await LocalStorage.GetItemAsStringAsync(LocalStorageKeys.SelectedPalette);
        if (!string.IsNullOrEmpty(savedPalette) && ColorPalettes.Palettes.ContainsKey(savedPalette))
        {
            await ThemeService.SetPaletteAsync(savedPalette!);
        }

        IsDarkMode = ThemeService.IsDarkMode;
        StateHasChanged();
    }
}
