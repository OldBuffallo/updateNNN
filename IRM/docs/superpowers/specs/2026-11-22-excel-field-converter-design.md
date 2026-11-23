# Design Spec — Bước Lọc Trường & Chuẩn Hóa Excel (Convert Before Import)

- **Ngày lập:** 22/11/2026
- **Nhánh áp dụng:** `custom/kh2` (Web IRM độc lập cho Khách hàng 2)
- **Phạm vi:** Tích hợp trực tiếp vào quy trình `/import` (`IRM/IRM/Features/ImportExport/`), bổ sung service chuẩn hóa ở `IRM.Application` và `IRM.Infrastructure/Excel/`.

---

## 1. Mục tiêu & Ngữ cảnh

Khách hàng thường có các file Excel nguồn lộn xộn (nhiều cột rác, tên cột mỗi đơn vị một kiểu, ngày tháng lẫn `yyyy-MM-dd` / `dd-MM-yyyy` / serial Excel, giới tính `Male`/`M`/`1`/`F`/`0`, quốc tịch tiếng Anh hoặc mã 2 ký tự, khoảng trắng thừa), nhưng dữ liệu vẫn tuân theo quy tắc **1 ô = 1 trường**.

Công cụ cần giúp người dùng không chuyên kỹ thuật:
1. **Chỉ chọn lấy các trường IRM họ thực sự cần** (tối thiểu `Số hộ chiếu`), tự động bỏ qua toàn bộ cột rác.
2. **Tự động chuẩn hóa định dạng** các ô đã chọn về đúng chuẩn của IRM trước khi vào Bước Xem trước & Import.
3. **Xem trực tiếp kết quả trước → sau** trên các dòng mẫu ngay khi chọn cột, và cho phép **tải file `.xlsx` đã chuẩn hóa** hoặc **đi thẳng sang Bước 3 (Xem trước & Import)** mà không cần upload lại.

### Ngoài phạm vi (Non-goals)
- Không tách nhiều trường bị gộp chung trong cùng 1 ô (ví dụ `"NGUYEN A - Nam - 1990"`).
- Không thay đổi cấu trúc cơ sở dữ liệu hay quy tắc *chỉ bắt buộc Số hộ chiếu* đã chốt trên `custom/kh2`.
- Không tạo trang menu rời (đã chọn **Cách A**).

---

## 2. Các phương án đã cân nhắc (Approaches)

| Phương án | Mô tả | Đánh đổi | Quyết định |
|---|---|---|---|
| **Cách A** | Tích hợp thành bước **Lọc trường & Chuẩn hóa** ngay tại Bước 2 của `/import`, có nút chuyển nhanh sang chế độ ghép 35 cột nâng cao và nút tải file `.xlsx` chuẩn. | Thay đổi giao diện Bước 2 mặc định của `/import`, nhưng luồng liền mạch 1 màn hình, không phải tải xuống rồi upload lại. | **Đã chọn** |
| **Cách B** | Tạo trang công cụ riêng `/excel-converter` trên menu, xuất file `.xlsx` rồi người dùng tự mang sang `/import`. | Không đụng `/import` nhưng buộc người dùng đi qua 2 màn hình và thao tác tải xuống/tải lên thừa. | Không chọn |

---

## 3. Thiết kế Giao diện (UI Proposal — Bước 2 của `/import`)

