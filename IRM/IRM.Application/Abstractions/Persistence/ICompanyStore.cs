using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Companies; query execution and transactions remain in Infrastructure.</summary>
public interface ICompanyStore
{
    Task<List<Company>> GetAllAsync(string? search = null, int? fieldId = null);
    Task<Company?> GetByIdAsync(int id);
    Task<int> GetEmployeeCountAsync(int companyId);
    Task CreateAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(int id);
}
