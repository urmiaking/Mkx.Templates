using Mkx.Templates.Client.Common;
using MudBlazor;

namespace Mkx.Templates.Client.Layout.Themes;

public static class ColorPalettes
{
    private static MudTheme CreateBaseTheme()
    {
        return new MudTheme
        {
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px",
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = ["IRANSans", "Vazirmatn", "Helvetica", "Arial", "sans-serif"]
                }
            }
        };
    }

    public static readonly Dictionary<string, MudTheme> Palettes = new()
    {
        [BuiltInThemes.OceanBlue] = CreateOceanBlueTheme(),
        [BuiltInThemes.GoldenAmber] = CreateGoldenAmberTheme(),
        [BuiltInThemes.EmeraldGreen] = CreateEmeraldGreenTheme(),
        [BuiltInThemes.RoyalPurple] = CreateRoyalPurpleTheme()
    };

    private static MudTheme CreateOceanBlueTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#0F172A",
            White = "#FFFFFF",

            Primary = "#2563EB",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#64748B",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#0EA5E9",
            TertiaryContrastText = "#FFFFFF",

            Info = "#0EA5E9",
            InfoContrastText = "#FFFFFF",
            Success = "#16A34A",
            SuccessContrastText = "#FFFFFF",
            Warning = "#D97706",
            WarningContrastText = "#FFFFFF",
            Error = "#DC2626",
            ErrorContrastText = "#FFFFFF",

            Dark = "#0F172A",
            DarkContrastText = "#FFFFFF",

            TextPrimary = "#0F172A",
            TextSecondary = "#475569",
            TextDisabled = "#94A3B880",
            ActionDefault = "#64748B",
            ActionDisabled = "#94A3B880",
            ActionDisabledBackground = "#E2E8F0",

            Background = "#EFF6FF", // Soft Blue tint
            BackgroundGray = "#E0ECFF",
            Surface = "#FFFFFF",
            DrawerBackground = "#EFF6FF",
            DrawerText = "#0F172A",
            DrawerIcon = "#475569",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",

            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",
            TableLines = "#E2E8F0",
            TableStriped = "#F8FAFC",
            TableHover = "#EFF6FF",
            Divider = "#E2E8F0",
            DividerLight = "#F1F5F9",

            Skeleton = "rgba(148, 163, 184, 0.18)",
            OverlayDark = "rgba(15, 23, 42, 0.60)",
            OverlayLight = "rgba(15, 23, 42, 0.40)",

            GrayDefault = "#94A3B8",
            GrayLight = "#CBD5E1",
            GrayLighter = "#E2E8F0",
            GrayDark = "#64748B",
            GrayDarker = "#334155",

            BorderOpacity = 1.0,
            HoverOpacity = 0.04,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.16
        };

        theme.PaletteDark = new PaletteDark
        {
            Black = "#020617",
            White = "#F8FAFC",

            Primary = "#60A5FA",
            PrimaryContrastText = "#0F172A",
            Secondary = "#94A3B8",
            SecondaryContrastText = "#0F172A",
            Tertiary = "#2DD4BF",
            TertiaryContrastText = "#0F172A",

            Info = "#38BDF8",
            InfoContrastText = "#0F172A",
            Success = "#4ADE80",
            SuccessContrastText = "#052E16",
            Warning = "#FBBF24",
            WarningContrastText = "#1F2937",
            Error = "#F87171",
            ErrorContrastText = "#7F1D1D",

            Dark = "#E2E8F0",
            DarkContrastText = "#0F172A",

            TextPrimary = "#E2E8F0",
            TextSecondary = "#94A3B8",
            TextDisabled = "rgba(148, 163, 184, 0.60)",
            ActionDefault = "#94A3B8",
            ActionDisabled = "rgba(148, 163, 184, 0.38)",
            ActionDisabledBackground = "rgba(148, 163, 184, 0.12)",

            Background = "#0B0F19", // Deep Navy-black
            BackgroundGray = "#0E1424",
            Surface = "#111827",
            DrawerBackground = "#0B0F19",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#94A3B8",

            AppbarBackground = "#111827",
            AppbarText = "#E2E8F0",

            LinesDefault = "rgba(148, 163, 184, 0.20)",
            LinesInputs = "rgba(148, 163, 184, 0.28)",
            TableLines = "rgba(148, 163, 184, 0.18)",
            TableStriped = "rgba(255, 255, 255, 0.02)",
            TableHover = "rgba(96, 165, 250, 0.08)",
            Divider = "rgba(148, 163, 184, 0.22)",
            DividerLight = "rgba(255, 255, 255, 0.08)",

            Skeleton = "rgba(148, 163, 184, 0.14)",
            OverlayDark = "rgba(2, 6, 23, 0.70)",
            OverlayLight = "rgba(15, 23, 42, 0.50)",

            GrayDefault = "#94A3B8",
            GrayLight = "#64748B",
            GrayLighter = "#334155",
            GrayDark = "#475569",
            GrayDarker = "#CBD5E1",

            BorderOpacity = 1.0,
            HoverOpacity = 0.06,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.18,
        };

        return theme;
    }

    private static MudTheme CreateGoldenAmberTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#1A1207",
            White = "#FFFFFF",

            Primary = "#B8860B",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#6B5A3A",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#D4A017",
            TertiaryContrastText = "#FFFFFF",

            Info = "#0C7C8C",
            InfoContrastText = "#FFFFFF",
            Success = "#1A7A42",
            SuccessContrastText = "#FFFFFF",
            Warning = "#B86E00",
            WarningContrastText = "#FFFFFF",
            Error = "#B83030",
            ErrorContrastText = "#FFFFFF",

            Dark = "#1A1207",
            DarkContrastText = "#FFFFFF",

            TextPrimary = "#1A1207",
            TextSecondary = "#4D3F26",
            TextDisabled = "#7A6A4C80",
            ActionDefault = "#5C4D30",
            ActionDisabled = "#7A6A4C80",
            ActionDisabledBackground = "#D4C8AE",

            Background = "#FBF8F0", // Warm parchment
            BackgroundGray = "#F0EAD8",
            Surface = "#FFFFFF",
            DrawerBackground = "#FBF8F0",
            DrawerText = "#1A1207",
            DrawerIcon = "#4D3F26",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#1A1207",

            LinesDefault = "#D0C4A4",
            LinesInputs = "#B5A580",
            TableLines = "#D0C4A4",
            TableStriped = "#FAF7F0",
            TableHover = "#F2ECDc",
            Divider = "#D0C4A4",
            DividerLight = "#E8E0D0",

            Skeleton = "rgba(122, 106, 76, 0.18)",
            OverlayDark = "rgba(26, 18, 7, 0.60)",
            OverlayLight = "rgba(26, 18, 7, 0.40)",

            GrayDefault = "#8C7A58",
            GrayLight = "#B5A580",
            GrayLighter = "#D0C4A4",
            GrayDark = "#5C4D30",
            GrayDarker = "#3A3020",

            BorderOpacity = 1.0,
            HoverOpacity = 0.04,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.16
        };

        theme.PaletteDark = new PaletteDark
        {
            Black = "#0A0806",
            White = "#FBF8F0",

            Primary = "#F0C75E",
            PrimaryContrastText = "#12100A",
            Secondary = "#BDA87A",
            SecondaryContrastText = "#12100A",
            Tertiary = "#F5D678",
            TertiaryContrastText = "#12100A",

            Info = "#38C4D8",
            InfoContrastText = "#0A1612",
            Success = "#5CB87E",
            SuccessContrastText = "#052E16",
            Warning = "#F0A030",
            WarningContrastText = "#1F2937",
            Error = "#F07070",
            ErrorContrastText = "#7F1D1D",

            Dark = "#EDE5D5",
            DarkContrastText = "#12100A",

            TextPrimary = "#EDE5D5",
            TextSecondary = "#BDA87A",
            TextDisabled = "rgba(189, 168, 122, 0.60)",
            ActionDefault = "#BDA87A",
            ActionDisabled = "rgba(189, 168, 122, 0.38)",
            ActionDisabledBackground = "rgba(189, 168, 122, 0.14)",

            Background = "#12100A", // Deep brown-black
            BackgroundGray = "#1C1812",
            Surface = "#1A170F",
            DrawerBackground = "#12100A",
            DrawerText = "#EDE5D5",
            DrawerIcon = "#BDA87A",

            AppbarBackground = "#1A170F",
            AppbarText = "#EDE5D5",

            LinesDefault = "rgba(189, 168, 122, 0.32)",
            LinesInputs = "rgba(189, 168, 122, 0.44)",
            TableLines = "rgba(189, 168, 122, 0.28)",
            TableStriped = "rgba(255, 255, 255, 0.03)",
            TableHover = "rgba(240, 199, 94, 0.10)",
            Divider = "rgba(189, 168, 122, 0.32)",
            DividerLight = "rgba(255, 255, 255, 0.10)",

            Skeleton = "rgba(189, 168, 122, 0.16)",
            OverlayDark = "rgba(10, 8, 6, 0.70)",
            OverlayLight = "rgba(18, 16, 10, 0.50)",

            GrayDefault = "#BDA87A",
            GrayLight = "#8C7A58",
            GrayLighter = "#5C4D30",
            GrayDark = "#6B5A3A",
            GrayDarker = "#D4C8AE",

            BorderOpacity = 1.0,
            HoverOpacity = 0.06,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.18,
        };

        return theme;
    }

    private static MudTheme CreateEmeraldGreenTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#0F1F18",
            White = "#FFFFFF",

            Primary = "#0D7C5F",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#3D6454",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#2AA88A",
            TertiaryContrastText = "#FFFFFF",

            Info = "#0C7C8C",
            InfoContrastText = "#FFFFFF",
            Success = "#1A7A42",
            SuccessContrastText = "#FFFFFF",
            Warning = "#B87A00",
            WarningContrastText = "#FFFFFF",
            Error = "#B83030",
            ErrorContrastText = "#FFFFFF",

            Dark = "#0F1F18",
            DarkContrastText = "#FFFFFF",

            TextPrimary = "#0F1F18",
            TextSecondary = "#2E4F42",
            TextDisabled = "#5C8A7880",
            ActionDefault = "#3D6454",
            ActionDisabled = "#5C8A7880",
            ActionDisabledBackground = "#C0D8CE",

            Background = "#F4FAF7", // Soft minty cream
            BackgroundGray = "#E2F0EA",
            Surface = "#FFFFFF",
            DrawerBackground = "#F4FAF7",
            DrawerText = "#0F1F18",
            DrawerIcon = "#2E4F42",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F1F18",

            LinesDefault = "#B8D4C8",
            LinesInputs = "#96BAA8",
            TableLines = "#B8D4C8",
            TableStriped = "#F6FBF9",
            TableHover = "#E6F3EC",
            Divider = "#B8D4C8",
            DividerLight = "#D8E8E2",

            Skeleton = "rgba(92, 138, 120, 0.18)",
            OverlayDark = "rgba(15, 31, 24, 0.60)",
            OverlayLight = "rgba(15, 31, 24, 0.40)",

            GrayDefault = "#6D9A88",
            GrayLight = "#96BAA8",
            GrayLighter = "#B8D4C8",
            GrayDark = "#3D6454",
            GrayDarker = "#1F3E32",

            BorderOpacity = 1.0,
            HoverOpacity = 0.04,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.16
        };

        theme.PaletteDark = new PaletteDark
        {
            Black = "#040E0B",
            White = "#F4FAF7",

            Primary = "#5EC4A6",
            PrimaryContrastText = "#0A1612",
            Secondary = "#8CBAA8",
            SecondaryContrastText = "#0A1612",
            Tertiary = "#4DDBB8",
            TertiaryContrastText = "#0A1612",

            Info = "#38C4D8",
            InfoContrastText = "#0A1612",
            Success = "#5CB87E",
            SuccessContrastText = "#052E16",
            Warning = "#F0C75E",
            WarningContrastText = "#1F2937",
            Error = "#F07070",
            ErrorContrastText = "#7F1D1D",

            Dark = "#DFF0E8",
            DarkContrastText = "#0A1612",

            TextPrimary = "#DFF0E8",
            TextSecondary = "#8CBAA8",
            TextDisabled = "rgba(140, 186, 168, 0.60)",
            ActionDefault = "#8CBAA8",
            ActionDisabled = "rgba(140, 186, 168, 0.38)",
            ActionDisabledBackground = "rgba(140, 186, 168, 0.14)",

            Background = "#0A1612", // Deep forest-black
            BackgroundGray = "#0F2019",
            Surface = "#121F1A",
            DrawerBackground = "#0A1612",
            DrawerText = "#DFF0E8",
            DrawerIcon = "#8CBAA8",

            AppbarBackground = "#121F1A",
            AppbarText = "#DFF0E8",

            LinesDefault = "rgba(140, 186, 168, 0.32)",
            LinesInputs = "rgba(140, 186, 168, 0.44)",
            TableLines = "rgba(140, 186, 168, 0.28)",
            TableStriped = "rgba(255, 255, 255, 0.03)",
            TableHover = "rgba(94, 196, 166, 0.10)",
            Divider = "rgba(140, 186, 168, 0.32)",
            DividerLight = "rgba(255, 255, 255, 0.10)",

            Skeleton = "rgba(140, 186, 168, 0.16)",
            OverlayDark = "rgba(4, 14, 11, 0.70)",
            OverlayLight = "rgba(10, 22, 18, 0.50)",

            GrayDefault = "#8CBAA8",
            GrayLight = "#5E8A7D",
            GrayLighter = "#3A5C50",
            GrayDark = "#4A6B5D",
            GrayDarker = "#BDD4CA",

            BorderOpacity = 1.0,
            HoverOpacity = 0.06,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.18,
        };

        return theme;
    }

    private static MudTheme CreateRoyalPurpleTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#150D1C",
            White = "#FFFFFF",

            Primary = "#7C3F8E",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#5E3D6E",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#A05DB8",
            TertiaryContrastText = "#FFFFFF",

            Info = "#0C7C8C",
            InfoContrastText = "#FFFFFF",
            Success = "#1A7A42",
            SuccessContrastText = "#FFFFFF",
            Warning = "#B87A00",
            WarningContrastText = "#FFFFFF",
            Error = "#B83030",
            ErrorContrastText = "#FFFFFF",

            Dark = "#150D1C",
            DarkContrastText = "#FFFFFF",

            TextPrimary = "#150D1C",
            TextSecondary = "#3D2850",
            TextDisabled = "#6E5A8080",
            ActionDefault = "#5E3D6E",
            ActionDisabled = "#6E5A8080",
            ActionDisabledBackground = "#D0BED8",

            Background = "#FAF5FC", // Soft lavender cream
            BackgroundGray = "#EFE4F4",
            Surface = "#FFFFFF",
            DrawerBackground = "#FAF5FC",
            DrawerText = "#150D1C",
            DrawerIcon = "#3D2850",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#150D1C",

            LinesDefault = "#C8B0D4",
            LinesInputs = "#AB90BA",
            TableLines = "#C8B0D4",
            TableStriped = "#FBF8FC",
            TableHover = "#F0E6F4",
            Divider = "#C8B0D4",
            DividerLight = "#E4D8EA",

            Skeleton = "rgba(110, 90, 128, 0.18)",
            OverlayDark = "rgba(21, 13, 28, 0.60)",
            OverlayLight = "rgba(21, 13, 28, 0.40)",

            GrayDefault = "#8A70A0",
            GrayLight = "#AB90BA",
            GrayLighter = "#C8B0D4",
            GrayDark = "#5E3D6E",
            GrayDarker = "#301E40",

            BorderOpacity = 1.0,
            HoverOpacity = 0.04,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.16
        };

        theme.PaletteDark = new PaletteDark
        {
            Black = "#080510",
            White = "#FAF5FC",

            Primary = "#C78FD6",
            PrimaryContrastText = "#0F0A14",
            Secondary = "#B098C0",
            SecondaryContrastText = "#0F0A14",
            Tertiary = "#D9A8E8",
            TertiaryContrastText = "#0F0A14",

            Info = "#38C4D8",
            InfoContrastText = "#0A1612",
            Success = "#5CB87E",
            SuccessContrastText = "#052E16",
            Warning = "#F0C75E",
            WarningContrastText = "#1F2937",
            Error = "#F07070",
            ErrorContrastText = "#7F1D1D",

            Dark = "#EAE0F0",
            DarkContrastText = "#0F0A14",

            TextPrimary = "#EAE0F0",
            TextSecondary = "#B098C0",
            TextDisabled = "rgba(176, 152, 192, 0.60)",
            ActionDefault = "#B098C0",
            ActionDisabled = "rgba(176, 152, 192, 0.38)",
            ActionDisabledBackground = "rgba(176, 152, 192, 0.14)",

            Background = "#0F0A14", // Deep purple-black
            BackgroundGray = "#18111E",
            Surface = "#18111E",
            DrawerBackground = "#0F0A14",
            DrawerText = "#EAE0F0",
            DrawerIcon = "#B098C0",

            AppbarBackground = "#18111E",
            AppbarText = "#EAE0F0",

            LinesDefault = "rgba(176, 152, 192, 0.32)",
            LinesInputs = "rgba(176, 152, 192, 0.44)",
            TableLines = "rgba(176, 152, 192, 0.28)",
            TableStriped = "rgba(255, 255, 255, 0.03)",
            TableHover = "rgba(199, 143, 214, 0.10)",
            Divider = "rgba(176, 152, 192, 0.32)",
            DividerLight = "rgba(255, 255, 255, 0.10)",

            Skeleton = "rgba(176, 152, 192, 0.16)",
            OverlayDark = "rgba(8, 5, 16, 0.70)",
            OverlayLight = "rgba(15, 10, 20, 0.50)",

            GrayDefault = "#B098C0",
            GrayLight = "#8A70A0",
            GrayLighter = "#4A3560",
            GrayDark = "#5E4A68",
            GrayDarker = "#D0BED8",

            BorderOpacity = 1.0,
            HoverOpacity = 0.06,
            RippleOpacity = 0.10,
            RippleOpacitySecondary = 0.18,
        };

        return theme;
    }
}
