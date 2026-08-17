using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class AccommodationService : IAccommodationService
{
    private readonly IrmDbContext _db;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public AccommodationService(IrmDbContext db, AuditService audit) { _db = db; _audit = audit; }
    public AccommodationService(IrmDbContext db, AuditService audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;

    public async Task<IReadOnlyList<Accommodation>> SearchAsync(string? search, int? administrativeUnitId,
        int? companyId, DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var at = (asOfDate ?? DateTime.Today).Date;
        var query = _db.Accommodations.AsNoTracking().Include(x => x.AdministrativeUnit)
            .Include(x => x.CompanyAgreements).ThenInclude(x => x.Company)
            .Include(x => x.ResidencePeriods).Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.AddressLine.Contains(term));
        }
        if (administrativeUnitId.HasValue) query = query.Where(x => x.AdministrativeUnitId == administrativeUnitId);
        if (companyId.HasValue) query = query.Where(x => x.CompanyAgreements.Any(a => a.CompanyId == companyId
            && a.ValidFrom <= at && (!a.ValidTo.HasValue || a.ValidTo.Value >= at)));
        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<int> SaveAsync(Accommodation accommodation, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(accommodation.Name) || string.IsNullOrWhiteSpace(accommodation.AddressLine))
            throw new ArgumentException("Tên và địa chỉ cơ sở lưu trú là bắt buộc.");
        accommodation.UpdatedAt = DateTime.UtcNow;
        if (accommodation.Id == 0) _db.Accommodations.Add(accommodation); else _db.Accommodations.Update(accommodation);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "Accommodation", accommodation.Id);
        return accommodation.Id;
    }

    public async Task<int> SaveAgreementAsync(CompanyAccommodationAgreement agreement, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (agreement.ValidTo.HasValue && agreement.ValidTo < agreement.ValidFrom)
            throw new ArgumentException("Khoảng thuê không hợp lệ.");
        if (agreement.Id == 0) _db.CompanyAccommodationAgreements.Add(agreement); else _db.CompanyAccommodationAgreements.Update(agreement);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "CompanyAccommodationAgreement", agreement.Id);
        return agreement.Id;
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var item = await _db.Accommodations.FindAsync([id], cancellationToken);
        if (item is null) return;
        item.IsDeleted = true;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("DELETE", "Accommodation", id, "Soft delete");
    }
}
