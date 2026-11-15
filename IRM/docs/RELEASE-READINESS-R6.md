# Biên Bản Sẵn Sàng Bàn Giao — IRM v1.0.2 (Bản R6 - UNSIGNED-TEST)

- **Ngày lập:** 15/11/2026 (Asia/Bangkok)
- **Gói bàn giao hiện hành:** `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`
- **Tệp cài đặt chính:** `IRM-v1.0.2-windows-x64-offline-setup-R6.exe`
- **Dung lượng:** `790.854.254` byte (~754,22 MiB)
- **SHA-256 EXE:** `2D6BA7B6E2FFEB8E6CE411C473965556F2FCEFADCEE5F77F3F2FDBD77CECF585`
- **Commit nguồn đóng gói:** `706f74e44e3b97b1c50d6e3c116d4f969eb5389c` (`dirty: false`, `builtAtUtc: 2026-11-15T12:53:21.0477741Z`)
- **Trạng thái ký số:** `NotSigned` (`-AllowUnsigned` — gói kiểm thử nghiệm thu `UNSIGNED-TEST`)

---

## 1. Các nội dung cập nhật trong Bản R6

1. **Tích hợp đầy đủ bản sửa quy tắc WPF & Cảnh báo trường bắt buộc (`7559cb9` → `706f74e`):**
   - Chuẩn hóa **34 mã quốc tịch 3 chữ cái** (`CHN`, `KOR`, `JPN`, `USA`, `GBR`, `AUS`, `IND`...) trong `DatabaseSeeder` và `LegacyImportRules`, kèm cơ chế tự nâng cấp danh mục mặc định cũ 2 chữ cái sang 3 chữ cái khi khởi động.
   - Tự động nhận diện và gắn nghề `Thân nhân` (nhóm `Khác`) khi cột `GPLĐ = Khác` ngay cả khi người dùng đang chọn nhóm nghề khác (`Kỹ thuật`, `Quản lý`...).
   - Hiển thị **viền đỏ + dòng chữ báo lỗi tiếng Việt trực tiếp dưới từng ô bắt buộc (`*`)** trên toàn bộ các biểu mẫu nhập liệu chính.
2. **Hiệu ứng UI ban đầu chỉ dẫn vị trí Feedback ngay trên ứng dụng Web:**
   - Bổ sung nút **Khảo sát & Góp ý (R6)** (`#btnCustomerFeedback`) trên thanh tiêu đề (`MainLayout.razor`).
   - Ngay lần đầu mở ứng dụng (`localStorage['irm-r6-feedback-seen']` chưa đặt), hệ thống tự động bật viền sáng nhịp đập (`irm-feedback-highlight`) và hộp chỉ dẫn spotlight `Driver.js` trỏ vào nút góp ý để khách hàng nhận biết ngay nơi mở phiếu khảo sát HTML hoặc tải phiếu khảo sát Excel.
3. **Chuẩn hóa tệp Excel mẫu kiểm thử (`IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`):**
   - Được sinh tự động bằng `ClosedXML` (`IRM.Infrastructure.Excel.DeliveryWorkbooks`) và kiểm chứng End-to-End trên database tạo mới mặc định (`DeliverySampleWorkbookTests`).
   - Nạp đạt **10/10 hồ sơ mới** (`IRMTEST26001` → `IRMTEST26010`) ngay lần đầu khi chọn Công ty `CÔNG TY MẪU - HÃY ĐỔI TÊN` và Nhóm nghề `Kỹ thuật`, bảo toàn số `0` đầu ở các cột mã định danh (`000000000001`...) và nhận diện **10/10 dòng trùng giữ nguyên** ở lần nạp thứ hai.
4. **Bổ sung cặp Phiếu Khảo Sát & Góp Ý Nghiệm Thu (`PHIEU-KHAO-SAT-GOP-Y.html` & `.xlsx`):**
   - Bao phủ 22 hạng mục đánh giá chi tiết (8 phân hệ chức năng, 4 hạng mục giao diện & cảnh báo trường bắt buộc, 8 hạng mục định dạng/quy tắc Excel) kèm bảng đề xuất cột Excel thực tế của đơn vị.
   - Bản HTML hoạt động 100% offline, có dải hướng dẫn 3 bước nổi bật khi mở, tự lưu nháp `localStorage`, xuất `.csv` (UTF-8 BOM cho Excel), `.json`, sao chép tóm tắt và in PDF A4.
   - Bản Excel gồm 3 sheet có menu thả xuống (`DataValidation`) và công thức `COUNTIF`/`COUNTA` tự động tổng hợp kết quả.
5. **Lưu trữ nguyên vẹn gói R5 cũ:**
   - Thư mục `v1.0.2-R5-UNSIGNED-TEST` đã được chuyển nguyên vẹn sang `BAN-GIAO/02-LUU-TRU/v1.0.2-R5-UNSIGNED-TEST/`, giữ nguyên SHA-256 `C10A1915646381331DB3481BCA0D2F546CAF3B54440721830DC38A8FB9125D45`.

---

