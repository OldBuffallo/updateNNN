using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IRM.Tests;

public class LegacyGoldenWorkbookTests
{
    private sealed record Case(Dictionary<string,string> Row, int Permit, string Number, int Settlement, string? Error = null);
    private static List<Case> Cases()
    {
        var list = new List<Case>();
        void Add(string passport, string permit, int expected, string number, string settlement = "", int result = 3)
        {
            var row = LegacyImportEndToEndTests.ValidRow(); row["Passport"] = passport;
            row["WorkPermit"] = permit; row["SettlementResults"] = settlement;
            list.Add(new(row, expected, number, result));
        }
        Add("GOLD-01", "", 2, " "); Add("GOLD-02", "Miễn", 0, " ");
        Add("GOLD-03", "Khác", 6, " "); Add("GOLD-04", "GP-123", 1, "GP-123");
        Add("INV-01", "", 5, " "); Add("INV-02", "Miễn", 3, " ");
        Add("INV-03", "Khác", 6, " "); Add("INV-04", "GP-123", 4, "GP-123");
        Add("GOLD-05", "GP-123", 1, "GP-123", "GHTT: DN1 03/06/2026-29/05/2028", 0);
        Add("GOLD-06", "GP-123", 1, "GP-123", "Visa: DN1 03/06/2026-29/05/2028", 1);
        Add("GOLD-07", "GP-123", 1, "GP-123", "TTT: DN1 03/06/2026-29/05/2028", 2);
        Add("GOLD-08", "GP-123", 1, "GP-123", "DN1 03/06/2026-29/05/2028", 3);
        (string Field, string Error)[] missing = [
            ("StaffName","Lỗi đọc tên nhân viên"), ("Passport","Lỗi đọc số hộ chiếu"),
            ("Gender","Lỗi đọc giới tính"), ("Birthday","Lỗi đọc ngày sinh nhật"),
            ("Nationality","Lỗi đọc quốc tịch"), ("Address","Lỗi đọc địa chỉ"),
            ("Career","Lỗi đọc tên nghề nghiệp"), ("VisaNumber","Lỗi đọc số visa"),
            ("TemporaryStay","Lỗi đọc thời hạn tạm trú")];
        foreach (var item in missing) {
            var row = LegacyImportEndToEndTests.ValidRow(); row["Passport"] = "ERR-" + list.Count;
            row[item.Field] = ""; list.Add(new(row,0,"",0,item.Error));
        }
        (string Field, string Value, string Error)[] invalid = [
            ("Nationality","UNKNOWN","Quốc tịch UNKNOWN chưa có trong dữ liệu. Vui lòng thêm quốc tịch"),
            ("Address","12 Lê Duẩn","Địa chỉ không có phường/ xã trong lưu trữ. Vui lòng kiểm tra lại"),
            ("Career","Không có","Nghề nghiệp chưa có trong dữ liệu. Vui lòng thêm nghề nghiệp"),
            ("SettlementResults","abc","Sai định dạng kết quả giải quyết"),
            ("SettlementResults","DN1 03/06/2026-30/05/2028","Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau"),
            ("SettlementResults","DN2 03/06/2026-29/05/2028","Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau")];
        foreach (var item in invalid) {
            var row = LegacyImportEndToEndTests.ValidRow(); row["Passport"] = "ERR-" + list.Count;
            row[item.Field] = item.Value; list.Add(new(row,0,"",0,item.Error));
        }
        return list;
    }

    [Fact]
    public async Task Sqlite_GoldenWorkbook_AllLegacyColumnsAndErrors() {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        await VerifyAsync(database.Context);
    }
    [SqlServerFact]
    public async Task SqlServer_GoldenWorkbook_AllLegacyColumnsAndErrors() {
        await using var database = await IsolatedImportSqlDatabase.CreateAsync();
        await VerifyAsync(database.Context);
    }

