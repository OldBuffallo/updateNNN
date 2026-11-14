# IRM R5 — trạng thái phát hành offline

Ngày: 14/11/2026 (Asia/Bangkok). **Đã dựng candidate unsigned, chưa xác nhận phát hành chính thức.**

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
| Publish và Inno compile | Đạt; Inno compile 175,266 giây |
| Checksum stage trước nén | 406/406 khớp; chưa xác minh payload giải nén từ EXE |
| SHA-256 EXE | Khớp sidecar; chữ ký `NotSigned` |
| Code Signing | Không tìm thấy Code Signing certificate ở CurrentUser/My và LocalMachine/My trong lần kiểm tra |
| Cài mới/nâng cấp/backup/restore từ EXE R5 | Chưa thực hiện |
| DPI thực, bàn phím, dialog, browser smoke trên bản đã cài | Chưa thực hiện |

Ảnh và script hỗ trợ ở `BAN-GIAO/05-HO-TRO/2026-11-14-R5/`. Harness scratch chỉ lấy đoạn dựng UI và ba hàm UI, không gọi scanner, worker, finish handler hay database. Đã dựng thêm dialog lỗi dài và HTML báo cáo từ dữ liệu giả; kiểm tra HTML vẫn encode ký tự đặc biệt. Render không phải nghiệm thu thao tác hoặc cài đặt thực.

Review độc lập fresh-context trên `d86815e..a58a2ad` không thấy Critical/Important/Minor trong source UI/metadata. Reviewer không chạy lại test, không xác nhận nghiệm thu EXE. Worker, schema, media hashes và cơ chế ký/provenance không đổi. Kiểm tra theo tên tệp trong stage không thấy test binaries/database/backup/keys/logs; connection strings và bootstrap password trong appsettings đều trống. Đây không phải kiểm toán bí mật toàn diện.

## Candidate đã dựng

- Thư mục: `BAN-GIAO/04-CHO-NGHIEM-THU/20261114-v1.0.2-R5-UNSIGNED-TEST/`.
- EXE: `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` — `790825037` byte.
- Source commit: `a58a2adfd75099afa41f939e78fe3a5b0a74013b`, `dirty=false` trong `SOURCE-REVISION.json`.
- SHA-256: `D3C28EB488C0C7E5A434D1CBD954A51586B9814221DFFBAD4DA080763810D4B4`.
- Build dùng `-AllowUnsigned`, chữ ký `NotSigned`; chỉ dùng kiểm thử. Không promote hoặc đổi con trỏ bản hiện hành R4.
- Commit tài liệu ghi kết quả sau build không thuộc source đã nhúng trong EXE. Không dùng HEAD tài liệu để thay source commit ở trên.

## Môi trường thử và điều kiện còn thiếu

Lệnh đọc Hyper-V bị từ chối quyền. Theo yêu cầu bổ sung của người dùng, đã bật Windows Sandbox bằng `Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All -NoRestart` qua UAC. Windows báo `Disabled → Enabled`, `RestartNeeded=true`; chưa khởi động lại máy.

Sau reboot cần kiểm tra Sandbox khởi chạy được trước khi dùng. Sandbox là môi trường tạm, không thay bằng chứng cài đặt/nâng cấp qua reboot trên VM có thể lưu trạng thái. Không tự cài đè lên máy làm việc để thay thế nghiệm thu.

Đã chuẩn bị `BAN-GIAO/05-HO-TRO/2026-11-14-R5/IRM-R5-OFFLINE.wsb`: RAM 8192 MB, tắt mạng/clipboard/audio/video/printer, chỉ map candidate đọc-only vào `C:\IRM-R5`, mở Explorer thay vì tự chạy installer. XML hợp lệ, thư mục host tồn tại; **chưa thử khởi chạy Sandbox sau reboot**. Hướng dẫn kèm tại `HUONG-DAN-SANDBOX.md`.

## Các quyết định thực thi và giới hạn

- Giữ checkout `develop` theo yêu cầu workspace thay vì worktree mới; cần kiểm tra thay đổi đồng thời trước mỗi commit.
- Thay đổi trình bày xác minh bằng render và kiểm tra hiện có; không thêm test lặp màu/markup. Render không chứng minh DPI hoặc tương tác thật.
- Khi Hyper-V chưa truy cập được, dùng harness dựng UI cô lập khỏi scanner/worker; các bước cài thực vẫn chờ môi trường thử.
- Khi thiếu chứng thư, dựng candidate unsigned có nhãn rõ ràng, không phát hành chính thức.
- Thực hiện review source trước khi hoàn tất đóng gói để sửa lỗi vào candidate nếu có; review không thay nghiệm thu artifact.
- Giữ thiết kế worker/Admin/schema/WPF ngoài phạm vi UI; không suy ra các hành vi cũ đã được nghiệm thu trên candidate mới.

Không có minor nào được reviewer đề nghị hoãn. Giữ ledger của kế hoạch để tiếp tục Task 1 phần nghiệm thu, Task 3 ký/kiểm tra EXE, Task 4 và Task 5 sau reboot/chuẩn bị chứng thư.

Máy cài cần sẵn .NET Framework 4.7.2+, Windows/Defender và tài nguyên theo hướng dẫn. Gói chưa bundle Framework, không tự tải mạng lúc cài. Thiếu chứng thư thì chỉ dựng candidate thử bằng `-AllowUnsigned`, giữ trong `04-CHO-NGHIEM-THU`; không đưa vào `01-HIEN-HANH` hoặc gọi là bản chính thức.

## Công việc còn lại trước phát hành

1. Kiểm tra UI thực trên môi trường biệt lập, gồm DPI 100/125/150/200%, focus/dialog và lỗi dài.
2. Nghiệm thu cài mới và nâng cấp offline bằng dữ liệu giả, smoke ứng dụng, backup/restore và dịch vụ sau reboot.
3. Ký số bằng chứng thư tin cậy, kiểm tra artifact cuối và SHA-256 sau ký; nghiệm thu đúng artifact đó.
4. Cập nhật biên bản, inventory và thư mục hiện hành sau khi các điều kiện đạt. Gói cũ giữ nguyên.
