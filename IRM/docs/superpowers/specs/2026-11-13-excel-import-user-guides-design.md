# Thiết kế bộ hướng dẫn import Excel hai mức

## 1. Yêu cầu đã chốt

Người dùng chọn phương án 2: một hướng dẫn nhanh cho người nhập Excel và một đặc tả chi tiết từng trường để người hướng dẫn dùng khi tập huấn, tra cứu và giải thích lỗi. Tài liệu mô tả hành vi thực tế, không bổ sung nghiệp vụ do người viết suy đoán.

Ứng dụng chỉ ghi nhận, quản lý và phân tích dữ liệu; không cấp hoặc tự sinh số định danh, giấy tờ, giấy phép. Các số trong dữ liệu import là số đã có.

Mốc mã đã khảo sát: `5c9808d1d213431d399ba51eb002e4844b9d69d5`. Trước khi viết nội dung, đối chiếu lại nếu mã import đã thay đổi. Phạm vi là luồng Excel `/import` hiện có, không phải mọi trường database hoặc mọi điều kiện của biểu mẫu nhập tay.

## 2. Cách trình bày

Đã cân nhắc hai cách: một bảng tra cứu duy nhất, hoặc hướng dẫn nhanh kèm đặc tả. Người dùng chọn cách thứ hai để người nhập có quy trình ngắn, người tập huấn có đầy đủ chi tiết.

### Phần 1 — Hướng dẫn nhanh

Tạo `docs/HUONG-DAN-IMPORT-EXCEL-NHANH.md`, tiếng Việt, dành cho người nhập:

1. Chuẩn bị danh mục và chọn đúng công ty, nhóm nghề.
2. Chuẩn bị `.xlsx`, ngày tháng, cột số nhận dạng dạng Text để tránh mất số 0 đầu.
3. Chọn sheet, dòng tiêu đề, dòng bắt đầu/kết thúc và ghép cột.
4. Kiểm tra 9 trường lõi bắt buộc; trường tùy chọn chỉ dùng khi có dữ liệu tương ứng.
5. Đọc kết quả mới/đã có/thay đổi/lỗi, cảnh báo và quyết định cập nhật.
6. Xử lý các lỗi thường gặp, xác nhận kết quả, nguyên tắc hoàn tác.

Giới hạn khoảng 120–180 dòng Markdown; bảng tra cứu lỗi khoảng 10 tình huống. Dùng ví dụ ngắn có điều kiện danh mục rõ ràng; liên kết sang đặc tả thay vì lặp toàn bộ 35 trường.

### Phần 2 — Đặc tả chi tiết

Mở rộng `docs/IMPORT-EXCEL-SPEC.md` hiện có thay vì tạo thêm một bản đặc tả trùng lặp. Giữ thông tin phân biệt file thử mới/bộ cài cũ và các bằng chứng đối chiếu file mẫu cũ.

Có đúng 35 mục trường, mỗi mục theo cùng mẫu:

- Tên hiển thị trên màn hình ghép cột và khóa trường để đối chiếu.
- Ý nghĩa, kiểu dữ liệu và cách định dạng ô Excel.
- Bắt buộc / tùy chọn / chỉ có tác dụng khi đi cùng trường khác.
- Giá trị chương trình nhận, danh mục cần đối chiếu, quy tắc chuyển đổi.
- Hành vi khi không ghép cột và khi ghép nhưng để ô trống, gồm cập nhật hồ sơ cũ nếu khác nhau.
- Ví dụ đúng có nêu tiền đề và ví dụ sai/dễ bị hiểu sai.
- Ràng buộc với trường khác, thông báo lỗi thực tế và cách xử lý.
- Khuyến nghị nhập liệu được đánh dấu riêng với kiểm tra bắt buộc trong chương trình.

Các mục dùng tiêu đề ``### `khóa-trường` — Tên hiển thị`` (khóa nằm trong dấu backtick). Các phần khác dùng cấp tiêu đề khác để kiểm tra đủ trường không nhầm.

## 3. Danh sách phải phủ

| Nhóm | Khóa trường |
|---|---|
| Lõi và giấy tờ cũ — 15 | `StaffName`, `Gender`, `Birthday`, `Nationality`, `Passport`, `Address`, `Career`, `WorkPermit`, `WorkPermitNumber`, `VisaNumber`, `VisaSymbol`, `TemporaryStay`, `Note`, `SettlementResults`, `CardCreationDate` |
| Thăm thân — 7 | `FamilyVisit`, `FamilyVisitRelativeName`, `FamilyVisitRelationship`, `FamilyVisitRelativeIdCard`, `FamilyVisitStartDate`, `FamilyVisitEndDate`, `FamilyVisitNote` |
| Cư trú và định danh — 4 | `StayPurposeCode`, `StayValidFrom`, `StayValidTo`, `ElectronicIdentityNumber` |
| Giấy tờ mở rộng — 4 | `DocumentTypeCode`, `DocumentNumber`, `DocumentValidFrom`, `DocumentValidTo` |
| Địa bàn, lưu trú — 4 | `AdministrativeUnitCode`, `ResidenceAddress`, `ResidenceValidFrom`, `ResidenceValidTo` |
| Công ty — 1 | `CompanyName` |

`skip` là lựa chọn bỏ qua cột, không phải trường dữ liệu thứ 36. `__RowNumber` và các khóa phối hợp nội bộ không được giới thiệu là cột người dùng có thể nhập.

