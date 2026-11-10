using IRM.Components.Theming;
using MudBlazor;
using MudBlazor.Utilities;

namespace IRM.Tests;

public sealed class IrmThemePaletteTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BodyAndSecondaryText_AreReadableOnBothPageAndSurface(bool dark)
    {
        var palette = Palette(dark);
        foreach (var background in new[] { palette.Background, palette.Surface, palette.BackgroundGray })
        {
            Readable(palette.TextPrimary, background, 4.5);
            Readable(palette.TextSecondary, background, 4.5);
            Readable(palette.ActionDefault, background, 3);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FilledActions_HaveReadableLabelsInNormalAndHoverStates(bool dark)
    {
        var palette = Palette(dark);
        foreach (var (text, normal, hover) in new[]
        {
            (palette.PrimaryContrastText, palette.Primary, new MudColor(palette.PrimaryDarken)),
            (palette.SecondaryContrastText, palette.Secondary, new MudColor(palette.SecondaryDarken)),
            (palette.InfoContrastText, palette.Info, new MudColor(palette.InfoDarken)),
            (palette.SuccessContrastText, palette.Success, new MudColor(palette.SuccessDarken)),
            (palette.WarningContrastText, palette.Warning, new MudColor(palette.WarningDarken)),
            (palette.ErrorContrastText, palette.Error, new MudColor(palette.ErrorDarken))
        })
        {
            Readable(text, normal, 4.5);
            Readable(text, hover, 4.5);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutlinedAndTextActions_AreReadableAgainstPageAndSurface(bool dark)
    {
        var palette = Palette(dark);
        foreach (var color in new[] { palette.Primary, palette.Secondary, palette.Info, palette.Success, palette.Warning, palette.Error })
        {
            Readable(color, palette.Surface, 4.5);
            Readable(color, palette.Background, 4.5);
        }
    }

    private static Palette Palette(bool dark)
    {
        var theme = IrmTheme.Create();
        return dark ? theme.PaletteDark : theme.PaletteLight;
    }

    private static void Readable(MudColor foreground, MudColor background, double minimum)
    {
        static double Linear(byte channel)
        {
            var value = channel / 255d;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        static double Luminance(MudColor color) => 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
        var first = Luminance(foreground);
        var second = Luminance(background);
        var ratio = (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
        Assert.True(ratio >= minimum, $"{foreground} on {background}: {ratio:F3}:1 < {minimum}:1");
    }
}
