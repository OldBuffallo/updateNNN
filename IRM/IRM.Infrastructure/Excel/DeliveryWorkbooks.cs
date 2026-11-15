using System.Globalization;
using ClosedXML.Excel;

namespace IRM.Infrastructure.Excel;

public static class DeliveryWorkbooks
{
    private static readonly string[] SampleHeaders =
    [
        "Họ tên",
        "Giới tính",
        "Ngày sinh",
        "Quốc tịch",
        "Số hộ chiếu",
        "Địa chỉ",
        "Nghề nghiệp",
        "GPLĐ",
        "Số GPLĐ",
        "Số Visa",
        "Hạn tạm trú",
        "Ghi chú",
        "Thăm thân",
        "Tên người thân",
        "Quan hệ",
        "CMND người thân",
        "Ngày bắt đầu thăm thân",
        "Hạn thăm thân",
        "Ghi chú thăm thân"
    ];

    private static readonly string[][] SampleRows =
    [
        ["TEST ALEX MORGAN", "Nam", "15/03/1985", "USA", "IRMTEST26001", "Lô A01, Khu nhà mẫu, Phường Hồng Gai", "Kỹ sư", "TEST-GP-001", "TEST-GP-001", "TEST-V-001", "15/06/2027", "Hồ sơ mẫu kỹ sư có GPLĐ và thăm thân", "Có", "NGUYỄN THỊ LAN", "Vợ/Chồng", "000000000001", "01/11/2026", "01/05/2027", "Thăm thân định kỳ"],
        ["TEST KIM HANA", "Nữ", "22/07/1990", "KOR", "IRMTEST26002", "Lô A02, Khu nhà mẫu, Phường Bãi Cháy", "Kỹ sư", "Miễn", "", "TEST-V-002", "30/11/2026", "Hồ sơ mẫu diện miễn GPLĐ (sắp hết hạn tạm trú)", "Không", "", "", "", "", "", ""],
        ["TEST SATO KEN", "Nam", "25/04/1988", "JPN", "IRMTEST26003", "Lô A03, Khu nhà mẫu, Phường Cẩm Trung", "Kỹ sư", "", "", "TEST-V-003", "15/10/2026", "Hồ sơ mẫu chưa có GPLĐ (đã quá hạn tạm trú để thử cảnh báo)", "Không", "", "", "", "", "", ""],
        ["TEST LI MEI", "Nữ", "18/08/1992", "CHN", "IRMTEST26004", "Lô A04, Khu nhà mẫu, Phường Hồng Gai", "Kỹ sư", "TEST-GP-004", "TEST-GP-004", "TEST-V-004", "25/01/2027", "Hồ sơ mẫu kỹ sư có GPLĐ và thăm thân con", "Có", "LI XIAO MING", "Con", "000000000004", "10/11/2026", "10/01/2027", "Con sang thăm"],
        ["TEST DANIEL WEBER", "Nam", "20/09/1980", "AUS", "IRMTEST26005", "Lô A05, Khu nhà mẫu, Phường Bãi Cháy", "Kỹ sư", "Miễn", "", "TEST-V-005", "20/03/2028", "Hồ sơ mẫu diện miễn GPLĐ dài hạn", "Không", "", "", "", "", "", ""],
        ["TEST EMMA MARTIN", "Nữ", "16/11/1991", "GBR", "IRMTEST26006", "Lô A06, Khu nhà mẫu, Phường Cẩm Trung", "Kỹ sư", "TEST-GP-006", "TEST-GP-006", "TEST-V-006", "16/09/2027", "Hồ sơ mẫu kỹ sư có GPLĐ", "Không", "", "", "", "", "", ""],
        ["TEST JAMES WILSON", "Nam", "17/02/1987", "GBR", "IRMTEST26007", "Lô A07, Khu nhà mẫu, Phường Hồng Gai", "Kỹ sư", "", "", "TEST-V-007", "17/02/2027", "Hồ sơ mẫu đang chờ cấp GPLĐ (ô GPLĐ để trống)", "Không", "", "", "", "", "", ""],
        ["TEST OLIVIA BROWN", "Nữ", "24/05/1994", "AUS", "IRMTEST26008", "Lô A08, Khu nhà mẫu, Phường Bãi Cháy", "Kỹ sư", "Miễn", "", "TEST-V-008", "24/08/2027", "Hồ sơ mẫu miễn GPLĐ có thăm thân", "Có", "BROWN MICHAEL", "Vợ/Chồng", "000000000008", "15/11/2026", "15/02/2027", "Thăm thân ngắn hạn"],
        ["TEST ARJUN KUMAR", "Nam", "19/06/1986", "IND", "IRMTEST26009", "Lô A09, Khu nhà mẫu, Phường Cẩm Trung", "Kỹ sư", "TEST-GP-009", "TEST-GP-009", "TEST-V-009", "19/07/2027", "Hồ sơ mẫu kỹ sư Ấn Độ có GPLĐ", "Không", "", "", "", "", "", ""],
        ["TEST ANNA TRAN", "Nữ", "21/12/1993", "USA", "IRMTEST26010", "Lô A10, Khu nhà mẫu, Phường Hồng Gai", "Thân nhân", "Khác", "", "TEST-V-010", "21/12/2027", "Hồ sơ mẫu diện Thân nhân (GPLĐ = Khác, tự khớp nghề Thân nhân)", "Có", "TRẦN VĂN MINH", "Vợ/Chồng", "000000000010", "01/11/2026", "21/12/2027", "Thân nhân đi cùng"]
    ];

