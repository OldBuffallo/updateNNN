using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.ImportExport;

public interface IImportService
{
    List<ColumnMapping> AutoMapColumns(List<ExcelColumnInfo> headers);
    Task<List<ImportPreviewRowDto>> GeneratePreviewAsync(Stream stream, List<ColumnMapping> mappings, int companyId);
    Task<ImportResult> ExecuteImportAsync(List<ImportPreviewRowDto> previewRows, Dictionary<int, string> duplicateDecisions, int companyId, string fileName, string? username = null);
    Task<bool> RollbackImportAsync(string sessionId);
    Task<List<ImportSessionInfo>> GetHistoryAsync();
    Task SaveTemplateAsync(string name, int? companyId, List<ColumnMapping> mappings, string? username);
    Task<List<ColumnMappingTemplateDto>> GetTemplatesAsync(int? companyId = null);
    List<ColumnMapping> ApplyTemplate(ColumnMappingTemplateDto template, List<ExcelColumnInfo> headers);
}
