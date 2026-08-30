using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class SearchFeatureTddTests
{
    [Fact]
    public async Task SearchAsync_MatchesMultipleFieldsAcrossEntities()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var searchService = new SearchService(db.Context);

        var company = new Company { CompanyName = "Foxconn Technology", IDField = 1, TrackerID = 1, Delete_flag = 0 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        var emp = new Employee
        {
            StaffName = "Zhang San",
            Passport = "C123456",
            IDCompany = company.IDCompany,
            IDCareer = 1,
            Nationality = "CHN",
            FamilyVisitRelativeName = "Nguyễn Thị Mai",
            Hidden_flag = 0,
            WorkingStatus = 0
        };
        var stu = new Student
        {
            FullName = "Li Si",
            Passport = "C999999",
            Nationality = "CHN",
            SchoolName = "Đại học Hạ Long",
            Hidden_flag = 0,
            Status = 0
        };
        db.Context.AddRange(emp, stu);
        await db.Context.SaveChangesAsync();

        // 1. Search by person name
        var res1 = await searchService.SearchAsync("Zhang San");
        Assert.Single(res1.Employees);

        // 2. Search by relative name
        var res2 = await searchService.SearchAsync("Nguyễn Thị Mai");
        Assert.Single(res2.Employees);

        // 3. Search by school name
        var res3 = await searchService.SearchAsync("Hạ Long");
        Assert.Single(res3.Students);

        // 4. Search by company name
        var res4 = await searchService.SearchAsync("Foxconn");
        Assert.Single(res4.Companies);
        Assert.Single(res4.Employees); // Employee has company Foxconn
    }

    [Fact]
    public async Task SearchAsync_FilterOption_IsolatesResults()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var searchService = new SearchService(db.Context);

        var keyword = "Alpha";
        db.Context.Companies.Add(new Company { CompanyName = "Alpha Corp", IDField = 1, TrackerID = 1, Delete_flag = 0 });
        db.Context.Employees.Add(new Employee { StaffName = "Alpha Employee", IDCompany = 1, IDCareer = 1, Nationality = "USA", Hidden_flag = 0 });
        db.Context.Students.Add(new Student { FullName = "Alpha Student", Nationality = "USA", Hidden_flag = 0 });
        await db.Context.SaveChangesAsync();

        var resAll = await searchService.SearchAsync(keyword, "all");
        Assert.Equal(3, resAll.TotalCount);

        var resEmp = await searchService.SearchAsync(keyword, "employee");
        Assert.Single(resEmp.Employees);
        Assert.Empty(resEmp.Companies);
        Assert.Empty(resEmp.Students);

        var resComp = await searchService.SearchAsync(keyword, "company");
        Assert.Empty(resComp.Employees);
        Assert.Single(resComp.Companies);
        Assert.Empty(resComp.Students);

        var resStu = await searchService.SearchAsync(keyword, "student");
        Assert.Empty(resStu.Employees);
        Assert.Empty(resStu.Companies);
        Assert.Single(resStu.Students);
    }

    [Fact]
    public async Task SearchAsync_IgnoresSoftDeletedRecords()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var searchService = new SearchService(db.Context);

        db.Context.Companies.Add(new Company { CompanyName = "Deleted Company", IDField = 1, TrackerID = 1, Delete_flag = 1 });
        db.Context.Employees.Add(new Employee { StaffName = "Deleted Employee", IDCompany = 1, IDCareer = 1, Nationality = "USA", Hidden_flag = 1 });
        db.Context.Students.Add(new Student { FullName = "Deleted Student", Nationality = "USA", Hidden_flag = 1 });
        await db.Context.SaveChangesAsync();

        var res = await searchService.SearchAsync("Deleted");
        Assert.Equal(0, res.TotalCount);
    }
}
