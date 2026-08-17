using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class InspectionService : IInspectionService
{
    private readonly IrmDbContext _db;
    private readonly IForeignerRegistryService _foreigners;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public InspectionService(IrmDbContext db, IForeignerRegistryService foreigners, AuditService audit)
    { _db = db; _foreigners = foreigners; _audit = audit; }
    public InspectionService(IrmDbContext db, IForeignerRegistryService foreigners, AuditService audit, IServiceAuthorizationGuard guard)
        : this(db, foreigners, audit) => _guard = guard;

    public async Task<IReadOnlyList<Inspection>> SearchAsync(DateTime? from, DateTime? to,
        int? administrativeUnitId, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var query = _db.Inspections.AsNoTracking().Include(x => x.AdministrativeUnit)
            .Include(x => x.Subjects).ThenInclude(x => x.ForeignPerson).AsQueryable();
        if (from.HasValue) query = query.Where(x => x.InspectedAt >= from.Value.Date);
        if (to.HasValue) query = query.Where(x => x.InspectedAt < to.Value.Date.AddDays(1));
        if (administrativeUnitId.HasValue) query = query.Where(x => x.AdministrativeUnitId == administrativeUnitId);
        return await query.OrderByDescending(x => x.InspectedAt).ToListAsync(cancellationToken);
    }

    public async Task<int> SaveAsync(Inspection inspection, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Inspector);
        if (inspection.InspectedAt == default) throw new ArgumentException("Thời gian kiểm tra là bắt buộc.");
        if (string.IsNullOrWhiteSpace(inspection.LocationText)) throw new ArgumentException("Địa điểm kiểm tra là bắt buộc.");
        if (inspection.Id == 0)
        {
            if (_guard is not null) inspection.CreatedByAccountId = await _guard.GetRequiredAccountIdAsync();
            _db.Inspections.Add(inspection);
        }
        else _db.Inspections.Update(inspection);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "Inspection", inspection.Id);
        return inspection.Id;
    }

    public async Task<int> AddSubjectAsync(int inspectionId, FamilyVisitInput personInput,
        InspectionSubject subject, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Inspector);
        var inspectionExists = await _db.Inspections.AnyAsync(x => x.Id == inspectionId, cancellationToken);
        if (!inspectionExists) throw new ArgumentException("Đợt kiểm tra không tồn tại.");
        var person = personInput.ForeignPersonId.HasValue
            ? await _db.ForeignPersons.FindAsync([personInput.ForeignPersonId.Value], cancellationToken)
            : await _foreigners.FindOrCreateMinimalAsync(personInput.FullName, personInput.PassportNumber,
                personInput.NationalityCode, cancellationToken);
        if (person is null) throw new ArgumentException("Người nước ngoài không tồn tại.");
        if (await _db.InspectionSubjects.AnyAsync(x => x.InspectionId == inspectionId && x.ForeignPersonId == person.Id, cancellationToken))
            throw new InvalidOperationException("Đối tượng đã có trong đợt kiểm tra.");
        subject.Id = 0;
        subject.InspectionId = inspectionId;
        subject.ForeignPersonId = person.Id;
        _db.InspectionSubjects.Add(subject);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("CREATE", "InspectionSubject", subject.Id,
            $"InspectionId={inspectionId}; ForeignPersonId={person.Id}; Result={subject.ResultCode}");
        return subject.Id;
    }

    public async Task<IReadOnlySet<int>> GetCheckedPersonIdsAsync(DateTime lookbackFrom, DateTime lookbackTo,
        int? administrativeUnitId, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        if (lookbackFrom.Date > lookbackTo.Date) throw new ArgumentException("Khoảng nhìn lại không hợp lệ.");
        var query = _db.InspectionSubjects.AsNoTracking().Where(x => x.Inspection!.InspectedAt >= lookbackFrom.Date
            && x.Inspection.InspectedAt < lookbackTo.Date.AddDays(1));
        if (administrativeUnitId.HasValue) query = query.Where(x => x.Inspection!.AdministrativeUnitId == administrativeUnitId);
        return (await query.Select(x => x.ForeignPersonId).Distinct().ToListAsync(cancellationToken)).ToHashSet();
    }
}
