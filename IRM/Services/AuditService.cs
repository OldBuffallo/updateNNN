using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service Nhật ký — Ghi log mọi thao tác
/// </summary>
public class AuditService
{
    private readonly IrmDbContext _db;
    public AuditService(IrmDbContext db) => _db = db;

    public async Task LogAsync(string action, string entityType, int? entityId = null,
        string? description = null, string? username = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            Username = username,
            Timestamp = DateTime.Now
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetRecentAsync(int count = 100)
    {
        return await _db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> SearchAsync(string? username = null,
        string? action = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(a => a.Username == username);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);
        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value.AddDays(1));

        return await query.OrderByDescending(a => a.Timestamp).Take(500).ToListAsync();
    }
}
