using System.Globalization;
using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data.Models;
using IRM.Infrastructure.Persistence.Stores.ImportExport;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public class LegacyImportEndToEndTests
{
    internal static Dictionary<string, string> ValidRow() => new()
    {
        ["StaffName"] = " TEST USER ", ["Gender"] = "Nữ", ["Birthday"] = "03/06/1990",
        ["Nationality"] = "trung quoc", ["Passport"] = "TEST-001", ["Address"] = "12 Lê Duẩn, Hải Châu",
        ["Career"] = "Kỹ sư", ["WorkPermit"] = "GP-123", ["VisaNumber"] = "DN1",
        ["TemporaryStay"] = "29/05/2028", ["SettlementResults"] = "TTT: DN1 03/06/2026-29/05/2028",
        ["__RowNumber"] = "9"
    };

    internal static List<Career> Careers() => [
        new() { IDCareer = 1, CareerName = "Kỹ sư", IDCG = 1 },
        new() { IDCareer = 14, CareerName = "Thân nhân", IDCG = 9 }];

    private static Employee Parse(Dictionary<string, string> row, bool investor = false) =>
        ImportMapping.ParseRowToEmployee(row, 1,
            [new() { NationalityCode = "CHN", NationalityName = "Trung Quốc" }], Careers(), 1,
            investor ? new HashSet<string> { "TEST-001" } : new HashSet<string>(), ["Hải Châu"]);

    [Theory]
    [InlineData("StaffName", "Lỗi đọc tên nhân viên tại dòng 9")]
    [InlineData("Passport", "Lỗi đọc số hộ chiếu tại dòng 9")]
    [InlineData("Gender", "Lỗi đọc giới tính tại dòng 9")]
    [InlineData("Birthday", "Lỗi đọc ngày sinh nhật tại dòng 9")]
    [InlineData("Nationality", "Lỗi đọc quốc tịch tại dòng 9")]
    [InlineData("Address", "Lỗi đọc địa chỉ tại dòng 9")]
    [InlineData("Career", "Lỗi đọc tên nghề nghiệp tại dòng 9")]
    [InlineData("VisaNumber", "Lỗi đọc số visa tại dòng 9")]
    [InlineData("TemporaryStay", "Lỗi đọc thời hạn tạm trú tại dòng 9")]
    public void MissingCoreField_IsAnExplicitRowError(string field, string message)
    {
        var row = ValidRow(); row[field] = " ";
        Assert.Equal(message, Assert.Throws<ImportRowException>(() => Parse(row)).Message);
    }

    [Theory]
    [InlineData("Birthday", "2026-06-03", "Lỗi đọc ngày sinh nhật tại dòng 9")]
    [InlineData("TemporaryStay", "31/02/2028", "Lỗi đọc thời hạn tạm trú tại dòng 9")]
    [InlineData("Nationality", "UNKNOWN", "Quốc tịch UNKNOWN chưa có trong dữ liệu. Vui lòng thêm quốc tịch tại dòng 9")]
    [InlineData("Address", "12 Lê Duẩn", "Địa chỉ không có phường/ xã trong lưu trữ. Vui lòng kiểm tra lại tại dòng 9")]
    [InlineData("Career", "Không có", "Nghề nghiệp chưa có trong dữ liệu. Vui lòng thêm nghề nghiệp tại dòng 9")]
    [InlineData("SettlementResults", "abc", "Sai định dạng kết quả giải quyết tại dòng 9")]
    [InlineData("SettlementResults", "DN1 03/06/2026-30/05/2028", "Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau tại dòng 9")]
    [InlineData("SettlementResults", "DN2 03/06/2026-29/05/2028", "Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau tại dòng 9")]
    public void InvalidCoreField_IsRejected(string field, string value, string message)
    {
        var row = ValidRow(); row[field] = value;
        Assert.Equal(message, Assert.Throws<ImportRowException>(() => Parse(row)).Message);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("")]
    public void ValidCoreRow_UsesLegacyValuesIndependentOfCulture(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var employee = Parse(ValidRow(), investor: true);
            Assert.Equal("TEST USER", employee.StaffName);
            Assert.Equal(2, (int)employee.Gender);
            Assert.Equal("Nữ", employee.GenderString);
            Assert.Equal(new DateTime(1990, 6, 3), employee.Birthday);
            Assert.Equal("CHN", employee.Nationality);
            Assert.Equal(4, (int)employee.WorkPermit);
            Assert.Equal("GP-123", employee.WorkPermitNumber);
            Assert.Equal(2, employee.SettlementResults);
            Assert.Equal(new DateTime(2026, 6, 3), employee.CardCreationDate);
            Assert.Equal(0, employee.Hidden_flag);
            Assert.Equal(0, employee.WorkingStatus);
            Assert.Null(employee.DateOfJoin);
            Assert.Null(employee.DateOfLeave);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void RelativePermit_UsesRelativeCareer_AndBlankPermitMeansNotYet()
    {
        var row = ValidRow(); row["WorkPermit"] = "Khác";
        var employee = Parse(row);
        Assert.Equal(6, (int)employee.WorkPermit);
        Assert.Equal(14, employee.IDCareer);
        row["WorkPermit"] = "";
        Assert.Equal(2, (int)Parse(row).WorkPermit);
        row["Career"] = "Thân nhân";
        employee = ImportMapping.ParseRowToEmployee(row, 1,
            [new() { NationalityCode = "CHN", NationalityName = "Trung Quốc" }], Careers(), 9,
            new HashSet<string>(), ["Hải Châu"]);
        Assert.Equal(6, (int)employee.WorkPermit);
        Assert.Equal("", employee.WorkPermitNumber);
    }

    internal static MemoryStream Workbook(IEnumerable<Dictionary<string, string>> rows, int startRow = 9)
    {
        using var book = new XLWorkbook(); var sheet = book.AddWorksheet("WPF");
        var fields = ValidRow().Keys.Where(k => k != "__RowNumber").ToArray();
        for (int c = 0; c < fields.Length; c++) sheet.Cell(startRow - 1, c + 1).Value = fields[c];
        foreach (var row in rows)
        {
            for (int c = 0; c < fields.Length; c++) sheet.Cell(startRow, c + 1).Value = row.GetValueOrDefault(fields[c], "");
            startRow++;
        }
        var stream = new MemoryStream(); book.SaveAs(stream); stream.Position = 0; return stream;
    }

    internal static List<ColumnMapping> Mappings() => ValidRow().Keys.Where(k => k != "__RowNumber")
        .Select((k, i) => new ColumnMapping { ExcelColumnIndex = i, SystemField = k }).ToList();

    [Fact]
    public async Task Preview_KeepsInvalidAndBlankRowsWithRealNumbers_NoDefaultCareer()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        database.Context.Wards.Add(new() { WardName = "Hải Châu" }); await database.Context.SaveChangesAsync();
        var bad = ValidRow(); bad["Career"] = "Không có";
        using var stream = Workbook([ValidRow(), new(), bad]);
        var store = new SqlImportStore(database.Context, new SqlAuditStore(database.Context));
        var preview = await store.GeneratePreviewAsync(stream, Mappings(), 1,
            new ImportReadOptions { CareerGroupId = 1, StartRow = 9, EndRow = 11 });
        Assert.Equal(3, preview.Count);
        Assert.Equal("new", preview[0].Status);
        Assert.Equal("error", preview[1].Status);
        Assert.Equal("Lỗi đọc tên nhân viên tại dòng 10", preview[1].ErrorMessage);
        Assert.Equal("error", preview[2].Status);
        Assert.Contains("Nghề nghiệp chưa có", preview[2].ErrorMessage);
        Assert.Equal(11, preview[2].RowNumber);
    }
}
