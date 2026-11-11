using MudBlazor;

namespace IRM.Components.Theming;

public static class IrmTheme
{
    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1d4ed8",
            PrimaryDarken = "#1e40af",
            PrimaryLighten = "#3b82f6",
            PrimaryContrastText = "#ffffff",
            Secondary = "#3730a3",
            SecondaryDarken = "#312e81",
            SecondaryContrastText = "#ffffff",
            Info = "#0369a1",
            InfoDarken = "#075985",
            InfoContrastText = "#ffffff",
            Success = "#15803d",
            SuccessDarken = "#166534",
            SuccessContrastText = "#ffffff",
            Warning = "#b45309",
            WarningDarken = "#92400e",
            WarningContrastText = "#ffffff",
            Error = "#b91c1c",
            ErrorDarken = "#991b1b",
            ErrorContrastText = "#ffffff",
            Background = "#f4f6f9",
            BackgroundGray = "#eef2f6",
            Surface = "#ffffff",
            AppbarBackground = "rgba(255, 255, 255, 0.95)",
            AppbarText = "#0f172a",
            DrawerBackground = "#ffffff",
            DrawerText = "#334155",
            DrawerIcon = "#475569",
            TextPrimary = "#0f172a",
            TextSecondary = "#475569",
            TextDisabled = "#64748b",
            Divider = "#cbd5e1",
            LinesInputs = "#64748b",
            ActionDefault = "#475569",
            ActionDisabled = "#64748b",
            ActionDisabledBackground = "#e2e8f0"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#60a5fa",
            PrimaryDarken = "#3b82f6",
            PrimaryLighten = "#93c5fd",
            PrimaryContrastText = "#0f172a",
            Secondary = "#a5b4fc",
            SecondaryDarken = "#818cf8",
            SecondaryContrastText = "#0f172a",
            Info = "#7dd3fc",
            InfoDarken = "#38bdf8",
            InfoContrastText = "#0f172a",
            Success = "#86efac",
            SuccessDarken = "#4ade80",
            SuccessContrastText = "#0f172a",
            Warning = "#fcd34d",
            WarningDarken = "#fbbf24",
            WarningContrastText = "#0f172a",
            Error = "#fca5a5",
            ErrorDarken = "#f87171",
            ErrorContrastText = "#0f172a",
            Background = "#0b1120",
            BackgroundGray = "#1e293b",
            Surface = "#0f172a",
            AppbarBackground = "rgba(15, 23, 42, 0.95)",
            AppbarText = "#f8fafc",
            DrawerBackground = "#0f172a",
            DrawerText = "#cbd5e1",
            DrawerIcon = "#94a3b8",
            TextPrimary = "#f8fafc",
            TextSecondary = "#cbd5e1",
            TextDisabled = "#94a3b8",
            Divider = "#334155",
            LinesInputs = "#94a3b8",
            ActionDefault = "#cbd5e1",
            ActionDisabled = "#94a3b8",
            ActionDisabledBackground = "#1e293b"
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "300px",
            DefaultBorderRadius = "6px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Segoe UI", "system-ui", "-apple-system", "BlinkMacSystemFont", "sans-serif"]
            },
            Button = new ButtonTypography
            {
                FontFamily = ["Segoe UI", "system-ui", "-apple-system", "BlinkMacSystemFont", "sans-serif"],
                TextTransform = "none",
                FontWeight = "600"
            }
        }
    };
}
