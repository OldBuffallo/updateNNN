using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class AccommodationFeatureTddTests
{
    [Fact]
    public async Task SaveAsync_ValidatesMandatoryNameAndAddress()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new AccommodationService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var emptyName = new Accommodation { Name = "", AddressLine = "123 Đường A" };
        var emptyAddress = new Accommodation { Name = "Nhà trọ A", AddressLine = "  " };

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(emptyName));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(emptyAddress));

        var valid = new Accommodation { Name = "Nhà trọ Hoa Sen", AddressLine = "123 Đường A", TypeCode = "RENTED" };
        var id = await service.SaveAsync(valid);
        Assert.True(id > 0);
    }

    [Fact]
    public async Task SaveAgreementAsync_ValidatesTemporalDateRange()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var audit = new AuditService(db.Context);
        var service = new AccommodationService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var acc = new Accommodation { Name = "CSLT Test", AddressLine = "Địa chỉ", TypeCode = "RENTED" };
        db.Context.Accommodations.Add(acc);
        await db.Context.SaveChangesAsync();

        var invalidAgreement = new CompanyAccommodationAgreement
        {
            AccommodationId = acc.Id,
            CompanyId = 1,
            ValidFrom = new DateTime(2026, 6, 1),
            ValidTo = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAgreementAsync(invalidAgreement));
        Assert.Contains("không hợp lệ", ex.Message);

        var validAgreement = new CompanyAccommodationAgreement
        {
            AccommodationId = acc.Id,
            CompanyId = 1,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31)
        };
        var id = await service.SaveAgreementAsync(validAgreement);
        Assert.True(id > 0);
    }

    [Fact]
    public async Task SearchAsync_FiltersByActiveCompanyAgreement_AtAsOfDate()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var audit = new AuditService(db.Context);
        var service = new AccommodationService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var compA = new Company { CompanyName = "Company A", IDField = 1, TrackerID = 1 };
        var compB = new Company { CompanyName = "Company B", IDField = 1, TrackerID = 1 };
        db.Context.Companies.AddRange(compA, compB);

        var acc1 = new Accommodation { Name = "CSLT 1", AddressLine = "Địa chỉ 1", TypeCode = "RENTED", IsDeleted = false };
        var acc2 = new Accommodation { Name = "CSLT 2", AddressLine = "Địa chỉ 2", TypeCode = "DORMITORY", IsDeleted = false };
        var accDeleted = new Accommodation { Name = "CSLT 3", AddressLine = "Địa chỉ 3", TypeCode = "RENTED", IsDeleted = true };
        db.Context.Accommodations.AddRange(acc1, acc2, accDeleted);
        await db.Context.SaveChangesAsync();

        // Agreement with compA active in 2026
        db.Context.CompanyAccommodationAgreements.Add(new CompanyAccommodationAgreement
        {
            AccommodationId = acc1.Id,
            CompanyId = compA.IDCompany,
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31)
        });
        // Agreement with compA active only in 2025
        db.Context.CompanyAccommodationAgreements.Add(new CompanyAccommodationAgreement
        {
            AccommodationId = acc2.Id,
            CompanyId = compA.IDCompany,
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2025, 12, 31)
        });
        await db.Context.SaveChangesAsync();

        var results2026 = await service.SearchAsync(null, null, compA.IDCompany, new DateTime(2026, 6, 1));
        Assert.Single(results2026);
        Assert.Equal("CSLT 1", results2026.Single().Name);

        // Search by keyword
        var searchByName = await service.SearchAsync("CSLT 2", null, null, null);
        Assert.Single(searchByName);
        Assert.Equal("CSLT 2", searchByName.Single().Name);
    }

    [Fact]
    public async Task SoftDeleteAsync_SetsIsDeleted_AndLogsAudit()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new AccommodationService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var acc = new Accommodation { Name = "Nhà trọ Cẩm Phả", AddressLine = "Đường B", TypeCode = "RENTED", IsDeleted = false };
        var id = await service.SaveAsync(acc);

        await service.SoftDeleteAsync(id);

        var activeList = await service.SearchAsync(null, null, null, null);
        Assert.Empty(activeList);

        var dbRecord = await db.Context.Accommodations.FindAsync(id);
        Assert.NotNull(dbRecord);
        Assert.True(dbRecord!.IsDeleted);

        var logs = await db.Context.AuditLogs.ToListAsync();
        Assert.Contains(logs, l => l.Action == "DELETE" && l.EntityType == "Accommodation" && l.EntityId == id);
    }

    [Fact]
    public async Task AccommodationService_EnforcesRbacGuard()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);

        var reporterGuard = TestSecurityContext.CreateGuard(IrmRoles.Reporter);
        var serviceReporter = new AccommodationService(db.Context, audit, reporterGuard);

        var acc = new Accommodation { Name = "CSLT Protected", AddressLine = "Địa chỉ", TypeCode = "HOTEL" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceReporter.SaveAsync(acc));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceReporter.SoftDeleteAsync(1));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => serviceReporter.SaveAgreementAsync(new CompanyAccommodationAgreement()));

        var editorGuard = TestSecurityContext.CreateGuard(IrmRoles.DataEditor);
        var serviceEditor = new AccommodationService(db.Context, audit, editorGuard);
        var id = await serviceEditor.SaveAsync(acc);
        Assert.True(id > 0);
    }
}
