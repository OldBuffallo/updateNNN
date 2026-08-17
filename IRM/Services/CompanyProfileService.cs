using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class CompanyProfileService : ICompanyProfileService
{
    private readonly IrmDbContext _db;
    private readonly IForeignerRegistryService _foreigners;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public CompanyProfileService(IrmDbContext db, IForeignerRegistryService foreigners, AuditService audit)
    { _db = db; _foreigners = foreigners; _audit = audit; }
    public CompanyProfileService(IrmDbContext db, IForeignerRegistryService foreigners, AuditService audit, IServiceAuthorizationGuard guard)
        : this(db, foreigners, audit) => _guard = guard;

    public async Task<CompanyProfileAggregate?> GetAsync(int companyId, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.IDCompany == companyId, cancellationToken);
        if (company is null) return null;
        var allFamily = await _foreigners.SearchFamilyVisitsAsync(new ForeignerSearchFilter { AsOfDate = DateTime.Today }, cancellationToken);
        return new CompanyProfileAggregate
        {
            Company = company,
            Profile = await _db.CompanyProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken),
            Sites = await _db.CompanySites.AsNoTracking().Include(x => x.AdministrativeUnit).Include(x => x.ZoneMemberships).ThenInclude(x => x.EconomicZone)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(cancellationToken),
            AccommodationAgreements = await _db.CompanyAccommodationAgreements.AsNoTracking().Include(x => x.Accommodation)
                .Where(x => x.CompanyId == companyId).OrderByDescending(x => x.ValidFrom).ToListAsync(cancellationToken),
            Representatives = await _db.CompanyRepresentatives.AsNoTracking().Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.ValidFrom).ToListAsync(cancellationToken),
            LegalDocuments = await _db.CompanyLegalDocuments.AsNoTracking().Include(x => x.StoredFile)
                .Where(x => x.CompanyId == companyId && !x.IsDeleted).ToListAsync(cancellationToken),
            SponsoredFamilyVisitors = allFamily.Where(x => x.SponsorCompanyId == companyId).ToList()
        };
    }

    public async Task SaveProfileAsync(CompanyProfile profile, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (profile.CompanyId <= 0) throw new ArgumentException("Doanh nghiệp không hợp lệ.");
        var existing = await _db.CompanyProfiles.FindAsync([profile.CompanyId], cancellationToken);
        if (existing is null) _db.CompanyProfiles.Add(profile);
        else
        {
            existing.OwnershipTypeCode = profile.OwnershipTypeCode;
            existing.TaxCode = profile.TaxCode;
            existing.BusinessRegistrationNumber = profile.BusinessRegistrationNumber;
            existing.ValidFrom = profile.ValidFrom;
            existing.ValidTo = profile.ValidTo;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "CompanyProfile", profile.CompanyId);
    }

    public async Task<int> SaveSiteAsync(CompanySite site, IEnumerable<int> economicZoneIds, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(site.Name) || string.IsNullOrWhiteSpace(site.AddressLine))
            throw new ArgumentException("Tên và địa chỉ địa điểm là bắt buộc.");
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        if (site.Id == 0) _db.CompanySites.Add(site); else _db.CompanySites.Update(site);
        await _db.SaveChangesAsync(cancellationToken);
        var old = await _db.SiteZoneMemberships.Where(x => x.CompanySiteId == site.Id).ToListAsync(cancellationToken);
        _db.SiteZoneMemberships.RemoveRange(old);
        foreach (var zoneId in economicZoneIds.Distinct())
            _db.SiteZoneMemberships.Add(new SiteZoneMembership { CompanySiteId = site.Id, EconomicZoneId = zoneId, ValidFrom = site.ValidFrom });
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "CompanySite", site.Id);
        await tx.CommitAsync(cancellationToken);
        return site.Id;
    }

    public async Task<int> SaveRepresentativeAsync(CompanyRepresentative representative, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(representative.FullName)) throw new ArgumentException("Tên người đại diện là bắt buộc.");
        if (representative.Id == 0) _db.CompanyRepresentatives.Add(representative); else _db.CompanyRepresentatives.Update(representative);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "CompanyRepresentative", representative.Id);
        return representative.Id;
    }
}
