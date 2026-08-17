using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class ResidenceService : IResidenceService
{
    private readonly IrmDbContext _db;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public ResidenceService(IrmDbContext db, AuditService audit) { _db = db; _audit = audit; }
    public ResidenceService(IrmDbContext db, AuditService audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;

    public async Task<IReadOnlyList<ResidencePeriod>> GetHistoryAsync(int foreignPersonId, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        return await _db.ResidencePeriods.AsNoTracking().Include(x => x.AdministrativeUnit).Include(x => x.Accommodation)
            .Where(x => x.ForeignPersonId == foreignPersonId).OrderByDescending(x => x.ValidFrom).ToListAsync(cancellationToken);
    }

    public async Task<int> SaveAsync(ResidencePeriod residence, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (residence.ValidTo.HasValue && residence.ValidTo.Value.Date < residence.ValidFrom.Date)
            throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");
        var overlap = await _db.ResidencePeriods.AnyAsync(x => x.ForeignPersonId == residence.ForeignPersonId
            && x.Id != residence.Id
            && x.ValidFrom <= (residence.ValidTo ?? DateTime.MaxValue)
            && (!x.ValidTo.HasValue || x.ValidTo.Value >= residence.ValidFrom), cancellationToken);
        if (overlap) throw new InvalidOperationException("Khoảng cư trú bị chồng lấn.");
        if (residence.Id == 0) _db.ResidencePeriods.Add(residence); else _db.ResidencePeriods.Update(residence);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(residence.Id == 0 ? "CREATE" : "UPDATE", "ResidencePeriod", residence.Id);
        return residence.Id;
    }
}
