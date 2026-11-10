using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Students;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllActiveAsync();
    Task<List<StudentDto>> GetAllVisibleAsync();
    Task<StudentDto?> GetByIdAsync(int id);
    Task<List<StudentDto>> GetBySchoolAsync(string schoolName);
    Task<List<StudentDto>> GetExpiringVisaAsync(int days = 30);
    Task CreateAsync(StudentDto student);
    Task UpdateAsync(StudentDto student);
    Task DeleteAsync(int id);
    Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0);
}
