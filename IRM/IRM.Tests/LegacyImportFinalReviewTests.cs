using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public class LegacyImportFinalReviewTests
{
    private static async Task<TestDatabase> Database()
    {
        var database = new TestDatabase(); await database.InitializeAsync();
        await LegacyImportPersistenceTests.SeedAsync(database.Context); return database;
    }
    private static Task<List<ImportPreviewRow>> Preview(IrmDbContext db, Dictionary<string,string> row) =>
        LegacyImportPersistenceTests.Preview(db,row);
    private static async Task<List<ImportPreviewRow>> WithFields(IrmDbContext db, Dictionary<string,string> row)
    {
        using var book = new XLWorkbook(); var sheet = book.AddWorksheet("Data");
        var mappings = new List<ColumnMapping>(); int index = 0;
        foreach (var item in row.Where(x => x.Key != "__RowNumber")) {
            sheet.Cell(8,index+1).Value=item.Key; sheet.Cell(9,index+1).Value=item.Value;
            mappings.Add(new() { ExcelColumnIndex=index++, SystemField=item.Key });
        }
        using var stream = new MemoryStream(); book.SaveAs(stream);
        return await LegacyImportPersistenceTests.Store(db).GeneratePreviewAsync(stream,mappings,1,new() { StartRow=9, CareerGroupId=1 });
    }
    private static async Task<ImportResult> Save(IrmDbContext db, List<ImportPreviewRow> rows)
    {
        var choices = rows.Where(r=>r.ExistingEmployeeId.HasValue).ToDictionary(r=>r.ExistingEmployeeId!.Value,_=>"update");
        var result=await LegacyImportPersistenceTests.Store(db).ExecuteImportAsync(rows,choices,1,"review.xlsx");
        Assert.True(result.Success,string.Join(";",result.Errors));return result;
    }

    [Fact]
    public async Task Rollback_RequiresReverseOrder_AndPreservesLaterVersions()
    {
        await using var database=await Database(); await VerifyRollbackChain(database.Context);
    }
    [SqlServerFact]
    public async Task SqlServer_Rollback_RequiresReverseOrder()
    {
        await using var database=await IsolatedImportSqlDatabase.CreateAsync();
        await LegacyImportPersistenceTests.SeedAsync(database.Context); await VerifyRollbackChain(database.Context);
    }
    private static async Task VerifyRollbackChain(IrmDbContext db)
    {
        var row=LegacyImportEndToEndTests.ValidRow(); await Save(db,await Preview(db,row));
        row["StaffName"]="Version A";var a=await Save(db,await Preview(db,row));
        row["StaffName"]="Version B";var b=await Save(db,await Preview(db,row));
        var store=LegacyImportPersistenceTests.Store(db);
        Assert.False(await store.RollbackImportAsync(a.SessionId));
        db.ChangeTracker.Clear();
        Assert.Equal("Version B",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
        Assert.True(await store.RollbackImportAsync(b.SessionId));
        Assert.True(await store.RollbackImportAsync(a.SessionId));
        Assert.Equal("TEST USER",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
    }

    [Fact]
    public async Task Rollback_RejectsLaterManualEdit()
    {
        await using var database=await Database(); var db=database.Context;
        var result=await Save(db,await Preview(db,LegacyImportEndToEndTests.ValidRow()));
        (await db.Employees.SingleAsync()).StaffName="Newer manual edit"; await db.SaveChangesAsync();
        Assert.False(await LegacyImportPersistenceTests.Store(db).RollbackImportAsync(result.SessionId));
        db.ChangeTracker.Clear(); Assert.Equal("Newer manual edit",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
    }

    [Fact]
    public async Task Rollback_DoesNotDeleteACreatedCompanyUsedByLaterRecords()
    {
        await using var database=await Database();var db=database.Context;
        var rows=await Preview(db,LegacyImportEndToEndTests.ValidRow());
        rows[0].ExtendedFields["__AutoCreateCompanyName"]="Created company";
        rows[0].ExtendedFields["__AllowCreateCompany"]="true";rows[0].ResolvedCompanyId=0;
        var created=await Save(db,rows);
        var company=await db.Companies.SingleAsync(c=>c.CompanyName=="Created company");
        db.Employees.Add(new() { StaffName="Later manual record",IDCompany=company.IDCompany,IDCareer=1,Nationality="CHN" });await db.SaveChangesAsync();
        Assert.False(await LegacyImportPersistenceTests.Store(db).RollbackImportAsync(created.SessionId));
        db.ChangeTracker.Clear();Assert.Equal(0,(await db.Companies.FindAsync(company.IDCompany))!.Delete_flag);
        Assert.Equal(2,await db.Employees.CountAsync(e=>e.IDCompany==company.IDCompany && e.Hidden_flag==0));
    }

    [Fact]
    public async Task FailedTransaction_DoesNotLeakTrackerChanges_AndSameServiceCanRetry()
    {
        await using var database=await Database();var db=database.Context;var store=LegacyImportPersistenceTests.Store(db);
        var row=LegacyImportEndToEndTests.ValidRow();await Save(db,await Preview(db,row));
        row["StaffName"]="Version A";var first=await Preview(db,row);
        var secondRow=LegacyImportEndToEndTests.ValidRow();secondRow["Passport"]="FAIL-LATE";
        var second=await Preview(db,secondRow);second[0].ExtendedFields["StayPurposeCode"]="INVALID";
        var result=await store.ExecuteImportAsync([first[0],second[0]],new() { [first[0].ExistingEmployeeId!.Value]="update" },1,"late-failure.xlsx");
        Assert.False(result.Success);
        Assert.Equal(0,(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).Hidden_flag);
        await db.SaveChangesAsync(); // A later operation on the same scoped context must be harmless.
        db.ChangeTracker.Clear(); Assert.Single(await db.Employees.ToListAsync());
        Assert.Equal("TEST USER",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
        Assert.Single(await db.ImportHistories.ToListAsync());
        var again=await Preview(db,row);
        var retry=await store.ExecuteImportAsync(again,new() { [again[0].ExistingEmployeeId!.Value]="update" },1,"retry.xlsx");
        Assert.True(retry.Success,string.Join(";",retry.Errors));Assert.Equal(1,retry.UpdatedCount);
    }

    [Fact]
    public async Task FailedRollback_DoesNotDeleteTheVersionOnALaterSave()
    {
        await using var database=await Database();var db=database.Context;
        var row=LegacyImportEndToEndTests.ValidRow();await Save(db,await Preview(db,row));
        row["StaffName"]="Version A";var update=await Save(db,await Preview(db,row));
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER fail_delete BEFORE DELETE ON Employees BEGIN SELECT RAISE(ABORT, 'blocked rollback'); END;");
        Assert.False(await LegacyImportPersistenceTests.Store(db).RollbackImportAsync(update.SessionId));
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER fail_delete;");
        await db.SaveChangesAsync();db.ChangeTracker.Clear();
        Assert.Equal(2,await db.Employees.CountAsync());
        Assert.Equal("Version A",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
    }

    [Fact]
    public async Task StalePreview_RejectsAnInPlaceEditToTheSameEmployee()
    {
        await using var database=await Database();var db=database.Context;
        var row=LegacyImportEndToEndTests.ValidRow();await Save(db,await Preview(db,row));
        row["StaffName"]="Incoming";var preview=await Preview(db,row);
        (await db.Employees.SingleAsync()).StaffName="Other user edit";await db.SaveChangesAsync();
        var result=await LegacyImportPersistenceTests.Store(db).ExecuteImportAsync(preview,new() { [preview[0].ExistingEmployeeId!.Value]="update" },1,"stale.xlsx");
        Assert.False(result.Success);db.ChangeTracker.Clear();
        Assert.Equal("Other user edit",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
    }

    [SqlServerFact]
    public async Task SqlServer_StalePreviewAndRollback_ReadFreshStateFromAnotherContext()
    {
        await using var database=await IsolatedImportSqlDatabase.CreateAsync();var db=database.Context;
        await LegacyImportPersistenceTests.SeedAsync(db);
        var row=LegacyImportEndToEndTests.ValidRow();var original=await Save(db,await Preview(db,row));
        row["StaffName"]="Incoming";var preview=await Preview(db,row);
        await using(var other=new IrmDbContext(new DbContextOptionsBuilder<IrmDbContext>().UseSqlServer(db.Database.GetConnectionString(),sql=>sql.UseCompatibilityLevel(120)).Options))
        { (await other.Employees.SingleAsync()).StaffName="Other context";await other.SaveChangesAsync(); }
        var store=LegacyImportPersistenceTests.Store(db);
        var result=await store.ExecuteImportAsync(preview,new() { [preview[0].ExistingEmployeeId!.Value]="update" },1,"other-context.xlsx");
        Assert.False(result.Success);Assert.False(await store.RollbackImportAsync(original.SessionId));
        db.ChangeTracker.Clear();Assert.Equal("Other context",(await db.Employees.SingleAsync(e=>e.Hidden_flag==0)).StaffName);
    }

    [Theory]
    [InlineData("VisaSymbol","LD2")]
    [InlineData("FamilyVisitRelativeName","NEW RELATIVE")]
    [InlineData("CardCreationDate","01/01/2026")]
    [InlineData("DocumentNumber","NEW-DOC")]
    public async Task ExtensionOnlyChange_IsPreviewedSavedAndRolledBack(string field,string value)
    {
        await using var database=await Database();var db=database.Context;
        var row=LegacyImportEndToEndTests.ValidRow();row["SettlementResults"]="";
        await Save(db,await WithFields(db,row));row[field]=value;
        var preview=await WithFields(db,row);Assert.Equal("changed",preview[0].Status);
        var update=await Save(db,preview);db.ChangeTracker.Clear();
        var saved=await db.Employees.SingleAsync(e=>e.Hidden_flag==0);
        if(field=="VisaSymbol") Assert.Equal(value,saved.VisaSymbol);
        if(field=="FamilyVisitRelativeName") Assert.Equal(value,saved.FamilyVisitRelativeName);
        if(field=="CardCreationDate") Assert.Equal(new DateTime(2026,1,1),saved.CardCreationDate);
        if(field=="DocumentNumber") Assert.Equal(value,(await db.ImmigrationDocuments.SingleAsync()).Number);
        Assert.Equal("unchanged",(await WithFields(db,row))[0].Status);
        Assert.True(await LegacyImportPersistenceTests.Store(db).RollbackImportAsync(update.SessionId));
        if(field=="DocumentNumber") Assert.Empty(await db.ImmigrationDocuments.ToListAsync());
    }

    [Fact]
    public async Task ExistingDocumentWithConflictingDates_IsNotSilentlyAccepted()
    {
        await using var database=await Database();var db=database.Context;
        var row=LegacyImportEndToEndTests.ValidRow();row["DocumentNumber"]="DOC-1";row["DocumentValidTo"]="01/01/2028";
        await Save(db,await WithFields(db,row));row["DocumentValidTo"]="02/01/2028";
        var preview=await WithFields(db,row);Assert.Equal("changed",preview[0].Status);
        var result=await LegacyImportPersistenceTests.Store(db).ExecuteImportAsync(preview,new() { [preview[0].ExistingEmployeeId!.Value]="update" },1,"date-conflict.xlsx");
        Assert.False(result.Success);Assert.Contains(result.Errors,e=>e.Contains("giấy tờ",StringComparison.OrdinalIgnoreCase));
        db.ChangeTracker.Clear();Assert.Equal(new DateTime(2028,1,1),(await db.ImmigrationDocuments.SingleAsync()).ValidTo);
        Assert.Single(await db.Employees.ToListAsync());
    }

    [Fact]
    public async Task Rollback_RejectsAManuallyEditedExtensionDocument()
    {
        await using var database=await Database();var db=database.Context;
        var row=LegacyImportEndToEndTests.ValidRow();row["DocumentNumber"]="DOC-1";
        var result=await Save(db,await WithFields(db,row));
        (await db.ImmigrationDocuments.SingleAsync()).Number="MANUALLY-EDITED";await db.SaveChangesAsync();
        Assert.False(await LegacyImportPersistenceTests.Store(db).RollbackImportAsync(result.SessionId));
        db.ChangeTracker.Clear();Assert.Equal("MANUALLY-EDITED",(await db.ImmigrationDocuments.SingleAsync()).Number);
    }

    [Theory]
    [InlineData("", "GP-123",1,"GP-123")]
    [InlineData("GP-123", "GP-123",1,"GP-123")]
    public void SeparatePermitNumber_IsConsumed(string combined,string separate,int permit,string number)
    {
        var row=LegacyImportEndToEndTests.ValidRow();row["WorkPermit"]=combined;row["WorkPermitNumber"]=separate;
        var employee=Parse(row);Assert.Equal(permit,(int)employee.WorkPermit);Assert.Equal(number,employee.WorkPermitNumber);
    }
    [Fact]
    public void ConflictingPermitColumns_AreRejected()
    {
        var row=LegacyImportEndToEndTests.ValidRow();row["WorkPermit"]="Miễn";row["WorkPermitNumber"]="GP-123";
        Assert.Throws<ImportRowException>(()=>Parse(row));
    }
    private static Employee Parse(Dictionary<string,string> row)=>ImportMapping.ParseRowToEmployee(row,1,
        [new() { NationalityCode="CHN",NationalityName="Trung Quốc" }],LegacyImportEndToEndTests.Careers(),1,new HashSet<string>(),["Hải Châu"]);

    [Theory]
    [InlineData("2026-06-03",2026,6,3)]
    [InlineData("03/06/2026",2026,6,3)]
    public void FamilyExtensionDate_PreservesIsoAndDayMonthYear(string text,int year,int month,int day)
    {
        var row=LegacyImportEndToEndTests.ValidRow();row["FamilyVisitStartDate"]=text;
        Assert.Equal(new DateTime(year,month,day),Parse(row).FamilyVisitStartDate);
    }
    [Fact]
    public void InvalidFamilyExtensionDate_IsAnExplicitRowError()
    {
        var row=LegacyImportEndToEndTests.ValidRow();row["FamilyVisitStartDate"]="not-a-date";
        Assert.Contains("dòng 9",Assert.Throws<ImportRowException>(()=>Parse(row)).Message);
    }

    [SqlServerFact]
    public async Task LegacyDatabaseImport_PreservesFemaleTwo()
    {
        await using var source=await IsolatedImportSqlDatabase.CreateAsync();await LegacyImportPersistenceTests.SeedAsync(source.Context);
        source.Context.Employees.Add(new() { StaffName="Legacy Female",Gender=(Gender)2,IDCompany=1,IDUser=1,IDCareer=1,Nationality="CHN",Passport="FEMALE-2" });
        await source.Context.SaveChangesAsync();
        await using var target=await IsolatedImportSqlDatabase.CreateAsync();await LegacyImportPersistenceTests.SeedAsync(target.Context);
        var store=new SqlFamilyVisitorStore(target.Context,new SqlAuditStore(target.Context));
        await store.ImportFromLegacyDatabaseAsync(source.Context.Database.GetConnectionString()!);
        var employee=await target.Context.Employees.SingleAsync(e=>e.Passport=="FEMALE-2");
        Assert.Equal(2,(int)employee.Gender);Assert.Equal("Nữ",employee.GenderString);
    }

    [Fact]
    public async Task SelectedHeader_DefaultStartRowBeginsAfterThatHeader()
    {
        await using var database=await Database();
        using var stream=LegacyImportEndToEndTests.Workbook([LegacyImportEndToEndTests.ValidRow()]);
        using(var book=new XLWorkbook(stream)) { book.Worksheets.First().Cell(1,1).Value="Company heading";using var updated=new MemoryStream();book.SaveAs(updated);stream.SetLength(0);updated.Position=0;updated.CopyTo(stream); }
        var preview=await LegacyImportPersistenceTests.Store(database.Context).GeneratePreviewAsync(stream,LegacyImportEndToEndTests.Mappings(),1,new() {HeaderRow=8,CareerGroupId=1});
        Assert.Equal(9,Assert.Single(preview).RowNumber);Assert.Equal("new",preview[0].Status);
    }
}
