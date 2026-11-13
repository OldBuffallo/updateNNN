using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public class ImportSampleWorkbookTests
{
    private static readonly string[] ExpectedKeys = [
        "StaffName", "Passport", "Gender", "Birthday", "Nationality", "Address", "Career", "VisaNumber", "TemporaryStay",
        "WorkPermit", "WorkPermitNumber", "VisaSymbol", "Note", "SettlementResults", "CardCreationDate",
        "FamilyVisit", "FamilyVisitRelativeName", "FamilyVisitRelationship", "FamilyVisitRelativeIdCard",
        "FamilyVisitStartDate", "FamilyVisitEndDate", "FamilyVisitNote", "StayPurposeCode", "StayValidFrom", "StayValidTo",
        "ElectronicIdentityNumber", "DocumentTypeCode", "DocumentNumber", "DocumentValidFrom", "DocumentValidTo",
        "AdministrativeUnitCode", "ResidenceAddress", "ResidenceValidFrom", "ResidenceValidTo", "CompanyName"
    ];

    [Fact]
    public async Task Sample_TargetCatalogsAreNotExposedToAnonymousCallers()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var store = new SqlExportStore(new SharedConnectionFactory(db.Context));
        var service = new ExportApplicationService(store, TestSecurityContext.CreateAnonymousGuard());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GenerateSampleExcelBytesAsync());
        var guardedStore = new SqlExportStore(new SharedConnectionFactory(db.Context), new SqlAuditStore(db.Context), TestSecurityContext.CreateAnonymousGuard());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => guardedStore.GenerateSampleExcelBytesAsync());
    }

    [Fact]
    public async Task Sample_IsBlankAndAll35HeadersRoundTripWithoutFieldCollisions()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        using var stream = new MemoryStream(await Generate(db));
        using var book = new XLWorkbook(stream);
        var sheet = book.Worksheet(1);
        Assert.Equal(35, sheet.LastColumnUsed()!.ColumnNumber());
        Assert.Equal("Hồ sơ", sheet.Name);
        Assert.Equal(1, sheet.LastRowUsed()!.RowNumber());
        var headers = ExcelReaderHelper.ReadHeaders(stream);
        Assert.Equal(35, headers.Select(h => h.HeaderName).Distinct().Count());
        Assert.Equal(ExpectedKeys, ImportMapping.AutoMapColumns(headers).Select(m => m.SystemField));
        Assert.Empty(ExcelReaderHelper.ReadAllRows(stream, ImportMapping.AutoMapColumns(headers)));
        Assert.All(book.Worksheets.SelectMany(s => s.CellsUsed()), cell => Assert.False(cell.HasFormula));
        foreach (var key in new[] { "Passport", "VisaNumber", "WorkPermitNumber", "ElectronicIdentityNumber", "FamilyVisitRelativeIdCard", "AdministrativeUnitCode", "DocumentNumber" })
            Assert.Equal("@", sheet.Column(Array.IndexOf(ExpectedKeys, key) + 1).Style.NumberFormat.Format);
    }

    [Fact]
    public async Task Sample_RealReaderAndParserAcceptRequiredFieldsWithLeadingZeroDocuments()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        db.Context.Wards.Add(new Ward { WardName = "Phường Minh Họa" }); await db.Context.SaveChangesAsync();
        using var sample = new MemoryStream(await Generate(db)); using var book = new XLWorkbook(sample);
        var values = new[] { "TEST USER", "00012345", "Nam", "03/06/1990", "USA", "12 Đường Mẫu, Phường Minh Họa", "Kỹ sư", "00098765", "30/12/2027" };
        for (var i = 0; i < values.Length; i++) book.Worksheet(1).Cell(2, i + 1).Value = values[i];
        using var input = new MemoryStream(); book.SaveAs(input);
        var mappings = ImportMapping.AutoMapColumns(ExcelReaderHelper.ReadHeaders(input));
        var row = Assert.Single(ExcelReaderHelper.ReadAllRows(input, mappings));
        var employee = ImportMapping.ParseRowToEmployee(row, 1,
            await db.Context.Nationality.ToListAsync(), await db.Context.Careers.ToListAsync(), 1,
            new HashSet<string>(), ["Phường Minh Họa"]);
        Assert.Equal("00012345", employee.Passport); Assert.Equal("00098765", employee.VisaNumber);
        Assert.Equal(new DateTime(1990, 6, 3), employee.Birthday);
        Assert.Equal("TEST USER", employee.StaffName);
    }

    [Fact]
    public void ExactLabels_MapCorrectlyWhenOptionalColumnsAreReorderedOrRemoved()
    {
        var keys = new[] { "DocumentValidTo", "StayValidFrom", "StayValidTo", "DocumentValidFrom", "VisaSymbol", "VisaNumber", "ResidenceAddress", "Address", "WorkPermitNumber", "WorkPermit" };
        var headers = keys.Select((key, index) => new ExcelColumnInfo { Index = index, HeaderName = SystemFields.All[key] }).ToList();
        Assert.Equal(keys, ImportMapping.AutoMapColumns(headers).Select(m => m.SystemField));
        headers.Add(new ExcelColumnInfo { Index = keys.Length, HeaderName = SystemFields.All["VisaSymbol"] });
        Assert.DoesNotContain(ImportMapping.AutoMapColumns(headers).GroupBy(m => m.SystemField), group => group.Key != "skip" && group.Count() > 1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExactVisaHeaderKeepsItsFieldWhenAnAuxiliaryColumnAlsoContainsVisa(bool canonicalFirst)
    {
        var labels = canonicalFirst ? new[] { "Số Visa", "Số Visa cũ" } : new[] { "Số Visa cũ", "Số Visa" };
        var headers = labels.Select((label, index) => new ExcelColumnInfo { Index = index, HeaderName = label }).ToList();
        var expected = canonicalFirst ? new[] { "VisaNumber", "skip" } : new[] { "skip", "VisaNumber" };
        Assert.Equal(expected, ImportMapping.AutoMapColumns(headers).Select(mapping => mapping.SystemField));
    }

    [Fact]
    public void DuplicateCanonicalVisaHeaderDoesNotGiveItsFieldToAnEarlierAuxiliaryColumn()
    {
        var headers = new[] { "Số Visa cũ", "Số Visa", "Số Visa" }
            .Select((label, index) => new ExcelColumnInfo { Index = index, HeaderName = label }).ToList();
        Assert.Equal(new[] { "skip", "VisaNumber", "skip" }, ImportMapping.AutoMapColumns(headers).Select(mapping => mapping.SystemField));
    }

    [Fact]
    public async Task Sample_GuideCoversAllFieldsAndWarnsAboutBlankUpdates()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        using var stream = new MemoryStream(await Generate(db)); using var book = new XLWorkbook(stream);
        var guide = book.Worksheet("Hướng dẫn");
        var labels = guide.RowsUsed().Select(row => row.Cell(1).GetString()).ToList();
        foreach (var key in ExpectedKeys) Assert.Contains(SystemFields.All[key], labels);
        var text = string.Join('\n', guide.CellsUsed().Select(cell => cell.GetString()));
        Assert.Contains("không ghép", text); Assert.Contains("ô trống", text);
        Assert.Contains("cập nhật", text); Assert.DoesNotContain("Mặc định: Nữ nếu để trống", text);
        Assert.Contains("Nam hoặc Nữ", text);
    }

    [Fact]
    public async Task Sample_ReferencesUseActualActiveCatalogsAndSeparateAddressCatalogs()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        db.Context.Nationality.AddRange(new NationalityEntity { NationalityCode = "D", NationalityName = "Đức thực tế" }, new NationalityEntity { NationalityCode = "HIDDEN", NationalityName = "HIDDEN-NATIONALITY", Delete_flag = 1 });
        db.Context.Careers.Add(new Career { CareerName = "HIDDEN-CAREER", IDCG = 1, Delete_flag = 1 });
        db.Context.Wards.AddRange(new Ward { WardName = "Phường lõi thực tế" }, new Ward { WardName = "HIDDEN-WARD", Delete_flag = 1 });
        db.Context.Companies.Add(new Company { CompanyName = "HIDDEN-COMPANY", Delete_flag = 1, IDField = 1, TrackerID = 1 });
        db.Context.AdministrativeUnits.Add(new AdministrativeUnit { Code = "HIDDEN-UNIT", Name = "HIDDEN-UNIT", IsDeleted = true });
        await db.Context.SaveChangesAsync();
        using var stream = new MemoryStream(await Generate(db)); using var book = new XLWorkbook(stream);
        var reference = book.Worksheet("Danh mục tham chiếu");
        var rows = reference.RowsUsed().Select(row => row.Cells(1, 4).Select(cell => cell.GetString()).ToArray()).ToList();
        Assert.Contains(rows, row => row[0] == "Quốc tịch" && row[1] == "D" && row[2] == "Đức thực tế");
        Assert.Contains(rows, row => row[0] == "Phường/xã cho địa chỉ lõi" && row[2] == "Phường lõi thực tế");
        Assert.Contains(rows, row => row[0] == "Mã địa bàn cư trú" && row[1] == "QN-W-DEFAULT");
        Assert.Contains(rows, row => row[0] == "Nghề nghiệp" && row[2] == "Kỹ sư" && row[3].Contains("Kỹ thuật"));
        Assert.DoesNotContain(reference.CellsUsed(), cell => cell.GetString().Contains("HIDDEN"));
        Assert.Contains(rows, row => row[1] == "BORDER_VISA_EXEMPT");
        Assert.Contains(rows, row => row[1] == "VISA_EXTENSION");
    }

    [Fact]
    public async Task Sample_EmptyCatalogsAreExplicitRatherThanFilledWithInventedValues()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync(seedBaseline: false);
        using var stream = new MemoryStream(await Generate(db)); using var book = new XLWorkbook(stream);
        var text = string.Join('\n', book.Worksheet("Danh mục tham chiếu").CellsUsed().Select(cell => cell.GetString()));
        Assert.Contains("Chưa có dữ liệu", text); Assert.DoesNotContain("USA", text); Assert.DoesNotContain("Kỹ sư", text);
    }

    private static Task<byte[]> Generate(TestDatabase db) => new SqlExportStore(new SharedConnectionFactory(db.Context)).GenerateSampleExcelBytesAsync();
    private sealed class SharedConnectionFactory(IrmDbContext context) : IDbContextFactory<IrmDbContext>
    {
        public IrmDbContext CreateDbContext() => new(new DbContextOptionsBuilder<IrmDbContext>().UseSqlite(context.Database.GetDbConnection()).Options);
        public Task<IrmDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}