## 2. Kết quả kiểm chứng tự động trước & sau đóng gói

| Hạng mục kiểm tra | Lệnh thực thi | Kết quả |
|---|---|---|
| Build Release toàn solution | `dotnet build IRM\IRM.sln -c Release` | **PASS** (`0 Warning(s), 0 Error(s)`) |
| Kiểm thử `.NET` / `xUnit` / `bUnit` / `SQLite` | `dotnet test IRM\IRM.Tests\IRM.Tests.csproj -c Release --no-build` | **PASS** (`363 passed, 0 failed, 6 SqlServerFact skipped`) |
| Kiểm thử End-to-End Excel mẫu R6 & Phiếu khảo sát | `dotnet test IRM\IRM.Tests\IRM.Tests.csproj --filter "FullyQualifiedName~DeliverySampleWorkbookTests"` | **PASS** (`3 passed, 0 failed`) |
| Kiểm thử trình cài đặt PowerShell 64-bit | `powershell -NoProfile -ExecutionPolicy Bypass -File IRM\deploy\windows\tests\environment.Tests.ps1` | **PASS** (`46 assertions`) |
| Kiểm thử trình cài đặt PowerShell 32-bit (WOW64) | `& "$env:SystemRoot\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File IRM\deploy\windows\tests\environment.Tests.ps1` | **PASS** (`47 assertions`) |
| Kiểm thử trình duyệt / JS (`node --test`) | `node --test IRM\IRM.Tests\Browser\*.test.cjs` | **PASS** (`50 passed, 0 failed`) |
| Đóng gói bộ cài Inno Setup 6 (`ISCC.exe`) | `.\deploy\windows\build-windows-installer.ps1 -AllowUnsigned` | **PASS** (Exit `0`, `dirty=false`, commit `706f74e44e3b97b1c50d6e3c116d4f969eb5389c`) |
| Đối chiếu `SHA256SUMS.txt` (11 tệp gói R6) | `Get-FileHash` đối chiếu từng dòng trong `SHA256SUMS.txt` | **PASS** (`10/10` dòng khớp tuyệt đối) |

---

## 3. Danh mục 11 tệp trong `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`

| Tên tệp | Kích thước (byte) | SHA-256 |
|---|---:|---|
| `DOC-TRUOC-KHI-CAI.txt` | 2.012 | `D36FC4787C650D5FE1CF9CBF39393F3498BB9D89C90C11E46C2BD69214DF275B` |
| `HUONG-DAN-CAI-DAT.html` | 20.291 | `CCC62183087687FE5C6369F023AEC09588C1D88F54366206469ADAD942BA8EB8` |
| `HUONG-DAN-CAI-DAT.md` | 9.614 | `F5D884A975F0123A15B899F2AC36DD702C1B7F7C2EE0E329947471C0EB05F0D2` |
| `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` | 10.467 | `0FD45444DD8183B0A96531ABD52EA87C2EF0F332FF14F9CBCE3A7A1CC6CE3791` |
| `IRM-v1.0.2-windows-x64-offline-setup-R6.exe` | 790.854.254 | `2D6BA7B6E2FFEB8E6CE411C473965556F2FCEFADCEE5F77F3F2FDBD77CECF585` |
| `IRM-v1.0.2-windows-x64-offline-setup-R6.exe.sha256` | 111 | `79465BC2360FA8227AB4F29557DF6C273DD2E6A54E815A626DC0509ABBA1FDCC` |
| `PHIEU-KHAO-SAT-GOP-Y.html` | 29.259 | `8C4788A799A941D30B2C7A10E57052A100588CB8C5C81CCDD002C3D18626CA61` |
| `PHIEU-KHAO-SAT-GOP-Y.xlsx` | 13.811 | `EEAB46A8356B7DC20C3094818FD97F892FF63A2CF7F57E6101B605EEAA715B41` |
| `SOURCE-REVISION.json` | 142 | `2E3F43879D00468DA3BC866C8B4C16E2A1AED31B400BFDF10F782834BE5860A8` |
| `VERIFICATION.json` | 1.386 | `64E16282F0471AFF48FD85C0FDFD3D28F6431130D57F60C2A97CE5CCAA00FB06` |
| `SHA256SUMS.txt` | *(bảng băm 10 tệp trên)* | *(kiểm chứng tự động)* |

---

## 4. Giới hạn đã biết

1. **Chưa ký số Authenticode (`NotSigned`):** Môi trường build không có chứng thư Code Signing nên gói mang nhãn `UNSIGNED-TEST`. Khi chạy trên máy khách có thể xuất hiện cảnh báo Windows SmartScreen.
2. **Phạm vi kiểm thử cài đặt thực tế:** Toàn bộ kiểm thử tự động (`.NET`, `ClosedXML`, `PowerShell x64/WOW64`, `Node.js`) và đóng gói Inno Setup đã chạy đạt 100%, nhưng chưa chạy cài đặt toàn trình SQL Server + Windows Service của file `R6.exe` trên máy ảo Windows sạch trong lượt đóng gói này (`fullR6InstallPerformed = false`).
