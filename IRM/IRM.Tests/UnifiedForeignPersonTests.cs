using IRM.Application.Models;
using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class UnifiedForeignPersonTests
{
    [Fact]
    public async Task GetUnifiedProfilesAsync_IncludesUnlinkedLegacyEmployeesAndStudents_EvenWithoutPassport()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new SqlAuditStore(db.Context);
        var store = new SqlFamilyVisitorStore(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        db.Context.Employees.AddRange(
            new Employee
            {
                StaffName = "Legacy Worker With Passport",
                Passport = "USA-111222",
                Nationality = "USA",
                IDCompany = 1,
                IDCareer = 1,
                WorkPermit = WorkPermitType.WorkerHasPermit,
                TemporaryStay = DateTime.Today.AddDays(15),
                Hidden_flag = 0,
                WorkingStatus = 0
            },
            new Employee
            {
                StaffName = "Legacy Worker No Passport",
                Passport = null,
                Nationality = "KOR",
                IDCompany = 1,
                IDCareer = 1,
                WorkPermit = WorkPermitType.WorkerExempt,
                Hidden_flag = 0,
                WorkingStatus = 0
            });

        db.Context.Students.Add(new Student
        {
            FullName = "Legacy Student",
            Passport = "JPN-333444",
            Nationality = "JPN",
            SchoolName = "Đại học Hạ Long",
            Major = "Tiếng Việt",
            EducationLevel = EducationLevel.University,
            ScholarshipType = ScholarshipType.SelfFunded,
            Status = StudentStatus.Studying,
            Hidden_flag = 0
        });

        await db.Context.SaveChangesAsync();

        var items = await store.GetUnifiedProfilesAsync(null, DateTime.Today);

        Assert.Equal(3, items.Count);
        Assert.Contains(items, x => x.FullName == "Legacy Worker With Passport" && x.HasWorkProfile && x.CompanyName == "Default Test Company");
        Assert.Contains(items, x => x.FullName == "Legacy Worker No Passport" && x.HasWorkProfile && x.EmployeeId.HasValue);
        Assert.Contains(items, x => x.FullName == "Legacy Student" && x.HasStudyProfile && x.SchoolName == "Đại học Hạ Long");
    }

    [Fact]
    public async Task SaveUnifiedProfileAsync_CreatesAndLinksEmployeeFamilyVisitAndEid()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new SqlAuditStore(db.Context);
        var store = new SqlFamilyVisitorStore(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var input = new UnifiedForeignPersonInput
        {
            FullName = "Kim Min Soo",
            Gender = 1,
            Birthday = new DateTime(1988, 4, 12),
            NationalityCode = "KOR",
            PassportNumber = "KR-889900",
            AdministrativeUnitId = 1,
            ResidenceAddress = "Tổ 1, Phường Hồng Gai",
            HasWorkProfile = true,
            CompanyId = 1,
            CareerId = 1,
            WorkPermit = WorkPermitType.WorkerHasPermit,
            WorkPermitNumber = "WP-2026-001",
            TemporaryStay = DateTime.Today.AddMonths(6),
            HasFamilyVisitProfile = true,
            RelativeName = "Nguyễn Thị Lan",
            Relationship = "Vợ",
            RelativeTypeCode = "VIETNAMESE_CITIZEN",
            RelativeIdNumber = "022188001122",
            VisitStartDate = DateTime.Today,
            VisitEndDate = DateTime.Today.AddMonths(6),
            DocumentTypeCode = ImmigrationDocumentTypeCodes.TemporaryResidenceCard,
            DocumentNumber = "TRC-8899",
            VisaSymbol = "LD2",
            DocumentValidFrom = DateTime.Today,
            DocumentValidTo = DateTime.Today.AddMonths(6),
            ElectronicIdentityNumber = "EID-KR-8899"
        };

        var foreignPersonId = await store.SaveUnifiedProfileAsync(input);
        Assert.True(foreignPersonId > 0);

        var person = await db.Context.ForeignPersons.SingleAsync();
        Assert.Equal("Kim Min Soo", person.FullName);

        var emp = await db.Context.Employees.SingleAsync();
        Assert.Equal("Kim Min Soo", emp.StaffName);
        Assert.Equal("WP-2026-001", emp.WorkPermitNumber);
        Assert.Equal(1, emp.FamilyVisit);
        Assert.Equal("Nguyễn Thị Lan", emp.FamilyVisitRelativeName);

        Assert.Single(await db.Context.ForeignPersonSourceLinks.Where(x => x.SourceType == "EMPLOYEE").ToListAsync());
        Assert.Equal(2, await db.Context.StayCases.CountAsync());
        Assert.Single(await db.Context.ElectronicIdentities.ToListAsync());
        Assert.Single(await db.Context.ImmigrationDocuments.ToListAsync());
    }

    [Fact]
    public async Task DeleteUnifiedProfileAsync_SoftDeletesBothForeignPersonAndLegacyRecords()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new SqlAuditStore(db.Context);
        var store = new SqlFamilyVisitorStore(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var personId = await store.SaveUnifiedProfileAsync(new UnifiedForeignPersonInput
        {
            FullName = "To Be Deleted",
            NationalityCode = "USA",
            PassportNumber = "US-DEL-01",
            HasWorkProfile = true,
            CompanyId = 1,
            CareerId = 1
        });

        var emp = await db.Context.Employees.SingleAsync();
        await store.DeleteUnifiedProfileAsync(personId, emp.IDEmployee, null);

        Assert.True((await db.Context.ForeignPersons.FindAsync(personId))!.IsDeleted);
        Assert.Equal(1, (await db.Context.Employees.FindAsync(emp.IDEmployee))!.Hidden_flag);
        Assert.Empty(await store.GetUnifiedProfilesAsync(null, DateTime.Today));
    }
}