    private static async Task VerifyAsync(IrmDbContext db)
    {
        await LegacyImportPersistenceTests.SeedAsync(db);
        for (int i = 1; i <= 4; i++) db.Investments.Add(new() { IDCompany = 1, Passport = $"INV-0{i}" });
        await db.SaveChangesAsync();
        var cases = Cases(); var trailing = new Dictionary<string,string> { ["Note"] = "Ghi chú cuối bảng, không phải hồ sơ" };
        var workbookPath = Environment.GetEnvironmentVariable("IRM_GOLDEN_WORKBOOK");
        using Stream stream = workbookPath == null
            ? LegacyImportEndToEndTests.Workbook(cases.Select(c => c.Row).Append(trailing))
            : File.OpenRead(workbookPath);
        var store = LegacyImportPersistenceTests.Store(db);
        var preview = await store.GeneratePreviewAsync(stream, LegacyImportEndToEndTests.Mappings(), 1,
            new() { StartRow = 9, CareerGroupId = 1 });
        Assert.Equal(cases.Count, preview.Count);
        for (int index = 0; index < cases.Count; index++) {
            var item = cases[index]; var parsed = preview[index];
            Assert.Equal(index + 9, parsed.RowNumber);
            if (item.Error != null) Assert.Equal(item.Error + $" tại dòng {index + 9}", parsed.ErrorMessage);
            else {
                Assert.Equal("new", parsed.Status);
                Assert.Equal(item.Permit, (int)parsed.ParsedEmployee!.WorkPermit);
                Assert.Equal(item.Number, parsed.ParsedEmployee.WorkPermitNumber);
                Assert.Equal(item.Settlement, parsed.ParsedEmployee.SettlementResults);
            }
        }
        var result = await store.ExecuteImportAsync(preview,[],1,"golden.xlsx");
        Assert.True(result.Success, string.Join(";",result.Errors));
        Assert.Equal(12, result.AddedCount); Assert.Equal(15, result.ErrorCount);
        db.ChangeTracker.Clear();
        foreach (var item in cases.Where(c => c.Error == null)) {
            var saved = await db.Employees.SingleAsync(e => e.Passport == item.Row["Passport"]);
            Assert.Equal("TEST USER", saved.StaffName); Assert.Equal(2,(int)saved.Gender);
            Assert.Equal(new DateTime(1990,6,3), saved.Birthday); Assert.Equal("CHN",saved.Nationality);
            Assert.Equal("12 Lê Duẩn, Hải Châu",saved.Address);
            Assert.Equal(item.Permit == 6 ? 14 : 1,saved.IDCareer);
            Assert.Equal(item.Permit,(int)saved.WorkPermit); Assert.Equal(item.Number,saved.WorkPermitNumber);
            Assert.Equal("DN1",saved.VisaNumber); Assert.Equal(new DateTime(2028,5,29),saved.TemporaryStay);
            Assert.Equal(item.Settlement,saved.SettlementResults);
            Assert.Equal(item.Row["SettlementResults"].Length == 0 ? " " : "DN1 03/06/2026-29/05/2028",saved.SettlementResultsString);
            Assert.Equal(item.Row["SettlementResults"].Length == 0 ? (DateTime?)null : new DateTime(2026,6,3),saved.CardCreationDate);
            Assert.Equal(" ",saved.Note); Assert.Equal(0,saved.Hidden_flag); Assert.Equal(0,saved.WorkingStatus);
            Assert.Null(saved.DateOfJoin); Assert.Null(saved.DateOfLeave);
        }
        var unchanged = await LegacyImportPersistenceTests.Preview(db,cases[0].Row);
        Assert.Equal("unchanged",unchanged[0].Status);
        var changedRow = new Dictionary<string,string>(cases[0].Row) { ["StaffName"] = "CHANGED GOLDEN" };
        var changed = await LegacyImportPersistenceTests.Preview(db,changedRow);
        Assert.Equal("changed",changed[0].Status);
        var accepted = await store.ExecuteImportAsync(changed,new() { [changed[0].ExistingEmployeeId!.Value] = "update" },1,"changed-golden.xlsx");
        Assert.True(accepted.Success); Assert.True(await store.RollbackImportAsync(accepted.SessionId));

        var export = Environment.GetEnvironmentVariable("IRM_GOLDEN_JSON");
        if (export != null && !db.Database.IsSqlServer()) {
            await File.WriteAllTextAsync(export,JsonSerializer.Serialize(new {
                Fields = LegacyImportEndToEndTests.ValidRow().Keys.Where(k => k != "__RowNumber"),
                Cases = cases.Select((c,i) => new { c.Row, c.Permit, c.Number, c.Settlement, Error = c.Error == null ? null : c.Error + $" tại dòng {i+9}", ExcelRow = i+9 })
            }));
        }
    }
}
