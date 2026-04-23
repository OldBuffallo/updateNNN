using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Danh mục — Field, Career, Nationality, District, Ward
/// </summary>
public class CatalogService
{
    private readonly IrmDbContext _db;
    public CatalogService(IrmDbContext db) => _db = db;

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

    public async Task<Field> CreateFieldAsync(Field field)
    {
        field.Delete_flag = 0;
        _db.Fields.Add(field);
        await _db.SaveChangesAsync();
        return field;
    }

    public async Task<bool> UpdateFieldAsync(Field field)
    {
        var existing = await _db.Fields.FirstOrDefaultAsync(f => f.IDField == field.IDField && f.Delete_flag == 0);
        if (existing == null) return false;

        existing.FieldName = field.FieldName;
        existing.Description = field.Description;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteFieldAsync(int id)
    {
        var existing = await _db.Fields.FirstOrDefaultAsync(f => f.IDField == id && f.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Career> CreateCareerAsync(Career career)
    {
        career.Delete_flag = 0;
        _db.Careers.Add(career);
        await _db.SaveChangesAsync();
        return career;
    }

    public async Task<bool> UpdateCareerAsync(Career career)
    {
        var existing = await _db.Careers.FirstOrDefaultAsync(c => c.IDCareer == career.IDCareer && c.Delete_flag == 0);
        if (existing == null) return false;

        existing.CareerName = career.CareerName;
        existing.IDCG = career.IDCG;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCareerAsync(int id)
    {
        var existing = await _db.Careers.FirstOrDefaultAsync(c => c.IDCareer == id && c.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<NationalityEntity> CreateNationalityAsync(NationalityEntity nationality)
    {
        nationality.Delete_flag = 0;
        _db.Nationality.Add(nationality);
        await _db.SaveChangesAsync();
        return nationality;
    }

    public async Task<bool> UpdateNationalityAsync(NationalityEntity nationality)
    {
        var existing = await _db.Nationality.FirstOrDefaultAsync(n => n.IDNationality == nationality.IDNationality && n.Delete_flag == 0);
        if (existing == null) return false;

        existing.NationalityName = nationality.NationalityName;
        existing.NationalityCode = nationality.NationalityCode;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNationalityAsync(int id)
    {
        var existing = await _db.Nationality.FirstOrDefaultAsync(n => n.IDNationality == id && n.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<District> CreateDistrictAsync(District district)
    {
        district.Delete_flag = 0;
        _db.Districts.Add(district);
        await _db.SaveChangesAsync();
        return district;
    }

    public async Task<bool> UpdateDistrictAsync(District district)
    {
        var existing = await _db.Districts.FirstOrDefaultAsync(d => d.IDDistrict == district.IDDistrict && d.Delete_flag == 0);
        if (existing == null) return false;

        existing.DisTrictName = district.DisTrictName;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDistrictAsync(int id)
    {
        var existing = await _db.Districts.FirstOrDefaultAsync(d => d.IDDistrict == id && d.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Ward> CreateWardAsync(Ward ward)
    {
        ward.Delete_flag = 0;
        _db.Wards.Add(ward);
        await _db.SaveChangesAsync();
        return ward;
    }

    public async Task<bool> UpdateWardAsync(Ward ward)
    {
        var existing = await _db.Wards.FirstOrDefaultAsync(w => w.IDWard == ward.IDWard && w.Delete_flag == 0);
        if (existing == null) return false;

        existing.WardName = ward.WardName;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWardAsync(int id)
    {
        var existing = await _db.Wards.FirstOrDefaultAsync(w => w.IDWard == id && w.Delete_flag == 0);
        if (existing == null) return false;

        existing.Delete_flag = 1;
        await _db.SaveChangesAsync();
        return true;
    }
}
