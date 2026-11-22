using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.ImportExport;

public interface IExportService
{
    Task<byte[]> ExportCompaniesAsync(string? search = null, int? fieldId = null);
    Task<byte[]> ExportEmployeesAsync(int? companyId = null, string? nationality = null, WorkPermitType? workPermit = null, bool expiringOnly = false);
    Task<byte[]> ExportReportAsync(List<string> selectedColumns, string groupBy = "company", WorkPermitType? filterWorkPermit = null, string? filterNationality = null, int? filterExpiringDays = null);
    Task<byte[]> GenerateSampleExcelBytesAsync();
    Task<byte[]> ExportStudentsAsync(string? nationality = null, string? school = null, EducationLevel? educationLevel = null, ScholarshipType? scholarshipType = null, StudentStatus? status = null, bool expiringOnly = false);
}
