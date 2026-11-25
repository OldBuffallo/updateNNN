using System.Globalization;
using ClosedXML.Excel;
using IRM.Application.Abstractions;
using IRM.Application.Features.ImportExport;
using IRM.Application.Models;
using IRM.Data.Models;

namespace IRM.Infrastructure.Excel;

/// <summary>Phân tích file Excel nguồn và xuất file Excel chuẩn IRM chỉ gồm các trường người dùng đã chọn.</summary>
public sealed class ExcelWorkbookConverter : IExcelWorkbookConverter
{
    private static readonly HashSet<string> TextFormattedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Passport", "VisaNumber", "WorkPermitNumber", "WorkPermit",
        "FamilyVisitRelativeIdCard", "ElectronicIdentityNumber", "DocumentNumber",
        "Birthday", "TemporaryStay", "CardCreationDate",
        "FamilyVisitStartDate", "FamilyVisitEndDate",
        "StayValidFrom", "StayValidTo",
        "DocumentValidFrom", "DocumentValidTo",
        "ResidenceValidFrom", "ResidenceValidTo"
    };

    public ExcelConvertAnalysis Analyze(
        Stream stream,
        IReadOnlyList<ExcelColumnInfo> headers,
        IReadOnlyList<ExcelFieldSelection> selections,
        IReadOnlyList<NationalityEntityDto> nationalities,
        IReadOnlySet<string> activeAdminUnitCodes,
        ImportReadOptions options,
        int sampleLimit = 5)
    {
        var activeSelections = selections
            .Where(s => s.IsSelected && s.SourceColumnIndex is >= 0)
            .ToList();

        var mappings = activeSelections
            .Select(s => new ColumnMapping
            {
                ExcelColumnIndex = s.SourceColumnIndex!.Value,
                SystemField = s.SystemField,
                SystemFieldLabel = s.Label
            })
            .ToList();

        var rows = ReadRowsForConverter(stream, mappings, options);
        return AnalyzeRows(rows, headers, selections, activeSelections, nationalities, activeAdminUnitCodes, sampleLimit).Analysis;
    }

    public ExcelConvertOutput ConvertToCleanWorkbook(
        Stream stream,
        IReadOnlyList<ExcelFieldSelection> selections,
        IReadOnlyList<NationalityEntityDto> nationalities,
        IReadOnlySet<string> activeAdminUnitCodes,
        ImportReadOptions options)
    {
        var headers = ExcelReaderHelper.ReadHeaders(stream, options.SheetIndex, options.HeaderRow);
        var activeSelections = selections
            .Where(s => s.IsSelected && s.SourceColumnIndex is >= 0)
            .ToList();

        var mappings = activeSelections
            .Select(s => new ColumnMapping
            {
                ExcelColumnIndex = s.SourceColumnIndex!.Value,
                SystemField = s.SystemField,
                SystemFieldLabel = s.Label
            })
            .ToList();

        var rows = ReadRowsForConverter(stream, mappings, options);
        var (analysis, normalizedRows) = AnalyzeRows(rows, headers, selections, activeSelections, nationalities, activeAdminUnitCodes, sampleLimit: 5);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DuLieuChuanHoa");

        var cleanMappings = new List<ColumnMapping>(activeSelections.Count);
        for (int c = 0; c < activeSelections.Count; c++)
        {
            var sel = activeSelections[c];
            var canonicalLabel = SystemFields.All.GetValueOrDefault(sel.SystemField, sel.Label);
            var headerCell = ws.Cell(1, c + 1);
            headerCell.Value = canonicalLabel;
            headerCell.Style.Font.Bold = true;
            headerCell.Style.Fill.BackgroundColor = sel.IsRequired ? XLColor.FromHtml("#1565c0") : XLColor.FromHtml("#eef4fb");
            headerCell.Style.Font.FontColor = sel.IsRequired ? XLColor.White : XLColor.FromHtml("#0f2942");

            cleanMappings.Add(new ColumnMapping
            {
                ExcelColumnIndex = c,
                ExcelColumnName = canonicalLabel,
                SystemField = sel.SystemField,
                SystemFieldLabel = canonicalLabel,
                IsAutoMapped = true
            });
        }

        for (int r = 0; r < normalizedRows.Count; r++)
        {
            var rowCells = normalizedRows[r];
            for (int c = 0; c < activeSelections.Count; c++)
            {
                var sel = activeSelections[c];
                var cell = ws.Cell(r + 2, c + 1);
                var val = rowCells[c];
                if (TextFormattedFields.Contains(sel.SystemField))
                {
                    cell.Style.NumberFormat.Format = "@";
                }
                cell.Value = val;
            }
        }

        ws.Columns().AdjustToContents(12, 42);
        using var outStream = new MemoryStream();
        wb.SaveAs(outStream);
        return new ExcelConvertOutput(outStream.ToArray(), cleanMappings, analysis);
    }

    private static List<Dictionary<string, string>> ReadRowsForConverter(
        Stream stream, List<ColumnMapping> mappings, ImportReadOptions options)
    {
        if (mappings.Count == 0) return [];
        var startRow = options.StartRow > 0 ? options.StartRow : (options.HeaderRow > 0 ? options.HeaderRow + 1 : 0);
        return ExcelReaderHelper.ReadAllRows(stream, mappings, options.SheetIndex, startRow, options.EndRow);
    }

    private static (ExcelConvertAnalysis Analysis, List<string[]> NormalizedRows) AnalyzeRows(
        List<Dictionary<string, string>> rows,
        IReadOnlyList<ExcelColumnInfo> headers,
        IReadOnlyList<ExcelFieldSelection> allSelections,
        IReadOnlyList<ExcelFieldSelection> activeSelections,
        IReadOnlyList<NationalityEntityDto> nationalities,
        IReadOnlySet<string> activeAdminUnitCodes,
        int sampleLimit)
    {
        var selectedIndices = activeSelections
            .Where(s => s.SourceColumnIndex is >= 0)
            .Select(s => s.SourceColumnIndex!.Value)
            .ToHashSet();

        var analysis = new ExcelConvertAnalysis
        {
            TotalDataRows = rows.Count,
            TotalSourceColumns = headers.Count,
            AutoMatchedColumnCount = allSelections.Count(s => s.SourceColumnIndex is >= 0),
            SelectedColumnCount = activeSelections.Count,
            IgnoredColumnCount = Math.Max(0, headers.Count - selectedIndices.Count),
            IgnoredColumnHeaders = headers
                .Where(h => !selectedIndices.Contains(h.Index))
                .Select(h => string.IsNullOrWhiteSpace(h.HeaderName) ? $"Cột {ExcelValueNormalizer.ToExcelColumnName(h.Index)}" : h.HeaderName)
                .ToList(),
            UnselectedCoreFieldLabels = allSelections
                .Where(s => s.Group == "core" && (!s.IsSelected || s.SourceColumnIndex is null or < 0))
                .Select(s => s.Label)
                .ToList()
        };

        var warnMap = new Dictionary<string, (string Label, List<int> Rows, List<string> Samples)>(StringComparer.OrdinalIgnoreCase);
        var emptyMap = new Dictionary<string, (string Label, List<int> Rows)>(StringComparer.OrdinalIgnoreCase);
        var firstSample = new Dictionary<string, (string Raw, NormalizedCellResult Norm)>(StringComparer.OrdinalIgnoreCase);
        var perFieldAuto = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var perFieldWarn = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var hasPassportSelection = activeSelections.Any(s => s.SystemField.Equals("Passport", StringComparison.OrdinalIgnoreCase));
        var normalizedMatrix = new List<string[]>(rows.Count);

        for (int rIdx = 0; rIdx < rows.Count; rIdx++)
        {
            var rowData = rows[rIdx];
            var rowNumber = rowData.TryGetValue(ExcelReaderHelper.RowNumberKey, out var rnText) && int.TryParse(rnText, CultureInfo.InvariantCulture, out var parsedRn)
                ? parsedRn
                : rIdx + 2;

            if (!hasPassportSelection)
            {
                analysis.MissingPassportRows.Add(rowNumber);
                analysis.ActionIssues.Add(new ExcelActionIssueItem(
                    rowNumber, "Passport", "Số hộ chiếu", "(chưa ghép cột)",
                    "Chưa chọn cột Số hộ chiếu trong bảng ghép cột.",
                    "Chọn cột chứa Số hộ chiếu ở Nhóm 1 phía trên.",
                    IsBlocking: true));
            }

            var cleanValues = new string[activeSelections.Count];
            List<ConvertedSampleCell>? sampleCells = rIdx < sampleLimit ? new List<ConvertedSampleCell>(activeSelections.Count) : null;

            for (int c = 0; c < activeSelections.Count; c++)
            {
                var sel = activeSelections[c];
                var raw = rowData.GetValueOrDefault(sel.SystemField, "");
                var norm = ExcelValueNormalizer.Normalize(sel.SystemField, raw, nationalities, activeAdminUnitCodes);
                cleanValues[c] = norm.Value;

                if (!firstSample.ContainsKey(sel.SystemField) && !string.IsNullOrWhiteSpace(raw))
                    firstSample[sel.SystemField] = (raw, norm);

                if (sel.SystemField.Equals("Passport", StringComparison.OrdinalIgnoreCase) && norm.Status == NormalizedCellStatus.Empty)
                {
                    analysis.MissingPassportRows.Add(rowNumber);
                    analysis.ActionIssues.Add(new ExcelActionIssueItem(
                        rowNumber, sel.SystemField, sel.Label, raw,
                        "Thiếu Số hộ chiếu (trường định danh bắt buộc duy nhất).",
                        "Điền Số hộ chiếu hợp lệ (có chữ/số) vào dòng này trong file Excel.",
                        IsBlocking: true));
                }
                else if (norm.Status == NormalizedCellStatus.Empty && !sel.IsRequired)
                {
                    if (!emptyMap.TryGetValue(sel.SystemField, out var eItem))
                        emptyMap[sel.SystemField] = eItem = (sel.Label, []);
                    eItem.Rows.Add(rowNumber);
                }
                else if (norm.Status == NormalizedCellStatus.AutoNormalized)
                {
                    analysis.TotalAutoNormalizedCells++;
                    perFieldAuto[sel.SystemField] = perFieldAuto.GetValueOrDefault(sel.SystemField) + 1;
                    analysis.NormalizedChanges.Add(new ExcelNormalizedDiffItem(
                        rowNumber, sel.SystemField, sel.Label, raw, norm.Value, norm.Note));
                }
                else if (norm.Status == NormalizedCellStatus.NeedsReview)
                {
                    analysis.TotalWarningCells++;
                    perFieldWarn[sel.SystemField] = perFieldWarn.GetValueOrDefault(sel.SystemField) + 1;
                    if (!warnMap.TryGetValue(sel.SystemField, out var wItem))
                        warnMap[sel.SystemField] = wItem = (sel.Label, [], []);
                    wItem.Rows.Add(rowNumber);
                    if (wItem.Samples.Count < 3) wItem.Samples.Add(raw);

                    analysis.ActionIssues.Add(new ExcelActionIssueItem(
                        rowNumber, sel.SystemField, sel.Label, raw,
                        norm.Note ?? "Giá trị chưa đúng định dạng chuẩn IRM.",
                        BuildSuggestedFix(sel.SystemField),
                        IsBlocking: false));
                }

                sampleCells?.Add(new ConvertedSampleCell(sel.SystemField, raw, norm.Value, norm.Status, norm.Note));
            }

            normalizedMatrix.Add(cleanValues);
            if (sampleCells is not null)
                analysis.SampleRows.Add(new ConvertedSampleRow(rowNumber, sampleCells));
        }

        foreach (var sel in activeSelections)
        {
            var (raw, norm) = firstSample.TryGetValue(sel.SystemField, out var found)
                ? found
                : ("", new NormalizedCellResult("", NormalizedCellStatus.Empty));
            analysis.FieldSummaries[sel.SystemField] = (raw, norm, perFieldAuto.GetValueOrDefault(sel.SystemField), perFieldWarn.GetValueOrDefault(sel.SystemField));
        }

        analysis.WarningFields = warnMap
            .Select(kv => new FieldIssueSummary(kv.Key, kv.Value.Label, kv.Value.Rows.Count, kv.Value.Rows, kv.Value.Samples))
            .ToList();

        analysis.EmptyCellFields = emptyMap
            .Select(kv => new FieldIssueSummary(kv.Key, kv.Value.Label, kv.Value.Rows.Count, kv.Value.Rows, []))
            .ToList();

        return (analysis, normalizedMatrix);
    }

    private static string BuildSuggestedFix(string systemField)
    {
        if (ExcelValueNormalizer.DateFields.Contains(systemField))
            return "Sửa thành định dạng ngày dd/MM/yyyy (VD: 15/08/1990).";

        return systemField switch
        {
            "Nationality" => "Dùng mã quốc tịch 3 chữ cái (VD: KOR, JPN, CHN, USA) hoặc tên quốc gia có trong Danh mục Quốc tịch.",
            "Gender" => "Chỉ nhập Nam hoặc Nữ.",
            "AdministrativeUnitCode" => "Dùng mã địa bàn đang hoạt động trong Danh mục Địa bàn hành chính.",
            "StayPurposeCode" => "Dùng một trong các mã diện: WORK, STUDY, FAMILY_VISIT, OTHER.",
            "DocumentTypeCode" => "Dùng mã loại giấy tờ: VISA, VISA_EXTENSION, TRC, VISA_EXEMPTION, OTHER.",
            _ => "Kiểm tra lại định dạng giá trị hoặc để trống nếu chưa có thông tin."
        };
    }
}
