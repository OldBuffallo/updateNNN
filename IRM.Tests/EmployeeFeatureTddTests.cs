using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class EmployeeFeatureTddTests
{
    [Theory]
    [InlineData(" b-12 34 ", "B1234", true)]
    [InlineData("B1234", "b1234", true)]
    [InlineData("P001", "P002", false)]
    [InlineData("", "P001", false)]
    public async Task CheckDuplicatePassport_NormalizesAndValidates(string existingPassport, string checkPassport, bool expectedDuplicate)
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new EmployeeService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var emp = new Employee
        {
            StaffName = "John Doe",
            Passport = existingPassport,
            IDCompany = 1,
            IDCareer = 1,
            Nationality = "USA",
            Hidden_flag = 0,
            WorkingStatus = 0
        };
        db.Context.Employees.Add(emp);
        await db.Context.SaveChangesAsync();

        var isDup = await service.CheckDuplicatePassportAsync(checkPassport);
        Assert.Equal(expectedDuplicate, isDup);

        // Exclude self when updating
        var isDupSelf = await service.CheckDuplicatePassportAsync(existingPassport, emp.IDEmployee);
        Assert.False(isDupSelf);
    }

    [Fact]
    public async Task GetExpiringAsync_Validates30DayWindow_Boundary()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new EmployeeService(db.Context, TestSecurityContext.CreateAllowAllGuard());
        var today = DateTime.Today;

        var e1 = new Employee { StaffName = "E1_ExpiredYesterday", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(-1), Hidden_flag = 0, WorkingStatus = 0 };
        var e2 = new Employee { StaffName = "E2_ExpiringToday", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today, Hidden_flag = 0, WorkingStatus = 0 };
        var e3 = new Employee { StaffName = "E3_ExpiringIn15Days", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(15), Hidden_flag = 0, WorkingStatus = 0 };
        var e4 = new Employee { StaffName = "E4_ExpiringIn30Days", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(30), Hidden_flag = 0, WorkingStatus = 0 };
        var e5 = new Employee { StaffName = "E5_ExpiringIn31Days", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(31), Hidden_flag = 0, WorkingStatus = 0 };

        db.Context.Employees.AddRange(e1, e2, e3, e4, e5);
        await db.Context.SaveChangesAsync();

        var expiring = await service.GetExpiringAsync(30);
        Assert.Equal(3, expiring.Count);
        Assert.Contains(expiring, x => x.StaffName == "E2_ExpiringToday");
        Assert.Contains(expiring, x => x.StaffName == "E3_ExpiringIn15Days");
        Assert.Contains(expiring, x => x.StaffName == "E4_ExpiringIn30Days");
    }

    [Fact]
    public async Task ArchiveExpiredTemporaryStayAsync_FullSnapshotToArchivedEmployees_AndSoftDeletes()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new EmployeeService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var field = new Field { FieldName = "Công nghệ" };
        var careerGroup = new CareerGroup { CareerGroupName = "CNTT Group" };
        db.Context.AddRange(field, careerGroup);
        await db.Context.SaveChangesAsync();

        var career = new Career { CareerName = "Kỹ sư phần mềm", IDCG = careerGroup.IDCG };
        var nationality = new NationalityEntity { NationalityCode = "US_CUSTOM", NationalityName = "Mỹ" };
        db.Context.AddRange(career, nationality);
        await db.Context.SaveChangesAsync();

        var company = new Company { CompanyName = "Tech Corp", IDField = field.IDField, TrackerID = 1 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        var expiredEmp = new Employee
        {
            StaffName = "Alice Smith",
            Passport = "US-99999",
            Nationality = "US_CUSTOM",
            IDCompany = company.IDCompany,
            IDCareer = career.IDCareer,
            TemporaryStay = DateTime.Today.AddDays(-5),
            WorkingStatus = 0,
            Hidden_flag = 0
        };
        var validEmp = new Employee
        {
            StaffName = "Bob Smith",
            Passport = "US-88888",
            Nationality = "US_CUSTOM",
            IDCompany = company.IDCompany,
            IDCareer = career.IDCareer,
            TemporaryStay = DateTime.Today.AddDays(60),
            WorkingStatus = 0,
            Hidden_flag = 0
        };
        db.Context.Employees.AddRange(expiredEmp, validEmp);
        await db.Context.SaveChangesAsync();

        var archivedCount = await service.ArchiveExpiredTemporaryStayAsync("admin_super");
        Assert.Equal(1, archivedCount);

        var archived = db.Context.ArchivedEmployees.Single();
        Assert.Equal("Alice Smith", archived.StaffName);
        Assert.Equal("Tech Corp", archived.CompanyName);
        Assert.Equal("Kỹ sư phần mềm", archived.CareerName);
        Assert.Equal("HET_HAN_TAM_TRU", archived.ArchiveReason);
        Assert.Equal("admin_super", archived.ArchivedBy);

        var activeEmployees = await service.GetAllActiveAsync();
        Assert.Single(activeEmployees);
        Assert.Equal("Bob Smith", activeEmployees.Single().StaffName);

        var dbExpired = await db.Context.Employees.FindAsync(expiredEmp.IDEmployee);
        Assert.NotNull(dbExpired);
        Assert.Equal(1, dbExpired!.Hidden_flag);
    }

    [Fact]
    public async Task ArchiveExpiredFamilyVisitAsync_OnlyArchivesFamilyVisitors()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new EmployeeService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var famExpired = new Employee
        {
            StaffName = "FamVisitor_Expired",
            IDCompany = 1,
            IDCareer = 1,
            Nationality = "USA",
            FamilyVisit = 1,
            FamilyVisitEndDate = DateTime.Today.AddDays(-2),
            Hidden_flag = 0
        };
        var nonFamExpired = new Employee
        {
            StaffName = "NonFam_Expired",
            IDCompany = 1,
            IDCareer = 1,
            Nationality = "USA",
            FamilyVisit = 0,
            FamilyVisitEndDate = DateTime.Today.AddDays(-2),
            Hidden_flag = 0
        };
        db.Context.Employees.AddRange(famExpired, nonFamExpired);
        await db.Context.SaveChangesAsync();

        var count = await service.ArchiveExpiredFamilyVisitAsync("auditor");
        Assert.Equal(1, count);

        var archived = db.Context.ArchivedEmployees.Single();
        Assert.Equal("FamVisitor_Expired", archived.StaffName);
        Assert.Equal("HET_HAN_THAM_THAN", archived.ArchiveReason);
    }

    [Fact]
    public async Task EmployeeService_EnforcesRbacGuard()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);

        var viewerGuard = TestSecurityContext.CreateGuard(IrmRoles.Viewer);
        var serviceViewer = new EmployeeService(db.Context, viewerGuard);

        var emp = new Employee { StaffName = "Test Guard", IDCompany = 1, IDCareer = 1, Nationality = "USA", Hidden_flag = 0 };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceViewer.CreateAsync(emp));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceViewer.UpdateAsync(emp));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceViewer.DeleteAsync(1));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceViewer.ArchiveExpiredTemporaryStayAsync());

        var editorGuard = TestSecurityContext.CreateGuard(IrmRoles.DataEditor);
        var serviceEditor = new EmployeeService(db.Context, editorGuard);
        await serviceEditor.CreateAsync(emp);
        Assert.True(emp.IDEmployee > 0);
    }
}
