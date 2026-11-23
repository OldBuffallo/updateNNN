using IRM.Application.Features.Administration.Contracts;
using IRM.Infrastructure.Excel;
using IRM.Services;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.Administration;

/// <summary>
/// Service Danh mục — Field, Career, Nationality, District, Ward
/// </summary>
public class SqlCatalogStore : ICatalogStore
{
    private readonly IrmDbContext _db;
    private readonly IServiceAuthorizationGuard? _guard;
    public SqlCatalogStore(IrmDbContext db) => _db = db;
    public SqlCatalogStore(IrmDbContext db, IServiceAuthorizationGuard guard) : this(db) => _guard = guard;

    // ── Fields (Lĩnh vực) ──
    public async Task<List<Field>> GetFieldsAsync()
        => await _db.Fields.Where(f => f.Delete_flag == 0).OrderBy(f => f.FieldName).ToListAsync();

    // ── Careers (Ngành nghề) ──
    public async Task<List<Career>> GetCareersAsync(int? careerGroupId = null)
    {
        var q = _db.Careers.Include(c => c.CareerGroup).Where(c => c.Delete_flag == 0);
        if (careerGroupId.HasValue)
            q = q.Where(c => c.IDCG == careerGroupId.Value);
        return await q.OrderBy(c => c.CareerName).ToListAsync();
    }

    public async Task<List<CareerGroup>> GetCareerGroupsAsync()
        => await _db.CareerGroups.OrderBy(cg => cg.CareerGroupName).ToListAsync();

    // ── Nationalities (Quốc tịch) ──
    public async Task<List<NationalityEntity>> GetNationalitiesAsync()
        => await _db.Nationality.Where(n => n.Delete_flag == 0)
            .OrderBy(n => n.NationalityName).ToListAsync();

    // ── Districts (Quận/huyện) ──
    public async Task<List<District>> GetDistrictsAsync()
        => await _db.Districts.Where(d => d.Delete_flag == 0)
            .OrderBy(d => d.DisTrictName).ToListAsync();

    // ── Wards (Phường/xã) ──
    public async Task<List<Ward>> GetWardsAsync()
        => await _db.Wards.Where(w => w.Delete_flag == 0)
            .OrderBy(w => w.WardName).ToListAsync();

    public async Task<List<AdministrativeUnit>> GetAdministrativeUnitsAsync(DateTime? asOfDate = null)
    {
        var at = (asOfDate ?? DateTime.Today).Date;
        return await _db.AdministrativeUnits.AsNoTracking()
            .Where(x => !x.IsDeleted && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
            .OrderBy(x => x.TypeCode).ThenBy(x => x.Name).ToListAsync();
    }

    public async Task<List<EconomicZone>> GetEconomicZonesAsync(DateTime? asOfDate = null)
    {
        var at = (asOfDate ?? DateTime.Today).Date;
        return await _db.EconomicZones.AsNoTracking()
            .Where(x => !x.IsDeleted && x.ValidFrom <= at && (!x.ValidTo.HasValue || x.ValidTo.Value >= at))
            .OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<int> SaveEconomicZoneAsync(EconomicZone zone)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(zone.Code) || string.IsNullOrWhiteSpace(zone.Name))
            throw new ArgumentException("Mã và tên khu/cụm là bắt buộc.");
        if (zone.Id == 0) _db.EconomicZones.Add(zone); else _db.EconomicZones.UpdateValues(_db, zone);
        await _db.SaveChangesAsync();
        return zone.Id;
    }

    public async Task<Field> CreateFieldAsync(Field field)
    {
        await RequireCatalogEditorAsync();
        field.Delete_flag = 0;
        _db.Fields.Add(field);
        await _db.SaveChangesAsync();
        return field;
    }

    public async Task<bool> UpdateFieldAsync(Field field)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Fields.FirstOrDefaultAsync(f => f.IDField == field.IDField && f.Delete_flag == 0);
        if (existing == null) return false;

