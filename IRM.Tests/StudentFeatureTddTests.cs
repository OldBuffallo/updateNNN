using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class StudentFeatureTddTests
{
    [Fact]
    public async Task GetBySchoolAsync_FiltersCaseInsensitive_ExcludesHidden()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new StudentService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var s1 = new Student { FullName = "Student A", Nationality = "USA", SchoolName = "Đại học Hạ Long", Hidden_flag = 0, Status = 0 };
        var s2 = new Student { FullName = "Student B", Nationality = "USA", SchoolName = "Đại học Ngoại Thương", Hidden_flag = 0, Status = 0 };
        var s3 = new Student { FullName = "Student C (Deleted)", Nationality = "USA", SchoolName = "Đại học Hạ Long", Hidden_flag = 1, Status = 0 };

        db.Context.Students.AddRange(s1, s2, s3);
        await db.Context.SaveChangesAsync();

        var results = await service.GetBySchoolAsync("hạ long");
        Assert.Single(results);
        Assert.Equal("Student A", results.Single().FullName);
    }

    [Fact]
    public async Task GetExpiringVisaAsync_CalculatesThreshold()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new StudentService(db.Context, TestSecurityContext.CreateAllowAllGuard());
        var today = DateTime.Today;

        var s1 = new Student { FullName = "S1_ActiveSoon", Nationality = "USA", VisaExpiry = today.AddDays(10), Status = StudentStatus.Studying, Hidden_flag = 0 };
        var s2 = new Student { FullName = "S2_Graduated", Nationality = "USA", VisaExpiry = today.AddDays(10), Status = StudentStatus.Graduated, Hidden_flag = 0 }; // Graduated -> not returned
        var s3 = new Student { FullName = "S3_FarFuture", Nationality = "USA", VisaExpiry = today.AddDays(120), Status = StudentStatus.Studying, Hidden_flag = 0 };

        db.Context.Students.AddRange(s1, s2, s3);
        await db.Context.SaveChangesAsync();

        var expiring = await service.GetExpiringVisaAsync(30);
        Assert.Single(expiring);
        Assert.Equal("S1_ActiveSoon", expiring.Single().FullName);
    }

    [Fact]
    public async Task DeleteAsync_PerformsSoftDelete()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new StudentService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var student = new Student { FullName = "Student To Delete", Nationality = "USA", Hidden_flag = 0, Status = 0 };
        db.Context.Students.Add(student);
        await db.Context.SaveChangesAsync();

        await service.DeleteAsync(student.IDStudent);

        var activeStudents = await service.GetAllActiveAsync();
        Assert.Empty(activeStudents);

        var dbRecord = await db.Context.Students.FindAsync(student.IDStudent);
        Assert.NotNull(dbRecord);
        Assert.Equal(1, dbRecord!.Hidden_flag);
    }

    [Fact]
    public async Task CheckDuplicatePassportAsync_MatchesExact_AndExcludesSelf()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new StudentService(db.Context, TestSecurityContext.CreateAllowAllGuard());

        var student = new Student { FullName = "Student Passport", Passport = "STU-001", Nationality = "USA", Hidden_flag = 0, Status = 0 };
        db.Context.Students.Add(student);
        await db.Context.SaveChangesAsync();

        Assert.True(await service.CheckDuplicatePassportAsync("STU-001"));
        Assert.False(await service.CheckDuplicatePassportAsync("STU-001", student.IDStudent));
        Assert.False(await service.CheckDuplicatePassportAsync("STU-999"));
    }

    [Fact]
    public async Task StudentService_EnforcesRbacGuard()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();

        var reporterGuard = TestSecurityContext.CreateGuard(IrmRoles.Reporter);
        var serviceReporter = new StudentService(db.Context, reporterGuard);

        var s = new Student { FullName = "Protected Student", Nationality = "USA" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceReporter.CreateAsync(s));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceReporter.UpdateAsync(s));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceReporter.DeleteAsync(1));

        var adminGuard = TestSecurityContext.CreateGuard(IrmRoles.Admin);
        var serviceAdmin = new StudentService(db.Context, adminGuard);
        await serviceAdmin.CreateAsync(s);
        Assert.True(s.IDStudent > 0);
    }
}
