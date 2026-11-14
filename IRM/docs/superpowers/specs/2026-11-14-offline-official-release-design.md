# IRM — phạm vi phát hành offline chính thức

Ngày: 14/11/2026 (Asia/Bangkok). Trạng thái: cơ sở cho kế hoạch phát hành; chưa phải biên bản nghiệm thu hoặc xác nhận đã phát hành.

## Yêu cầu đã được người dùng chốt

- Phát hành bản chính thức bằng bộ cài offline Windows.
- Người dùng xác nhận bộ cài lần trước đã hoạt động tốt; phần còn thiếu là UI phù hợp giao diện ứng dụng mới. Ghi nhận xác nhận này làm căn cứ kế thừa cơ chế cài đặt. Không tự gán xác nhận đó cho một checksum/revision cụ thể khi người dùng chưa nêu.
- Giữ bộ máy cài đặt hiện có; cập nhật diện mạo bộ cài và đóng gói source mới. Không thiết kế lại hệ thống triển khai.

## Căn cứ hiện tại

- Source tại thời điểm lập kế hoạch: `2fcc3c3`, nhánh `develop`, Git root là `NNN/`.
- Kiểm tra trực tiếp ngày 14/11: build Release 0 warning/error; .NET 337/337 (SQL test biệt lập bật, 0 skip); JavaScript 47/47. Đây là bằng chứng source, không thay nghiệm thu EXE cuối.
- `IRM/wwwroot/app.css` đã dùng mint/emerald. `deploy/windows/installer-theme.ps1`, UI WinForms, báo cáo HTML và `irm.iss` vẫn theo dark/blue.
- R4 và candidate ngày 11/11 không phải build từ source hiện tại; giữ nguyên các gói lịch sử.

## Cách thực hiện

Giữ Inno Setup + Windows PowerShell 5.1/WinForms và worker hiện có. Thay palette, bố cục co giãn cần thiết, trạng thái tương tác và tài liệu; publish ứng dụng hiện tại vào payload mới. Không chuyển sang WebView/Electron vì sẽ thêm runtime và đường cài đặt khác cho một yêu cầu đồng bộ UI. Không chỉ đổi tên bộ cài cũ vì payload thiếu các thay đổi ứng dụng mới.

Các quyết định triển khai dưới đây là đề xuất của người lập kế hoạch, không phải những lựa chọn người dùng đã xác nhận riêng:

- Dùng giao diện sáng mint/emerald cố định cho installer, lấy light theme mới làm chuẩn; không thêm bộ chọn theme.
- Giữ application version `1.0.2`, định danh đợt giao mới `R5`; phân biệt bản bằng revision, commit và SHA-256. Không đổi database version theo revision UI.
- Duy trì quy tắc ký số đang có trong build script và tài liệu. Nếu muốn thay quy tắc phát hành unsigned, cần quyết định riêng của người dùng; câu “bộ cài cũ chạy tốt” không tự thay quy tắc này.

## UI đích

Palette: Window `#E6F3F0`, Surface/Input `#FFFFFF`, Header `#11534E`, Text `#0F2E2B`, Muted `#4A6B66`, Accent/Button `#157A72`, ButtonHover `#0E5E57`, Border `#B7D5CE`, Success `#15803D`, Warning `#8A4B0F`, Error `#BE123C`. Chữ trên header/nút chính `#FFFFFF`. Font Segoe UI. Màu cảnh báo dùng biến thể đậm để đọc trên nền sáng.

```text
┌ IRM · Cài đặt offline ────────────────────────┐
│ 1. Kiểm tra máy → 2. Cấu hình → 3. Hoàn tất   │
│ [Bảng kết quả / biểu mẫu cấu hình]            │
│ Trạng thái có chữ và màu; tiến trình          │
│ Đóng       Mở báo cáo          Tiếp tục       │
└──────────────────────────────────────────────┘
```

Đây là sơ đồ vùng nội dung, không thay nhãn/nút và phase machine hiện có. Đồng bộ cửa sổ Inno, WinForms, thông báo lỗi và báo cáo HTML; màn hình in nền trắng. Hộp UAC do Windows quản lý. Kiểm tra tab/focus, ô mật khẩu, disabled/read-only, thử lại, resize, DPI 100/125/150/200%; nút chính luôn tiếp cận được.

## Ràng buộc toàn đợt

- Giữ `IRM.exe`, AppId `{A7A7AA33-A680-4E29-93C4-9F4720376A6B}`, service `IRM`, SQL instance `IRMEXPRESS`, database `IRM`, data root `C:\ProgramData\IRM`, port `5050/5443/14331`.
- Giữ schema, migrations, quyền, password compatibility, validation và luồng import/rollback; không sửa WPF hoặc danh mục nghiệp vụ thật.
- Giữ SQL media/CU và SHA-256 đã khóa trong `deploy/release-manifest.json`; không nâng dependency/runtime trong đợt chỉnh UI.
- Offline nghĩa là không tải Internet khi cài trên máy đủ điều kiện; .NET 8 self-contained và SQL base/CU nằm trong EXE. .NET Framework 4.7.2+ (Release >= 461808) là điều kiện có sẵn, chưa được bundle. Không tự thêm bootstrap tải mạng.
- Giữ ngưỡng kỹ thuật hiện cấu hình Windows build 14393 x64; không coi ngưỡng này là cam kết hỗ trợ mọi edition/OS cũ. Biên bản phải ghi đúng OS/build thực tế đã thử.
- Không đưa CDN/font/tài nguyên mạng mới vào installer; không thay theme/registry Windows để đổi giao diện.
- Mọi thử nghiệm có ghi dữ liệu dùng VM/máy thử biệt lập và dữ liệu giả; không cài đè trên máy làm việc hoặc database khách để kiểm tra.
- Source phải commit và Git tree sạch trước đóng gói. Mỗi build có `SOURCE-REVISION.json`, checksum payload và SHA-256 EXE cuối sau ký.
- Gói mới vào `BAN-GIAO/04-CHO-NGHIEM-THU/`; chỉ đưa vào `01-HIEN-HANH/` sau nghiệm thu. Không ghi đè gói cũ; không commit binary/log/runtime/credentials; không push.

## Điều kiện phát hành

UI đạt checklist; source build/test đạt; EXE cuối truy vết đúng commit, payload khớp checksum và ký số theo quy tắc hiện có; cài mới offline và nâng cấp từ gói cũ trên môi trường thử đạt. Smoke ứng dụng kiểm tra login, hồ sơ, tải mẫu/import/hoàn tác, báo cáo, upload/quét, health và backup/restore bằng dữ liệu giả. Ghi rõ cơ chế đặt lại tài khoản Admin khi nâng cấp vốn có.

Đây là kiểm tra hồi quy bản sắp giao, không phủ nhận bộ cài cũ đã chạy tốt và không mở lại toàn bộ thiết kế. Thay đổi nghiệp vụ/migration phát hiện ngoài phạm vi phải được nêu thành vấn đề riêng. Phát hành phần mềm không đồng nghĩa đã hoàn tất chuyển dữ liệu khách hàng hoặc nghiệm thu vận hành trên database thật.
