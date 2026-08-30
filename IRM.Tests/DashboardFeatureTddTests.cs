using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class DashboardFeatureTddTests
{
    [Fact]
    public async Task GetDashboardAsync_EmptyDatabase_ReturnsZeros()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);
        var service = new DashboardService(db.Context);

        var data = await service.GetDashboardAsync();
        Assert.NotNull(data);
        Assert.Equal(0, data.TotalCompanies);
        Assert.Equal(0, data.TotalEmployees);
        Assert.Equal(0, data.ExpiringCount);
        Assert.Equal(0, data.WithWorkPermit);
        Assert.Equal(0, data.FamilyVisitCount);
        Assert.Equal(0, data.TotalStudents);
        Assert.Empty(data.NationalityStats);
        Assert.Empty(data.WorkPermitStats);
        Assert.Empty(data.ExpiringEmployees);
    }

    [Fact]
    public async Task GetDashboardAsync_CalculatesKPIThresholds_30And90Days()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new DashboardService(db.Context);
        var today = DateTime.Today;

        var e1 = new Employee { StaffName = "E1_ExpiringIn10Days", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(10), Hidden_flag = 0, WorkingStatus = 0 };
        var e2 = new Employee { StaffName = "E2_ExpiringIn60Days", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(60), Hidden_flag = 0, WorkingStatus = 0 };
        var e3 = new Employee { StaffName = "E3_ExpiringIn100Days", IDCompany = 1, IDCareer = 1, Nationality = "USA", TemporaryStay = today.AddDays(100), Hidden_flag = 0, WorkingStatus = 0 };
        db.Context.Employees.AddRange(e1, e2, e3);

        var s1 = new Student { FullName = "S1", Nationality = "USA", VisaExpiry = today.AddDays(15), Status = 0, Hidden_flag = 0 };
        db.Context.Students.Add(s1);
        await db.Context.SaveChangesAsync();

        var data = await service.GetDashboardAsync();
        // 30 days threshold
        Assert.Equal(1, data.ExpiringCount);
        Assert.Equal(1, data.StudentVisaExpiringCount);

        // 90 days warning table contains E1 and E2
        Assert.Equal(2, data.ExpiringEmployees.Count);
        Assert.Contains(data.ExpiringEmployees, x => x.StaffName == "E1_ExpiringIn10Days");
        Assert.Contains(data.ExpiringEmployees, x => x.StaffName == "E2_ExpiringIn60Days");
    }

    [Fact]
    public async Task GetDashboardAsync_WorkPermitMapping_AndTop10Nationalities()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var service = new DashboardService(db.Context);

        db.Context.Employees.Add(new Employee { StaffName = "K1", IDCompany = 1, IDCareer = 1, Nationality = "KOR", WorkPermit = WorkPermitType.WorkerHasPermit, Hidden_flag = 0, WorkingStatus = 0 });
        db.Context.Employees.Add(new Employee { StaffName = "K2", IDCompany = 1, IDCareer = 1, Nationality = "KOR", WorkPermit = WorkPermitType.InvestorHasPermit, Hidden_flag = 0, WorkingStatus = 0 });
        db.Context.Employees.Add(new Employee { StaffName = "J1", IDCompany = 1, IDCareer = 1, Nationality = "JPN", WorkPermit = WorkPermitType.WorkerExempt, Hidden_flag = 0, WorkingStatus = 0 });
        await db.Context.SaveChangesAsync();

        var data = await service.GetDashboardAsync();
        Assert.Equal(3, data.TotalEmployees);
        Assert.Equal(2, data.WithWorkPermit); // WorkPermit == 1 or 4

        Assert.Equal(2, data.NationalityStats.Count);
        Assert.Equal("Hàn Quốc", data.NationalityStats.First().Label);
        Assert.Equal(2, data.NationalityStats.First().Value);
    }
}
