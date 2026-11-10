using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for SearchResult; never tracked by EF.</summary>
public sealed class SearchResultDto
{
    public string Keyword { get; set; } = "";
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<CompanyDto> Companies { get; set; } = new();
    public List<StudentDto> Students { get; set; } = new();
    public int TotalCount => Employees.Count + Companies.Count + Students.Count;
}
