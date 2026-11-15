using System.Globalization;
using IRM.Application.Features.ImportExport;
using Xunit;

namespace IRM.Tests;

/// <summary>Luật import kế thừa từ WPF ReportManager (ReadFileViewModel / MethodHandler).</summary>
public class LegacyImportRulesTests
{
    private static T UnderCulture<T>(string culture, Func<T> action)
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture.Length == 0
                ? CultureInfo.InvariantCulture
                : new CultureInfo(culture);
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("vi-VN")]
    [InlineData("")]
    public void Date_DayMonthYear_IsIndependentOfServerCulture(string culture)
    {
        var ok = UnderCulture(culture, () =>
        {
            var parsed = LegacyImportRules.TryParseLegacyDate("03/06/2026", out var value);
            return (parsed, value);
        });

        Assert.True(ok.parsed);
        Assert.Equal(new DateTime(2026, 6, 3), ok.value);
    }

    [Theory]
    [InlineData("3-6-26", 2026, 6, 3)]
    [InlineData("1/1/45", 1945, 1, 1)]
    [InlineData("1/1/39", 2039, 1, 1)]
    [InlineData("1/1/40", 1940, 1, 1)]
    [InlineData("15/03/1980 (nam)", 1980, 3, 15)]
    [InlineData("  15-03-1980  ", 1980, 3, 15)]
    public void Date_AcceptsLegacyShapes(string text, int y, int m, int d)
    {
        Assert.True(LegacyImportRules.TryParseLegacyDate(text, out var value));
        Assert.Equal(new DateTime(y, m, d), value);
    }

    [Theory]
    [InlineData("31/02/2026")]
    [InlineData("2026-06-03")]
    [InlineData("06/13/2026")]
    [InlineData("01/01/1899")]
    [InlineData("01/01/0001")]
    [InlineData("01/01/2100")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Date_RejectsAnythingElse(string? text)
    {
        Assert.False(LegacyImportRules.TryParseLegacyDate(text, out _));
    }

    [Theory]
    [InlineData("01/01/1900", 1900, 1, 1)]
    [InlineData("31/12/2099", 2099, 12, 31)]
    public void Date_AcceptsLegacy1900To2099Boundaries(string text, int y, int m, int d)
    {
        Assert.True(LegacyImportRules.TryParseLegacyDate(text, out var value));
        Assert.Equal(new DateTime(y, m, d), value);
    }

    [Theory]
    [InlineData("nam", 1)]
    [InlineData("NAM ", 1)]
    [InlineData("Nam", 1)]
    [InlineData("Nữ", 2)]
    [InlineData("x", 2)]
    public void Gender_OnlyNamIsMale_LikeLegacy(string text, int expected)
    {
        Assert.Equal(expected, LegacyImportRules.ParseGender(text));
    }

    [Theory]
    [InlineData("", false, 2, " ")]
    [InlineData("   ", false, 2, " ")]
    [InlineData("", true, 5, " ")]
    [InlineData("Miễn", false, 0, " ")]
    [InlineData("MIEN GPLD", true, 3, " ")]
    [InlineData("Khác", false, 6, " ")]
    [InlineData("khac", true, 6, " ")]
    [InlineData("GP-123 ", false, 1, "GP-123")]
    [InlineData("GP-123", true, 4, "GP-123")]
    public void WorkPermit_FollowsLegacyTable(string cell, bool investor, int expected, string number)
    {
        var (type, num) = LegacyImportRules.ParseWorkPermit(cell, investor);
        Assert.Equal(expected, type);
        Assert.Equal(number, num);
    }

    [Fact]
    public void WorkPermit_NullCell_IsSameAsEmpty()
    {
        Assert.Equal((2, " "), LegacyImportRules.ParseWorkPermit(null, false));
    }

    [Theory]
    [InlineData("Đà Nẵng", "Da Nang")]
    [InlineData("đà nẵng", "da nang")]
    [InlineData("Hải Châu\u200D", "Hai Chau")]
    [InlineData("  Thân nhân ", "Than nhan")]
    public void RemoveDiacritics_HandlesVietnameseAndHiddenChars(string text, string expected)
    {
        Assert.Equal(expected, LegacyImportRules.RemoveDiacritics(text));
    }

    [Fact]
    public void SameAs13Fields_IsCaseDiacriticAndBlankInsensitiveLikeLegacy()
    {
        var existing = new IRM.Data.Models.Employee
        {
            StaffName = "Nguyễn Văn A",
            Gender = IRM.Data.Models.Gender.Male,
            Birthday = new DateTime(1990, 5, 1),
            Nationality = "CHN",
            Passport = "e123456",
            Address = "12 Lê Duẩn, Hải Châu",
            IDCareer = 1,
            WorkPermit = IRM.Data.Models.WorkPermitType.WorkerExempt,
            WorkPermitNumber = " ",
            VisaNumber = "dn1",
            TemporaryStay = new DateTime(2028, 5, 29),
            SettlementResults = 2,
            SettlementResultsString = "TTT: DN1 03/06/2026-29/05/2028",
            Note = null
        };

        var importedSame = new IRM.Data.Models.Employee
        {
            StaffName = "NGUYEN VAN A",
            Gender = IRM.Data.Models.Gender.Male,
            Birthday = new DateTime(1990, 5, 1),
            Nationality = "chn",
            Passport = "E123456",
            Address = "12 LE DUAN, HAI CHAU",
            IDCareer = 1,
            WorkPermit = IRM.Data.Models.WorkPermitType.WorkerExempt,
            WorkPermitNumber = "",
            VisaNumber = "DN1",
            TemporaryStay = new DateTime(2028, 5, 29),
            SettlementResults = 2,
            SettlementResultsString = "ttt: dn1 03/06/2026-29/05/2028",
            Note = "   "
        };

        Assert.True(LegacyImportRules.SameAs13Fields(existing, importedSame));

        importedSame.StaffName = "Nguyễn Văn B";
        Assert.False(LegacyImportRules.SameAs13Fields(existing, importedSame));
    }
}
