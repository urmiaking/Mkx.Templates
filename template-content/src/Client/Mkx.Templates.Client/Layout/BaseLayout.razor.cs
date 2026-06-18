using Blazored.LocalStorage;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Mkx.Templates.Client.Layout;

public partial class BaseLayout
{
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
        SetThemeMode(IsDarkMode);
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
        var themeMode = await LocalStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);

        if (!string.IsNullOrEmpty(themeMode) && bool.TryParse(themeMode, out var isDarkMode))
            SetThemeMode(isDarkMode);
    }

    private void SetThemeMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        LocalStorage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, IsDarkMode.ToString());
        StateHasChanged();
    }
}
