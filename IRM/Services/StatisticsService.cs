using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly IrmDbContext _db;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public StatisticsService(IrmDbContext db, AuditService audit) { _db = db; _audit = audit; }
    public StatisticsService(IrmDbContext db, AuditService audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;

    public async Task<TerritoryStatistics> GetTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        filter.Validate();
        if (!filter.AdministrativeUnitId.HasValue) throw new ArgumentException("Phải chọn địa bàn.");
        var at = (filter.AsOfDate ?? filter.To ?? DateTime.Today).Date;
        var unitId = filter.AdministrativeUnitId.Value;
        var residentIds = CurrentResidences(at).Where(x => x.AdministrativeUnitId == unitId).Select(x => x.ForeignPersonId).Distinct();
        var primaryCases = CurrentPrimaryCases(at).Where(x => residentIds.Contains(x.ForeignPersonId));
        if (!string.IsNullOrWhiteSpace(filter.StayPurposeCode)) primaryCases = primaryCases.Where(x => x.PurposeCode == filter.StayPurposeCode);
        var grouped = await primaryCases.GroupBy(x => x.PurposeCode).Select(x => new { x.Key, Count = x.Select(y => y.ForeignPersonId).Distinct().Count() })
            .ToListAsync(cancellationToken);
        var companies = await _db.CompanySites.AsNoTracking().Where(x => !x.IsDeleted && x.AdministrativeUnitId == unitId
                && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
            .Select(x => x.CompanyId).Distinct().ToListAsync(cancellationToken);
        var companyRows = await _db.Companies.AsNoTracking().Where(x => companies.Contains(x.IDCompany))
            .Select(x => new CompanySummary { CompanyId = x.IDCompany, CompanyName = x.CompanyName }).ToListAsync(cancellationToken);
        var profiles = await _db.CompanyProfiles.AsNoTracking().Where(x => companies.Contains(x.CompanyId)).ToDictionaryAsync(x => x.CompanyId, cancellationToken);
        foreach (var company in companyRows)
        {
            company.OwnershipTypeCode = profiles.GetValueOrDefault(company.CompanyId)?.OwnershipTypeCode ?? "UNCLASSIFIED";
            company.CurrentWorkerCount = await primaryCases.CountAsync(x => x.PurposeCode == StayPurposeCodes.Work && x.SponsorCompanyId == company.CompanyId, cancellationToken);
        }
        var accommodationRows = await _db.Accommodations.AsNoTracking().Where(x => !x.IsDeleted && x.AdministrativeUnitId == unitId)
            .Select(x => new AccommodationSummary { AccommodationId = x.Id, Name = x.Name, TypeCode = x.TypeCode }).ToListAsync(cancellationToken);
        foreach (var accommodation in accommodationRows)
        {
            accommodation.CurrentResidentCount = await CurrentResidences(at).Where(x => x.AccommodationId == accommodation.AccommodationId)
                .Select(x => x.ForeignPersonId).Distinct().CountAsync(cancellationToken);
            accommodation.Companies = string.Join(", ", await _db.CompanyAccommodationAgreements.AsNoTracking()
                .Where(x => x.AccommodationId == accommodation.AccommodationId && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
                .Select(x => x.Company!.CompanyName).Distinct().ToListAsync(cancellationToken));
        }
        var unitName = await _db.AdministrativeUnits.Where(x => x.Id == unitId).Select(x => x.Name).SingleAsync(cancellationToken);
        return new TerritoryStatistics
        {
            AdministrativeUnitName = unitName,
            TotalForeigners = await primaryCases.Select(x => x.ForeignPersonId).Distinct().CountAsync(cancellationToken),
            PeopleByPurpose = grouped.ToDictionary(x => x.Key, x => x.Count),
            CompanyCount = companyRows.Count,
            AccommodationCount = accommodationRows.Count,
            Companies = companyRows,
            Accommodations = accommodationRows
        };
    }

    public async Task<IReadOnlyList<TerritoryOverview>> GetTerritoryOverviewAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        filter.Validate();
        var at = (filter.AsOfDate ?? filter.To ?? DateTime.Today).Date;
        var units = await _db.AdministrativeUnits.AsNoTracking().Where(x => !x.IsDeleted && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
            .OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var residencePairs = await CurrentResidences(at).Select(x => new { x.AdministrativeUnitId, x.ForeignPersonId }).Distinct().ToListAsync(cancellationToken);
        var personIds = residencePairs.Select(x => x.ForeignPersonId).Distinct().ToArray();
        var cases = await CurrentPrimaryCases(at).Where(x => personIds.Contains(x.ForeignPersonId))
            .Select(x => new { x.ForeignPersonId, x.PurposeCode }).ToListAsync(cancellationToken);
        return units.Select(unit =>
        {
            var ids = residencePairs.Where(x => x.AdministrativeUnitId == unit.Id).Select(x => x.ForeignPersonId).ToHashSet();
            var rows = cases.Where(x => ids.Contains(x.ForeignPersonId)).GroupBy(x => x.PurposeCode)
                .ToDictionary(x => x.Key, x => x.Select(y => y.ForeignPersonId).Distinct().Count());
            return new TerritoryOverview { AdministrativeUnitId = unit.Id, Code = unit.Code, Name = unit.Name,
                TypeCode = unit.TypeCode, TotalForeigners = ids.Count, PeopleByPurpose = rows };
        }).ToList();
    }

    public async Task<IReadOnlyList<ZoneStatistics>> GetZoneStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        filter.Validate();
        var at = (filter.AsOfDate ?? filter.To ?? DateTime.Today).Date;
        var zones = await _db.EconomicZones.AsNoTracking().Where(x => !x.IsDeleted && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
            .ToListAsync(cancellationToken);
        var result = new List<ZoneStatistics>();
        foreach (var zone in zones)
        {
            var companyIds = await _db.SiteZoneMemberships.AsNoTracking().Where(x => x.EconomicZoneId == zone.Id
                    && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
                .Select(x => x.CompanySite!.CompanyId).Distinct().ToListAsync(cancellationToken);
            result.Add(new ZoneStatistics
            {
                EconomicZoneId = zone.Id, ZoneName = zone.Name, TypeCode = zone.TypeCode,
                CompanyCount = companyIds.Count,
                ForeignPersonCount = await CurrentPrimaryCases(at).Where(x => x.SponsorCompanyId.HasValue && companyIds.Contains(x.SponsorCompanyId.Value))
                    .Select(x => x.ForeignPersonId).Distinct().CountAsync(cancellationToken)
            });
        }
        return result;
    }

    public async Task<DocumentIdentityStatistics> GetDocumentIdentityStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        filter.Validate();
        var at = (filter.AsOfDate ?? filter.To ?? DateTime.Today).Date;
        var personIds = CurrentPrimaryCases(at).Select(x => x.ForeignPersonId).Distinct();
        var documents = await _db.ImmigrationDocuments.AsNoTracking().Where(x => personIds.Contains(x.ForeignPersonId)
                && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at) && x.StatusCode == "VALID")
            .GroupBy(x => x.TypeCode).Select(x => new { x.Key, Count = x.Select(y => y.ForeignPersonId).Distinct().Count() })
            .ToListAsync(cancellationToken);
        var total = await personIds.CountAsync(cancellationToken);
        var withIdentity = await _db.ElectronicIdentities.AsNoTracking().Where(x => personIds.Contains(x.ForeignPersonId)
                && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at) && x.StatusCode == "ACTIVE")
            .Select(x => x.ForeignPersonId).Distinct().CountAsync(cancellationToken);
        await _audit.LogAsync("VIEW", "Statistics", null, $"AsOf={at:yyyy-MM-dd}");
        return new DocumentIdentityStatistics { DocumentsByType = documents.ToDictionary(x => x.Key, x => x.Count),
            WithElectronicIdentity = withIdentity, WithoutElectronicIdentity = total - withIdentity };
    }

    private IQueryable<StayCase> CurrentPrimaryCases(DateTime at) => _db.StayCases.AsNoTracking()
        .Where(x => x.IsPrimary && x.StatusCode == "ACTIVE" && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at));
    private IQueryable<ResidencePeriod> CurrentResidences(DateTime at) => _db.ResidencePeriods.AsNoTracking()
        .Where(x => x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at));
}
