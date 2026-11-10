# Quản lý source và bàn giao IRM

Cập nhật ngày 10/11/2026 (Asia/Bangkok). Git root là workspace `NNN/`, nhánh `develop`; solution nâng cấp nằm trong `IRM/`, còn `immigration-reportmanager-master/` là source WPF cũ ngang cấp. Các gói cài đặt nằm trong `BAN-GIAO/` ở workspace root và được Git bỏ qua. Các đường dẫn `../BAN-GIAO/` bên dưới được tính từ thư mục solution nâng cấp.

## Thư mục chung

| Vị trí trong BAN-GIAO | Nội dung |
|---|---|
| `01-HIEN-HANH/v1.0.2-DARK-R4-UNSIGNED-TEST/` | Bộ cài R4 ngày 03/11/2026, bản mới nhất tìm thấy; giữ nguyên EXE, hướng dẫn và checksum đã có. |
| `02-LUU-TRU/` | R2, R3, thư mục bàn giao 28/09/2026, các gói ZIP/RAR cũ và ZIP source ban đầu. |
| `03-DU-LIEU-TEST/2026-11-10/` | File `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` đã sửa 10 mã quốc tịch để khớp danh mục mặc định. |
| `04-CHO-NGHIEM-THU/` | Các bản build mới, mỗi lần một thư mục timestamp và phiên bản riêng. |
| `05-HO-TRO/` | Mã tạo workbook, preview và tệp hỗ trợ kiểm tra trước đây. |

Mở `../BAN-GIAO/README.md` để tìm gói cần dùng. `DELIVERY-INVENTORY.json` ghi đường dẫn cũ/mới, kích thước và SHA-256; bản sao nằm tại `../BAN-GIAO/DANH-MUC-TEP.json`.

Đã tập trung và xác minh 160 tệp. Khi tổ chức lại, workbook đã sửa đang mở trong Excel nên không thể di chuyển trực tiếp: bản sao cùng SHA-256 đã được đặt tại `03-DU-LIEU-TEST/2026-11-10/`. Trường `pendingOriginalRemoval` trong danh mục ghi bản trùng còn giữ tại `../outputs/irm-import-fixed-20261110/`. Chờ đóng workbook trước khi chuyển bản trùng vào lưu trữ; không tự tắt Excel hoặc bỏ thay đổi của người dùng. Thao tác xóa bản trùng đã bị xét duyệt tự động từ chối với thông báo `blocked by policy`.

## Phân biệt bản đã giao và source

R4 giữ payload ứng dụng cũ từ R3. Nó không chứa đợt tách kiến trúc hoặc bản sửa validation quốc tịch vừa commit. Không có bằng chứng về Git commit chính xác dùng để build R4; không tự gán commit mới cho EXE này.

File Excel mẫu cũ vẫn được giữ trong các gói gốc để bảo toàn checksum. Khi thử import với danh mục mặc định, dùng file đã sửa ở `03-DU-LIEU-TEST/2026-11-10/`. Ba hồ sơ giả có quốc tịch Đức, Pháp và Canada đã đổi sang Úc, Anh và Hoa Kỳ; các ô ngoài quốc tịch được giữ nguyên. Đây là sửa dữ liệu test, không phải nâng cấp EXE trên máy khách.

## Kết thúc mỗi lần sửa source

1. Kiểm tra `git status` và giữ nguyên thay đổi không liên quan.
2. Chạy build/test phù hợp; ghi rõ test SQL bị bỏ qua khi thiếu `IRM_TEST_SQLSERVER`.
3. Rà soát staged diff; commit đủ source, test, cấu hình và tài liệu liên quan. Không commit credentials, database, log hoặc bộ cài.
4. Báo mã commit và kết quả kiểm tra; không để phần sửa đã hoàn thành ở trạng thái chưa commit. Chỉ push khi người dùng yêu cầu.

## Tạo gói mới

Sau khi commit và Git tree sạch, chạy `deploy/windows/build-windows-installer.ps1` từ thư mục solution nâng cấp `IRM/`. Output mặc định là `../BAN-GIAO/04-CHO-NGHIEM-THU/<timestamp>-v1.0.2/`. Có thể truyền `-OutputDirectory` cho môi trường build riêng; bản bàn giao cuối cùng vẫn phải chuyển vào thư mục chung.

Script đóng gói ghi commit vào `SOURCE-REVISION.json` trước khi tạo checksum. Giữ các giới hạn ký số/nghiệm thu hiện có; không tự chạy cài đặt lên máy đang dùng để kiểm tra việc dọn thư mục. Không ghi đè bộ cài hoặc cập nhật checksum của gói đã giao để biến nó thành bản mới.

Khi bàn giao revision mới, ghi đúng phiên bản/revision, commit nguồn, SHA-256, ngày build, kết quả nghiệm thu và các giới hạn còn lại. Cập nhật README và danh mục tệp trong thư mục chung; giữ bản trước ở `02-LUU-TRU/`.

## Kiểm tra ngày 10/11/2026

Build Release không lỗi/cảnh báo. Bộ test có 117 ca đạt và 1 ca SQL Server bị bỏ qua do chưa cấu hình kết nối test. Kiểm tra bộ cài PowerShell có 46 assertion đạt ở x64 và 47 ở x86. Đợt dọn thư mục này không build lại hoặc triển khai bộ cài lên máy khách.

Lần build kiểm tra lại sau khi chuẩn hóa khoảng trắng vẫn đạt, nhưng Windows Application Control chặn nạp `IRM.Tests.dll` (`0x800711C7`), khiến runner không tìm được test và lệnh test thất bại. Kết quả 117 đạt / 1 bỏ qua ở trên thuộc lần chạy trước trong cùng phiên; không coi lần bị chặn là test đã đạt. Không thay chính sách bảo mật Windows để chạy test.
