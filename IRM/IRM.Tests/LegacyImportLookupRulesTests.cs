using IRM.Application.Features.ImportExport;
using IRM.Data.Models;
using Xunit;

namespace IRM.Tests;

/// <summary>Task 2: kết quả giải quyết, đuôi địa chỉ, tra nghề theo nhóm (luật WPF).</summary>
public class LegacyImportLookupRulesTests
{
    // 5 chuỗi thật lấy từ ReportManagerDB.Employees (SettlementResults = 2, TTT)
    [Theory]
    [InlineData("DN2601428LĐ2 03/06/2026-29/05/2028", "DN2601428LĐ2", 2026, 6, 3, 2028, 5, 29)]
    [InlineData("DN2601427TT 03/06/2026-29/05/2028", "DN2601427TT", 2026, 6, 3, 2028, 5, 29)]
    [InlineData("DN2601421LĐ2 03/06/2026-30/04/2028", "DN2601421LĐ2", 2026, 6, 3, 2028, 4, 30)]
    [InlineData("DN2601415LĐ2 01/06/2026-19/05/2028", "DN2601415LĐ2", 2026, 6, 1, 2028, 5, 19)]
    [InlineData("DN2601382LĐ2 28/05/2026-28/05/2028", "DN2601382LĐ2", 2026, 5, 28, 2028, 5, 28)]
    public void Settlement_RealLegacyStrings_AreAcceptedWithTTTPrefix(
        string text, string visa, int cy, int cm, int cd, int ty, int tm, int td)
    {
        var parsed = LegacyImportRules.ParseSettlement("TTT: " + text, visa, new DateTime(ty, tm, td));

        Assert.Null(parsed.Error);
        Assert.Equal(2, parsed.Result);
        Assert.Equal(text, parsed.Text);
        Assert.Equal(new DateTime(cy, cm, cd), parsed.CardCreationDate);
    }

    [Fact]
    public void Settlement_TemporaryStayOffByOneDay_IsAnError()
    {
        var parsed = LegacyImportRules.ParseSettlement(
            "TTT: DN2601428LĐ2 03/06/2026-29/05/2028", "DN2601428LĐ2", new DateTime(2028, 5, 28));

        Assert.NotNull(parsed.Error);
        Assert.Contains("Thời hạn tạm trú", parsed.Error);
    }

    [Fact]
    public void Settlement_VisaNumberMismatch_IsAnError_CaseInsensitive()
    {
        var same = LegacyImportRules.ParseSettlement(
            "TTT: dn2601428lĐ2 03/06/2026-29/05/2028", "DN2601428LĐ2", new DateTime(2028, 5, 29));
        Assert.Null(same.Error);

        var other = LegacyImportRules.ParseSettlement(
            "TTT: DN9999999 03/06/2026-29/05/2028", "DN2601428LĐ2", new DateTime(2028, 5, 29));
        Assert.NotNull(other.Error);
        Assert.Contains("Thẻ tạm trú", other.Error);
    }

