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

    /// <summary>
    /// Đếm số lao động đã hết hạn tạm trú (TemporaryStay < hôm nay)
    /// </summary>
    public async Task<int> CountExpiredTemporaryStayAsync()
    {
        return await _db.Employees.CountAsync(e =>
            e.Hidden_flag == 0 && e.WorkingStatus == 0
            && e.TemporaryStay != null && e.TemporaryStay < DateTime.Today);
    }

    /// <summary>
    /// Đếm số lao động đã hết hạn thăm thân (FamilyVisitEndDate < hôm nay)
    /// </summary>
    public async Task<int> CountExpiredFamilyVisitAsync()
    {
        return await _db.Employees.CountAsync(e =>
            e.Hidden_flag == 0 && e.FamilyVisit == 1
            && e.FamilyVisitEndDate != null && e.FamilyVisitEndDate < DateTime.Today);
    }

    /// <summary>
    /// Di chuyển tất cả lao động hết hạn tạm trú sang bảng ArchivedEmployees
    /// </summary>
    public async Task<int> ArchiveExpiredTemporaryStayAsync(string? archivedBy = null)
    {
        var expired = await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.Career)
            .Where(e => e.Hidden_flag == 0 && e.WorkingStatus == 0
                && e.TemporaryStay != null && e.TemporaryStay < DateTime.Today)
            .ToListAsync();

        if (!expired.Any()) return 0;

        foreach (var emp in expired)
        {
            _db.ArchivedEmployees.Add(MapToArchive(emp, "HET_HAN_TAM_TRU", archivedBy));
            emp.Hidden_flag = 1; // soft delete from main table
        }

        await _db.SaveChangesAsync();
        return expired.Count;
    }

    /// <summary>
    /// Di chuyển tất cả lao động hết hạn thăm thân sang bảng ArchivedEmployees
    /// </summary>
    public async Task<int> ArchiveExpiredFamilyVisitAsync(string? archivedBy = null)
    {
        var expired = await _db.Employees
            .Include(e => e.Company)
            .Include(e => e.Career)
            .Where(e => e.Hidden_flag == 0 && e.FamilyVisit == 1
                && e.FamilyVisitEndDate != null && e.FamilyVisitEndDate < DateTime.Today)
            .ToListAsync();

        if (!expired.Any()) return 0;

        foreach (var emp in expired)
        {
            _db.ArchivedEmployees.Add(MapToArchive(emp, "HET_HAN_THAM_THAN", archivedBy));
            emp.Hidden_flag = 1;
        }

        await _db.SaveChangesAsync();
        return expired.Count;
    }

    private static ArchivedEmployee MapToArchive(Employee emp, string reason, string? archivedBy)
    {
        return new ArchivedEmployee
        {
            OriginalId = emp.IDEmployee,
            StaffName = emp.StaffName,
            Gender = emp.Gender,
            Birthday = emp.Birthday,
            Nationality = emp.Nationality,
            Passport = emp.Passport,
            Address = emp.Address,
            IDCareer = emp.IDCareer,
            WorkPermit = emp.WorkPermit,
            WorkPermitNumber = emp.WorkPermitNumber,
            VisaNumber = emp.VisaNumber,
            TemporaryStay = emp.TemporaryStay,
            Note = emp.Note,
            SettlementResults = emp.SettlementResults,
            SettlementResultsString = emp.SettlementResultsString,
            IDUser = emp.IDUser,
            IDCompany = emp.IDCompany,
            DateCreated = emp.DateCreated,
            CardCreationDate = emp.CardCreationDate,
            WorkingStatus = emp.WorkingStatus,
            DateOfJoin = emp.DateOfJoin,
            DateOfLeave = emp.DateOfLeave,
            FamilyVisit = emp.FamilyVisit,
            FamilyVisitRelativeName = emp.FamilyVisitRelativeName,
            FamilyVisitRelationship = emp.FamilyVisitRelationship,
            FamilyVisitRelativeIdCard = emp.FamilyVisitRelativeIdCard,
            FamilyVisitStartDate = emp.FamilyVisitStartDate,
            FamilyVisitEndDate = emp.FamilyVisitEndDate,
            FamilyVisitNote = emp.FamilyVisitNote,
            CompanyName = emp.Company?.CompanyName,
            CareerName = emp.Career?.CareerName,
            ArchiveReason = reason,
            ArchivedBy = archivedBy,
            ArchivedAt = DateTime.Now
        };
    }
}
