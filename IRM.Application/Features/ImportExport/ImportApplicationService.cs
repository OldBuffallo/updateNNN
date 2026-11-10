using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.ImportExport;

public sealed class ImportApplicationService(IImportStore store, IServiceAuthorizationGuard guard) : IImportService
{
    private readonly IImportStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public List<ColumnMapping> AutoMapColumns(List<ExcelColumnInfo> headers)
    {
        var result = ImportMapping.AutoMapColumns(headers);
        return result;
    }

    public async Task<List<ImportPreviewRowDto>> GeneratePreviewAsync(Stream stream, List<ColumnMapping> mappings, int companyId)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.GeneratePreviewAsync(stream, mappings, companyId);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<ImportResult> ExecuteImportAsync(List<ImportPreviewRowDto> previewRows, Dictionary<int, string> duplicateDecisions, int companyId, string fileName, string? username = null)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var previewRowsData = previewRows.Select(item0 => ToEntity(item0)).ToList();
        var result = await _store.ExecuteImportAsync(previewRowsData, duplicateDecisions, companyId, fileName, username);
        for (var index = 0; index < previewRows.Count; index++)
            DtoMapper.CopyToDto(previewRowsData[index], previewRows[index]);
        return result;
    }

    public async Task<bool> RollbackImportAsync(string sessionId)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var result = await _store.RollbackImportAsync(sessionId);
        return result;
    }

    public async Task<List<ImportSessionInfo>> GetHistoryAsync()
    {
        var result = await _store.GetHistoryAsync();
        return result;
    }

    public async Task SaveTemplateAsync(string name, int? companyId, List<ColumnMapping> mappings, string? username)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        await _store.SaveTemplateAsync(name, companyId, mappings, username);
    }

    public async Task<List<ColumnMappingTemplateDto>> GetTemplatesAsync(int? companyId = null)
    {
        var result = await _store.GetTemplatesAsync(companyId);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public List<ColumnMapping> ApplyTemplate(ColumnMappingTemplateDto template, List<ExcelColumnInfo> headers)
    {
        var templateData = ToEntity(template);
        var result = ImportMapping.ApplyTemplate(templateData, headers);
        DtoMapper.CopyToDto(templateData, template);
        return result;
    }
}
