namespace IRM.Application.Abstractions;

public interface IDatabaseReadiness
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    string SchemaVersion { get; }
    string SchemaChecksum { get; }
}