    [Theory]
    [InlineData("GHTT: DN1 01/01/2026-01/01/2028", 0)]
    [InlineData("ghtt:DN1 01/01/2026-01/01/2028", 0)]
    [InlineData("Visa: DN1 01/01/2026-01/01/2028", 1)]
    [InlineData("VISA:DN1 01/01/2026-01/01/2028", 1)]
    [InlineData("TTT: DN1 01/01/2026-01/01/2028", 2)]
    public void Settlement_PrefixSelectsResultAndIsStripped(string cell, int expected)
    {
        var parsed = LegacyImportRules.ParseSettlement(cell, "DN1", new DateTime(2028, 1, 1));

        Assert.Equal(expected, parsed.Result);
        Assert.Equal("DN1 01/01/2026-01/01/2028", parsed.Text);
        Assert.Null(parsed.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Settlement_Empty_IsOtherWithSingleSpace(string? cell)
    {
        var parsed = LegacyImportRules.ParseSettlement(cell, "ANY", new DateTime(2030, 1, 1));

        Assert.Equal(3, parsed.Result);
        Assert.Equal(" ", parsed.Text);
        Assert.Null(parsed.CardCreationDate);
        Assert.Null(parsed.Error);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("TTT: abc")]
    [InlineData("TTT:")]
    [InlineData("TTT: DN1 01/01/2026")]
    [InlineData("TTT: DN1 2026-01-01-2028-01-01")]
    [InlineData("TTT: DN1 31/02/2026-01/01/2028")]
    public void Settlement_BadFormat_IsRejected(string cell)
    {
        var parsed = LegacyImportRules.ParseSettlement(cell, "DN1", new DateTime(2028, 1, 1));

        Assert.NotNull(parsed.Error);
        Assert.Contains("Sai định dạng", parsed.Error);
    }

    [Fact]
    public void Settlement_UnprefixedText_IsOtherAndStillValidated()
    {
        var ok = LegacyImportRules.ParseSettlement("DN1 01/01/2026-01/01/2028", "DN1", new DateTime(2028, 1, 1));
        Assert.Equal(3, ok.Result);
        Assert.Null(ok.Error);

        var bad = LegacyImportRules.ParseSettlement("abc", "DN1", new DateTime(2028, 1, 1));
        Assert.Equal(3, bad.Result);
        Assert.NotNull(bad.Error);
    }

    [Fact]
    public void Settlement_DatesOnly_HasNoVisaToCompare()
    {
        var parsed = LegacyImportRules.ParseSettlement(
            "TTT: 01/01/2026-01/01/2028", "WHATEVER", new DateTime(2028, 1, 1));

        Assert.Null(parsed.Error);
        Assert.Equal(new DateTime(2026, 1, 1), parsed.CardCreationDate);
    }

    [Fact]
    public void Settlement_SixPartDashedDates_AreAccepted()
    {
        var parsed = LegacyImportRules.ParseSettlement(
            "TTT: DN1 01-02-2026-03-04-2028", "DN1", new DateTime(2028, 4, 3));

        Assert.Null(parsed.Error);
        Assert.Equal(new DateTime(2026, 2, 1), parsed.CardCreationDate);
    }

    [Fact]
    public void Settlement_ThreeTokens_TakesMiddleTokenAsVisa_LikeLegacy()
    {
        var ok = LegacyImportRules.ParseSettlement(
            "TTT: A B 01/01/2026-01/01/2028", "B", new DateTime(2028, 1, 1));
        Assert.Null(ok.Error);

        var bad = LegacyImportRules.ParseSettlement(
            "TTT: A B 01/01/2026-01/01/2028", "A", new DateTime(2028, 1, 1));
        Assert.NotNull(bad.Error);
    }

    [Theory]
    [InlineData("12 Lê Duẩn, Hải Châu", true)]
    [InlineData("12 Lê Duẩn, HẢI CHÂU", true)]
    [InlineData("12 Le Duan, hai chau  ", true)]
    [InlineData("12 Lê Duẩn, Hải Châu\u200D", true)]
    [InlineData("12 Lê Duẩn, Thanh Khê", false)]
    [InlineData("Hải Châu, 12 Lê Duẩn", false)]
    [InlineData("", false)]
    public void Address_MustEndWithAKnownWard(string address, bool expected)
    {
        var wards = new[] { "Hải Châu", "Sơn Trà\u200D" };

        Assert.Equal(expected, LegacyImportRules.AddressEndsWithWard(address, wards));
    }

    [Fact]
    public void Address_WardWithHiddenCharacter_StillMatches()
    {
        Assert.True(LegacyImportRules.AddressEndsWithWard("5 Ngô Quyền, Sơn Trà", new[] { "Sơn Trà\u200D" }));
    }

    [Fact]
    public void Address_BlankWardEntries_AreIgnored()
    {
        Assert.False(LegacyImportRules.AddressEndsWithWard("5 Ngô Quyền, Sơn Trà", new[] { "", "  ", "\u200D" }));
    }

    private static List<Career> Careers() => new()
    {
        new Career { IDCareer = 4, CareerName = "Giám đốc", IDCG = 1 },
        new Career { IDCareer = 23, CareerName = "Giám đốc", IDCG = 4 },
        new Career { IDCareer = 3038, CareerName = "Giám đốc", IDCG = 7 },
        new Career { IDCareer = 3044, CareerName = "Giám đốc", IDCG = 2 },
        new Career { IDCareer = 14, CareerName = "Thân nhân", IDCG = 9 },
        new Career { IDCareer = 99, CareerName = "Kế toán", IDCG = 7, Delete_flag = 1 },
    };

    [Theory]
    [InlineData("Giám đốc", 7, 3038)]
    [InlineData("giám đốc", 1, 4)]
    [InlineData("  Giám đốc  ", 4, 23)]
    [InlineData("Thân nhân", 9, 14)]
    public void Career_IsResolvedInsideTheChosenGroup(string name, int group, int expectedId)
    {
        Assert.Equal(expectedId, LegacyImportRules.FindCareerId(name, group, Careers()));
    }

    [Theory]
    [InlineData("Giám đốc", 6)]
    [InlineData("Kế toán", 7)]
    [InlineData("Giam doc", 7)]
    [InlineData("", 7)]
    public void Career_NotInGroupOrDeleted_IsNull(string name, int group)
    {
        Assert.Null(LegacyImportRules.FindCareerId(name, group, Careers()));
    }

    [Fact]
    public void RelativeCareer_IsRecognisedByNameInOtherGroup()
    {
        var careers = Careers();

        Assert.True(LegacyImportRules.IsRelativeCareer(careers.Single(c => c.IDCareer == 14)));
        Assert.True(LegacyImportRules.IsRelativeCareer(new Career
        {
            IDCareer = 11,
            CareerName = "Thân nhân",
            IDCG = 5,
            CareerGroup = new CareerGroup { IDCG = 5, CareerGroupName = "Khác" }
        }));
        Assert.False(LegacyImportRules.IsRelativeCareer(careers.Single(c => c.IDCareer == 3038)));
        Assert.False(LegacyImportRules.IsRelativeCareer(
            new Career { IDCareer = 1, CareerName = "Thân nhân", IDCG = 7 }));
    }

    [Fact]
    public void ResolveNationality_HandlesThreeLetterCodesAndErrorsWithRowNumber()
    {
        List<NationalityEntity> catalog = [new() { NationalityCode = "KOR", NationalityName = "Hàn Quốc" }];

        Assert.Equal("KOR", LegacyImportRules.ResolveNationality("kor", catalog, 9));
        Assert.Equal("KOR", LegacyImportRules.ResolveNationality("kr", catalog, 9));
        Assert.Equal("KOR", LegacyImportRules.ResolveNationality("Han Quoc", catalog, 9));

        var missing = Assert.Throws<ImportRowException>(() => LegacyImportRules.ResolveNationality(" ", catalog, 9));
        Assert.Equal("Lỗi đọc quốc tịch tại dòng 9", missing.Message);

        var unknown = Assert.Throws<ImportRowException>(() => LegacyImportRules.ResolveNationality("ZZZ", catalog, 9));
        Assert.Equal("Quốc tịch ZZZ chưa có trong dữ liệu. Vui lòng thêm quốc tịch tại dòng 9", unknown.Message);
    }
}
