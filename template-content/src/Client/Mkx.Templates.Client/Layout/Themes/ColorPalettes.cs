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
        [BuiltInThemes.EnterpriseBlue] = CreateEnterpriseBlueTheme(),
        [BuiltInThemes.EnterpriseSlate] = CreateEnterpriseSlateTheme(),
        [BuiltInThemes.EnterpriseTeal] = CreateEnterpriseTealTheme(),
        [BuiltInThemes.EnterpriseIndigo] = CreateEnterpriseIndigoTheme()
    };

    private static MudTheme CreateEnterpriseBlueTheme()
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

    private static MudTheme CreateEnterpriseSlateTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#0F172A",
            White = "#FFFFFF",

            Primary = "#334155",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#64748B",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#0284C7",
            TertiaryContrastText = "#FFFFFF",

            Info = "#0284C7",
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

            Background = "#F1F5F9", // Neutral Slate
            BackgroundGray = "#E2E8F0",
            Surface = "#FFFFFF",
            DrawerBackground = "#F1F5F9",
            DrawerText = "#0F172A",
            DrawerIcon = "#475569",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",

            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",
            TableLines = "#E2E8F0",
            TableStriped = "#F8FAFC",
            TableHover = "#F1F5F9",
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

            Primary = "#94A3B8",
            PrimaryContrastText = "#0F172A",
            Secondary = "#64748B",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#38BDF8",
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

            Background = "#0E0E11", // Pure Charcoal-black
            BackgroundGray = "#18181B",
            Surface = "#18181B",
            DrawerBackground = "#0E0E11",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#94A3B8",

            AppbarBackground = "#18181B",
            AppbarText = "#E2E8F0",

            LinesDefault = "rgba(148, 163, 184, 0.20)",
            LinesInputs = "rgba(148, 163, 184, 0.28)",
            TableLines = "rgba(148, 163, 184, 0.18)",
            TableStriped = "rgba(255, 255, 255, 0.02)",
            TableHover = "rgba(148, 163, 184, 0.08)",
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

    private static MudTheme CreateEnterpriseTealTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#0F172A",
            White = "#FFFFFF",

            Primary = "#0F766E",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#14B8A6",
            SecondaryContrastText = "#0F172A",
            Tertiary = "#2563EB",
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

            Background = "#F2FAF9", // Soft Warm Mint
            BackgroundGray = "#E0F2F1",
            Surface = "#FFFFFF",
            DrawerBackground = "#F2FAF9",
            DrawerText = "#0F172A",
            DrawerIcon = "#475569",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",

            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",
            TableLines = "#E2E8F0",
            TableStriped = "#F8FAFC",
            TableHover = "#F0FDFA",
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

            Primary = "#2DD4BF",
            PrimaryContrastText = "#0F172A",
            Secondary = "#14B8A6",
            SecondaryContrastText = "#0F172A",
            Tertiary = "#60A5FA",
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

            Background = "#041010", // Deep Teal-black
            BackgroundGray = "#0A2424",
            Surface = "#0C1D1D",
            DrawerBackground = "#041010",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#94A3B8",

            AppbarBackground = "#0C1D1D",
            AppbarText = "#E2E8F0",

            LinesDefault = "rgba(148, 163, 184, 0.20)",
            LinesInputs = "rgba(148, 163, 184, 0.28)",
            TableLines = "rgba(148, 163, 184, 0.18)",
            TableStriped = "rgba(255, 255, 255, 0.02)",
            TableHover = "rgba(45, 212, 191, 0.08)",
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

    private static MudTheme CreateEnterpriseIndigoTheme()
    {
        var theme = CreateBaseTheme();

        theme.PaletteLight = new PaletteLight
        {
            Black = "#0F172A",
            White = "#FFFFFF",

            Primary = "#4F46E5",
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

            Background = "#F5F3FF", // Soft Lavender
            BackgroundGray = "#EDE9FE",
            Surface = "#FFFFFF",
            DrawerBackground = "#F5F3FF",
            DrawerText = "#0F172A",
            DrawerIcon = "#475569",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",

            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",
            TableLines = "#E2E8F0",
            TableStriped = "#F8FAFC",
            TableHover = "#EEF2FF",
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

            Primary = "#818CF8",
            PrimaryContrastText = "#0F172A",
            Secondary = "#94A3B8",
            SecondaryContrastText = "#0F172A",
            Tertiary = "#38BDF8",
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

            Background = "#09071A", // Deep Lavender-black
            BackgroundGray = "#16113A",
            Surface = "#120E2E",
            DrawerBackground = "#09071A",
            DrawerText = "#E2E8F0",
            DrawerIcon = "#94A3B8",

            AppbarBackground = "#120E2E",
            AppbarText = "#E2E8F0",

            LinesDefault = "rgba(148, 163, 184, 0.20)",
            LinesInputs = "rgba(148, 163, 184, 0.28)",
            TableLines = "rgba(148, 163, 184, 0.18)",
            TableStriped = "rgba(255, 255, 255, 0.02)",
            TableHover = "rgba(129, 140, 248, 0.08)",
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
}
