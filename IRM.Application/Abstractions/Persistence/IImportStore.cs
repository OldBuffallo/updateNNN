using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for ImportExport; query execution and transactions remain in Infrastructure.</summary>
public interface IImportStore
{
    Task<List<ImportPreviewRow>> GeneratePreviewAsync(Stream stream, List<ColumnMapping> mappings, int companyId);
    Task<ImportResult> ExecuteImportAsync(List<ImportPreviewRow> previewRows, Dictionary<int, string> duplicateDecisions, int companyId, string fileName, string? username = null);
    Task<bool> RollbackImportAsync(string sessionId);
    Task<List<ImportSessionInfo>> GetHistoryAsync();
    Task SaveTemplateAsync(string name, int? companyId, List<ColumnMapping> mappings, string? username);
    Task<List<ColumnMappingTemplate>> GetTemplatesAsync(int? companyId = null);
}
