using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Công ty — CRUD + tìm kiếm
/// </summary>
public class CompanyService
{
    private readonly IrmDbContext _db;
    public CompanyService(IrmDbContext db) => _db = db;

    public async Task<List<Company>> GetAllAsync(string? search = null, int? fieldId = null)
    {
        var query = _db.Companies
            .Include(c => c.Field)
            .Include(c => c.Tracker)
            .Where(c => c.Delete_flag == 0);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.CompanyName.ToLower().Contains(search) ||
                (c.Address != null && c.Address.ToLower().Contains(search)) ||
                (c.LegalRepresentative != null && c.LegalRepresentative.ToLower().Contains(search)));
        }

        if (fieldId.HasValue && fieldId > 0)
            query = query.Where(c => c.IDField == fieldId);

        return await query.OrderBy(c => c.CompanyName).ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _db.Companies
            .Include(c => c.Field)
            .Include(c => c.Tracker)
            .Include(c => c.Employees.Where(e => e.Hidden_flag == 0))
            .Include(c => c.PhoneNumbers.Where(p => p.Delete_flag == 0))
            .Include(c => c.Emails.Where(e => e.Delete_flag == 0))
            .Include(c => c.Investments)
            .Include(c => c.Attachments.Where(a => a.Delete_flag == 0))
            .FirstOrDefaultAsync(c => c.IDCompany == id);
    }

    public async Task<int> GetEmployeeCountAsync(int companyId)
    {
        return await _db.Employees.CountAsync(e =>
            e.IDCompany == companyId && e.Hidden_flag == 0 && e.WorkingStatus == 0);
    }

    public async Task CreateAsync(Company company)
    {
        company.Delete_flag = 0;
        company.UpdateDay = DateTime.Now.ToString("yyyy-MM-dd");
        _db.Companies.Add(company);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Company company)
    {
        company.UpdateDay = DateTime.Now.ToString("yyyy-MM-dd");
        _db.Companies.Update(company);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var company = await _db.Companies.FindAsync(id);
        if (company != null)
        {
            company.Delete_flag = 1; // soft delete
            await _db.SaveChangesAsync();
        }
    }
}
