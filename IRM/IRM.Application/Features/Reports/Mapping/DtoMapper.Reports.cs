using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

internal static partial class DtoMapper
{
    public static DashboardDataDto ToDto(DashboardData value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static DashboardDataDto ToDto(DashboardData value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (DashboardDataDto)existing;
        var result = new DashboardDataDto();
        seen[value] = result;
        result.TotalCompanies = value.TotalCompanies;
        result.TotalEmployees = value.TotalEmployees;
        result.ExpiringCount = value.ExpiringCount;
        result.WithWorkPermit = value.WithWorkPermit;
        result.FamilyVisitCount = value.FamilyVisitCount;
        result.FamilyVisitExpiringCount = value.FamilyVisitExpiringCount;
        result.TotalStudents = value.TotalStudents;
        result.StudentVisaExpiringCount = value.StudentVisaExpiringCount;
        result.NationalityStats = value.NationalityStats;
        result.WorkPermitStats = value.WorkPermitStats;
        result.ExpiringEmployees = value.ExpiringEmployees.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static DashboardData ToEntity(DashboardDataDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static DashboardData ToEntity(DashboardDataDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (DashboardData)existing;
        var result = new DashboardData();
        seen[value] = result;
        result.TotalCompanies = value.TotalCompanies;
        result.TotalEmployees = value.TotalEmployees;
        result.ExpiringCount = value.ExpiringCount;
        result.WithWorkPermit = value.WithWorkPermit;
        result.FamilyVisitCount = value.FamilyVisitCount;
        result.FamilyVisitExpiringCount = value.FamilyVisitExpiringCount;
        result.TotalStudents = value.TotalStudents;
        result.StudentVisaExpiringCount = value.StudentVisaExpiringCount;
        result.NationalityStats = value.NationalityStats;
        result.WorkPermitStats = value.WorkPermitStats;
        result.ExpiringEmployees = value.ExpiringEmployees.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(DashboardData value, DashboardDataDto target)
    {
        var mapped = ToDto(value);
        target.TotalCompanies = mapped.TotalCompanies;
        target.TotalEmployees = mapped.TotalEmployees;
        target.ExpiringCount = mapped.ExpiringCount;
        target.WithWorkPermit = mapped.WithWorkPermit;
        target.FamilyVisitCount = mapped.FamilyVisitCount;
        target.FamilyVisitExpiringCount = mapped.FamilyVisitExpiringCount;
        target.TotalStudents = mapped.TotalStudents;
        target.StudentVisaExpiringCount = mapped.StudentVisaExpiringCount;
        target.NationalityStats = mapped.NationalityStats;
        target.WorkPermitStats = mapped.WorkPermitStats;
        target.ExpiringEmployees = mapped.ExpiringEmployees;
    }

    public static SearchResultDto ToDto(SearchResult value) => ToDto(value, new(ReferenceEqualityComparer.Instance));

    private static SearchResultDto ToDto(SearchResult value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (SearchResultDto)existing;
        var result = new SearchResultDto();
        seen[value] = result;
        result.Keyword = value.Keyword;
        result.Employees = value.Employees.Select(item0 => ToDto(item0, seen)).ToList();
        result.Companies = value.Companies.Select(item0 => ToDto(item0, seen)).ToList();
        result.Students = value.Students.Select(item0 => ToDto(item0, seen)).ToList();
        return result;
    }

    public static SearchResult ToEntity(SearchResultDto value) => ToEntity(value, new(ReferenceEqualityComparer.Instance));

    private static SearchResult ToEntity(SearchResultDto value, Dictionary<object, object> seen)
    {
        if (seen.TryGetValue(value, out var existing)) return (SearchResult)existing;
        var result = new SearchResult();
        seen[value] = result;
        result.Keyword = value.Keyword;
        result.Employees = value.Employees.Select(item0 => ToEntity(item0, seen)).ToList();
        result.Companies = value.Companies.Select(item0 => ToEntity(item0, seen)).ToList();
        result.Students = value.Students.Select(item0 => ToEntity(item0, seen)).ToList();
        return result;
    }

    public static void CopyToDto(SearchResult value, SearchResultDto target)
    {
        var mapped = ToDto(value);
        target.Keyword = mapped.Keyword;
        target.Employees = mapped.Employees;
        target.Companies = mapped.Companies;
        target.Students = mapped.Students;
    }
}
