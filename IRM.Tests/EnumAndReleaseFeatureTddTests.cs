using System.IO;
using ClosedXML.Excel;
using IRM.Data;
using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IRM.Tests;

public sealed class EnumAndReleaseFeatureTddTests
{
    [Fact]
    public async Task EnumRoundtrip_Employee_PreservesEnumValuesAndDisplayStrings()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        var employee = new Employee
        {
            StaffName = "Elena Petrova",
            Gender = Gender.Female,
            WorkPermit = WorkPermitType.WorkerHasPermit,
            WorkPermitNumber = "WP-2026-999",
            Nationality = "VN",
            IDCompany = 1,
            IDCareer = 1,
            Hidden_flag = 0,
            WorkingStatus = 0
        };

        db.Context.Employees.Add(employee);
        await db.Context.SaveChangesAsync();

        var reloaded = await db.Context.Employees.FindAsync(employee.IDEmployee);
        Assert.NotNull(reloaded);
        Assert.Equal(Gender.Female, reloaded!.Gender);
        Assert.Equal("Nữ", reloaded.GenderString);
        Assert.Equal(WorkPermitType.WorkerHasPermit, reloaded.WorkPermit);
        Assert.Equal("NLĐ đã có GPLĐ", reloaded.WorkPermitString);

        // Update to InvestorExempt
        reloaded.WorkPermit = WorkPermitType.InvestorExempt;
        reloaded.Gender = Gender.Male;
        await db.Context.SaveChangesAsync();

