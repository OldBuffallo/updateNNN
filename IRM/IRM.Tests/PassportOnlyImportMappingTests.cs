using IRM.Application.Features.ImportExport;
using IRM.Data.Models;

namespace IRM.Tests;

public sealed class PassportOnlyImportMappingTests
{
    private static List<NationalityEntity> SampleNationalities =>
    [
        new() { IDNationality = 1, NationalityCode = "USA", NationalityName = "Hoa Kỳ" },
        new() { IDNationality = 2, NationalityCode = "KOR", NationalityName = "Hàn Quốc" }
    ];

    private static List<Career> SampleCareers =>
    [
        new() { IDCareer = 1, CareerName = "Kỹ sư", IDCG = 1 },
        new() { IDCareer = 2, CareerName = "Quản lý", IDCG = 2 }
    ];

    [Fact]
    public void ParseRowWithWarnings_OnlyPassportFilled_SucceedsWithMissingFieldWarnings()
    {
        var row = new Dictionary<string, string>
        {
            ["__RowNumber"] = "2",
            ["Passport"] = "P123456"
        };

        var result = ImportMapping.ParseRowWithWarnings(
            row, null, SampleNationalities, SampleCareers, null, new HashSet<string>(), ["Hải Châu"]);

        Assert.Empty(result.Errors);
        Assert.NotNull(result.Employee);
        Assert.Equal("P123456", result.Employee!.Passport);
        Assert.Null(result.Employee.IDCompany);
        Assert.Equal("", result.Employee.StaffName);
        Assert.Contains(result.Warnings, w => w.Contains("họ tên", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, w => w.Contains("ngày sinh", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, w => w.Contains("quốc tịch", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseRowWithWarnings_MissingPassport_ReturnsPassportRequiredError()
    {
        var row = new Dictionary<string, string>
        {
            ["__RowNumber"] = "3",
            ["StaffName"] = "NGUYEN VAN A"
        };

        var result = ImportMapping.ParseRowWithWarnings(
            row, null, SampleNationalities, SampleCareers, null, new HashSet<string>(), ["Hải Châu"]);

        Assert.Null(result.Employee);
        Assert.Contains(result.Errors, e => e.Contains("hộ chiếu", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseRowWithWarnings_MultipleInvalidOptionalFields_CollectsAllErrorsInOnePass()
    {
        var row = new Dictionary<string, string>
        {
            ["__RowNumber"] = "4",
            ["Passport"] = "P777777",
            ["Birthday"] = "not-a-date",
            ["Nationality"] = "ZZZ",
            ["TemporaryStay"] = "99/99/2099"
        };

        var result = ImportMapping.ParseRowWithWarnings(
            row, null, SampleNationalities, SampleCareers, null, new HashSet<string>(), ["Hải Châu"]);

        Assert.Null(result.Employee);
        Assert.True(result.Errors.Count >= 3, $"Expected >=3 errors, got {result.Errors.Count}: {string.Join(" | ", result.Errors)}");
    }

    [Fact]
    public void ParseRowWithWarnings_AmbiguousGenderWorkPermitFamilyVisit_EmitsCoercionWarnings()
    {
        var row = new Dictionary<string, string>
        {
            ["__RowNumber"] = "5",
            ["Passport"] = "P888888",
            ["Gender"] = "Male",
            ["WorkPermit"] = "Chưa có",
            ["FamilyVisit"] = "true"
        };

        var result = ImportMapping.ParseRowWithWarnings(
            row, null, SampleNationalities, SampleCareers, null, new HashSet<string>(), ["Hải Châu"]);

        Assert.Empty(result.Errors);
        Assert.NotNull(result.Employee);
        Assert.Contains(result.Warnings, w => w.Contains("Giới tính", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, w => w.Contains("GPLĐ", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, w => w.Contains("Thăm thân", StringComparison.OrdinalIgnoreCase));
    }
}
