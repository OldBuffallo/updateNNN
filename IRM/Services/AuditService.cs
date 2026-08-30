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

    /// <summary>
    /// Ghi log với structured JSON diff cho phép truy vấn chính xác "ai đã thay đổi field X".
    /// </summary>
    public async Task LogWithChangesAsync(string action, string entityType, int? entityId,
        string? description, List<FieldChange>? changes, string? username = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            ChangesJson = changes != null ? System.Text.Json.JsonSerializer.Serialize(changes) : null,
            Username = username,
            Timestamp = DateTime.Now
        };
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    /// <summary>Một field thay đổi trong structured audit log.</summary>
    public record FieldChange(string Field, string? Old, string? New);

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
