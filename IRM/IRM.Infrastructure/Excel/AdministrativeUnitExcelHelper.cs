using System.Globalization;
using ClosedXML.Excel;
using IRM.Application.Features.Administration.Contracts;
using IRM.Data.Models;

namespace IRM.Infrastructure.Excel;

public static class AdministrativeUnitExcelHelper
{
    private static readonly string[] CodeHeaders = ["mã địa bàn", "mã", "mã đơn vị", "code", "ma dia ban", "ma"];
    private static readonly string[] NameHeaders = ["tên địa bàn", "tên đơn vị", "tên", "địa bàn", "name", "ten dia ban", "ten"];
    private static readonly string[] TypeHeaders = ["loại", "loại đơn vị", "cấp", "type", "typecode", "loai"];
    private static readonly string[] ValidFromHeaders = ["hiệu lực từ", "ngày hiệu lực", "validfrom", "valid from", "hieu luc tu"];

    public static byte[] BuildTemplateWorkbook()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DiaBanHanhChinh");

        ws.Cell(1, 1).Value = "Mã địa bàn";
        ws.Cell(1, 2).Value = "Tên địa bàn";
        ws.Cell(1, 3).Value = "Loại";
        ws.Cell(1, 4).Value = "Hiệu lực từ";

        var header = ws.Range(1, 1, 1, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
        header.Style.Font.FontColor = XLColor.White;

        ws.Cell(2, 1).Value = "DB-W-001";
        ws.Cell(2, 2).Value = "Phường Trung Tâm";
        ws.Cell(2, 3).Value = "Phường";
        ws.Cell(2, 4).Value = "01/07/2025";

        ws.Cell(3, 1).Value = "DB-C-001";
        ws.Cell(3, 2).Value = "Xã Bình Minh";
        ws.Cell(3, 3).Value = "Xã";
        ws.Cell(3, 4).Value = "01/07/2025";

        ws.Column(4).Style.NumberFormat.Format = "@";
        ws.Columns(1, 4).AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public static AdministrativeUnitImportPreview ParseWorkbook(
        Stream stream,
        string fileName,
        IReadOnlyCollection<AdministrativeUnit> existingUnits)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("File Excel không có sheet nào.");

        var rangeUsed = ws.RangeUsed();
        var preview = new AdministrativeUnitImportPreview { FileName = fileName };
        if (rangeUsed is null)
        {
            return preview;
        }

        var firstRow = rangeUsed.FirstRow().RowNumber();
        var lastRow = rangeUsed.LastRow().RowNumber();
        var firstCol = rangeUsed.FirstColumn().ColumnNumber();
        var lastCol = rangeUsed.LastColumn().ColumnNumber();

        int codeCol = -1, nameCol = -1, typeCol = -1, validFromCol = -1;
        for (var c = firstCol; c <= lastCol; c++)
        {
            var headerText = ws.Cell(firstRow, c).GetString().Trim().ToLowerInvariant();
            if (codeCol < 0 && CodeHeaders.Contains(headerText)) codeCol = c;
            else if (nameCol < 0 && NameHeaders.Contains(headerText)) nameCol = c;
            else if (typeCol < 0 && TypeHeaders.Contains(headerText)) typeCol = c;
            else if (validFromCol < 0 && ValidFromHeaders.Contains(headerText)) validFromCol = c;
        }

        // Fallback to standard 4-column positional layout if header names are custom
        if (codeCol < 0 && nameCol < 0)
        {
            codeCol = firstCol;
            nameCol = firstCol + 1 <= lastCol ? firstCol + 1 : -1;
            typeCol = firstCol + 2 <= lastCol ? firstCol + 2 : -1;
            validFromCol = firstCol + 3 <= lastCol ? firstCol + 3 : -1;
        }

        var existingByCode = existingUnits
            .GroupBy(u => u.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(u => !u.IsDeleted).ThenByDescending(u => u.ValidFrom).First(), StringComparer.OrdinalIgnoreCase);

        var seenCodesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var r = firstRow + 1; r <= lastRow; r++)
        {
            var rawCode = codeCol > 0 ? ws.Cell(r, codeCol).GetString().Trim() : "";
            var rawName = nameCol > 0 ? ws.Cell(r, nameCol).GetString().Trim() : "";
            var rawType = typeCol > 0 ? ws.Cell(r, typeCol).GetString().Trim() : "";
            var validFromCell = validFromCol > 0 ? ws.Cell(r, validFromCol) : null;
            var rawValidFrom = validFromCell?.GetString().Trim() ?? "";

            // Skip completely blank trailing rows
            if (string.IsNullOrWhiteSpace(rawCode)
                && string.IsNullOrWhiteSpace(rawName)
                && string.IsNullOrWhiteSpace(rawType)
                && string.IsNullOrWhiteSpace(rawValidFrom))
            {
                continue;
            }

            var row = new AdministrativeUnitImportRow
            {
                RowNumber = r,
                Code = rawCode,
                Name = rawName
            };

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(rawCode))
            {
                errors.Add("Thiếu Mã địa bàn.");
            }
            else if (rawCode.Length > 450)
            {
                errors.Add("Mã địa bàn vượt quá giới hạn 450 ký tự.");
            }
            else if (!seenCodesInFile.Add(rawCode))
            {
                errors.Add($"Mã địa bàn '{rawCode}' bị trùng lặp trong cùng file Excel.");
            }