        existing.FieldName = field.FieldName;
        existing.Description = field.Description;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteFieldAsync(int id)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Fields.FirstOrDefaultAsync(f => f.IDField == id && f.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Career> CreateCareerAsync(Career career)
    {
        await RequireCatalogEditorAsync();
        career.Delete_flag = 0;
        _db.Careers.Add(career);
        await _db.SaveChangesAsync();
        return career;
    }

    public async Task<bool> UpdateCareerAsync(Career career)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Careers.FirstOrDefaultAsync(c => c.IDCareer == career.IDCareer && c.Delete_flag == 0);
        if (existing == null) return false;

        existing.CareerName = career.CareerName;
        existing.IDCG = career.IDCG;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCareerAsync(int id)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Careers.FirstOrDefaultAsync(c => c.IDCareer == id && c.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<NationalityEntity> CreateNationalityAsync(NationalityEntity nationality)
    {
        await RequireCatalogEditorAsync();
        nationality.Delete_flag = 0;
        _db.Nationality.Add(nationality);
        await _db.SaveChangesAsync();
        return nationality;
    }

    public async Task<bool> UpdateNationalityAsync(NationalityEntity nationality)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Nationality.FirstOrDefaultAsync(n => n.IDNationality == nationality.IDNationality && n.Delete_flag == 0);
        if (existing == null) return false;

        existing.NationalityName = nationality.NationalityName;
        existing.NationalityCode = nationality.NationalityCode;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNationalityAsync(int id)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Nationality.FirstOrDefaultAsync(n => n.IDNationality == id && n.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<District> CreateDistrictAsync(District district)
    {
        await RequireCatalogEditorAsync();
        district.Delete_flag = 0;
        _db.Districts.Add(district);
        await _db.SaveChangesAsync();
        return district;
    }

    public async Task<bool> UpdateDistrictAsync(District district)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Districts.FirstOrDefaultAsync(d => d.IDDistrict == district.IDDistrict && d.Delete_flag == 0);
        if (existing == null) return false;

        existing.DisTrictName = district.DisTrictName;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDistrictAsync(int id)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Districts.FirstOrDefaultAsync(d => d.IDDistrict == id && d.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Ward> CreateWardAsync(Ward ward)
    {
        await RequireCatalogEditorAsync();
        ward.Delete_flag = 0;
        _db.Wards.Add(ward);
        await _db.SaveChangesAsync();
        return ward;
    }

    public async Task<bool> UpdateWardAsync(Ward ward)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Wards.FirstOrDefaultAsync(w => w.IDWard == ward.IDWard && w.Delete_flag == 0);
        if (existing == null) return false;

        existing.WardName = ward.WardName;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWardAsync(int id)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.Wards.FirstOrDefaultAsync(w => w.IDWard == id && w.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    private Task RequireCatalogEditorAsync() => _guard is null
        ? Task.CompletedTask
        : _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);

    // ── AdministrativeUnits (Địa bàn hành chính) ──

    public async Task<List<AdministrativeUnit>> GetAllAdministrativeUnitsAsync()
    {
        return await _db.AdministrativeUnits.AsNoTracking()
            .Include(x => x.Parent)
            .Include(x => x.Predecessor)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.TypeCode).ThenBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<AdministrativeUnit> CreateAdministrativeUnitAsync(AdministrativeUnit unit)
    {
        await RequireCatalogEditorAsync();
        unit.Code = (unit.Code ?? "").Trim();
        unit.Name = (unit.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(unit.Code) || string.IsNullOrWhiteSpace(unit.Name))
        {
            throw new InvalidOperationException("Mã và tên địa bàn không được để trống.");
        }

        var normalizedCode = unit.Code.ToUpperInvariant();
        var validFromDate = unit.ValidFrom.Date;
        var exists = await _db.AdministrativeUnits.AsNoTracking()
            .AnyAsync(x => x.Code.ToUpper() == normalizedCode && x.ValidFrom == validFromDate);
        if (exists)
        {
            throw new InvalidOperationException($"Địa bàn với mã '{unit.Code}' và ngày hiệu lực {validFromDate:dd/MM/yyyy} đã tồn tại.");
        }

        unit.ValidFrom = validFromDate;
        _db.AdministrativeUnits.Add(unit);
        await _db.SaveChangesAsync();
        return unit;
    }

    public async Task<bool> UpdateAdministrativeUnitAsync(AdministrativeUnit unit)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.AdministrativeUnits.FirstOrDefaultAsync(x => x.Id == unit.Id && !x.IsDeleted);
        if (existing == null) return false;

        var code = (unit.Code ?? "").Trim();
        var name = (unit.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Mã và tên địa bàn không được để trống.");
        }

        var normalizedCode = code.ToUpperInvariant();
        var validFromDate = unit.ValidFrom.Date;
        var duplicate = await _db.AdministrativeUnits.AsNoTracking()
            .AnyAsync(x => x.Id != unit.Id && x.Code.ToUpper() == normalizedCode && x.ValidFrom == validFromDate);
        if (duplicate)
        {
            throw new InvalidOperationException($"Địa bàn với mã '{code}' và ngày hiệu lực {validFromDate:dd/MM/yyyy} đã tồn tại.");
        }

        existing.Code = code;
        existing.Name = name;
        existing.TypeCode = unit.TypeCode;
        existing.ParentId = unit.ParentId;
        existing.ValidFrom = validFromDate;
        existing.ValidTo = unit.ValidTo;
        existing.LegacyDistrictId = unit.LegacyDistrictId;
        existing.LegacyWardId = unit.LegacyWardId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAdministrativeUnitAsync(int id, int? successorId)
    {
        await RequireCatalogEditorAsync();
        var existing = await _db.AdministrativeUnits.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (existing == null) return false;

        existing.ValidTo = DateTime.Today;
        existing.IsDeleted = true;

        if (successorId.HasValue)
        {
            var successor = await _db.AdministrativeUnits.FirstOrDefaultAsync(x => x.Id == successorId.Value);
            if (successor != null)
            {
                successor.PredecessorId = id;
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public byte[] GenerateAdministrativeUnitTemplate() => AdministrativeUnitExcelHelper.BuildTemplateWorkbook();

    public async Task<AdministrativeUnitImportPreview> PreviewAdministrativeUnitsExcelAsync(Stream stream, string fileName)
    {
        await RequireCatalogEditorAsync();
        var allExisting = await _db.AdministrativeUnits.AsNoTracking().ToListAsync();
        return AdministrativeUnitExcelHelper.ParseWorkbook(stream, fileName, allExisting);
    }

    public async Task<AdministrativeUnitImportResult> ImportAdministrativeUnitsExcelAsync(Stream stream, string fileName)
    {
        await RequireCatalogEditorAsync();
        var allExisting = await _db.AdministrativeUnits.ToListAsync();
        var preview = AdministrativeUnitExcelHelper.ParseWorkbook(stream, fileName, allExisting);

        var existingByCodeAndDate = allExisting
            .ToDictionary(
                u => (Code: u.Code.Trim().ToUpperInvariant(), ValidFrom: u.ValidFrom.Date),
                u => u);

        var existingByCode = allExisting
            .GroupBy(u => u.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(u => !u.IsDeleted).ThenByDescending(u => u.ValidFrom).First(), StringComparer.OrdinalIgnoreCase);

        var result = new AdministrativeUnitImportResult();

        foreach (var row in preview.Rows)
        {
            if (row.Action == AdministrativeUnitImportAction.Error)
            {
                result.SkippedErrorCount++;
                result.Errors.Add($"Dòng {row.RowNumber}: {row.ErrorMessage}");
                continue;
            }

            var targetDate = row.ValidFrom.Date;
            var exactKey = (Code: row.Code.Trim().ToUpperInvariant(), ValidFrom: targetDate);
            AdministrativeUnit? existing = null;
            if (existingByCodeAndDate.TryGetValue(exactKey, out var exactMatch))
            {
                existing = exactMatch;
            }
            else if (existingByCode.TryGetValue(row.Code, out var codeMatch))
            {
                existing = codeMatch;
            }

            if (existing is not null)
            {
                var wasInactive = existing.IsDeleted || existing.ValidTo.HasValue;
                var oldKey = (Code: existing.Code.Trim().ToUpperInvariant(), ValidFrom: existing.ValidFrom.Date);

                existing.Code = row.Code;
                existing.Name = row.Name;
                existing.TypeCode = row.TypeCode;
                if (oldKey != exactKey && !existingByCodeAndDate.ContainsKey(exactKey))
                {
                    existingByCodeAndDate.Remove(oldKey);
                    existing.ValidFrom = targetDate;
                    existingByCodeAndDate[exactKey] = existing;
                }
                existing.ValidTo = null;
                existing.IsDeleted = false;

                if (wasInactive)
                {
                    result.ReactivatedCount++;
                }
                else
                {
                    result.UpdatedCount++;
                }
            }
            else
            {
                var created = new AdministrativeUnit
                {
                    Code = row.Code,
                    Name = row.Name,
                    TypeCode = row.TypeCode,
                    ValidFrom = targetDate,
                    IsDeleted = false
                };
                _db.AdministrativeUnits.Add(created);
                existingByCode[row.Code] = created;
                existingByCodeAndDate[exactKey] = created;
                result.CreatedCount++;
            }
        }

        await _db.SaveChangesAsync();
        return result;
    }
}
