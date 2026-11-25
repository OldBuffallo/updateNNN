using IRM.Data.Models;

namespace IRM.Application.Features.ImportExport;

public enum NormalizedCellStatus
{
    Empty,
    Valid,
    AutoNormalized,
    NeedsReview
}

public sealed record NormalizedCellResult(string Value, NormalizedCellStatus Status, string? Note = null);

public sealed class ExcelFieldSelection
{
    public string SystemField { get; set; } = "";
    public string Label { get; set; } = "";
    public string Group { get; set; } = "core"; // "required" | "core" | "extended"
    public string Hint { get; set; } = "";
    public bool IsRequired { get; set; }
    public bool IsSelected { get; set; }
    public int? SourceColumnIndex { get; set; }
}

public sealed record ConvertedSampleCell(
    string SystemField,
    string RawValue,
    string NormalizedValue,
    NormalizedCellStatus Status,
    string? Note);

public sealed record ConvertedSampleRow(int RowNumber, List<ConvertedSampleCell> Cells);

public sealed record FieldIssueSummary(
    string SystemField,
    string Label,
    int Count,
    List<int> RowNumbers,
    List<string> SampleBadValues);

public sealed record ExcelNormalizedDiffItem(
    int RowNumber,
    string SystemField,
    string FieldLabel,
    string RawValue,
    string NormalizedValue,
    string? Note);

public sealed record ExcelActionIssueItem(
    int RowNumber,
    string SystemField,
    string FieldLabel,
    string RawValue,
    string Reason,
    string SuggestedFix,
    bool IsBlocking);

public sealed class ExcelConvertAnalysis
{
    public int TotalDataRows { get; set; }
    public int TotalSourceColumns { get; set; }
    public int AutoMatchedColumnCount { get; set; }
    public int SelectedColumnCount { get; set; }
    public int IgnoredColumnCount { get; set; }
    public List<string> IgnoredColumnHeaders { get; set; } = [];
    public List<int> MissingPassportRows { get; set; } = [];
    public int TotalAutoNormalizedCells { get; set; }
    public int TotalWarningCells { get; set; }
    public List<string> UnselectedCoreFieldLabels { get; set; } = [];
    public List<FieldIssueSummary> WarningFields { get; set; } = [];
    public List<FieldIssueSummary> EmptyCellFields { get; set; } = [];
    public List<ExcelNormalizedDiffItem> NormalizedChanges { get; set; } = [];
    public List<ExcelActionIssueItem> ActionIssues { get; set; } = [];
    public Dictionary<string, (string Raw, NormalizedCellResult Normalized, int AutoCount, int WarnCount)> FieldSummaries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ConvertedSampleRow> SampleRows { get; set; } = [];
}

public sealed record ExcelConvertOutput(
    byte[] WorkbookBytes,
    List<ColumnMapping> CleanMappings,
    ExcelConvertAnalysis Analysis);
