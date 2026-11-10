using IRM.Data.Models;

namespace IRM.Services;

public interface ILegacySyncService
{
    Task<int> ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = default);
}
