using MudBlazor;

namespace IRM.Components.Theming;

public static class IrmTheme
{
    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6e39cb",
            PrimaryDarken = "#5b21b6",
            PrimaryLighten = "#8c57ff",
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
            Background = "#f4f5fa",
            BackgroundGray = "#eceef5",
            Surface = "#ffffff",
            AppbarBackground = "rgba(244, 245, 250, 0.88)",
            AppbarText = "#3a3541",
            DrawerBackground = "#f4f5fa",
            DrawerText = "#3a3541",
            DrawerIcon = "#544f5a",
            TextPrimary = "#3a3541",
            TextSecondary = "#544f5a",
            TextDisabled = "#787380",
            Divider = "#dbdade",
            LinesInputs = "#787380",
            ActionDefault = "#544f5a",
            ActionDisabled = "#787380",
            ActionDisabledBackground = "#e6e5eb"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#b794f4",
            PrimaryDarken = "#9f7aea",
            PrimaryLighten = "#d6bcfa",
            PrimaryContrastText = "#181526",
            Secondary = "#c4b5fd",
            SecondaryDarken = "#a78bfa",
            SecondaryContrastText = "#181526",
            Info = "#7dd3fc",
            InfoDarken = "#38bdf8",
            InfoContrastText = "#181526",
            Success = "#86efac",
            SuccessDarken = "#4ade80",
            SuccessContrastText = "#181526",
            Warning = "#fcd34d",
            WarningDarken = "#fbbf24",
            WarningContrastText = "#181526",
            Error = "#fca5a5",
            ErrorDarken = "#f87171",
            ErrorContrastText = "#181526",
            Background = "#28243d",
            BackgroundGray = "#231f36",
            Surface = "#312d4b",
            AppbarBackground = "rgba(49, 45, 75, 0.88)",
            AppbarText = "#e7e3fc",
            DrawerBackground = "#28243d",
            DrawerText = "#c9c4df",
            DrawerIcon = "#c9c4df",
            TextPrimary = "#e7e3fc",
            TextSecondary = "#c9c4df",
            TextDisabled = "#9e99b8",
            Divider = "#474360",
            LinesInputs = "#9e99b8",
            ActionDefault = "#c9c4df",
            ActionDisabled = "#9e99b8",
            ActionDisabledBackground = "#3d3759"
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "260px",
            DefaultBorderRadius = "10px"
        }
    };
}
