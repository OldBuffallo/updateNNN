using System.Text.RegularExpressions;

namespace IRM.Tests;

public sealed class AppProductTourMarkupTests
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
    public void NavMenu_ContainsAllSidebarTourAnchorsInExactOrderAndAdminInsideAuthorizeView()
    {
        var path = Path.Combine(ResolveSolutionRoot(), "IRM", "Components", "Shared", "NavMenu.razor");
        var markup = File.ReadAllText(path);

        var expectedSidebarOrder = new[]
        {
            "nav-dashboard",
            "nav-foreign-persons",
            "nav-companies",
            "nav-accommodations",
            "nav-inspections",
            "nav-search",
            "nav-reports",
            "nav-statistics",
            "nav-import"
        };

        var previousIndex = -1;
        foreach (var anchor in expectedSidebarOrder)
        {
            var token = $"data-app-tour=\"{anchor}\"";
            var idx = markup.IndexOf(token, StringComparison.Ordinal);
            Assert.True(idx > previousIndex, $"Expected '{token}' in NavMenu.razor after index {previousIndex}, got {idx}.");
            previousIndex = idx;
        }

        var adminAuthStart = markup.IndexOf("<AuthorizeView Roles=\"ADMIN\"", StringComparison.Ordinal);
        var adminAuthEnd = markup.IndexOf("</AuthorizeView>", adminAuthStart, StringComparison.Ordinal);
        var adminTourIdx = markup.IndexOf("data-app-tour=\"admin-system\"", StringComparison.Ordinal);

        Assert.True(adminAuthStart >= 0 && adminAuthEnd > adminAuthStart, "Expected <AuthorizeView Roles=\"ADMIN\"> block in NavMenu.razor.");
        Assert.True(adminTourIdx > adminAuthStart && adminTourIdx < adminAuthEnd, "Expected data-app-tour=\"admin-system\" inside <AuthorizeView Roles=\"ADMIN\">.");
    }

    [Fact]
    public void MainLayout_WiresProductTourButtonAutoInitAndPassportRequiredBanner()
    {
        var path = Path.Combine(ResolveSolutionRoot(), "IRM", "Components", "Shared", "MainLayout.razor");
        var markup = File.ReadAllText(path);

        Assert.Contains("id=\"btnAppUserGuide\"", markup, StringComparison.Ordinal);
        Assert.Contains("irmAppTour.init", markup, StringComparison.Ordinal);
        Assert.Contains("data-passport-required-notice", markup, StringComparison.Ordinal);
        Assert.Contains("PHẢI có Số hộ chiếu", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AppRazor_LoadsAppProductTourScript()
    {
        var path = Path.Combine(ResolveSolutionRoot(), "IRM", "Components", "App.razor");
        var markup = File.ReadAllText(path);

        Assert.Contains("js/app-product-tour.js", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportUploadAndForeignPersonsModal_DisplayPassportRequiredReminder()
    {
        var root = ResolveSolutionRoot();
        var importUpload = File.ReadAllText(Path.Combine(root, "IRM", "Features", "ImportExport", "Components", "ImportUploadSection.razor"));
        var foreignPersons = File.ReadAllText(Path.Combine(root, "IRM", "Features", "ForeignPersons", "ForeignPersons.razor"));

        Assert.Contains("data-passport-required-notice", importUpload, StringComparison.Ordinal);
        Assert.Contains("Số hộ chiếu", importUpload, StringComparison.Ordinal);

        Assert.Contains("data-passport-required-notice", foreignPersons, StringComparison.Ordinal);
        Assert.Contains("Số hộ chiếu", foreignPersons, StringComparison.Ordinal);
    }
}
