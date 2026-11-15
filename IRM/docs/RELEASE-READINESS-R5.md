# IRM R5 — trạng thái phát hành offline

Ngày: 15/11/2026 (Asia/Bangkok). **Đang hoàn thiện bản đóng gói R5 giao diện Landing Page (unsigned, bàn giao kiểm thử nội bộ).**

## Phạm vi và căn cứ

Người dùng xác nhận bộ cài trước đã hoạt động tốt và yêu cầu đồng bộ giao diện bộ cài sử dụng hình nền Landing Page (Phương án A: Split 2 cột). R5 kế thừa Inno/WinForms/worker, thay giao diện sang nền xanh olive vát khối (`#C2D67C → #9AB55C → #6D8446`), Công an hiệu (`logo-cand.png`), nhân vật 3D Công an Nhân dân (`police-3d.png`) đứng trên bệ ở cột phải, khung bảng kiểm tra/cấu hình sáng rõ ở cột trái, nút hành động chính màu cam (`#EA580C`) và đóng gói source hiện tại. Không đổi worker cài đặt, SQL media, database schema, dịch vụ, cổng hoặc quy tắc đặt lại Admin khi nâng cấp.

Version ứng dụng `1.0.2`, revision `R5`; artifact `IRM-v1.0.2-windows-x64-offline-setup-R5.exe`. Commit nguồn cuối và SHA-256 phải lấy từ candidate thực sau build; không gán commit mới cho R4/candidate cũ.

## Kiểm chứng trong phiên (15/11/2026)

| Hạng mục | Trạng thái |
|---|---|
| Render WinForms dữ liệu giả: scan/configure/upgrade/busy/retry/done | Đã dựng và xem ảnh cả 6 trạng thái theo Landing Page (Phương án A) |
| Khóa input khi busy, mở lại khi retry; che ô mật khẩu | Đạt trong harness dựng UI, không chạy worker |
| Báo cáo HTML & hộp thoại lỗi dài | Đạt; HTML encode bảo toàn ký tự đặc biệt |
| Parser Windows PowerShell 5.1 & Preflight x64/x86 | Đạt: 46/46 assertion x64, 47/47 assertion x86 (`SysWOW64`) |
| Build Release | Đạt, 0 warning/error |
| .NET với SQL biệt lập (`.\SQLEXPRESS`) | 337/337 đạt, 0 fail/skip |
| JavaScript (`node --test`) | 47/47 đạt, 0 fail/skip |
| Publish và Inno compile | Đang tiến hành đóng gói bản Landing Page (xem mục Candidate) |
| Code Signing | Không tìm thấy Code Signing certificate trên máy build; đóng gói với `-AllowUnsigned` |
| Cài mới/nâng cấp/backup/restore từ EXE R5 | Bị chặn bởi môi trường Windows Sandbox trên máy chủ; chưa nghiệm thu cài đặt end-to-end |

Ảnh UI dữ liệu giả và báo cáo HTML mới lưu tại `BAN-GIAO/05-HO-TRO/2026-11-15-R5-LANDING/after/`. Các ảnh mint/emerald ngày 14/11 được giữ nguyên tại `BAN-GIAO/05-HO-TRO/2026-11-14-R5/`.

## Mốc candidate R5 trước đó (14/11/2026 — UI mint/emerald)

- Thư mục: `BAN-GIAO/04-CHO-NGHIEM-THU/20261114-v1.0.2-R5-UNSIGNED-TEST/`.
- EXE: `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` — `790825037` byte.
- Source commit: `a58a2adfd75099afa41f939e78fe3a5b0a74013b`, `dirty=false`.
- SHA-256: `D3C28EB488C0C7E5A434D1CBD954A51586B9814221DFFBAD4DA080763810D4B4` (`NotSigned`).

## Môi trường thử và điều kiện còn thiếu (Sandbox ngày 14/11)

Lệnh đọc Hyper-V trước đó bị từ chối quyền. Đã bật Windows Sandbox bằng `Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All -NoRestart` qua UAC và reboot.

Sau reboot, Windows tự cập nhật client Sandbox lên `0.8.114.0`. Máy ảo khởi chạy được nhưng desktop không đăng nhập được, mất kết nối lặp lại. Guest Application log ghi `dwm.exe 10.0.26100.9818` crash trong `udwm.dll 10.0.26100.9974`, exception `0xc00001ad`. Thử cấu hình riêng `VGpu=Disable` vẫn gặp lỗi.

Đã chạy `BAN-GIAO/05-HO-TRO/2026-11-14-R5/IRM-R5-OFFLINE.wsb`: guest Windows 11 Enterprise build 26100 có 8 CPU logic, khoảng 7 GB RAM trống, hơn 76 GB đĩa trống, Framework Release 533320 và 0 adapter mạng Up. CLI Sandbox thực thi được lệnh System; `ExistingLogin` báo không có logon session (`0x80070520`). Defender `AntivirusEnabled=false`, service `WinDefend` ở trạng thái Stopped/Disabled. Thử EXE dưới System thì Inno giải nén thành công nhưng WinForms từ chối `ShowDialog` do không có `UserInteractive` session. Chưa thực hiện cài mới/nâng cấp/backup/restore/browser smoke trong VM.

## Các quyết định thực thi và giới hạn

- Giữ checkout `develop` theo yêu cầu workspace; không thay đổi worker/Admin/schema/WPF.
- Đọc ảnh `logo-cand.png` và `police-3d.png` qua `MemoryStream` + clone `Bitmap` để không giữ khóa tệp trong `$PayloadRoot`.
- Giữ nguyên gói R4 tại `BAN-GIAO/01-HIEN-HANH/v1.0.2-DARK-R4-UNSIGNED-TEST/` và candidate 14/11 tại `04-CHO-NGHIEM-THU/`; đưa bản R5 Landing Page vào `04-CHO-NGHIEM-THU/` và hoàn thiện bản giao tại `01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`.
- Thiếu chứng thư Code Signing nên gói vẫn gắn nhãn `UNSIGNED-TEST`, lưu ý SmartScreen khi chạy trên máy khách.

## Công việc tiếp theo (nếu phát hành chính thức có ký số)

1. Chuẩn bị VM/máy Windows thử có desktop hoạt động và Defender; nghiệm thu cài mới, nâng cấp offline bằng dữ liệu giả, DPI 100/125/150/200%, smoke ứng dụng và backup/restore.
2. Ký số bằng chứng thư Code Signing tin cậy, kiểm tra lại chữ ký và SHA-256 sau ký.
