namespace IRM.Application.Features.ImportExport;

public sealed record ImportReadOptions
{
    public int SheetIndex { get; init; }
    public int HeaderRow { get; init; }
    public int StartRow { get; init; }
    public int? EndRow { get; init; }
    public int CareerGroupId { get; init; }
    public bool AllowCreateCompanies { get; init; }
}
