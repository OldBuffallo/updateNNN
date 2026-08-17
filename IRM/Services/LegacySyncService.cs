using System.Xml.Linq;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class LegacySyncService : ILegacySyncService
{
    private readonly IrmDbContext _db;
    private readonly IForeignerRegistryService _registry;
    public LegacySyncService(IrmDbContext db, IForeignerRegistryService registry) { _db = db; _registry = registry; }

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

public sealed class LegacySyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LegacySyncWorker> _logger;
    public LegacySyncWorker(IServiceScopeFactory scopeFactory, ILogger<LegacySyncWorker> logger)
    { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ILegacySyncService>().ProcessPendingAsync(100, stoppingToken);
            }
            catch (Exception ex) { _logger.LogError(ex, "Legacy sync batch failed"); }
        }
    }
}
