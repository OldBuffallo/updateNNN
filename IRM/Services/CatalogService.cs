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
}
