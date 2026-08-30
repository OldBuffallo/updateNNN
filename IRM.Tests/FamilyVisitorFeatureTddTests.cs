using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class FamilyVisitorFeatureTddTests
{
    [Fact]
    public async Task SaveFamilyVisitAsync_ExecutesAtomicTransaction_Across6Entities()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var unit = new AdministrativeUnit { Code = "ADM-01", Name = "Phường Bãi Cháy", TypeCode = AdministrativeUnitTypeCodes.Ward, ValidFrom = new DateTime(2020, 1, 1) };
        db.Context.AdministrativeUnits.Add(unit);
        await db.Context.SaveChangesAsync();

        var input = new FamilyVisitInput
        {
            FullName = "David Beckham",
            Gender = 1,
            Birthday = new DateTime(1975, 5, 2),
            NationalityCode = "GBR",
            PassportNumber = "GB-123456",
            RelativeName = "Victoria",
            Relationship = "Vợ",
            RelativeTypeCode = "VIETNAMESE_CITIZEN",
            RelativeIdNumber = "001200000001",
            RelativePhone = "0901234567",
            RelativeAddress = "Hạ Long, Quảng Ninh",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31),
            AdministrativeUnitId = unit.Id,
            ResidenceAddress = "123 Đường Hạ Long",
            ElectronicIdentityNumber = "EID-9999",
            DocumentNumber = "VISA-8888",
            DocumentTypeCode = ImmigrationDocumentTypeCodes.Visa,
            DocumentValidTo = new DateTime(2026, 12, 31)
        };

        var stayCaseId = await service.SaveFamilyVisitAsync(input);
        Assert.True(stayCaseId > 0);

        var person = await db.Context.ForeignPersons.SingleAsync();
        Assert.Equal("David Beckham", person.FullName);
        Assert.Equal("GB123456", person.PassportNumber);
        Assert.NotNull(person.PassportSearchKey);

        var stayCase = await db.Context.StayCases.Include(x => x.FamilyVisitDetail).SingleAsync();
        Assert.Equal(StayPurposeCodes.FamilyVisit, stayCase.PurposeCode);
        Assert.True(stayCase.IsPrimary);
        Assert.Equal("ACTIVE", stayCase.StatusCode);
        Assert.NotNull(stayCase.FamilyVisitDetail);
        Assert.Equal("Victoria", stayCase.FamilyVisitDetail!.RelativeName);
        Assert.Equal("VIETNAMESE_CITIZEN", stayCase.FamilyVisitDetail.RelativeTypeCode);

        var residence = await db.Context.ResidencePeriods.SingleAsync();
        Assert.Equal(unit.Id, residence.AdministrativeUnitId);
        Assert.Equal("123 Đường Hạ Long", residence.AddressLine);

        var eid = await db.Context.ElectronicIdentities.SingleAsync();
        Assert.Equal("EID-9999", eid.IdentityNumber);

        var doc = await db.Context.ImmigrationDocuments.SingleAsync();
        Assert.Equal("VISA-8888", doc.Number);

        var auditLog = await db.Context.AuditLogs.FirstOrDefaultAsync(x => x.EntityType == "StayCase");
        Assert.NotNull(auditLog);
        Assert.Equal("CREATE", auditLog!.Action);
    }

    [Fact]
    public async Task SaveFamilyVisitAsync_ReusesForeignPerson_ByPassportSearchKey()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        // First visit in 2025
        var input1 = new FamilyVisitInput
        {
            FullName = "David Beckham",
            PassportNumber = "gb-123456",
            NationalityCode = "GBR",
            RelativeName = "Victoria",
            Relationship = "Vợ",
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2025, 6, 30)
        };
        await service.SaveFamilyVisitAsync(input1);
        Assert.Single(db.Context.ForeignPersons);

        // Second visit in 2026 (non-overlapping)
        var input2 = new FamilyVisitInput
        {
            FullName = "David Beckham",
            PassportNumber = "GB 123456",
            NationalityCode = "GBR",
            RelativeName = "Victoria",
            Relationship = "Vợ",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 6, 30)
        };
        await service.SaveFamilyVisitAsync(input2);

        // ForeignPersons should still be exactly 1, but StayCases has 2
        Assert.Single(db.Context.ForeignPersons);
        Assert.Equal(2, db.Context.StayCases.Count());
    }

    [Fact]
    public async Task SaveFamilyVisitAsync_RejectsOverlappingStayCases()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var input1 = new FamilyVisitInput
        {
            FullName = "Alice",
            PassportNumber = "P001",
            RelativeName = "Relative 1",
            Relationship = "Mẹ",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31)
        };
        await service.SaveFamilyVisitAsync(input1);

        var inputOverlap = new FamilyVisitInput
        {
            FullName = "Alice",
            PassportNumber = "P001",
            RelativeName = "Relative 1",
            Relationship = "Mẹ",
            ValidFrom = new DateTime(2026, 6, 1),
            ValidTo = new DateTime(2027, 6, 1)
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveFamilyVisitAsync(inputOverlap));
        Assert.Contains("trùng", ex.Message);
        Assert.Single(db.Context.StayCases);
    }

    [Fact]
    public async Task SearchFamilyVisitsAsync_FiltersBySearchAndAdministrativeUnit()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var unitA = new AdministrativeUnit { Code = "A", Name = "Unit A", TypeCode = "W", ValidFrom = new DateTime(2020, 1, 1) };
        var unitB = new AdministrativeUnit { Code = "B", Name = "Unit B", TypeCode = "W", ValidFrom = new DateTime(2020, 1, 1) };
        db.Context.AdministrativeUnits.AddRange(unitA, unitB);
        await db.Context.SaveChangesAsync();

        await service.SaveFamilyVisitAsync(new FamilyVisitInput
        {
            FullName = "Person In Unit A",
            PassportNumber = "P_A",
            RelativeName = "Thân nhân A",
            Relationship = "Bố",
            AdministrativeUnitId = unitA.Id,
            ResidenceAddress = "Địa chỉ A",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31)
        });

        await service.SaveFamilyVisitAsync(new FamilyVisitInput
        {
            FullName = "Person In Unit B",
            PassportNumber = "P_B",
            RelativeName = "Thân nhân B",
            Relationship = "Mẹ",
            AdministrativeUnitId = unitB.Id,
            ResidenceAddress = "Địa chỉ B",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31)
        });

        var resultsUnitA = await service.SearchFamilyVisitsAsync(new ForeignerSearchFilter
        {
            AdministrativeUnitId = unitA.Id,
            AsOfDate = new DateTime(2026, 6, 1)
        });
        Assert.Single(resultsUnitA);
        Assert.Equal("Person In Unit A", resultsUnitA.Single().FullName);

        var searchByName = await service.SearchFamilyVisitsAsync(new ForeignerSearchFilter
        {
            Search = "Thân nhân B",
            AsOfDate = new DateTime(2026, 6, 1)
        });
        Assert.Single(searchByName);
        Assert.Equal("Person In Unit B", searchByName.Single().FullName);
    }

    [Fact]
    public async Task SoftDeleteAsync_CascadesStayCasesDeletedStatus()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var service = new FamilyVisitorService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var id = await service.SaveFamilyVisitAsync(new FamilyVisitInput
        {
            FullName = "To Delete",
            PassportNumber = "DEL-01",
            RelativeName = "Relative",
            Relationship = "Vợ",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 12, 31)
        });

        var person = await db.Context.ForeignPersons.SingleAsync();
        await service.SoftDeleteAsync(person.Id);

        var deletedPerson = await db.Context.ForeignPersons.FindAsync(person.Id);
        Assert.True(deletedPerson!.IsDeleted);

        var stayCase = await db.Context.StayCases.SingleAsync();
        Assert.Equal("DELETED", stayCase.StatusCode);
    }
}
