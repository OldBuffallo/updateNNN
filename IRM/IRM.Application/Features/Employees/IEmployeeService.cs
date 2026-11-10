using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Employees;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllActiveAsync();
    Task<List<EmployeeDto>> GetByCompanyAsync(int companyId, bool includeHidden = false);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<List<EmployeeDto>> GetExpiringAsync(int days = 30);
    Task CreateAsync(EmployeeDto employee);
    Task UpdateAsync(EmployeeDto employee);
    Task DeleteAsync(int id);
    Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0);
    Task<int> CountExpiredTemporaryStayAsync();
    Task<int> CountExpiredFamilyVisitAsync();
    Task<int> ArchiveExpiredTemporaryStayAsync(string? archivedBy = null);
    Task<int> ArchiveExpiredFamilyVisitAsync(string? archivedBy = null);
}
