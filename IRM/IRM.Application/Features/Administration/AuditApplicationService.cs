using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Administration;

public sealed class AuditApplicationService(IAuditStore store, IServiceAuthorizationGuard guard) : IAuditService
{
    private readonly IAuditStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task LogAsync(string action, string entityType, int? entityId = null, string? description = null, string? username = null)
    {
        await _store.LogAsync(action, entityType, entityId, description, username);
    }

    public async Task LogWithChangesAsync(string action, string entityType, int? entityId, string? description, List<FieldChange>? changes, string? username = null)
    {
        await _store.LogWithChangesAsync(action, entityType, entityId, description, changes, username);
    }

    public async Task<List<AuditLogDto>> GetRecentAsync(int count = 100)
    {
        var result = await _store.GetRecentAsync(count);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<AuditLogDto>> SearchAsync(string? username = null, string? action = null, DateTime? from = null, DateTime? to = null)
    {
        var result = await _store.SearchAsync(username, action, from, to);
        return result.Select(item0 => ToDto(item0)).ToList();
    }
}