### 3.1. Bố cục tổng thể

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│ 📄 danh_sach_nguoi_nuoc_ngoai.xlsx  ·  Sheet1  ·  120 dòng dữ liệu           │
│ [ ● Chế độ đơn giản: Lọc trường & Chuẩn hóa ]  [ ○ Nâng cao: Ghép 35 cột ]   │
│                                     [↻ Tự động ghép]  [✓ Chỉ chọn cột khớp] │
├──────────────────────────────────────────────────────────────────────────────┤
│ NHÓM 1 — BẮT BUỘC (1 trường)                                                 │
│ ┌──────────────────────────────────────────────────────────────────────────┐ │
│ │ ☑ Số hộ chiếu *  │ [Cột C: "So HC" ▼] │ Mẫu: " 001234 " ──▶ 001234  ✓   │ │
│ └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ NHÓM 2 — THÔNG TIN CƠ BẢN (tích chọn trường muốn lấy, bỏ tích để bỏ qua)     │
│ ┌──────────────────────────────────────────────────────────────────────────┐ │
│ │ ☑ Họ và tên      │ [Cột B: "Full Name" ▼] │ " nguyen a " ──▶ nguyen a ✨ │ │
│ │ ☑ Giới tính      │ [Cột F: "Sex" ▼]       │ "Male"       ──▶ Nam      ✨ │ │
│ │ ☑ Ngày sinh      │ [Cột D: "DOB" ▼]       │ "1990-06-03" ──▶ 03/06/1990✨│ │
│ │ ☑ Quốc tịch      │ [Cột E: "Country" ▼]   │ "Korea"      ──▶ KOR      ✨ │ │
│ │ ☑ Công ty        │ [Cột G: "Company" ▼]   │ "Cty ABC"    ──▶ Cty ABC  ✓ │ │
│ │ ☐ Địa chỉ        │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ │ ☐ Nghề nghiệp    │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ │ ☐ Số Visa        │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ │ ☐ Ký hiệu Visa   │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ │ ☐ Hạn tạm trú    │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ │ ☐ GPLĐ / Số GPLĐ │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ │ ☐ Ghi chú        │ (Chưa chọn — bỏ qua)   │ —                           │ │
│ └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ ▸ NHÓM 3 — MỞ RỘNG: Thăm thân (7 trường) & V0.1.0 (12 trường)  [0 đã chọn]   │
├──────────────────────────────────────────────────────────────────────────────┤
│ XEM TRƯỚC KẾT QUẢ CHUẨN HÓA (5 dòng đầu · 6 cột đã chọn · ✨ 14 ô tự sửa)   │
│ ┌──────┬──────────┬──────────┬───────────┬────────────┬───────────┬────────┐ │
│ │ Dòng │ Hộ chiếu │ Họ tên   │ Giới tính │ Ngày sinh  │ Quốc tịch │ Công ty│ │
│ ├──────┼──────────┼──────────┼───────────┼────────────┼───────────┼────────┤ │
│ │  2   │ 001234   │ nguyen a │ Nam ✨    │ 03/06/1990✨│ KOR ✨   │ Cty ABC│ │
│ │  3   │ P987654  │ KIM MIN  │ Nữ  ✨    │ 15/08/1988 │ KOR ✓    │ Cty ABC│ │
│ └──────┴──────────┴──────────┴───────────┴────────────┴───────────┴────────┘ │
├──────────────────────────────────────────────────────────────────────────────┤
│ [ ← Quay lại ]    [ ⬇ Tải file Excel đã chuẩn hóa ]  [ Tiếp tục Xem trước → ]│
└──────────────────────────────────────────────────────────────────────────────┘
```

### 3.2. Hành vi tương tác chính
1. **Hướng chọn ngược (Trường IRM → Cột Excel):** Thay vì hiện 30–40 cột rác của file Excel, giao diện chỉ liệt kê các trường đích của IRM.
2. **Tự động tích & ghép thông minh khi vào Bước 2:**
   - `Số hộ chiếu` luôn ở đầu, bắt buộc tích.
   - Các trường cơ bản nào có cột Excel khớp tên tự động sẽ được tích sẵn (`☑`) và chọn sẵn cột tương ứng. Các trường không có trong file để trống (`☐`) và làm mờ nhẹ.
3. **Xem mẫu Trước → Sau tức thì (Live Conversion Preview):**
   - Mỗi dòng chọn trường hiển thị ngay giá trị đầu tiên tìm thấy trong cột Excel gốc và giá trị sau chuẩn hóa kèm badge: `✓ Giữ nguyên`, `✨ Tự chuẩn hóa`, hoặc `⚠ Cần kiểm tra`.
   - Khung bảng bên dưới hiển thị **5 dòng dữ liệu đầu tiên** chỉ gồm các cột đã tích. Hover vào ô có `✨` sẽ hiện tooltip `"Gốc: Male → Chuẩn: Nam"`.
4. **Chế độ nâng cao & Template:**
   - Nút chuyển nhanh sang bảng ghép 35 cột cũ (giữ nguyên tương thích với các `ColumnMappingTemplate` đã lưu và các test hiện có).
5. **Hệ thống Thông báo 2 lớp (Notifications) trực tiếp tại Bước 2:**
   - **Thông báo 1 — Tình trạng file Excel mới tải lên:** Hiển thị ngay khi đọc file xong: tổng số dòng dữ liệu, tổng số cột gốc, số cột tự động khớp với IRM, số cột không khớp/cột rác sẽ bỏ qua, và trạng thái trường bắt buộc `Số hộ chiếu` (đủ 100% hay có dòng trống).
   - **Thông báo 2 — Kết quả sau khi lọc/chuẩn hóa & Danh sách thông tin còn thiếu:** Cập nhật tức thì mỗi khi người dùng tích/bỏ tích trường hoặc đổi cột:
     - Số ô đã được **tự động chuẩn hóa (`✨`)** và số ô **lỗi định dạng (`⚠`)** kèm chi tiết dòng/cột.
     - **Các trường cơ bản chưa chọn lấy** (bỏ tích / không có trong file).
     - **Các ô bị bỏ trống bên trong những trường đã chọn** (ví dụ: *Họ và tên thiếu 1/5 dòng — dòng 5; Công ty thiếu 1/5 dòng — dòng 5*), kèm ghi chú rõ ràng rằng hồ sơ vẫn được phép import (vì chỉ bắt buộc Số hộ chiếu) và sẽ gắn trạng thái thiếu thông tin để bổ sung sau.
6. **Hai lối ra ở cuối Bước 2:**
   - **Tải file Excel đã chuẩn hóa (`.xlsx`):** Sinh file Excel sạch chỉ gồm đúng các cột đã tích (với tiêu đề chuẩn IRM, cột Hộ chiếu/Số Visa/Số GPLĐ định dạng Text giữ số `0` đầu) để khách lưu trữ hoặc gửi đi.
   - **Tiếp tục Xem trước & Import:** Chuyển đổi toàn bộ dòng của file trong bộ nhớ sang `MemoryStream` `.xlsx` chuẩn (hoặc danh sách dòng đã chuẩn hóa) và đi thẳng vào **Bước 3 (`ImportPreviewSection`)** với bộ mapping 1:1 chuẩn, không cần upload lại.

---

## 4. Quy tắc Chuẩn hóa Tự động (Normalization Rules)

Tách thành một lớp thuần trong `IRM.Application/Features/ImportExport/ExcelValueNormalizer.cs` (dễ unit test):

| Loại trường | Đầu vào lộn xộn phổ biến | Đầu ra chuẩn IRM |
|---|---|---|
| **Số hộ chiếu** (`Passport`) | `" 001234 "`, khoảng trắng thừa ở giữa/hai đầu | Trim sạch, giữ nguyên số `0` đầu, định dạng cell Text khi xuất `.xlsx`. |
| **Giới tính** (`Gender`) | `"Male"`, `"M"`, `"1"`, `"nam"`, `"Boy"` → `"Nam"`; `"Female"`, `"F"`, `"0"`, `"2"`, `"nu"`, `"Nữ"` → `"Nữ"` | `"Nam"` hoặc `"Nữ"` (ghi nhận `WasAutoNormalized = true` nếu khác nguyên văn). |
| **Ngày tháng** (`Birthday`, `TemporaryStay`, `CardCreationDate`, `FamilyVisit*Date`, `*ValidFrom`, `*ValidTo`) | **Chỉ giữ chuẩn `dd/MM/yyyy` (VN)**: đổi `yyyy-MM-dd`, `yyyy/MM/dd`, `dd-MM-yyyy`, `dd.MM.yyyy`, `d/M/yyyy`, số serial Excel → `"dd/MM/yyyy"`. Không hỗ trợ đảo `MM/dd/yyyy`. | `"dd/MM/yyyy"` chuẩn (vd `"03/06/1990"`). Nếu không đọc được ngày hoặc ngày không tồn tại → giữ nguyên + gắn cờ `NeedsReview` (`⚠`). |
| **Quốc tịch** (`Nationality`) | Tên tiếng Việt không dấu (`"han quoc"`), mã ISO 2 ký tự (`"KR"`, `"US"`, `"CN"`), tên tiếng Anh phổ biến (`"Korea"`, `"South Korea"`, `"China"`, `"Japan"`, `"USA"`, `"United States"`, `"India"`, `"Taiwan"`, `"UK"`, `"Australia"`, `"France"`, `"Germany"`, `"Thailand"`, `"Singapore"`, `"Malaysia"`, `"Philippines"`, `"Indonesia"`, `"Russia"`, `"Canada"`...) | Mã 3 ký tự đang hoạt động trong bảng `Nationality` (`KOR`, `USA`, `CHN`...). |
| **GPLĐ** (`WorkPermit`) | `"Chưa có"`, `"Chưa có GPLĐ"`, `"Không"`, `"Không có"`, `"0"` → `""` (để trống = Chưa có); `"Miễn GPLĐ"` → `"Miễn"` | Đúng quy ước `LegacyImportRules.ParseWorkPermit`, tránh lưu nhầm chữ mô tả thành số GPLĐ. |
| **Thăm thân** (`FamilyVisit`) | `"true"`, `"yes"`, `"đúng"`, `"co"`, `"có"`, `"x"`, `"1"` → `"Có"`; `"false"`, `"no"`, `"sai"`, `"khong"`, `"không"`, `"0"` → `"Không"` | `"Có"` hoặc `"Không"`. |
| **Chuỗi văn bản khác** (`StaffName`, `Address`, `CompanyName`, `Note`, ...) | Nhiều khoảng trắng liên tiếp, ký tự xuống dòng `\r\n` thừa | Trim & gom khoảng trắng thừa về 1 dấu cách. |

---

## 5. Kiến trúc mã nguồn dự kiến

- **`IRM.Application/Features/ImportExport/ExcelValueNormalizer.cs`** *(NEW)*: Chứa các hàm chuẩn hóa thuần theo từng loại trường + trả về `NormalizedCellResult(string Value, bool WasNormalized, bool NeedsReview, string? Note)`.
- **`IRM.Application/Features/ImportExport/Models/ExcelConvertModels.cs`** *(NEW)*: `FieldSelectionItem`, `ConvertedSampleRow`, `ExcelConvertSummary`.
- **`IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs`** *(NEW)*: Đọc file Excel thô theo danh sách trường đã tích chọn, chạy `ExcelValueNormalizer` trên từng ô, và xuất ra `byte[]` / `MemoryStream` `.xlsx` chuẩn IRM (chỉ gồm các cột đã chọn, header chuẩn `SystemFields.All`, cell Text cho Passport/Visa/WorkPermitNumber).
- **`IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor`** *(MODIFY)*: Mặc định hiển thị giao diện **Lọc trường & Chuẩn hóa** (3 nhóm trường + bảng mẫu 5 dòng + nút Tải file chuẩn), kèm nút chuyển sang chế độ **Nâng cao (35 cột)**.
- **`IRM/IRM/Features/ImportExport/Import.razor.cs`** *(MODIFY)*: Nối state chọn trường, xem mẫu live 5 dòng, xuất file `.xlsx` đã chuẩn hóa qua JS download, và khi bấm **"Tiếp tục Xem trước"** sẽ chạy converter tạo `MemoryStream` sạch đẩy thẳng vào `GeneratePreviewAsync`.
