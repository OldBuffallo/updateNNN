using MudBlazor;

namespace IRM.Components.Theming;

public static class IrmTheme
{
    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2563eb",
            PrimaryDarken = "#1d4ed8",
            PrimaryLighten = "#3b82f6",
            PrimaryContrastText = "#ffffff",
            Secondary = "#4f46e5",
            SecondaryDarken = "#4338ca",
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
            Background = "#f8fafc",
            BackgroundGray = "#f1f5f9",
            Surface = "#ffffff",
            AppbarBackground = "rgba(255, 255, 255, 0.85)",
            AppbarText = "#0f172a",
            DrawerBackground = "#0f172a",
            DrawerText = "#94a3b8",
            DrawerIcon = "#cbd5e1",
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
            Background = "#090d16",
            BackgroundGray = "#1e293b",
            Surface = "#0f172a",
            AppbarBackground = "rgba(15, 23, 42, 0.85)",
            AppbarText = "#f8fafc",
            DrawerBackground = "#090d16",
            DrawerText = "#94a3b8",
            DrawerIcon = "#94a3b8",
            TextPrimary = "#f8fafc",
            TextSecondary = "#cbd5e1",
            TextDisabled = "#94a3b8",
            Divider = "#475569",
            LinesInputs = "#94a3b8",
            ActionDefault = "#cbd5e1",
            ActionDisabled = "#94a3b8",
            ActionDisabledBackground = "#1e293b"
        },
        LayoutProperties = new LayoutProperties { DrawerWidthLeft = "260px" }
    };
}
