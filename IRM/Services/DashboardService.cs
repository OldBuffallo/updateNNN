using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Dashboard — Lấy KPI, thống kê tổng quan
/// </summary>
public class DashboardService
{
    private readonly IrmDbContext _db;
    public DashboardService(IrmDbContext db) => _db = db;

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
            && (e.WorkPermit == 1 || e.WorkPermit == 4));

        // Top 10 quốc tịch
        data.NationalityStats = await _db.Employees
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0 && e.Nationality != null)
            .Join(_db.Nationality, e => e.Nationality, n => n.NationalityCode,
                (e, n) => new { n.NationalityName })
            .GroupBy(x => x.NationalityName)
            .Select(g => new ChartItem { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync();

        // Thống kê GPLĐ
        data.WorkPermitStats = await _db.Employees
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0)
            .GroupBy(e => e.WorkPermit)
            .Select(g => new ChartItem
            {
                Label = g.Key == 0 ? "Miễn GPLĐ" :
                        g.Key == 1 ? "Đã có GPLĐ" :
                        g.Key == 2 ? "Chưa có GPLĐ" :
                        g.Key == 3 ? "NĐT miễn" :
                        g.Key == 4 ? "NĐT đã có" :
                        g.Key == 5 ? "NĐT chưa có" : "Khác",
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

        return data;
    }
}

// DTOs
public class DashboardData
{
    public int TotalCompanies { get; set; }
    public int TotalEmployees { get; set; }
    public int ExpiringCount { get; set; }
    public int WithWorkPermit { get; set; }
    public List<ChartItem> NationalityStats { get; set; } = new();
    public List<ChartItem> WorkPermitStats { get; set; } = new();
    public List<Employee> ExpiringEmployees { get; set; } = new();
}

public class ChartItem
{
    public string Label { get; set; } = "";
    public int Value { get; set; }
}
