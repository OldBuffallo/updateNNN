using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Nhân viên — CRUD + thống kê
/// </summary>
public class EmployeeService
{
    private readonly IrmDbContext _db;
    public EmployeeService(IrmDbContext db) => _db = db;

    public async Task<List<Employee>> GetAllActiveAsync()
    {
        return await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.Career)
            .Include(e => e.NationalityNav)
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0)
            .OrderBy(e => e.StaffName)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetByCompanyAsync(int companyId, bool includeHidden = false)
    {
        var query = _db.Employees
            .Include(e => e.Company)
            .Include(e => e.Career)
            .Include(e => e.NationalityNav)
            .Where(e => e.IDCompany == companyId);

        if (!includeHidden)
            query = query.Where(e => e.Hidden_flag == 0);

        return await query.OrderBy(e => e.StaffName).ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.Career).ThenInclude(c => c!.CareerGroup)
            .Include(e => e.NationalityNav)
            .FirstOrDefaultAsync(e => e.IDEmployee == id);
    }

    public async Task<List<Employee>> GetExpiringAsync(int days = 30)
    {
        var deadline = DateTime.Today.AddDays(days);
        return await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.NationalityNav)
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0
                && e.TemporaryStay != null
                && e.TemporaryStay >= DateTime.Today
                && e.TemporaryStay <= deadline)
            .OrderBy(e => e.TemporaryStay)
            .ToListAsync();
    }

    public async Task CreateAsync(Employee employee)
    {
        employee.Hidden_flag = 0;
        employee.DateCreated = DateTime.Now;
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp != null)
        {
            emp.Hidden_flag = 1; // soft delete
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(passport)) return false;
        return await _db.Employees.AnyAsync(e =>
            e.Passport == passport && e.Hidden_flag == 0 && e.IDEmployee != excludeId);
    }
}
