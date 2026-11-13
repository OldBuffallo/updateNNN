using ClosedXML.Excel;
using IRM.Data.Models;
using IRM.Features.ImportExport.Help;

namespace IRM.Infrastructure.Excel;

internal sealed record ImportSampleCatalogRow(string Group, string Code, string Name, string Context);

/// <summary>Formats a blank import template from shared guidance and materialized target catalogs.</summary>
internal static class ImportSampleWorkbook
{
    private static readonly string[] RequiredOrder = [
        "StaffName", "Passport", "Gender", "Birthday", "Nationality", "Address", "Career", "VisaNumber", "TemporaryStay"
    ];
    private static readonly string[] CatalogGroups = [
        "Quốc tịch", "Nghề nghiệp", "Nhóm nghề", "Phường/xã cho địa chỉ lõi", "Mã địa bàn cư trú", "Công ty", "Diện cư trú", "Loại giấy tờ"
    ];

    public static byte[] Create(IReadOnlyList<ImportSampleCatalogRow> catalogs, DateTime exportedAt)
    {
        using var book = new XLWorkbook();
        var keys = RequiredOrder.Concat(SystemFields.All.Keys.Where(key => key != "skip" && !RequiredOrder.Contains(key))).ToArray();
        var data = book.Worksheets.Add("Hồ sơ");
        data.SheetView.FreezeRows(1);
        for (var index = 0; index < keys.Length; index++)
        {
            var entry = ImportFieldHelpCatalog.All[keys[index]];
            var column = data.Column(index + 1);
            column.Width = index < 9 ? 24 : 28;
            column.Style.NumberFormat.Format = "@";
            var cell = data.Cell(1, index + 1);
            cell.Value = entry.Label;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(entry.IsRequired ? "#1559A8" : "#607086");
            cell.Style.Alignment.WrapText = true;
            cell.GetComment().AddText((entry.IsRequired ? "Bắt buộc. " : "Thông tin bổ sung. ") + entry.Summary + "\n" + entry.Recommendation + "\n" + entry.EmptyBehavior);
        }
        data.Row(1).Height = 44;
        data.Range(1, 1, 1, keys.Length).SetAutoFilter();
        data.TabColor = XLColor.FromHtml("#1559A8");

        var guide = book.Worksheets.Add("Hướng dẫn");
        guide.Cell(1, 1).Value = "HƯỚNG DẪN ĐIỀN 35 TRƯỜNG IMPORT";
        guide.Cell(1, 1).Style.Font.Bold = true;
        guide.Cell(1, 1).Style.Font.FontSize = 16;
        guide.Cell(2, 1).Value = "Nhập tại sheet Hồ sơ từ dòng 2, mỗi dòng một người; giữ nguyên tiêu đề. Chỉ dùng Nam hoặc Nữ; ngày dd/MM/yyyy, năm bốn chữ số. Số giấy tờ/mã: Text để giữ số 0 đầu. Ô trống khác với không ghép cột: một số ô trống có thể xóa/đổi dữ liệu khi cập nhật. Nếu không muốn ghép trường tùy chọn, bỏ cột đó hoặc điều chỉnh ghép cột; không bỏ chín cột bắt buộc. Ví dụ bên dưới chỉ để đọc, không phải dữ liệu sẽ import.";
        guide.Range(2, 1, 2, 10).Merge().Style.Alignment.WrapText = true;
        guide.Row(2).Height = 80;
        WriteHeader(guide, 3, ["Tên cột", "Yêu cầu", "Định dạng", "Ý nghĩa", "Chương trình kiểm tra", "Không ghép / ô trống", "Ví dụ có điều kiện", "Dễ nhập sai", "Cách xử lý", "Khuyến nghị"]);
        for (var index = 0; index < keys.Length; index++)
        {
            var entry = ImportFieldHelpCatalog.All[keys[index]];
            string[] values = [entry.Label, entry.IsRequired ? "Bắt buộc" : string.IsNullOrEmpty(entry.Condition) ? "Thông tin bổ sung" : entry.Condition,
                entry.ExcelType, entry.Summary, entry.Checks, entry.EmptyBehavior, entry.ValidExample, entry.Pitfall, entry.Troubleshooting, entry.Recommendation];
            for (var column = 0; column < values.Length; column++) guide.Cell(index + 4, column + 1).Value = values[column];
            guide.Row(index + 4).Height = 110;
        }
        guide.Columns(1, 3).Width = 25;
        guide.Columns(4, 10).Width = 45;
        guide.Range(4, 1, keys.Length + 3, 10).Style.Alignment.WrapText = true;
        guide.Range(1, 1, keys.Length + 3, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        guide.SheetView.FreezeRows(3);
        guide.Range(3, 1, keys.Length + 3, 10).SetAutoFilter();

        var reference = book.Worksheets.Add("Danh mục tham chiếu");
        reference.Cell(1, 1).Value = "DANH MỤC THỰC TẾ TẠI THỜI ĐIỂM TẢI";
        reference.Cell(1, 1).Style.Font.Bold = true;
        reference.Cell(2, 1).Value = $"Xuất lúc {exportedAt:dd/MM/yyyy HH:mm:ss}. Đối chiếu lại nếu danh mục thay đổi. Tên nghề phải đúng nhóm; mã quốc tịch không tự đổi sang ISO. Địa chỉ lõi phải kết thúc bằng tên phường/xã lõi; mã địa bàn cư trú là danh mục riêng. Mã nghề/công ty chỉ để tham chiếu: cột Nghề nghiệp và Công ty trong file dùng tên.";
        reference.Range(2, 1, 2, 4).Merge().Style.Alignment.WrapText = true;
        reference.Row(2).Height = 70;
        WriteHeader(reference, 3, ["Danh mục", "Mã tham chiếu", "Tên / Giá trị", "Lưu ý / Nhóm"]);
        var rowIndex = 4;
        foreach (var group in CatalogGroups)
        {
            var entries = catalogs.Where(row => row.Group == group).ToArray();
            if (entries.Length == 0) entries = [new(group, "", "Chưa có dữ liệu", "Chuẩn bị danh mục tại hệ thống đích; không thay giá trị theo suy đoán.")];
            foreach (var entry in entries)
            {
                string[] values = [entry.Group, entry.Code, entry.Name, entry.Context];
                for (var index = 0; index < values.Length; index++) reference.Cell(rowIndex, index + 1).Value = values[index];
                rowIndex++;
            }
        }
        reference.Column(1).Width = 29; reference.Column(2).Width = 32;
        reference.Column(3).Width = 42; reference.Column(4).Width = 72;
        reference.Columns(1, 4).Style.NumberFormat.Format = "@";
        reference.Range(4, 1, rowIndex - 1, 4).Style.Alignment.WrapText = true;
        reference.Range(3, 1, rowIndex - 1, 4).SetAutoFilter();
        reference.SheetView.FreezeRows(3);
        using var output = new MemoryStream(); book.SaveAs(output); return output.ToArray();
    }

    private static void WriteHeader(IXLWorksheet sheet, int row, string[] labels)
    {
        for (var i = 0; i < labels.Length; i++) sheet.Cell(row, i + 1).Value = labels[i];
        var header = sheet.Range(row, 1, row, labels.Length);
        header.Style.Font.Bold = true; header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1559A8");
    }
}
