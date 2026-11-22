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

    private static readonly (string Group, string Title, string Description)[] SurveyItems =
    [
        ("II. Chức năng (1/8)", "1. Bộ cài đặt & Khởi chạy R6", "Cài đặt offline R6, tạo shortcut, tự động khởi chạy Dịch vụ IRM và mở trình duyệt."),
        ("II. Chức năng (2/8)", "2. Đăng nhập, Phân quyền & Đổi mật khẩu", "Đăng nhập tài khoản quản trị/biên tập/xem, buộc đổi mật khẩu lần đầu và phân quyền chức năng."),
        ("II. Chức năng (3/8)", "3. Trang Tổng quan (Dashboard)", "Thống kê tổng số nhân viên, công ty, cảnh báo sắp hết hạn tạm trú (≤15 ngày) và biểu đồ phân bố."),
        ("II. Chức năng (4/8)", "4a. Quản lý Nhân viên NNN — Danh sách & Hồ sơ", "Tìm kiếm, lọc theo công ty/quốc tịch/trạng thái, thêm/sửa/xóa hồ sơ nhân viên người nước ngoài."),
        ("II. Chức năng (4/8)", "4b. Quản lý Nhân viên NNN — Thăm thân & Lưu trú", "Quản lý thông tin người thân đi cùng, cơ sở lưu trú, mục đích tạm trú và giấy tờ tùy thân."),
        ("II. Chức năng (5/8)", "5a. Quản lý Công ty & Nhà đầu tư", "Thêm/sửa/xóa doanh nghiệp sử dụng lao động NNN, quản lý danh sách nhà đầu tư theo công ty."),
        ("II. Chức năng (5/8)", "5b. Quản lý Dự án / Nhà thầu & Gia hạn", "Theo dõi hợp đồng/nhà thầu, lịch sử gia hạn tạm trú và tài liệu đính kèm của doanh nghiệp."),
        ("II. Chức năng (6/8)", "6. Quản lý Du học sinh", "Quản lý hồ sơ du học sinh, trường đào tạo, thời hạn khóa học và trạng thái lưu trú."),
        ("II. Chức năng (7/8)", "7. Xuất báo cáo, Kiểm tra & Đối chiếu", "Xuất báo cáo định kỳ ra Excel, lập đợt kiểm tra thực tế và đối chiếu dữ liệu lưu trú."),
        ("II. Chức năng (8/8)", "8. Danh mục, Sao lưu & Nhật ký hệ thống", "Quản lý danh mục Quốc tịch (3 chữ cái), Phường/Xã, Nhóm nghề/Nghề nghiệp, Sao lưu/Phục hồi và Nhật ký kiểm toán."),
        ("III. Giao diện & Cảnh báo", "9. Giao diện Bộ cài đặt Windows (Inno Setup)", "Màu sắc Olive/Cam đồng bộ, hướng dẫn từng bước rõ ràng, không lỗi font tiếng Việt."),
        ("III. Giao diện & Cảnh báo", "10. Bố cục Web, Chế độ Sáng/Tối & Mật độ bảng", "Thanh điều hướng trái, Topbar, chuyển chế độ Sáng/Tối (Light/Dark) và chế độ bảng Gọn/Thoáng."),
        ("III. Giao diện & Cảnh báo", "11. Tiếng Việt có dấu & Thuật ngữ nghiệp vụ", "Nhãn trường, nút bấm, thông báo và thuật ngữ quản lý xuất nhập cảnh rõ ràng, chuẩn xác."),
        ("III. Giao diện & Cảnh báo", "12. Cảnh báo trường bắt buộc (*) trên các biểu mẫu", "Khi bỏ trống ô bắt buộc (*) và bấm Lưu, hiển thị ngay viền đỏ kèm dòng chữ báo lỗi tiếng Việt bên dưới từng ô."),
        ("IV. Định dạng & Quy tắc Excel", "13. File mẫu kiểm thử (IRM-MAU-IMPORT-TEST-DA-SUA.xlsx)", "Nạp thử 10/10 hồ sơ mẫu thành công ngay lần đầu trên bản cài mới R6; giữ nguyên số 0 đầu ở cột mã định danh."),
        ("IV. Định dạng & Quy tắc Excel", "14. Chọn Công ty & Nhóm nghề trước khi đọc file", "Cảnh báo rõ khi chưa chọn Công ty hoặc Nhóm nghề tại Bước 1 của màn hình Import Excel."),
        ("IV. Định dạng & Quy tắc Excel", "15. Tự động ghép cột (Auto-map) & Nút trợ giúp (?)", "Tự động nhận diện đúng 19 cột của file mẫu; hộp thoại trợ giúp (?) giải thích rõ quy tắc từng cột."),
        ("IV. Định dạng & Quy tắc Excel", "16. Quy tắc Quốc tịch (Mã 3 chữ cái chuẩn WPF)", "Chấp nhận mã 3 chữ cái (CHN, KOR, JPN, USA...) hoặc tên tiếng Việt; báo lỗi rõ dòng nếu mã 2 chữ cái chưa có trong danh mục."),
        ("IV. Định dạng & Quy tắc Excel", "17. Quy tắc Địa chỉ kết thúc bằng tên Phường/Xã", "Kiểm tra cuối chuỗi địa chỉ khớp Phường/Xã trong Danh mục; thông báo rõ số dòng nếu thiếu hoặc chưa khai báo phường."),
        ("IV. Định dạng & Quy tắc Excel", "18. Quy tắc Giấy phép lao động (GPLĐ) & Nghề Thân nhân", "Phân biệt đúng 4 trường hợp: Mã số GPLĐ, 'Miễn', để trống (Chưa có), và 'Khác' (tự động gắn nghề 'Thân nhân')."),
        ("IV. Định dạng & Quy tắc Excel", "19. Xem trước (Preview), Phát hiện trùng, Cập nhật & Hoàn tác", "Hiển thị rõ dòng Mới / Trùng giữ nguyên / Cập nhật / Lỗi trước khi lưu; cho phép Hoàn tác (Rollback) lần import."),
        ("IV. Định dạng & Quy tắc Excel", "20. Định dạng file Excel khi Xuất báo cáo", "Bố cục cột, định dạng ngày tháng (dd/MM/yyyy), tiêu đề và phông chữ của các file Excel xuất ra từ hệ thống.")
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

    public static byte[] CreateFeedbackSurveyWorkbook()
    {
        using var wb = new XLWorkbook();

        // Sheet 1: Tổng hợp & Thông tin
        var s1 = wb.Worksheets.Add("Tổng hợp & Thông tin");
        s1.Cell("A1").Value = "PHIẾU KHẢO SÁT & GÓP Ý NGHIỆM THU HỆ THỐNG IRM v1.0.2 (GÓI BÀN GIAO R6)";
        s1.Range("A1:D1").Merge();
        s1.Range("A1:D1").Style.Font.Bold = true;
        s1.Range("A1:D1").Style.Font.FontSize = 14;
        s1.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.FromHtml("#3f5223");
        s1.Range("A1:D1").Style.Font.FontColor = XLColor.White;

        var infoRows = new (string Label, string Placeholder)[]
        {
            ("Đơn vị / Phòng ban thử nghiệm:", "Điền tên đơn vị hoặc phòng ban"),
            ("Họ tên cán bộ đánh giá:", "Điền họ tên người trực tiếp kiểm thử"),
            ("Chức vụ / Vai trò sử dụng:", "Quản trị viên / Cán bộ nhập liệu / Lãnh đạo xem báo cáo"),
            ("Số điện thoại / Email liên hệ:", "Điền thông tin để đội dự án trao đổi khi cần"),
            ("Ngày đánh giá:", "15/11/2026"),
            ("Phiên bản bộ cài kiểm thử:", "IRM-v1.0.2-windows-x64-offline-setup-R6.exe")
        };

        for (var i = 0; i < infoRows.Length; i++)
        {
            var r = i + 3;
            s1.Cell(r, 1).Value = infoRows[i].Label;
            s1.Cell(r, 1).Style.Font.Bold = true;
            s1.Cell(r, 2).Value = infoRows[i].Placeholder;
            s1.Range(r, 2, r, 4).Merge();
        }

        s1.Cell("A10").Value = "BẢNG TỔNG HỢP KẾT QUẢ ĐÁNH GIÁ TỰ ĐỘNG (TỪ SHEET 'Chi tiết khảo sát')";
        s1.Range("A10:D10").Merge();
        s1.Range("A10:D10").Style.Font.Bold = true;
        s1.Range("A10:D10").Style.Fill.BackgroundColor = XLColor.FromHtml("#eef5dc");

        s1.Cell("A11").Value = "Mức độ đánh giá";
        s1.Cell("B11").Value = "Số lượng hạng mục";
        s1.Range("A11:B11").Style.Font.Bold = true;

        var summaryMetrics = new (string Status, string Formula)[]
        {
            ("Đạt yêu cầu", "COUNTIF('Chi tiết khảo sát'!$E$2:$E$23,\"Đạt yêu cầu\")"),
            ("Cần chỉnh sửa nhỏ", "COUNTIF('Chi tiết khảo sát'!$E$2:$E$23,\"Cần chỉnh sửa nhỏ\")"),
            ("Không đạt / Lỗi", "COUNTIF('Chi tiết khảo sát'!$E$2:$E$23,\"Không đạt / Lỗi\")"),
            ("Chưa thử", "COUNTIF('Chi tiết khảo sát'!$E$2:$E$23,\"Chưa thử\")"),
            ("Tổng số mục đã chọn đánh giá", "COUNTA('Chi tiết khảo sát'!$E$2:$E$23)")
        };

        for (var i = 0; i < summaryMetrics.Length; i++)
        {
            var r = 12 + i;
            s1.Cell(r, 1).Value = summaryMetrics[i].Status;
            s1.Cell(r, 2).FormulaA1 = summaryMetrics[i].Formula;
            if (i == summaryMetrics.Length - 1)
            {
                s1.Range(r, 1, r, 2).Style.Font.Bold = true;
            }
        }

        s1.Cell("A18").Value = "KẾT LUẬN NGHIỆM THU TỔNG THỂ:";
        s1.Cell("A18").Style.Font.Bold = true;
        s1.Cell("B18").Value = "Đồng ý nghiệm thu";
        s1.Cell("B18").CreateDataValidation().List("\"Đồng ý nghiệm thu,Đồng ý nghiệm thu có điều kiện (cần chỉnh sửa nhỏ),Cần hiệu chỉnh lại trước khi nghiệm thu\"", true);

        s1.Cell("A19").Value = "Đề xuất ưu tiên / Ý kiến tổng kết:";
        s1.Cell("A19").Style.Font.Bold = true;
        s1.Range("B19:D21").Merge();
        s1.Range("B19:D21").Style.Alignment.WrapText = true;

        s1.Column(1).Width = 38;
        s1.Column(2).Width = 28;
        s1.Column(3).Width = 24;
        s1.Column(4).Width = 24;

        // Sheet 2: Chi tiết khảo sát
        var s2 = wb.Worksheets.Add("Chi tiết khảo sát");
        var detailHeaders = new[]
        {
            "STT",
            "Phân nhóm",
            "Hạng mục chức năng / Giao diện / Excel",
            "Nội dung kiểm tra gợi ý",
            "Mức đánh giá (Chọn từ danh sách)",
            "Ý kiến góp ý chi tiết / Mô tả lỗi",
            "Mức ưu tiên (Cao/TB/Thấp)",
            "Đề xuất hướng điều chỉnh"
        };

        for (var c = 0; c < detailHeaders.Length; c++)
        {
            var cell = s2.Cell(1, c + 1);
            cell.Value = detailHeaders[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#3f5223");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.WrapText = true;
        }

        for (var i = 0; i < SurveyItems.Length; i++)
        {
            var rowNumber = i + 2;
            var item = SurveyItems[i];
            s2.Cell(rowNumber, 1).Value = i + 1;
            s2.Cell(rowNumber, 2).Value = item.Group;
            s2.Cell(rowNumber, 3).Value = item.Title;
            s2.Cell(rowNumber, 3).Style.Font.Bold = true;
            s2.Cell(rowNumber, 4).Value = item.Description;
            s2.Cell(rowNumber, 5).Value = "Đạt yêu cầu";
            s2.Cell(rowNumber, 7).Value = "Trung bình";
            s2.Range(rowNumber, 2, rowNumber, 8).Style.Alignment.WrapText = true;
        }

        var ratingValidation = s2.Range("E2:E23").CreateDataValidation();
        ratingValidation.List("\"Đạt yêu cầu,Cần chỉnh sửa nhỏ,Không đạt / Lỗi,Chưa thử\"", true);

        var priorityValidation = s2.Range("G2:G23").CreateDataValidation();
        priorityValidation.List("\"Cao,Trung bình,Thấp\"", true);

        s2.SheetView.FreezeRows(1);
        s2.Column(1).Width = 7;
        s2.Column(2).Width = 24;
        s2.Column(3).Width = 38;
        s2.Column(4).Width = 52;
        s2.Column(5).Width = 24;
        s2.Column(6).Width = 40;
        s2.Column(7).Width = 18;
        s2.Column(8).Width = 36;

        // Sheet 3: Đề xuất cột Excel thực tế
        var s3 = wb.Worksheets.Add("Đề xuất cột Excel thực tế");
        var customColHeaders = new[]
        {
            "STT",
            "Áp dụng cho (Import / Export)",
            "Tên cột trong file Excel thực tế của đơn vị",
            "Bắt buộc / Tùy chọn",
            "Ví dụ dữ liệu thực tế",
            "Yêu cầu kiểm tra / Quy tắc xử lý mong muốn"
        };

        for (var c = 0; c < customColHeaders.Length; c++)
        {
            var cell = s3.Cell(1, c + 1);
            cell.Value = customColHeaders[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#3f5223");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.WrapText = true;
        }

        for (var i = 1; i <= 10; i++)
        {
            var r = i + 1;
            s3.Cell(r, 1).Value = i;
            s3.Cell(r, 2).Value = "Import nhân viên";
            s3.Cell(r, 4).Value = "Tùy chọn";
        }
        s3.Range("B2:B11").CreateDataValidation().List("\"Import nhân viên,Export báo cáo,Danh mục khác\"", true);
        s3.Range("D2:D11").CreateDataValidation().List("\"Bắt buộc,Tùy chọn\"", true);
        s3.SheetView.FreezeRows(1);
        s3.Column(1).Width = 8;
        s3.Column(2).Width = 24;
        s3.Column(3).Width = 36;
        s3.Column(4).Width = 18;
        s3.Column(5).Width = 28;
        s3.Column(6).Width = 46;

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

    public static void WriteFeedbackSurveyWorkbook(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllBytes(path, CreateFeedbackSurveyWorkbook());
    }

    public static void WriteAll(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
        WriteSampleImportWorkbook(Path.Combine(directoryPath, "IRM-MAU-IMPORT-TEST-DA-SUA.xlsx"));
        WriteFeedbackSurveyWorkbook(Path.Combine(directoryPath, "PHIEU-KHAO-SAT-GOP-Y.xlsx"));
    }
}
