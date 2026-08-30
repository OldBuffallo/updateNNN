using IRM.Data.Models;
using IRM.Services;

namespace IRM.Tests;

public sealed class StatisticsFeatureTddTests
{
    [Fact]
    public void TemporalReportFilter_ValidatesMutualExclusivity_AndRangeOrder()
    {
        var invalidFilterBoth = new TemporalReportFilter
        {
            AsOfDate = DateTime.Today,
            From = DateTime.Today.AddDays(-10),
            To = DateTime.Today
        };
        var exBoth = Assert.Throws<ArgumentException>(() => invalidFilterBoth.Validate());
        Assert.Contains("không được dùng đồng thời", exBoth.Message);

        var invalidRangeOrder = new TemporalReportFilter
        {
            From = DateTime.Today,
            To = DateTime.Today.AddDays(-5)
        };
        var exOrder = Assert.Throws<ArgumentException>(() => invalidRangeOrder.Validate());
        Assert.Contains("From phải nhỏ hơn hoặc bằng To.", exOrder.Message);
    }

    [Fact]
    public async Task GetTerritoryAsync_DistinctCounting_AtAsOfDate()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var statsService = new StatisticsService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var unit = new AdministrativeUnit { Code = "ADM-HL", Name = "Hạ Long", TypeCode = "W", ValidFrom = new DateTime(2020, 1, 1) };
        var person = new ForeignPerson { FullName = "Distinct Tester", PassportSearchKey = "KEY1" };
        db.Context.AddRange(unit, person);
        await db.Context.SaveChangesAsync();

        // 1 Primary stay case (Work)
        db.Context.StayCases.Add(new StayCase
        {
            ForeignPersonId = person.Id,
            PurposeCode = StayPurposeCodes.Work,
            IsPrimary = true,
            StatusCode = "ACTIVE",
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2027, 1, 1)
        });

        // 2 residence periods at 2 different addresses within Hạ Long in 2026
        db.Context.ResidencePeriods.AddRange(
            new ResidencePeriod { ForeignPersonId = person.Id, AdministrativeUnitId = unit.Id, AddressLine = "Địa chỉ 1", ValidFrom = new DateTime(2026, 1, 1), ValidTo = new DateTime(2026, 6, 1) },
            new ResidencePeriod { ForeignPersonId = person.Id, AdministrativeUnitId = unit.Id, AddressLine = "Địa chỉ 2", ValidFrom = new DateTime(2026, 6, 2), ValidTo = new DateTime(2026, 12, 31) }
        );
        await db.Context.SaveChangesAsync();

        // Query at AsOfDate = 2026-03-01 -> Total should be exactly 1
        var result = await statsService.GetTerritoryAsync(new TemporalReportFilter
        {
            AdministrativeUnitId = unit.Id,
            AsOfDate = new DateTime(2026, 3, 1)
        });

        Assert.Equal(1, result.TotalForeigners);
        Assert.Equal(1, result.PeopleByPurpose[StayPurposeCodes.Work]);
    }

    [Fact]
    public async Task GetZoneStatisticsAsync_AggregatesCompaniesAndForeigners()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new AuditService(db.Context);
        var statsService = new StatisticsService(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var zone = new EconomicZone { Code = "KCN-DM", Name = "KCN Đông Mai", TypeCode = "IZ", ValidFrom = new DateTime(2020, 1, 1) };
        var field = new Field { FieldName = "F1" };
        db.Context.AddRange(zone, field);
        await db.Context.SaveChangesAsync();

        var company = new Company { CompanyName = "Comp in Zone", IDField = field.IDField, TrackerID = 1 };
        var person = new ForeignPerson { FullName = "Worker" };
        var unit = new AdministrativeUnit { Code = "U1", Name = "Unit 1", TypeCode = "W", ValidFrom = new DateTime(2020, 1, 1) };
        db.Context.AddRange(company, person, unit);
        await db.Context.SaveChangesAsync();

        var site = new CompanySite { CompanyId = company.IDCompany, AdministrativeUnitId = unit.Id, Name = "Site", AddressLine = "A", ValidFrom = new DateTime(2020, 1, 1) };
        db.Context.CompanySites.Add(site);
        await db.Context.SaveChangesAsync();

        db.Context.SiteZoneMemberships.Add(new SiteZoneMembership
        {
            CompanySiteId = site.Id,
            EconomicZoneId = zone.Id,
            ValidFrom = new DateTime(2020, 1, 1)
        });

        db.Context.StayCases.Add(new StayCase
        {
            ForeignPersonId = person.Id,
            SponsorCompanyId = company.IDCompany,
            PurposeCode = StayPurposeCodes.Work,
            IsPrimary = true,
            StatusCode = "ACTIVE",
            ValidFrom = new DateTime(2025, 1, 1),
            ValidTo = new DateTime(2027, 1, 1)
        });
        await db.Context.SaveChangesAsync();

        var zoneStats = await statsService.GetZoneStatisticsAsync(new TemporalReportFilter
        {
            AsOfDate = new DateTime(2026, 1, 1)
        });

        Assert.Contains(zoneStats, x => x.ZoneName == "KCN Đông Mai" && x.CompanyCount == 1 && x.ForeignPersonCount == 1);
    }
}
