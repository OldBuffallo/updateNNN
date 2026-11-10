using IRM.Data.Models;

namespace IRM.Services;

public interface ISchemaVersionService
{
    Task ValidateAsync(CancellationToken cancellationToken = default);
}
