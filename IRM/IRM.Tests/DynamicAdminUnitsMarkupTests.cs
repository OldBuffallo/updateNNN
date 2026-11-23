namespace IRM.Tests;

public sealed class DynamicAdminUnitsMarkupTests
{
    private static string ResolveSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IRM.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    [Fact]
    public void UiAndTourFiles_DoNotHardcode54UnitsOrQuangNinhSidebarBadges()
    {
        var root = ResolveSolutionRoot();
        var files = new[]
        {
            Path.Combine(root, "IRM", "Components", "Shared", "NavMenu.razor"),
            Path.Combine(root, "IRM", "Features", "Reports", "Home.razor"),
            Path.Combine(root, "IRM", "Features", "Reports", "Statistics.razor"),
            Path.Combine(root, "IRM", "Components", "Shared", "SpotlightSearch.razor"),
            Path.Combine(root, "IRM", "wwwroot", "js", "app-product-tour.js")
        };

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("54 địa bàn", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("54 xã", text, StringComparison.OrdinalIgnoreCase);
        }

        var mainLayout = File.ReadAllText(Path.Combine(root, "IRM", "Components", "Shared", "MainLayout.razor"));
        Assert.DoesNotContain(">QN<", mainLayout, StringComparison.Ordinal);
        Assert.DoesNotContain(">HL<", mainLayout, StringComparison.Ordinal);
        Assert.DoesNotContain(">MC<", mainLayout, StringComparison.Ordinal);
        Assert.DoesNotContain(">CP<", mainLayout, StringComparison.Ordinal);
    }

    [Fact]
    public void StatisticsPage_LinksAdminToAdminUnitsConfiguration()
    {
        var root = ResolveSolutionRoot();
        var statistics = File.ReadAllText(Path.Combine(root, "IRM", "Features", "Reports", "Statistics.razor"));

        Assert.Contains("/admin/admin-units", statistics, StringComparison.Ordinal);
        Assert.Contains("Thống kê Lịch sử & Địa bàn", statistics, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminUnitsSection_HasDdMmYyyyDatePickerSearchAndExcelImportControls()
    {
        var root = ResolveSolutionRoot();
        var adminUnits = File.ReadAllText(Path.Combine(root, "IRM", "Features", "Administration", "Components", "AdminAdministrativeUnitsSection.razor"));

        Assert.Contains("DateFormat=\"dd/MM/yyyy\"", adminUnits, StringComparison.Ordinal);
        Assert.Contains("adminUnitExcelInput", adminUnits, StringComparison.Ordinal);
        Assert.Contains("DownloadAdminUnitTemplateAsync", adminUnits, StringComparison.Ordinal);
        Assert.Contains("ConfirmImportAdminUnitsAsync", adminUnits, StringComparison.Ordinal);
        Assert.Contains("_adminUnitSearch", adminUnits, StringComparison.Ordinal);
    }
}
