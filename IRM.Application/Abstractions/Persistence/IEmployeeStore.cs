using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Employees; query execution and transactions remain in Infrastructure.</summary>
public interface IEmployeeStore
{
    Task<List<Employee>> GetAllActiveAsync();
    Task<List<Employee>> GetByCompanyAsync(int companyId, bool includeHidden = false);
    Task<Employee?> GetByIdAsync(int id);
    Task<List<Employee>> GetExpiringAsync(int days = 30);
    Task CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
    Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0);
    Task<int> CountExpiredTemporaryStayAsync();
    Task<int> CountExpiredFamilyVisitAsync();
    Task<int> ArchiveExpiredTemporaryStayAsync(string? archivedBy = null);
    Task<int> ArchiveExpiredFamilyVisitAsync(string? archivedBy = null);
}
