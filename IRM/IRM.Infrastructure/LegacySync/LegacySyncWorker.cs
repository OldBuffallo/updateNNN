using IRM.Services;
using System.Xml.Linq;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.LegacySync;

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
