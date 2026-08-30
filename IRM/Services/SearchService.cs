using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Tìm kiếm toàn cục — full-text trên mọi trường
/// Sử dụng EF.Functions.Like thay vì ToLower().Contains() để tận dụng collation.
/// </summary>
public class SearchService
{
    private readonly IrmDbContext _db;
    public SearchService(IrmDbContext db) => _db = db;

    public async Task<SearchResult> SearchAsync(string keyword, string filter = "all")
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new SearchResult();

        var pattern = $"%{keyword.Trim()}%";
        var result = new SearchResult { Keyword = keyword };

        // Tìm nhân viên
        if (filter == "all" || filter == "employee")
        {
            result.Employees = await _db.Employees.AsNoTracking()
                .Include(e => e.Company)
                .Include(e => e.NationalityNav)
                .Include(e => e.Career)
                .Where(e => e.Hidden_flag == 0 && (
                    EF.Functions.Like(e.StaffName, pattern) ||
                    (e.Passport != null && EF.Functions.Like(e.Passport, pattern)) ||
                    (e.VisaNumber != null && EF.Functions.Like(e.VisaNumber, pattern)) ||
                    (e.WorkPermitNumber != null && EF.Functions.Like(e.WorkPermitNumber, pattern)) ||
                    (e.Address != null && EF.Functions.Like(e.Address, pattern)) ||
                    (e.Note != null && EF.Functions.Like(e.Note, pattern)) ||
                    // Mở rộng: tìm theo tên công ty
                    (e.Company != null && EF.Functions.Like(e.Company.CompanyName, pattern)) ||
                    // Mở rộng: tìm theo tên quốc tịch
                    (e.NationalityNav != null && EF.Functions.Like(e.NationalityNav.NationalityName, pattern)) ||
                    // Mở rộng: tìm theo tên nghề nghiệp
                    (e.Career != null && EF.Functions.Like(e.Career.CareerName, pattern)) ||
                    // Mở rộng: tìm theo thăm thân
                    (e.FamilyVisitRelativeName != null && EF.Functions.Like(e.FamilyVisitRelativeName, pattern)) ||
                    (e.FamilyVisitRelativeIdCard != null && EF.Functions.Like(e.FamilyVisitRelativeIdCard, pattern))
                ))
                .OrderBy(e => e.StaffName)
                .Take(100)
                .ToListAsync();
        }

        // Tìm công ty
        if (filter == "all" || filter == "company")
        {
            result.Companies = await _db.Companies.AsNoTracking()
                .Include(c => c.Field)
                .Where(c => c.Delete_flag == 0 && (
                    EF.Functions.Like(c.CompanyName, pattern) ||
                    (c.Address != null && EF.Functions.Like(c.Address, pattern)) ||
                    (c.LegalRepresentative != null && EF.Functions.Like(c.LegalRepresentative, pattern)) ||
                    (c.Note != null && EF.Functions.Like(c.Note, pattern)) ||
                    // Mở rộng: tìm theo lĩnh vực
                    (c.Field != null && EF.Functions.Like(c.Field.FieldName, pattern)) ||
                    // Mở rộng: tìm theo loại hình
                    (c.TypeOfBusiniess != null && EF.Functions.Like(c.TypeOfBusiniess, pattern))
                ))
                .OrderBy(c => c.CompanyName)
                .Take(50)
                .ToListAsync();
        }

        // Tìm du học sinh
        if (filter == "all" || filter == "student")
        {
            result.Students = await _db.Students.AsNoTracking()
                .Include(s => s.NationalityNav)
                .Where(s => s.Hidden_flag == 0 && (
                    EF.Functions.Like(s.FullName, pattern) ||
                    (s.Passport != null && EF.Functions.Like(s.Passport, pattern)) ||
                    (s.SchoolName != null && EF.Functions.Like(s.SchoolName, pattern)) ||
                    (s.Major != null && EF.Functions.Like(s.Major, pattern)) ||
                    (s.VisaNumber != null && EF.Functions.Like(s.VisaNumber, pattern)) ||
                    (s.StudentCode != null && EF.Functions.Like(s.StudentCode, pattern)) ||
                    (s.Address != null && EF.Functions.Like(s.Address, pattern)) ||
                    (s.Note != null && EF.Functions.Like(s.Note, pattern)) ||
                    (s.NationalityNav != null && EF.Functions.Like(s.NationalityNav.NationalityName, pattern))
                ))
                .OrderBy(s => s.FullName)
                .Take(100)
                .ToListAsync();
        }

        return result;
    }
}

public class SearchResult
{
    public string Keyword { get; set; } = "";
    public List<Employee> Employees { get; set; } = new();
    public List<Company> Companies { get; set; } = new();
    public List<Student> Students { get; set; } = new();
    public int TotalCount => Employees.Count + Companies.Count + Students.Count;
}
