namespace IRM.Application.Features.ImportExport;

public sealed class ImportRowException(string message) : InvalidOperationException(message);
