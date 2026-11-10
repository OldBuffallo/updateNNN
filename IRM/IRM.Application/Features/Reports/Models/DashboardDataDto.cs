using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for DashboardData; never tracked by EF.</summary>
public sealed class DashboardDataDto
{
    public int TotalCompanies { get; set; }
    public int TotalEmployees { get; set; }
    public int ExpiringCount { get; set; }
    public int WithWorkPermit { get; set; }
    public int FamilyVisitCount { get; set; }
    public int FamilyVisitExpiringCount { get; set; }
    public int TotalStudents { get; set; }
    public int StudentVisaExpiringCount { get; set; }
    public List<ChartItem> NationalityStats { get; set; } = new();
    public List<ChartItem> WorkPermitStats { get; set; } = new();
    public List<EmployeeDto> ExpiringEmployees { get; set; } = new();
}
