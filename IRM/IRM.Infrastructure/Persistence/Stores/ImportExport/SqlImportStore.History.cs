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
    // HISTORY
    // ══════════════════════════════════════════

    /// <summary>
    /// Lấy danh sách phiên import
    /// </summary>
    public async Task<List<ImportSessionInfo>> GetHistoryAsync()
    {
        return await _db.ImportHistories
            .OrderByDescending(h => h.ImportDate)
            .Take(50)
            .Select(h => new ImportSessionInfo
            {
                Id = h.Id,
                SessionId = h.SessionId,
                Time = h.ImportDate.ToString("dd/MM/yyyy HH:mm"),
                FileName = h.FileName,
                Company = h.CompanyName ?? "",
                Added = h.AddedRows,
                Updated = h.UpdatedRows,
                Errors = h.ErrorRows,
                Status = h.Status,
                User = h.Username
            })
            .ToListAsync();
    }
}
