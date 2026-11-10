using IRM.Services;
using System.Xml.Linq;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.LegacySync;

public sealed class LegacySyncService : ILegacySyncService
{
    private readonly IrmDbContext _db;
    private readonly IForeignerRegistryStore _registry;
    public LegacySyncService(IrmDbContext db, IForeignerRegistryStore registry) { _db = db; _registry = registry; }

    public async Task<int> ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var events = await _db.LegacyChangeEvents.Where(x => x.StatusCode == "PENDING" || x.StatusCode == "RETRY")
            .OrderBy(x => x.Id).Take(batchSize).ToListAsync(cancellationToken);
        var processed = 0;
        foreach (var item in events)
        {
            try
            {
                if (item.EntityType is "Employees" or "Students")
                {
                    var xml = XElement.Parse(item.AfterXml ?? "<row />");
                    var nameField = item.EntityType == "Employees" ? "StaffName" : "FullName";
                    var person = await _registry.FindOrCreateMinimalAsync(Value(xml, nameField) ?? "Chưa xác định",
                        Value(xml, "Passport"), Value(xml, "Nationality"), cancellationToken);
                    var sourceType = item.EntityType == "Employees" ? "EMPLOYEE" : "STUDENT";
                    if (!await _db.ForeignPersonSourceLinks.AnyAsync(x => x.SourceType == sourceType
                        && x.SourceId == item.EntityId, cancellationToken))
                        _db.ForeignPersonSourceLinks.Add(new ForeignPersonSourceLink
                        { ForeignPersonId = person.Id, SourceType = sourceType, SourceId = item.EntityId });
                }
                item.StatusCode = "PROCESSED";
                item.ProcessedAt = DateTime.UtcNow;
                item.LastError = null;
                processed++;
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.StatusCode = item.RetryCount >= 5 ? "DEAD_LETTER" : "RETRY";
                item.LastError = ex.Message[..Math.Min(ex.Message.Length, 2000)];
            }
            await _db.SaveChangesAsync(cancellationToken);
        }
        return processed;
    }

    private static string? Value(XElement element, string name) =>
        element.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
}
