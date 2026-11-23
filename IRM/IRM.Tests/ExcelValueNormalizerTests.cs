using IRM.Application.Features.ImportExport;
using IRM.Application.Models;

namespace IRM.Tests;

public sealed class ExcelValueNormalizerTests
{
    private static readonly List<NationalityEntityDto> SampleNationalities =
    [
        new() { IDNationality = 1, NationalityCode = "KOR", NationalityName = "Hàn Quốc" },
        new() { IDNationality = 2, NationalityCode = "JPN", NationalityName = "Nhật Bản" },
        new() { IDNationality = 3, NationalityCode = "USA", NationalityName = "Hoa Kỳ" },
        new() { IDNationality = 4, NationalityCode = "CHN", NationalityName = "Trung Quốc" }
    ];

    [Fact]
    public void Normalize_Passport_TrimsWhitespacePreservesLeadingZeros_AndFlagsPunctuationOnly()
    {
        var ok = ExcelValueNormalizer.Normalize("Passport", " 0012345 ", SampleNationalities);
        Assert.Equal("0012345", ok.Value);
        Assert.Equal(NormalizedCellStatus.AutoNormalized, ok.Status);

        var exact = ExcelValueNormalizer.Normalize("Passport", "0012345", SampleNationalities);
        Assert.Equal("0012345", exact.Value);
        Assert.Equal(NormalizedCellStatus.Valid, exact.Status);

        var bad = ExcelValueNormalizer.Normalize("Passport", "---", SampleNationalities);
        Assert.Equal(NormalizedCellStatus.NeedsReview, bad.Status);
    }

    [Fact]
    public void Normalize_Dates_StrictlyUsesDdMmYyyyAndFlagsInvalidCalendarDate()
    {
        var iso = ExcelValueNormalizer.Normalize("Birthday", "1990-06-03", SampleNationalities);
        Assert.Equal("03/06/1990", iso.Value);
        Assert.Equal(NormalizedCellStatus.AutoNormalized, iso.Status);

        var vn = ExcelValueNormalizer.Normalize("Birthday", "05/06/1990", SampleNationalities);
        Assert.Equal("05/06/1990", vn.Value);
        Assert.Equal(NormalizedCellStatus.Valid, vn.Status);

        var dot = ExcelValueNormalizer.Normalize("TemporaryStay", "3.11.2027", SampleNationalities);
        Assert.Equal("03/11/2027", dot.Value);
        Assert.Equal(NormalizedCellStatus.AutoNormalized, dot.Status);

        var invalid = ExcelValueNormalizer.Normalize("TemporaryStay", "31/02/2027", SampleNationalities);
        Assert.Equal("31/02/2027", invalid.Value);
        Assert.Equal(NormalizedCellStatus.NeedsReview, invalid.Status);
    }

    [Fact]
    public void Normalize_Gender_MapsEnglishAndNumericToNamNu_AndFlagsUnknown()
    {
        Assert.Equal(("Nam", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Gender", "Male", SampleNationalities)));
        Assert.Equal(("Nam", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Gender", "1", SampleNationalities)));
        Assert.Equal(("Nữ", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Gender", "Female", SampleNationalities)));
        Assert.Equal(("Nữ", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Gender", "0", SampleNationalities)));
        Assert.Equal(("Nữ", NormalizedCellStatus.Valid), ToPair(ExcelValueNormalizer.Normalize("Gender", "Nữ", SampleNationalities)));
        Assert.Equal(NormalizedCellStatus.NeedsReview, ExcelValueNormalizer.Normalize("Gender", "Alien", SampleNationalities).Status);
    }

    [Fact]
    public void Normalize_Nationality_MapsEnglishVietnameseAndIso2ToActiveCode()
    {
        Assert.Equal(("KOR", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Nationality", "Korea", SampleNationalities)));
        Assert.Equal(("KOR", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Nationality", "KR", SampleNationalities)));
        Assert.Equal(("KOR", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Nationality", "han quoc", SampleNationalities)));
        Assert.Equal(("JPN", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("Nationality", "Japan", SampleNationalities)));
        Assert.Equal(("USA", NormalizedCellStatus.Valid), ToPair(ExcelValueNormalizer.Normalize("Nationality", "USA", SampleNationalities)));
        Assert.Equal(NormalizedCellStatus.NeedsReview, ExcelValueNormalizer.Normalize("Nationality", "ZZZ", SampleNationalities).Status);
    }

    [Fact]
    public void Normalize_WorkPermit_CleansStatusPhrasesAndPreservesRealPermit()
    {
        Assert.Equal(("", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("WorkPermit", "Chưa có GPLĐ", SampleNationalities)));
        Assert.Equal(("", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("WorkPermit", "Không có", SampleNationalities)));
        Assert.Equal(("", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("WorkPermit", "0", SampleNationalities)));
        Assert.Equal(("Miễn", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("WorkPermit", "Miễn GPLĐ", SampleNationalities)));
        Assert.Equal(("GP-2026-01", NormalizedCellStatus.Valid), ToPair(ExcelValueNormalizer.Normalize("WorkPermit", "GP-2026-01", SampleNationalities)));
    }

    [Fact]
    public void Normalize_FamilyVisitAndV010Codes_NormalizesBooleansAndUppercasesValidCodes()
    {
        var units = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "QN-W-001" };

        Assert.Equal(("Có", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("FamilyVisit", "true", SampleNationalities)));
        Assert.Equal(("Không", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("FamilyVisit", "false", SampleNationalities)));
        Assert.Equal(("QN-W-001", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("AdministrativeUnitCode", "qn-w-001", SampleNationalities, units)));
        Assert.Equal(NormalizedCellStatus.NeedsReview, ExcelValueNormalizer.Normalize("AdministrativeUnitCode", "BAD-UNIT", SampleNationalities, units).Status);
        Assert.Equal(("WORK", NormalizedCellStatus.AutoNormalized), ToPair(ExcelValueNormalizer.Normalize("StayPurposeCode", "work", SampleNationalities, units)));
        Assert.Equal(NormalizedCellStatus.NeedsReview, ExcelValueNormalizer.Normalize("StayPurposeCode", "INVALID", SampleNationalities, units).Status);
    }

    private static (string Value, NormalizedCellStatus Status) ToPair(NormalizedCellResult r) => (r.Value, r.Status);
}
