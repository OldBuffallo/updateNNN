using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Companies;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync(string? search = null, int? fieldId = null);
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<int> GetEmployeeCountAsync(int companyId);
    Task CreateAsync(CompanyDto company);
    Task UpdateAsync(CompanyDto company);
    Task DeleteAsync(int id);
}
