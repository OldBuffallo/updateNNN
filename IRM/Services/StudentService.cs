using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Du học sinh — CRUD + thống kê
/// </summary>
public class StudentService
{
    private readonly IrmDbContext _db;
    public StudentService(IrmDbContext db) => _db = db;

    public async Task<List<Student>> GetAllActiveAsync()
    {
        return await _db.Students
            .Include(s => s.NationalityNav)
            .Where(s => s.Hidden_flag == 0 && s.Status == 0)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<List<Student>> GetAllVisibleAsync()
    {
        return await _db.Students
            .Include(s => s.NationalityNav)
            .Where(s => s.Hidden_flag == 0)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _db.Students
            .Include(s => s.NationalityNav)
            .FirstOrDefaultAsync(s => s.IDStudent == id);
    }

    public async Task<List<Student>> GetBySchoolAsync(string schoolName)
    {
        return await _db.Students
            .Include(s => s.NationalityNav)
            .Where(s => s.Hidden_flag == 0 && s.SchoolName != null && s.SchoolName.Contains(schoolName))
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<List<Student>> GetExpiringVisaAsync(int days = 30)
    {
        var deadline = DateTime.Today.AddDays(days);
        return await _db.Students
            .Include(s => s.NationalityNav)
            .Where(s => s.Hidden_flag == 0 && s.Status == 0
                && s.VisaExpiry != null
                && s.VisaExpiry >= DateTime.Today
                && s.VisaExpiry <= deadline)
            .OrderBy(s => s.VisaExpiry)
            .ToListAsync();
    }

    public async Task CreateAsync(Student student)
    {
        student.Hidden_flag = 0;
        student.DateCreated = DateTime.Now;
        _db.Students.Add(student);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _db.Students.Update(student);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var stu = await _db.Students.FindAsync(id);
        if (stu != null)
        {
            stu.Hidden_flag = 1; // soft delete
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(passport)) return false;
        return await _db.Students.AnyAsync(s =>
            s.Passport == passport && s.Hidden_flag == 0 && s.IDStudent != excludeId);
    }
}
