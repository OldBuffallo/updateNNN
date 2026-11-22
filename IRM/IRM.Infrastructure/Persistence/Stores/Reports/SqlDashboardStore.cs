using IRM.Services;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.Reports;

/// <summary>
/// Service Dashboard — Lấy KPI, thống kê tổng quan
/// </summary>
public class SqlDashboardStore : IDashboardStore
{
    private readonly IrmDbContext _db;
    public SqlDashboardStore(IrmDbContext db) => _db = db;

    public async Task<DashboardData> GetDashboardAsync()
    {
        var data = new DashboardData();

        // KPI Cards
        data.TotalCompanies = await _db.Companies.CountAsync(c => c.Delete_flag == 0);
        data.TotalEmployees = await _db.Employees.CountAsync(e => e.Hidden_flag == 0 && e.WorkingStatus == 0);

        var today = DateTime.Today;
        var in30Days = today.AddDays(30);

        data.ExpiringCount = await _db.Employees.CountAsync(e =>
            e.Hidden_flag == 0 && e.WorkingStatus == 0
            && e.TemporaryStay != null
            && e.TemporaryStay >= today
            && e.TemporaryStay <= in30Days);

        data.WithWorkPermit = await _db.Employees.CountAsync(e =>
            e.Hidden_flag == 0 && e.WorkingStatus == 0
            && (e.WorkPermit == WorkPermitType.WorkerHasPermit || e.WorkPermit == WorkPermitType.InvestorHasPermit));

        // Thăm thân — V0.1.0 nguồn chính + legacy fallback
        var syncedEmpIds = await _db.ForeignPersonSourceLinks.AsNoTracking()
            .Where(x => x.SourceType == "EMPLOYEE")
            .Select(x => x.SourceId)
            .ToListAsync();
        var syncedSet = new HashSet<int>(syncedEmpIds.Select(s => int.TryParse(s, out var id) ? id : -1));

        var v010FamilyCount = await _db.StayCases.AsNoTracking()
            .CountAsync(x => x.PurposeCode == StayPurposeCodes.FamilyVisit
                && x.StatusCode == "ACTIVE"
                && x.ValidFrom <= today
                && (!x.ValidTo.HasValue || x.ValidTo.Value >= today));
        var legacyOnlyFamilyCount = await _db.Employees.AsNoTracking().CountAsync(e =>
            e.Hidden_flag == 0 && e.WorkingStatus == 0 && e.FamilyVisit == 1
            && !syncedSet.Contains(e.IDEmployee));
        data.FamilyVisitCount = v010FamilyCount + legacyOnlyFamilyCount;

        var v010FamilyExpiringCount = await _db.StayCases.AsNoTracking()
            .CountAsync(x => x.PurposeCode == StayPurposeCodes.FamilyVisit
                && x.StatusCode == "ACTIVE"
                && x.ValidTo.HasValue && x.ValidTo.Value >= today && x.ValidTo.Value <= in30Days);
        var legacyOnlyFamilyExpiringCount = await _db.Employees.AsNoTracking().CountAsync(e =>
            e.Hidden_flag == 0 && e.WorkingStatus == 0 && e.FamilyVisit == 1
            && !syncedSet.Contains(e.IDEmployee)
            && e.FamilyVisitEndDate != null
            && e.FamilyVisitEndDate >= today
            && e.FamilyVisitEndDate <= in30Days);
        data.FamilyVisitExpiringCount = v010FamilyExpiringCount + legacyOnlyFamilyExpiringCount;

        // Top 10 quốc tịch (bao gồm nhóm "Chưa rõ" cho hồ sơ thiếu quốc tịch)
        var natMap = await _db.Nationality.AsNoTracking()
            .ToDictionaryAsync(n => n.NationalityCode, n => n.NationalityName, StringComparer.OrdinalIgnoreCase);
        var activeNationalities = await _db.Employees.AsNoTracking()
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0)
            .Select(e => e.Nationality)
            .ToListAsync();
        data.NationalityStats = activeNationalities
            .GroupBy(code => !string.IsNullOrWhiteSpace(code) && natMap.TryGetValue(code.Trim(), out var name) ? name : "Chưa rõ")
            .Select(g => new ChartItem { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();

        // Thống kê GPLĐ
        data.WorkPermitStats = await _db.Employees
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0)
            .GroupBy(e => e.WorkPermit)
            .Select(g => new ChartItem
            {
                Label = g.Key == WorkPermitType.WorkerExempt ? "Miễn GPLĐ" :
                        g.Key == WorkPermitType.WorkerHasPermit ? "Đã có GPLĐ" :
                        g.Key == WorkPermitType.WorkerNoPermit ? "Chưa có GPLĐ" :
                        g.Key == WorkPermitType.InvestorExempt ? "NĐT miễn" :
                        g.Key == WorkPermitType.InvestorHasPermit ? "NĐT đã có" :
                        g.Key == WorkPermitType.InvestorNoPermit ? "NĐT chưa có" : "Khác",
                Value = g.Count()
            })
            .ToListAsync();

        // Nhân viên sắp hết hạn (chi tiết)
        data.ExpiringEmployees = await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.NationalityNav)
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0
                && e.TemporaryStay != null
                && e.TemporaryStay >= today
                && e.TemporaryStay <= today.AddDays(90))
            .OrderBy(e => e.TemporaryStay)
            .Take(50)
            .ToListAsync();

        // Du học sinh
        data.TotalStudents = await _db.Students.CountAsync(s =>
            s.Hidden_flag == 0 && s.Status == StudentStatus.Studying);
        data.StudentVisaExpiringCount = await _db.Students.CountAsync(s =>
            s.Hidden_flag == 0 && s.Status == StudentStatus.Studying
            && s.VisaExpiry != null
            && s.VisaExpiry >= today
            && s.VisaExpiry <= in30Days);

        return data;
    }
}
