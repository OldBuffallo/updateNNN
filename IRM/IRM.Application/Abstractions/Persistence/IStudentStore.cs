using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Students; query execution and transactions remain in Infrastructure.</summary>
public interface IStudentStore
{
    Task<List<Student>> GetAllActiveAsync();
    Task<List<Student>> GetAllVisibleAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<List<Student>> GetBySchoolAsync(string schoolName);
    Task<List<Student>> GetExpiringVisaAsync(int days = 30);
    Task CreateAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(int id);
    Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0);
}
