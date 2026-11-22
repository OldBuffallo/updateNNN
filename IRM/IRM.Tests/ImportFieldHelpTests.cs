using IRM.Data.Models;
using IRM.Features.ImportExport.Help;

namespace IRM.Tests;

public class ImportFieldHelpTests
{
    [Fact]
    public void Catalog_CoversExactlySupportedFields()
    {
        var expected = SystemFields.All.Keys.Where(key => key != "skip").OrderBy(key => key).ToArray();
        Assert.Equal(35, ImportFieldHelpCatalog.All.Count);
        Assert.Equal(expected, ImportFieldHelpCatalog.All.Keys.OrderBy(key => key).ToArray());
    }

    [Fact]
    public void Catalog_RequiredFieldIsPassportOnly()
    {
        string[] required = ["Passport"];
        Assert.Equal(required, ImportFieldHelpCatalog.All.Values
            .Where(entry => entry.IsRequired).Select(entry => entry.Key).OrderBy(key => key));
    }

    [Fact]
    public void Catalog_UsesSystemLabelsAndHasCompleteGuidance()
    {
        foreach (var (key, entry) in ImportFieldHelpCatalog.All)
        {
            Assert.Equal(key, entry.Key);
            Assert.Equal(SystemFields.All[key], entry.Label);
            foreach (var text in new[] { entry.Group, entry.ExcelType, entry.Summary, entry.Checks,
                         entry.EmptyBehavior, entry.ValidExample, entry.Pitfall, entry.Troubleshooting, entry.Recommendation })
                Assert.False(string.IsNullOrWhiteSpace(text), $"Incomplete help for {key}");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("skip")]
    [InlineData("unknown")]
    public void Lookup_UnknownAndSkipReturnNull(string? key) => Assert.Null(ImportFieldHelpCatalog.Get(key));
}
