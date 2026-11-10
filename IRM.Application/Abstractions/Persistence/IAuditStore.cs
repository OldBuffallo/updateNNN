using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Administration; query execution and transactions remain in Infrastructure.</summary>
public interface IAuditStore
{
    Task LogAsync(string action, string entityType, int? entityId = null, string? description = null, string? username = null);
    Task LogWithChangesAsync(string action, string entityType, int? entityId, string? description, List<FieldChange>? changes, string? username = null);
    Task<List<AuditLog>> GetRecentAsync(int count = 100);
    Task<List<AuditLog>> SearchAsync(string? username = null, string? action = null, DateTime? from = null, DateTime? to = null);
}
