using ClosedXML.Excel;

var workbook = new XLWorkbook();
var ws = workbook.AddWorksheet("DanhSachNLD");

// ── Header ──
var headers = new[] {
    "STT", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch",
    "Số hộ chiếu", "Địa chỉ", "Chức danh", "GPLĐ",
    "Số Visa", "Hạn tạm trú", "Ghi chú"
};
for (int i = 0; i < headers.Length; i++)
{
    var cell = ws.Cell(1, i + 1);
    cell.Value = headers[i];
    cell.Style.Font.Bold = true;
    cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
}

// ── Data — 10 NLĐ mẫu ──
var data = new object[][] {
    new object[] { 1, "Kim Soo-hyun", "Nam", "15/05/1988", "Hàn Quốc", "M98765432", "Seoul, Hàn Quốc", "Quản lý", "Có GPLĐ", "V001234", "30/06/2027", "" },
    new object[] { 2, "Tanaka Yuki", "Nữ", "22/09/1992", "Nhật Bản", "TK4567890", "Tokyo, Nhật Bản", "Phiên dịch", "Miễn", "V005678", "31/12/2027", "" },
    new object[] { 3, "Wang Xiaoming", "Nam", "10/03/1990", "Trung Quốc", "E55667788", "Quảng Châu, Trung Quốc", "Kỹ sư", "Có GPLĐ", "V009012", "15/08/2027", "Chuyên gia tự động hóa" },
    new object[] { 4, "Lee Ji-eun", "Nữ", "01/12/1995", "Hàn Quốc", "M44556677", "Busan, Hàn Quốc", "Kỹ thuật viên", "Chưa có GPLĐ", "", "30/09/2027", "" },
    new object[] { 5, "Chen Wei", "Nam", "18/07/1987", "Trung Quốc", "E99001122", "Thâm Quyến, Trung Quốc", "Chuyên gia", "Có GPLĐ", "V003456", "28/02/2028", "" },
    new object[] { 6, "Sato Haruto", "Nam", "05/11/1991", "Nhật Bản", "TK7788990", "Osaka, Nhật Bản", "Giám đốc", "Có GPLĐ", "V007890", "31/03/2028", "Giám đốc kỹ thuật" },
    new object[] { 7, "Park Min-ji", "Nữ", "25/06/1993", "Hàn Quốc", "M22334455", "Incheon, Hàn Quốc", "Tư vấn", "Miễn", "", "30/06/2027", "" },
    // TRÙNG (passport E12345678 đã có trong DB — Zhang Wei)
    new object[] { 8, "Zhang Wei (cập nhật)", "Nam", "15/03/1985", "Trung Quốc", "E12345678", "Bắc Kinh — ĐỊA CHỈ MỚI", "Kỹ sư", "Có GPLĐ", "V111222", "31/12/2028", "Gia hạn tạm trú" },
    // TRÙNG (passport E87654321 đã có — Li Ming)
    new object[] { 9, "Li Ming (cập nhật)", "Nam", "22/07/1990", "Trung Quốc", "E87654321", "Thượng Hải — ĐỊA CHỈ MỚI", "Kỹ sư", "Có GPLĐ", "V333444", "31/12/2028", "Gia hạn" },
    // LỖI (thiếu họ tên)
    new object[] { 10, "", "Nam", "01/01/1990", "Trung Quốc", "", "", "", "", "", "", "Bản ghi lỗi" },
};

for (int r = 0; r < data.Length; r++)
{
    for (int c = 0; c < data[r].Length; c++)
    {
        ws.Cell(r + 2, c + 1).Value = data[r][c]?.ToString() ?? "";
    }
}

ws.Columns().AdjustToContents();

var filePath = Path.Combine("..", "test-import-data.xlsx");
filePath = Path.GetFullPath(filePath);
workbook.SaveAs(filePath);
Console.WriteLine($"[OK] File: {filePath}");
Console.WriteLine($"     10 dòng: 7 mới + 2 trùng + 1 lỗi");