        var updated = await db.Context.Employees.FindAsync(employee.IDEmployee);
        Assert.NotNull(updated);
        Assert.Equal(Gender.Male, updated!.Gender);
        Assert.Equal("Nam", updated.GenderString);
        Assert.Equal(WorkPermitType.InvestorExempt, updated.WorkPermit);
        Assert.Equal("NĐT miễn GPLĐ", updated.WorkPermitString);
    }

    [Fact]
    public async Task EnumRoundtrip_Student_PreservesAllEnumsAndDisplays()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        var student = new Student
        {
            FullName = "Kim Min-ji",
            Gender = Gender.Female,
            EducationLevel = EducationLevel.Master,
            ScholarshipType = ScholarshipType.ForeignScholarship,
            Status = StudentStatus.Studying,
            SchoolName = "Đại học Hạ Long",
            Major = "Du lịch & Lữ hành",
            Nationality = "KOR",
            Hidden_flag = 0
        };

        db.Context.Students.Add(student);
        await db.Context.SaveChangesAsync();

        var reloaded = await db.Context.Students.FindAsync(student.IDStudent);
        Assert.NotNull(reloaded);
        Assert.Equal(Gender.Female, reloaded!.Gender);
        Assert.Equal("Nữ", reloaded.GenderString);
        Assert.Equal(EducationLevel.Master, reloaded.EducationLevel);
        Assert.Equal("Thạc sĩ", reloaded.EducationLevelString);
        Assert.Equal(ScholarshipType.ForeignScholarship, reloaded.ScholarshipType);
        Assert.Equal("HB nước ngoài", reloaded.ScholarshipTypeString);
        Assert.Equal(StudentStatus.Studying, reloaded.Status);
        Assert.Equal("Đang học", reloaded.StatusString);

        // Transition status to Graduated
        reloaded.Status = StudentStatus.Graduated;
        reloaded.ScholarshipType = ScholarshipType.VietnamGovernment;
        await db.Context.SaveChangesAsync();

        var updated = await db.Context.Students.FindAsync(student.IDStudent);
        Assert.NotNull(updated);
        Assert.Equal(StudentStatus.Graduated, updated!.Status);
        Assert.Equal("Đã tốt nghiệp", updated.StatusString);
        Assert.Equal(ScholarshipType.VietnamGovernment, updated.ScholarshipType);
        Assert.Equal("HB Chính phủ VN", updated.ScholarshipTypeString);
    }

    [Fact]
    public async Task EnumRoundtrip_Account_PreservesPermissionValues()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);

        var admin = new Account { Username = "admin_user", Name = "Admin User", Password = "pwd", Permission = AccountPermission.Admin };
        var standardUser = new Account { Username = "standard_user", Name = "Standard User", Password = "pwd", Permission = AccountPermission.User };

        db.Context.Accounts.AddRange(admin, standardUser);
        await db.Context.SaveChangesAsync();

        var reloadedAdmin = await db.Context.Accounts.SingleAsync(a => a.Username == "admin_user");
        var reloadedUser = await db.Context.Accounts.SingleAsync(a => a.Username == "standard_user");

        Assert.Equal(AccountPermission.Admin, reloadedAdmin.Permission);
        Assert.Equal("Admin", reloadedAdmin.PermissionString);

        Assert.Equal(AccountPermission.User, reloadedUser.Permission);
        Assert.Equal("User", reloadedUser.PermissionString);
    }

    [Fact]
    public async Task Import_ParsesGenderAndWorkPermitEnum_FromExcelStream()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Import");
        ws.Cell(1, 1).Value = "Họ tên";
        ws.Cell(1, 2).Value = "Giới tính";
        ws.Cell(1, 3).Value = "GPLĐ";
        ws.Cell(1, 4).Value = "Quốc tịch";
        ws.Cell(1, 5).Value = "Hộ chiếu";

        ws.Cell(2, 1).Value = "Nguyen Van Nam";
        ws.Cell(2, 2).Value = "Nam";
        ws.Cell(2, 3).Value = "Có GPLĐ";
        ws.Cell(2, 4).Value = "Việt Nam";
        ws.Cell(2, 5).Value = "P-VN-001";

        ws.Cell(3, 1).Value = "Tran Thi Nu";
        ws.Cell(3, 2).Value = "Nữ";
        ws.Cell(3, 3).Value = "Miễn";
        ws.Cell(3, 4).Value = "Việt Nam";
        ws.Cell(3, 5).Value = "P-VN-002";

        ws.Cell(4, 1).Value = "Li Qiang";
        ws.Cell(4, 2).Value = "male";
        ws.Cell(4, 3).Value = "Nhà đầu tư có";
        ws.Cell(4, 4).Value = "Trung Quốc";
        ws.Cell(4, 5).Value = "P-CN-003";

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        var importService = new ImportService(db.Context, new AuditService(db.Context), TestSecurityContext.CreateAllowAllGuard());
        var mappings = new List<ColumnMapping>
        {
            new() { ExcelColumnIndex = 0, ExcelColumnName = "Họ tên", SystemField = "StaffName", SystemFieldLabel = "Họ tên" },
            new() { ExcelColumnIndex = 1, ExcelColumnName = "Giới tính", SystemField = "Gender", SystemFieldLabel = "Giới tính" },
            new() { ExcelColumnIndex = 2, ExcelColumnName = "GPLĐ", SystemField = "WorkPermit", SystemFieldLabel = "GPLĐ" },
            new() { ExcelColumnIndex = 3, ExcelColumnName = "Quốc tịch", SystemField = "Nationality", SystemFieldLabel = "Quốc tịch" },
            new() { ExcelColumnIndex = 4, ExcelColumnName = "Hộ chiếu", SystemField = "Passport", SystemFieldLabel = "Hộ chiếu" }
        };

        var preview = await importService.GeneratePreviewAsync(stream, mappings, 1);
        Assert.Equal(3, preview.Count);

        Assert.Equal(Gender.Male, preview[0].ParsedEmployee!.Gender);
        Assert.Equal(WorkPermitType.WorkerHasPermit, preview[0].ParsedEmployee!.WorkPermit);

        Assert.Equal(Gender.Female, preview[1].ParsedEmployee!.Gender);
        Assert.Equal(WorkPermitType.WorkerExempt, preview[1].ParsedEmployee!.WorkPermit);

        Assert.Equal(Gender.Male, preview[2].ParsedEmployee!.Gender);
        Assert.Equal(WorkPermitType.InvestorHasPermit, preview[2].ParsedEmployee!.WorkPermit);
    }

    [Fact]
    public async Task Export_EmployeesEnumFilter_FiltersCorrectly()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        var e1 = new Employee { StaffName = "Emp With Permit", WorkPermit = WorkPermitType.WorkerHasPermit, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 };
        var e2 = new Employee { StaffName = "Emp Exempt", WorkPermit = WorkPermitType.WorkerExempt, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 };
        var e3 = new Employee { StaffName = "Emp No Permit", WorkPermit = WorkPermitType.WorkerNoPermit, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 };

        db.Context.Employees.AddRange(e1, e2, e3);
        await db.Context.SaveChangesAsync();

        var factory = new TestDbContextFactory(db.Context);
        var exportService = new ExportService(factory, new AuditService(db.Context), TestSecurityContext.CreateAllowAllGuard());

        var bytes = await exportService.ExportEmployeesAsync(workPermit: WorkPermitType.WorkerHasPermit);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheet(1);

        // Header + 1 data row
        Assert.Equal("Emp With Permit", ws.Cell(2, 2).GetString());
        Assert.True(ws.Cell(3, 2).IsEmpty());
    }

    [Fact]
    public async Task DatabaseSeeder_SeedCatalogsIfEmpty_PopulatesStandardLookupsAndAdmin()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);

        // Initially empty
        Assert.False(await db.Context.Accounts.AnyAsync());
        Assert.False(await db.Context.Fields.AnyAsync());
        Assert.False(await db.Context.CareerGroups.AnyAsync());
        Assert.False(await db.Context.Careers.AnyAsync());

        await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db.Context);

        // Verify lookups populated
        Assert.True(await db.Context.Accounts.AnyAsync(a => a.Username == "admin" && a.Permission == AccountPermission.Admin));
        Assert.Equal(8, await db.Context.Fields.CountAsync());
        Assert.Equal(4, await db.Context.CareerGroups.CountAsync());
        Assert.Equal(4, await db.Context.Careers.CountAsync());
        Assert.True(await db.Context.SchemaVersions.AnyAsync(s => s.Version == "0.1.0"));

        // Verify idempotency
        await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db.Context);
        Assert.Single(await db.Context.Accounts.Where(a => a.Username == "admin").ToListAsync());
        Assert.Equal(8, await db.Context.Fields.CountAsync());
    }

    [Fact]
    public async Task DashboardService_WorkPermitStats_GroupsAndLabelsAccurately()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        db.Context.Employees.AddRange(
            new Employee { StaffName = "E1", WorkPermit = WorkPermitType.WorkerHasPermit, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "E2", WorkPermit = WorkPermitType.WorkerHasPermit, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "E3", WorkPermit = WorkPermitType.WorkerExempt, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "E4", WorkPermit = WorkPermitType.InvestorHasPermit, IDCompany = 1, IDCareer = 1, Nationality = "VN", Hidden_flag = 0, WorkingStatus = 0 }
        );
        await db.Context.SaveChangesAsync();

        var dashboardService = new DashboardService(db.Context);
        var dashboard = await dashboardService.GetDashboardAsync();

        Assert.NotNull(dashboard.WorkPermitStats);
        var hasPermitStat = dashboard.WorkPermitStats.FirstOrDefault(s => s.Label == "Đã có GPLĐ");
        var exemptStat = dashboard.WorkPermitStats.FirstOrDefault(s => s.Label == "Miễn GPLĐ");
        var investorPermitStat = dashboard.WorkPermitStats.FirstOrDefault(s => s.Label == "NĐT đã có");

        Assert.NotNull(hasPermitStat);
        Assert.Equal(2, hasPermitStat!.Value);

        Assert.NotNull(exemptStat);
        Assert.Equal(1, exemptStat!.Value);

        Assert.NotNull(investorPermitStat);
        Assert.Equal(1, investorPermitStat!.Value);
    }

    [Fact]
    public async Task ImportRollback_PolymorphicBackup_StoresEntityTypeAndId()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        var backup1 = new ImportBackup
        {
            ImportSessionId = "IMP-SESSION-001",
            ActionType = "INSERT",
            EntityType = "Employee",
            EntityId = 101,
            CreatedAt = DateTime.UtcNow
        };

        var backup2 = new ImportBackup
        {
            ImportSessionId = "IMP-SESSION-001",
            ActionType = "UPDATE",
            EntityType = "Student",
            EntityId = 202,
            OldData = "{\"Status\":0,\"ScholarshipType\":1}",
            CreatedAt = DateTime.UtcNow
        };

        db.Context.ImportBackups.AddRange(backup1, backup2);
        await db.Context.SaveChangesAsync();

        var reloadedBackups = await db.Context.ImportBackups
            .Where(b => b.ImportSessionId == "IMP-SESSION-001")
            .ToListAsync();

        Assert.Equal(2, reloadedBackups.Count);
        Assert.Contains(reloadedBackups, b => b.EntityType == "Employee" && b.EntityId == 101);
        Assert.Contains(reloadedBackups, b => b.EntityType == "Student" && b.EntityId == 202 && b.OldData != null);
    }

    [Fact]
    public async Task AuditLog_StoresChangesJson_Properly()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        var audit = new AuditLog
        {
            Action = "UPDATE",
            EntityType = "Employee",
            EntityId = 1,
            Description = "Cập nhật loại GPLĐ",
            ChangesJson = "[{\"Field\":\"WorkPermit\",\"Old\":\"0\",\"New\":\"1\"}]",
            Username = "admin",
            Timestamp = DateTime.UtcNow,
            IpAddress = "127.0.0.1"
        };

        db.Context.AuditLogs.Add(audit);
        await db.Context.SaveChangesAsync();

        var saved = await db.Context.AuditLogs.FindAsync(audit.Id);
        Assert.NotNull(saved);
        Assert.Contains("WorkPermit", saved!.ChangesJson);
    }

    private sealed class TestDbContextFactory(IrmDbContext context) : IDbContextFactory<IrmDbContext>
    {
        public IrmDbContext CreateDbContext() => context;
        public Task<IrmDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(context);
    }
}
