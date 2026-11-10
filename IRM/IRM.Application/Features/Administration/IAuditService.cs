using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Administration;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, int? entityId = null, string? description = null, string? username = null);
    Task LogWithChangesAsync(string action, string entityType, int? entityId, string? description, List<FieldChange>? changes, string? username = null);
    Task<List<AuditLogDto>> GetRecentAsync(int count = 100);
    Task<List<AuditLogDto>> SearchAsync(string? username = null, string? action = null, DateTime? from = null, DateTime? to = null);
}
