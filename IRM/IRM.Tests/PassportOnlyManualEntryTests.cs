using IRM.Application.Features.Employees;
using IRM.Application.Features.ForeignPersons;
using IRM.Application.Models;
using IRM.Infrastructure.Persistence.Stores.Administration;
using IRM.Infrastructure.Persistence.Stores.Employees;
using IRM.Infrastructure.Persistence.Stores.ForeignPersons;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class PassportOnlyManualEntryTests
{
    [Fact]
    public async Task SaveUnifiedProfileAsync_OnlyPassportFilled_SucceedsAndMarksDataIncomplete()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var store = new SqlFamilyVisitorStore(td.Context, new SqlAuditStore(td.Context), guard);
        var service = new FamilyVisitorApplicationService(store, guard);

        var personId = await service.SaveUnifiedProfileAsync(new UnifiedForeignPersonInput
        {
            FullName = "",
            PassportNumber = "PP-UNI-01",
            HasWorkProfile = false
        });

        Assert.True(personId > 0);
        var saved = await td.Context.ForeignPersons.SingleAsync(p => p.Id == personId);
        Assert.Equal("PPUNI01", saved.PassportNumber);
        Assert.Equal("", saved.FullName);
        Assert.True(saved.IsDataIncomplete);
    }

    [Fact]
    public async Task SaveUnifiedProfileAsync_BlankPassport_ThrowsArgumentException()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var store = new SqlFamilyVisitorStore(td.Context, new SqlAuditStore(td.Context), guard);
        var service = new FamilyVisitorApplicationService(store, guard);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveUnifiedProfileAsync(new UnifiedForeignPersonInput
        {
            FullName = "NGUYEN VAN A",
            PassportNumber = "   "
        }));
    }

    [Fact]
    public async Task EmployeeApplicationService_CreateAsync_OnlyPassportSucceedsAndBlankPassportThrows()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var store = new SqlEmployeeStore(td.Context, guard);
        var service = new EmployeeApplicationService(store, guard);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new EmployeeDto
        {
            StaffName = "NO PASSPORT",
            Passport = ""
        }));

        var dto = new EmployeeDto
        {
            StaffName = "",
            Passport = "PP-EMP-01",
            IDCompany = null
        };
        await service.CreateAsync(dto);

        Assert.True(dto.IDEmployee > 0);
        var saved = await td.Context.Employees.SingleAsync(e => e.IDEmployee == dto.IDEmployee);
        Assert.Equal("PP-EMP-01", saved.Passport);
        Assert.Null(saved.IDCompany);
    }
}
