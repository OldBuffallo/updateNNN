# IRM R5 — trạng thái phát hành offline

Ngày: 14/11/2026 (Asia/Bangkok). **Đang chuẩn bị candidate, chưa xác nhận phát hành chính thức.**

## Phạm vi và căn cứ

Người dùng xác nhận bộ cài trước đã hoạt động tốt và yêu cầu UI phù hợp ứng dụng mới. R5 kế thừa Inno/WinForms/worker, thay palette sang mint/emerald và đóng gói source hiện tại. Không đổi worker cài đặt, SQL media, database schema, dịch vụ, cổng hoặc quy tắc đặt lại Admin khi nâng cấp.

Version ứng dụng `1.0.2`, revision `R5`; artifact `IRM-v1.0.2-windows-x64-offline-setup-R5.exe`. Commit nguồn cuối và SHA-256 phải lấy từ candidate thực sau build; không gán commit mới cho R4/candidate 11/11.

## Kiểm chứng trong phiên

| Hạng mục | Trạng thái |
|---|---|
| Render WinForms dữ liệu giả: scan/configure/upgrade/busy/retry/done | Đã dựng và xem ảnh cả 6 trạng thái |
| Khóa input khi busy, mở lại khi retry; che ô mật khẩu | Đạt trong harness dựng UI, không chạy worker |
| Parser Windows PowerShell 5.1 | 3 tệp UI/theme/environment không lỗi |
| Preflight x64/x86 | 46/47 assertion đạt |
| Build Release | Đạt, 0 warning/error |
| .NET với SQL biệt lập | 337/337 đạt, 0 fail/skip |
| JavaScript | 47/47 đạt, 0 fail/skip |
| Inno compile, publish, checksum payload/EXE | Chưa thực hiện |
| Code Signing | Không tìm thấy Code Signing certificate ở CurrentUser/My và LocalMachine/My trong lần kiểm tra |
| Cài mới/nâng cấp/backup/restore từ EXE R5 | Chưa thực hiện |
| DPI thực, bàn phím, dialog, browser smoke trên bản đã cài | Chưa thực hiện |

Ảnh và script hỗ trợ ở `BAN-GIAO/05-HO-TRO/2026-11-14-R5/`. Harness scratch chỉ lấy đoạn dựng UI và ba hàm UI, không gọi scanner, worker, finish handler hay database. Render không phải nghiệm thu thao tác hoặc cài đặt thực.

## Môi trường thử và điều kiện còn thiếu

Lệnh đọc Hyper-V bị từ chối quyền. Theo yêu cầu bổ sung của người dùng, đã bật Windows Sandbox bằng `Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All -NoRestart` qua UAC. Windows báo `Disabled → Enabled`, `RestartNeeded=true`; chưa khởi động lại máy.

Sau reboot cần kiểm tra Sandbox khởi chạy được trước khi dùng. Sandbox là môi trường tạm, không thay bằng chứng cài đặt/nâng cấp qua reboot trên VM có thể lưu trạng thái. Không tự cài đè lên máy làm việc để thay thế nghiệm thu.

Máy cài cần sẵn .NET Framework 4.7.2+, Windows/Defender và tài nguyên theo hướng dẫn. Gói chưa bundle Framework, không tự tải mạng lúc cài. Thiếu chứng thư thì chỉ dựng candidate thử bằng `-AllowUnsigned`, giữ trong `04-CHO-NGHIEM-THU`; không đưa vào `01-HIEN-HANH` hoặc gọi là bản chính thức.

## Công việc còn lại trước phát hành

1. Kiểm tra UI thực trên môi trường biệt lập, gồm DPI 100/125/150/200%, focus/dialog và lỗi dài.
2. Nghiệm thu cài mới và nâng cấp offline bằng dữ liệu giả, smoke ứng dụng, backup/restore và dịch vụ sau reboot.
3. Ký số bằng chứng thư tin cậy, kiểm tra artifact cuối và SHA-256 sau ký; nghiệm thu đúng artifact đó.
4. Cập nhật biên bản, inventory và thư mục hiện hành sau khi các điều kiện đạt. Gói cũ giữ nguyên.
