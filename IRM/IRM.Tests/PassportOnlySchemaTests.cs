using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class PassportOnlySchemaTests
{
    [Fact]
    public async Task Employee_CanBeSavedWithOnlyPassportAndNullCompany()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();

        var emp = new Employee
        {
            StaffName = "",
            Passport = "P99887766",
            IDCompany = null,
            IDUser = 1,
            Hidden_flag = 0,
            WorkingStatus = 0
        };
        td.Context.Employees.Add(emp);
        await td.Context.SaveChangesAsync();

        var loaded = await td.Context.Employees
            .Include(e => e.Company)
            .SingleAsync(e => e.Passport == "P99887766");

        Assert.Null(loaded.IDCompany);
        Assert.Null(loaded.Company);
        Assert.Equal("P99887766", loaded.Passport);
    }
}
