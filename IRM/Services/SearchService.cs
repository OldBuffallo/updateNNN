using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Tìm kiếm toàn cục — full-text trên mọi trường
/// </summary>
public class SearchService
{
    private readonly IrmDbContext _db;
    public SearchService(IrmDbContext db) => _db = db;

    public async Task<SearchResult> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new SearchResult();

        var kw = keyword.ToLower().Trim();
        var result = new SearchResult { Keyword = keyword };

        // Tìm nhân viên
        result.Employees = await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.NationalityNav)
            .Include(e => e.Career)
            .Where(e => e.Hidden_flag == 0 && (
                e.StaffName.ToLower().Contains(kw) ||
                (e.Passport != null && e.Passport.ToLower().Contains(kw)) ||
                (e.VisaNumber != null && e.VisaNumber.ToLower().Contains(kw)) ||
                (e.WorkPermitNumber != null && e.WorkPermitNumber.ToLower().Contains(kw)) ||
                (e.Address != null && e.Address.ToLower().Contains(kw)) ||
                (e.Note != null && e.Note.ToLower().Contains(kw))
            ))
            .OrderBy(e => e.StaffName)
            .Take(100)
            .ToListAsync();

        // Tìm công ty
        result.Companies = await _db.Companies
            .Include(c => c.Field)
            .Where(c => c.Delete_flag == 0 && (
                c.CompanyName.ToLower().Contains(kw) ||
                (c.Address != null && c.Address.ToLower().Contains(kw)) ||
                (c.LegalRepresentative != null && c.LegalRepresentative.ToLower().Contains(kw)) ||
                (c.Note != null && c.Note.ToLower().Contains(kw))
            ))
            .OrderBy(c => c.CompanyName)
            .Take(50)
            .ToListAsync();

        return result;
    }
}

public class SearchResult
{
    public string Keyword { get; set; } = "";
    public List<Employee> Employees { get; set; } = new();
    public List<Company> Companies { get; set; } = new();
    public int TotalCount => Employees.Count + Companies.Count;
}