    private static readonly HashSet<int> TextColumnIndices = [5, 8, 9, 10, 16];
    private static readonly HashSet<int> DateColumnIndices = [3, 11, 17, 18];

    public static byte[] CreateSampleImportWorkbook()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Mẫu Import");

        foreach (var col in TextColumnIndices)
        {
            ws.Column(col).Style.NumberFormat.Format = "@";
        }

        for (var c = 0; c < SampleHeaders.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = SampleHeaders[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#3f5223");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        for (var r = 0; r < SampleRows.Length; r++)
        {
            var rowData = SampleRows[r];
            var rowNumber = r + 2;
            for (var c = 0; c < rowData.Length; c++)
            {
                var colNumber = c + 1;
                var rawValue = rowData[c];
                var cell = ws.Cell(rowNumber, colNumber);

                if (TextColumnIndices.Contains(colNumber))
                {
                    cell.Style.NumberFormat.Format = "@";
                    cell.SetValue(rawValue);
                }
                else if (DateColumnIndices.Contains(colNumber) && !string.IsNullOrWhiteSpace(rawValue))
                {
                    var dt = DateTime.ParseExact(rawValue, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else
                {
                    cell.SetValue(rawValue);
                }
            }
        }

        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents(1, 11, 12, 42);

        var guide = wb.Worksheets.Add("Hướng dẫn");
        var guideLines = new (string Topic, string Detail)[]
        {
            ("1. Điều kiện nạp thử 10/10 hồ sơ trên bản cài mới (R6)", "Tại bước 1 (Chọn nguồn dữ liệu): chọn Công ty 'CÔNG TY MẪU - HÃY ĐỔI TÊN' và Nhóm nghề 'Kỹ thuật', sau đó tải tệp IRM-MAU-IMPORT-TEST-DA-SUA.xlsx lên."),
            ("2. Quy tắc Mã Quốc Tịch (3 chữ cái chuẩn WPF)", "Cột 'Quốc tịch' sử dụng mã 3 chữ cái có sẵn trong danh mục mặc định (USA, KOR, JPN, CHN, AUS, GBR, IND...) hoặc tên tiếng Việt tương ứng."),
            ("3. Quy tắc Địa chỉ kết thúc bằng Phường/Xã", "Cột 'Địa chỉ' phải kết thúc bằng tên Phường/Xã đã có trong Danh mục. Bản cài mới tạo sẵn 3 phường: 'Phường Hồng Gai', 'Phường Bãi Cháy', 'Phường Cẩm Trung'."),
            ("4. Quy tắc Nghề nghiệp & Giấy phép lao động (GPLĐ)", "- Dòng 1..9 dùng nghề 'Kỹ sư' thuộc nhóm 'Kỹ thuật'.\n- Cột 'GPLĐ' hỗ trợ 4 giá trị: Mã số GPLĐ (ví dụ TEST-GP-001), 'Miễn', để trống (Chưa có), hoặc 'Khác' (dòng 10 'Thân nhân' tự động khớp sang nghề 'Thân nhân' thuộc nhóm 'Khác')."),
            ("5. Bảo toàn số 0 ở đầu mã định danh", "Các cột Số hộ chiếu, Số GPLĐ, Số Visa, CMND người thân được định dạng Text (@) để giữ nguyên số 0 đầu (ví dụ 000000000001)."),
            ("6. Cách thử phát hiện trùng & Hoàn tác", "Sau khi nạp thành công 10 hồ sơ mới, nạp lại đúng tệp này lần 2 sẽ báo 10 dòng 'Giữ nguyên (trùng)'. Bạn có thể sửa 1 ô trên Excel để thử chế độ 'Cập nhật' hoặc bấm 'Hoàn tác' trong Lịch sử Import.")
        };

        guide.Cell(1, 1).Value = "Mục hướng dẫn";
        guide.Cell(1, 2).Value = "Chi tiết áp dụng trên IRM v1.0.2 (R6)";
        guide.Range(1, 1, 1, 2).Style.Font.Bold = true;
        guide.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#3f5223");
        guide.Range(1, 1, 1, 2).Style.Font.FontColor = XLColor.White;

        for (var i = 0; i < guideLines.Length; i++)
        {
            guide.Cell(i + 2, 1).Value = guideLines[i].Topic;
            guide.Cell(i + 2, 1).Style.Font.Bold = true;
            guide.Cell(i + 2, 2).Value = guideLines[i].Detail;
            guide.Cell(i + 2, 2).Style.Alignment.WrapText = true;
        }
        guide.Column(1).Width = 42;
        guide.Column(2).Width = 95;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public static void WriteSampleImportWorkbook(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllBytes(path, CreateSampleImportWorkbook());
    }

    public static void WriteAll(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
        WriteSampleImportWorkbook(Path.Combine(directoryPath, "IRM-MAU-IMPORT-TEST-DA-SUA.xlsx"));
    }
}
