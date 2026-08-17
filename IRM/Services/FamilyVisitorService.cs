using System.Security.Cryptography;
using System.Text;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class FamilyVisitorService : IForeignerRegistryService
{
    private readonly IrmDbContext _db;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;

    public FamilyVisitorService(IrmDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }
    public FamilyVisitorService(IrmDbContext db, AuditService audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;

    public static string? NormalizePassport(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();
    }

    public static string? SearchKey(string? value)
    {
        var normalized = NormalizePassport(value);
        return normalized is null ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    public async Task<IReadOnlyList<ForeignPerson>> SearchAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var query = _db.ForeignPersons.AsNoTracking().Include(x => x.StayCases).Include(x => x.ImmigrationDocuments)
            .Include(x => x.ElectronicIdentities).Include(x => x.ResidencePeriods).ThenInclude(x => x.AdministrativeUnit)
            .Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim(); var key = SearchKey(term);
            query = query.Where(x => x.FullName.Contains(term) || x.PassportSearchKey == key);
        }
        return await query.OrderBy(x => x.FullName).Take(500).ToListAsync(cancellationToken);
    }

    public async Task<int> SaveDocumentAsync(ImmigrationDocument document, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(document.Number)) throw new ArgumentException("Số giấy tờ là bắt buộc.");
        if (document.ValidTo.HasValue && document.ValidTo < document.ValidFrom) throw new ArgumentException("Hiệu lực giấy tờ không hợp lệ.");
        if (document.Id == 0) _db.ImmigrationDocuments.Add(document); else _db.ImmigrationDocuments.Update(document);
        await _db.SaveChangesAsync(cancellationToken); await _audit.LogAsync("UPSERT", "ImmigrationDocument", document.Id); return document.Id;
    }

    public async Task<int> SaveElectronicIdentityAsync(ElectronicIdentity identity, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(identity.IdentityNumber)) throw new ArgumentException("Số định danh là bắt buộc.");
        identity.IdentitySearchKey = SearchKey(identity.IdentityNumber)!;
        if (identity.Id == 0) _db.ElectronicIdentities.Add(identity); else _db.ElectronicIdentities.Update(identity);
        await _db.SaveChangesAsync(cancellationToken); await _audit.LogAsync("UPSERT", "ElectronicIdentity", identity.Id); return identity.Id;
    }

    public async Task<IReadOnlyList<FamilyVisitor>> SearchFamilyVisitsAsync(
        ForeignerSearchFilter filter, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var asOf = filter.AsOfDate.Date;
        var query = _db.StayCases.AsNoTracking()
            .Include(x => x.ForeignPerson)!.ThenInclude(x => x!.ElectronicIdentities)
            .Include(x => x.FamilyVisitDetail)
            .Where(x => x.PurposeCode == StayPurposeCodes.FamilyVisit && x.StatusCode != "DELETED")
            .Where(x => x.ValidFrom <= asOf && (!x.ValidTo.HasValue || x.ValidTo.Value >= asOf));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            var passportKey = SearchKey(term);
            query = query.Where(x => x.ForeignPerson!.FullName.Contains(term)
                || x.ForeignPerson.PassportSearchKey == passportKey
                || x.FamilyVisitDetail!.RelativeName.Contains(term)
                || (x.FamilyVisitDetail.RelativeIdNumber != null && x.FamilyVisitDetail.RelativeIdNumber.Contains(term)));
        }

        if (filter.AdministrativeUnitId.HasValue)
        {
            var unitId = filter.AdministrativeUnitId.Value;
            var personIds = _db.ResidencePeriods
                .Where(x => x.AdministrativeUnitId == unitId && x.ValidFrom <= asOf && (!x.ValidTo.HasValue || x.ValidTo.Value >= asOf))
                .Select(x => x.ForeignPersonId);
            query = query.Where(x => personIds.Contains(x.ForeignPersonId));
        }

        var cases = await query.OrderBy(x => x.ForeignPerson!.FullName).ToListAsync(cancellationToken);
        var personIdsForAddress = cases.Select(x => x.ForeignPersonId).Distinct().ToArray();
        var residences = await _db.ResidencePeriods.AsNoTracking()
            .Where(x => personIdsForAddress.Contains(x.ForeignPersonId) && x.ValidFrom <= asOf && (!x.ValidTo.HasValue || x.ValidTo.Value >= asOf))
            .OrderByDescending(x => x.ValidFrom)
            .ToListAsync(cancellationToken);

        return cases.Select(x => new FamilyVisitor
        {
            ForeignPersonId = x.ForeignPersonId,
            StayCaseId = x.Id,
            FullName = x.ForeignPerson!.FullName,
            Gender = x.ForeignPerson.Gender,
            Birthday = x.ForeignPerson.Birthday,
            Nationality = x.ForeignPerson.NationalityCode,
            Passport = x.ForeignPerson.PassportNumber,
            Address = residences.FirstOrDefault(r => r.ForeignPersonId == x.ForeignPersonId)?.AddressLine,
            RelativeName = x.FamilyVisitDetail?.RelativeName,
            RelativeIdCard = x.FamilyVisitDetail?.RelativeIdNumber,
            RelativePhone = x.FamilyVisitDetail?.RelativePhone,
            RelativeAddress = x.FamilyVisitDetail?.RelativeAddress,
            Relationship = x.FamilyVisitDetail?.Relationship,
            RelativeTypeCode = x.FamilyVisitDetail?.RelativeTypeCode ?? "VIETNAMESE_CITIZEN",
            RelativeNationality = x.FamilyVisitDetail?.RelativeNationalityCode,
            SponsorForeignPersonId = x.SponsorForeignPersonId,
            SponsorCompanyId = x.SponsorCompanyId,
            VisitStartDate = x.ValidFrom,
            VisitEndDate = x.ValidTo,
            ElectronicIdNumber = x.ForeignPerson.ElectronicIdentities
                .Where(e => e.ValidFrom <= asOf && (!e.ValidTo.HasValue || e.ValidTo.Value >= asOf) && e.StatusCode == "ACTIVE")
                .OrderByDescending(e => e.ValidFrom).Select(e => e.IdentityNumber).FirstOrDefault(),
            Note = x.Note
        }).ToList();
    }

    public async Task<int> SaveFamilyVisitAsync(FamilyVisitInput input, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        Validate(input);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var person = input.ForeignPersonId.HasValue
            ? await _db.ForeignPersons.FirstOrDefaultAsync(x => x.Id == input.ForeignPersonId.Value, cancellationToken)
            : null;

        if (person is null)
        {
            person = await FindOrCreateMinimalAsync(input.FullName, input.PassportNumber, input.NationalityCode, cancellationToken);
        }
        else
        {
            person.FullName = input.FullName.Trim();
            person.Gender = input.Gender;
            person.Birthday = input.Birthday;
            person.NationalityCode = input.NationalityCode;
            person.PassportNumber = NormalizePassport(input.PassportNumber);
            person.PassportSearchKey = SearchKey(input.PassportNumber);
            person.UpdatedAt = DateTime.UtcNow;
        }

        var overlaps = await _db.StayCases.AnyAsync(x => x.ForeignPersonId == person.Id
            && x.IsPrimary && x.StatusCode != "DELETED" && x.Id != input.StayCaseId
            && x.ValidFrom <= (input.ValidTo ?? DateTime.MaxValue)
            && (!x.ValidTo.HasValue || x.ValidTo.Value >= input.ValidFrom), cancellationToken);
        if (overlaps)
            throw new InvalidOperationException("Khoảng hiệu lực trùng với một diện cư trú chính khác.");

        var stay = input.StayCaseId.HasValue
            ? await _db.StayCases.Include(x => x.FamilyVisitDetail)
                .FirstOrDefaultAsync(x => x.Id == input.StayCaseId.Value, cancellationToken)
            : null;
        if (stay is null)
        {
            stay = new StayCase { ForeignPersonId = person.Id, PurposeCode = StayPurposeCodes.FamilyVisit };
            _db.StayCases.Add(stay);
        }

        stay.IsPrimary = true;
        stay.ValidFrom = input.ValidFrom.Date;
        stay.ValidTo = input.ValidTo?.Date;
        stay.SponsorCompanyId = input.SponsorCompanyId;
        stay.SponsorForeignPersonId = input.SponsorForeignPersonId;
        stay.StatusCode = "ACTIVE";
        stay.Note = input.Note;
        stay.UpdatedAt = DateTime.UtcNow;
        stay.FamilyVisitDetail ??= new FamilyVisitDetail { StayCase = stay };
        stay.FamilyVisitDetail.RelativeName = input.RelativeName.Trim();
        stay.FamilyVisitDetail.Relationship = input.Relationship.Trim();
        stay.FamilyVisitDetail.RelativeTypeCode = input.RelativeTypeCode;
        stay.FamilyVisitDetail.RelativeIdNumber = input.RelativeIdNumber;
        stay.FamilyVisitDetail.RelativePhone = input.RelativePhone;
        stay.FamilyVisitDetail.RelativeAddress = input.RelativeAddress;

        if (input.AdministrativeUnitId.HasValue && !string.IsNullOrWhiteSpace(input.ResidenceAddress))
        {
            _db.ResidencePeriods.Add(new ResidencePeriod
            {
                ForeignPerson = person,
                AdministrativeUnitId = input.AdministrativeUnitId.Value,
                AddressLine = input.ResidenceAddress.Trim(),
                ValidFrom = input.ValidFrom.Date,
                ValidTo = input.ValidTo?.Date
            });
        }

        if (!string.IsNullOrWhiteSpace(input.ElectronicIdentityNumber))
        {
            _db.ElectronicIdentities.Add(new ElectronicIdentity
            {
                ForeignPerson = person,
                IdentityNumber = input.ElectronicIdentityNumber.Trim(),
                IdentitySearchKey = SearchKey(input.ElectronicIdentityNumber)!,
                ValidFrom = input.ValidFrom.Date,
                ValidTo = input.ValidTo?.Date
            });
        }

        if (!string.IsNullOrWhiteSpace(input.DocumentNumber))
        {
            _db.ImmigrationDocuments.Add(new ImmigrationDocument
            {
                ForeignPerson = person,
                TypeCode = input.DocumentTypeCode ?? ImmigrationDocumentTypeCodes.Visa,
                Number = input.DocumentNumber.Trim(),
                ValidFrom = input.ValidFrom.Date,
                ValidTo = input.DocumentValidTo?.Date
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(input.StayCaseId.HasValue ? "UPDATE" : "CREATE", "StayCase", stay.Id,
            $"Diện thăm thân; ForeignPersonId={person.Id}");
        await transaction.CommitAsync(cancellationToken);
        return stay.Id;
    }

    public async Task SoftDeleteAsync(int foreignPersonId, CancellationToken cancellationToken = default)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var person = await _db.ForeignPersons.FindAsync([foreignPersonId], cancellationToken);
        if (person is null) return;
        person.IsDeleted = true;
        person.UpdatedAt = DateTime.UtcNow;
        foreach (var item in await _db.StayCases.Where(x => x.ForeignPersonId == foreignPersonId).ToListAsync(cancellationToken))
            item.StatusCode = "DELETED";
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("DELETE", "ForeignPerson", foreignPersonId, "Soft delete");
    }

    public async Task<ForeignPerson> FindOrCreateMinimalAsync(string fullName, string? passportNumber,
        string? nationalityCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Họ tên là bắt buộc.");
        var key = SearchKey(passportNumber);
        ForeignPerson? person = null;
        if (key is not null)
            person = await _db.ForeignPersons.FirstOrDefaultAsync(x => !x.IsDeleted && x.PassportSearchKey == key, cancellationToken);
        if (person is not null) return person;

        person = new ForeignPerson
        {
            FullName = fullName.Trim(),
            PassportNumber = NormalizePassport(passportNumber),
            PassportSearchKey = key,
            NationalityCode = nationalityCode,
            IsDataIncomplete = key is null
        };
        _db.ForeignPersons.Add(person);
        await _db.SaveChangesAsync(cancellationToken);
        return person;
    }

    public async Task<BackfillResult> BackfillLegacyAsync(CancellationToken cancellationToken = default)
    {
        var linked = 0;
        var created = 0;
        var issues = 0;
        foreach (var employee in await _db.Employees.AsNoTracking().Where(x => x.Hidden_flag == 0).ToListAsync(cancellationToken))
        {
            (linked, created, issues) = await LinkLegacyAsync("EMPLOYEE", employee.IDEmployee.ToString(), employee.StaffName,
                employee.Passport, employee.Nationality, employee.Birthday, StayPurposeCodes.Work,
                employee.DateCreated ?? DateTime.Today, linked, created, issues, cancellationToken);
        }
        foreach (var student in await _db.Students.AsNoTracking().Where(x => x.Hidden_flag == 0).ToListAsync(cancellationToken))
        {
            (linked, created, issues) = await LinkLegacyAsync("STUDENT", student.IDStudent.ToString(), student.FullName,
                student.Passport, student.Nationality, student.Birthday, StayPurposeCodes.Study,
                student.DateCreated ?? DateTime.Today, linked, created, issues, cancellationToken);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return new BackfillResult(linked, created, issues);
    }

    private async Task<(int linked, int created, int issues)> LinkLegacyAsync(string sourceType, string sourceId,
        string name, string? passport, string? nationality, DateTime? birthday, string purpose, DateTime from,
        int linked, int created, int issues, CancellationToken cancellationToken)
    {
        if (await _db.ForeignPersonSourceLinks.AnyAsync(x => x.SourceType == sourceType && x.SourceId == sourceId, cancellationToken))
            return (linked + 1, created, issues);
        var key = SearchKey(passport);
        var matches = key is null ? [] : await _db.ForeignPersons.Where(x => x.PassportSearchKey == key).ToListAsync(cancellationToken);
        if (key is null || matches.Count > 1)
        {
            _db.MigrationIssues.Add(new MigrationIssue
            {
                SourceType = sourceType, SourceId = sourceId,
                IssueCode = key is null ? "MISSING_PASSPORT" : "DUPLICATE_PASSPORT",
                Description = $"Cần xác minh thủ công hồ sơ {name}."
            });
            return (linked, created, issues + 1);
        }
        var person = matches.SingleOrDefault();
        if (person is null)
        {
            person = new ForeignPerson { FullName = name, Birthday = birthday, NationalityCode = nationality,
                PassportNumber = NormalizePassport(passport), PassportSearchKey = key };
            _db.ForeignPersons.Add(person);
            created++;
        }
        _db.ForeignPersonSourceLinks.Add(new ForeignPersonSourceLink { ForeignPerson = person, SourceType = sourceType, SourceId = sourceId });
        _db.StayCases.Add(new StayCase { ForeignPerson = person, PurposeCode = purpose, ValidFrom = from.Date, IsPrimary = true });
        return (linked + 1, created, issues);
    }

    private static void Validate(FamilyVisitInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName)) throw new ArgumentException("Họ tên là bắt buộc.");
        if (string.IsNullOrWhiteSpace(input.RelativeName)) throw new ArgumentException("Thông tin thân nhân là bắt buộc.");
        if (input.ValidTo.HasValue && input.ValidTo.Value.Date < input.ValidFrom.Date)
            throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");
    }
}