Chín trường lõi bắt buộc: `StaffName`, `Passport`, `Gender`, `Birthday`, `Nationality`, `Address`, `Career`, `VisaNumber`, `TemporaryStay`. Thứ tự bảng hướng dẫn có thể thuận theo file Excel, không bắt người dùng nhớ thứ tự kiểm tra trong mã.

## 4. Nguồn đối chiếu

Các đường dẫn dưới đây tương đối với `IRM/`:

- `IRM.Application/Features/ImportExport/Models/SystemFields.cs`: danh sách trường và tên ghép cột.
- `IRM.Application/Features/ImportExport/ImportMapping.cs`: bắt buộc, parse, ngày mở rộng, quốc tịch và danh sách phân cách `;`.
- `IRM.Application/Features/ImportExport/LegacyImportRules.cs`: ngày lõi, giới tính, GPLĐ, kết quả giải quyết, địa chỉ, nghề.
- `IRM.Infrastructure/Excel/ExcelReaderHelper.cs`: ô ngày/số/chữ, dòng thật, dòng trống và footer.
- `IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs`: công ty, nhóm nghề, danh mục, nhà đầu tư, trạng thái và cảnh báo.
- `SqlImportStore.Comparison.cs`, `SqlImportStore.Execution.cs`, `SqlImportStore.ExtensionHistory.cs`, `SqlImportStore.Rollback.cs` trong cùng thư mục: bảo toàn trường không ánh xạ, thực ghi dữ liệu mở rộng, lỗi giao dịch, trùng và hoàn tác.
- `IRM.Core/Entities/ForeignPersons/StayPurposeCodes.cs` và `ImmigrationDocumentTypeCodes.cs`: tập mã thực tế, không lấy nhãn gợi ý trên UI làm toàn bộ tập mã.
- `IRM.Tests/LegacyImport*`, `LegacyExcelReaderTests.cs`, `LegacyGoldenWorkbookTests.cs`, `NationalityImportTests.cs`: ví dụ và hành vi đã được kiểm chứng.

Giới hạn độ dài hoặc định dạng chỉ ghi là bắt buộc nếu có bằng chứng trong luồng import và cấu hình lưu trữ. Không tự lấy quy định từ biểu mẫu nhập tay hoặc chuẩn pháp lý bên ngoài để biến thành điều kiện Excel.

## 5. Ràng buộc chung

- Chỉ viết tài liệu; không sửa mã ứng dụng, schema, seed, cấu hình chạy hoặc dữ liệu nghiệp vụ.
- Phủ đúng 35 trường trong `SystemFields.All`, không tính `skip`, không thêm cột import không được hỗ trợ.
- Phân biệt “Chương trình kiểm tra” và “Khuyến nghị nhập liệu”; không trình bày khuyến nghị như lỗi bắt buộc.
- Danh mục database đích là chuẩn; không tự đổi mã quốc tịch, nhóm nghề, nghề hoặc địa bàn sang một bộ mã khác.
- Ngày lõi ưu tiên hướng dẫn `dd/MM/yyyy`; ngày mở rộng có thể nhận thêm ISO theo mã; số nhận dạng được ghi nhận từ dữ liệu đã có, không tự sinh.
- Không hứa mọi file đều import thành công; nêu đúng lỗi theo dòng, lỗi danh mục thay đổi và lỗi có thể hoàn tác cả giao dịch.
- Giữ Git root và nhánh `develop`; không push, không sửa WPF/reference và không đổi nhãn bộ cài cũ.

Không tạo Word/PDF, workbook mới hoặc phần mềm kiểm tra Excel trong phạm vi này. Các định dạng và tiện ích đó chưa được người dùng yêu cầu.

## 6. Tình huống cần giải thích rõ

1. Quốc tịch theo tên hoặc mã thật; tên nghề phải thuộc nhóm chọn; địa chỉ và mã địa bàn là hai cơ chế khác nhau.
2. `03/06/2026` là ngày 3 tháng 6, năm hai chữ số có chuyển đổi, mã/hộ chiếu phải giữ số 0 đầu; danh sách giấy tờ dùng `;` phải giữ thứ tự tương ứng. Chỉ số giấy tờ/loại/hạn đến nhận danh sách; ngày hiệu lực từ áp dụng chung.
3. GPLĐ trống không đồng nghĩa bỏ qua; “Đã có”, “Chưa có” có thể bị hiểu là số GPLĐ. Nhà đầu tư theo Investment của công ty; hai cột GPLĐ mâu thuẫn bị từ chối.
4. Cột thăm thân và V010 không tự làm mất điều kiện lõi; có trường tùy chọn nhưng cần trường đi cùng mới được ghi. Không tự khẳng định mọi tổ hợp thiếu đều được chương trình báo lỗi.
5. Preview hợp lệ không bảo đảm ghi thành công nếu dữ liệu thay đổi; lỗi khi ghi phần mở rộng có thể hoàn tác cả giao dịch; rollback bị chặn khi có thay đổi/phụ thuộc mới hơn.

## 7. Tiêu chí hoàn tất

Hai tài liệu đọc được độc lập và liên kết qua lại; đủ 35 mục trường với mẫu thống nhất; 9 trường bắt buộc không bị lẫn với tùy chọn. Mỗi khẳng định có nguồn mã hoặc test, mọi hạn chế được ghi rõ thay vì sửa luật để khớp văn bản. Người mới có thể chuẩn bị một file, chọn đúng cấu hình, đọc preview và biết cách sửa các lỗi thường gặp. Người hướng dẫn có thể tra từng trường mà không cần đọc C#.
