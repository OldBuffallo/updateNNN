using IRM.Services;
using IRM.Application.Features.ImportExport;
using static IRM.Application.Features.ImportExport.ImportMapping;
using System.Text.Json;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.ImportExport;

/// <summary>
/// Service xử lý nghiệp vụ Import Excel vào CSDL
/// </summary>
public partial class SqlImportStore
{

    // ══════════════════════════════════════════
    // AUTO-MAP COLUMNS
    // ══════════════════════════════════════════

    /// <summary>
    /// Tự động ghép cột Excel → trường hệ thống dựa trên header name
    /// </summary>
    public List<ColumnMapping> AutoMapColumns(List<ExcelColumnInfo> headers)
     => ImportMapping.AutoMapColumns(headers);

    // ══════════════════════════════════════════
    // TEMPLATE
    // ══════════════════════════════════════════

    /// <summary>
    /// Lưu template column mapping
    /// </summary>
    public async Task SaveTemplateAsync(string name, int? companyId, List<ColumnMapping> mappings, string? username)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var json = JsonSerializer.Serialize(mappings.Select(m => new { m.ExcelColumnIndex, m.SystemField }));

        var existing = await _db.ColumnMappingTemplates
            .FirstOrDefaultAsync(t => t.TemplateName == name && t.CompanyId == companyId);

        if (existing != null)
        {
            existing.MappingJson = json;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            _db.ColumnMappingTemplates.Add(new ColumnMappingTemplate
            {
                TemplateName = name,
                CompanyId = companyId,
                MappingJson = json,
                CreatedBy = username,
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Lấy danh sách template
    /// </summary>
    public async Task<List<ColumnMappingTemplate>> GetTemplatesAsync(int? companyId = null)
    {
        var query = _db.ColumnMappingTemplates.AsQueryable();
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId || t.CompanyId == null);
        return await query.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Áp dụng template vào danh sách headers
    /// </summary>
    public List<ColumnMapping> ApplyTemplate(ColumnMappingTemplate template, List<ExcelColumnInfo> headers)
     => ImportMapping.ApplyTemplate(template, headers);
}
