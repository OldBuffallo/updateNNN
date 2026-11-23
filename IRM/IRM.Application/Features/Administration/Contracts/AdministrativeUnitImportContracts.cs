namespace IRM.Application.Features.Administration.Contracts;

public enum AdministrativeUnitImportAction
{
    Create,
    Update,
    Error
}

public sealed class AdministrativeUnitImportRow
{
    public int RowNumber { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public DateTime ValidFrom { get; set; }
    public AdministrativeUnitImportAction Action { get; set; }
    public bool WillReactivate { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class AdministrativeUnitImportPreview
{
    public string FileName { get; set; } = "";
    public int TotalRows { get; set; }
    public int ValidCount { get; set; }
    public int CreateCount { get; set; }
    public int UpdateCount { get; set; }
    public int ErrorCount { get; set; }
    public List<AdministrativeUnitImportRow> Rows { get; set; } = new();
}

public sealed class AdministrativeUnitImportResult
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ReactivatedCount { get; set; }
    public int SkippedErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