            if (string.IsNullOrWhiteSpace(rawName))
            {
                errors.Add("Thiếu Tên địa bàn.");
            }

            if (!TryNormalizeTypeCode(rawType, out var normalizedType))
            {
                errors.Add($"Loại địa bàn '{rawType}' không hợp lệ (chỉ nhận: Phường, Xã, Đặc khu, Tỉnh/TP).");
            }
            else
            {
                row.TypeCode = normalizedType;
            }

            if (!TryParseValidFrom(validFromCell, out var parsedDate))
            {
                errors.Add($"Ngày hiệu lực '{rawValidFrom}' sai định dạng chuẩn dd/MM/yyyy.");
            }
            else
            {
                row.ValidFrom = parsedDate;
            }

            if (errors.Count > 0)
            {
                row.Action = AdministrativeUnitImportAction.Error;
                row.ErrorMessage = string.Join(" ", errors);
                preview.ErrorCount++;
            }
            else if (existingByCode.TryGetValue(rawCode, out var existing))
            {
                row.Action = AdministrativeUnitImportAction.Update;
                row.WillReactivate = existing.IsDeleted || existing.ValidTo.HasValue;
                preview.UpdateCount++;
                preview.ValidCount++;
            }
            else
            {
                row.Action = AdministrativeUnitImportAction.Create;
                preview.CreateCount++;
                preview.ValidCount++;
            }

            preview.Rows.Add(row);
        }

        preview.TotalRows = preview.Rows.Count;
        return preview;
    }

    private static bool TryNormalizeTypeCode(string rawType, out string typeCode)
    {
        if (string.IsNullOrWhiteSpace(rawType))
        {
            typeCode = AdministrativeUnitTypeCodes.Commune;
            return true;
        }

        switch (rawType.Trim().ToLowerInvariant())
        {
            case "phường":
            case "phuong":
            case "ward":
                typeCode = AdministrativeUnitTypeCodes.Ward;
                return true;
            case "xã":
            case "xa":
            case "thị trấn":
            case "thi tran":
            case "commune":
                typeCode = AdministrativeUnitTypeCodes.Commune;
                return true;
            case "đặc khu":
            case "dac khu":
            case "special_zone":
            case "specialzone":
                typeCode = AdministrativeUnitTypeCodes.SpecialZone;
                return true;
            case "tỉnh/tp":
            case "tỉnh":
            case "tp":
            case "thành phố":
            case "province":
                typeCode = AdministrativeUnitTypeCodes.Province;
                return true;
            default:
                typeCode = "";
                return false;
        }
    }

    private static bool TryParseValidFrom(IXLCell? cell, out DateTime date)
    {
        if (cell is null || cell.IsEmpty())
        {
            date = DateTime.Today;
            return true;
        }

        if (cell.DataType == XLDataType.DateTime)
        {
            date = cell.GetDateTime().Date;
            return true;
        }

        var text = cell.GetString().Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            date = DateTime.Today;
            return true;
        }

        if (DateTime.TryParseExact(text, ["dd/MM/yyyy", "d/M/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            date = exact.Date;
            return true;
        }

        date = default;
        return false;
    }
}
