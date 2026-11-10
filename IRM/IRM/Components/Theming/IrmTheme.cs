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
            Secondary = "#6366f1",
            Background = "#f8fafc",
            Surface = "#ffffff",
            AppbarBackground = "rgba(255, 255, 255, 0.85)",
            AppbarText = "#0f172a",
            DrawerBackground = "#0f172a",
            DrawerText = "#94a3b8",
            DrawerIcon = "#cbd5e1",
            TextPrimary = "#0f172a",
            TextSecondary = "#475569",
            Divider = "#e2e8f0",
            ActionDefault = "#64748b"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#3b82f6",
            PrimaryDarken = "#2563eb",
            PrimaryLighten = "#60a5fa",
            Secondary = "#818cf8",
            Background = "#090d16",
            Surface = "#0f172a",
            AppbarBackground = "rgba(15, 23, 42, 0.85)",
            AppbarText = "#f8fafc",
            DrawerBackground = "#090d16",
            DrawerText = "#94a3b8",
            DrawerIcon = "#94a3b8",
            TextPrimary = "#f8fafc",
            TextSecondary = "#94a3b8",
            Divider = "#1e293b",
            ActionDefault = "#94a3b8"
        },
        LayoutProperties = new LayoutProperties { DrawerWidthLeft = "260px" }
    };
}
