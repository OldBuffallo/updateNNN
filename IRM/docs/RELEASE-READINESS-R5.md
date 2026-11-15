# IRM R5 — trạng thái phát hành offline

Ngày: 15/11/2026 (Asia/Bangkok). **Đã dựng và hoàn thiện bàn giao gói R5 giao diện Landing Page (`UNSIGNED-TEST`) tại `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`.**

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
| Publish và Inno compile | Đạt; Inno compile 174,265 giây (`790828274` byte); payload trước nén 406/406 khớp SHA-256 |
| Code Signing | Không tìm thấy Code Signing certificate trên máy build; đóng gói với `-AllowUnsigned` (`NotSigned`) |
| Giải nén thực EXE R5 trong Windows Sandbox ngoại tuyến | Đạt: Inno thoát mã `0`, giải nén `406/406` tệp (`407` gồm `SHA256SUMS.txt`, `0` tệp dư/sai/thiếu), `SOURCE-REVISION.json` khớp commit `b42a277` (`dirty=false`), AST `9/9` tệp `.ps1` đạt trên x64 & x86 |
| Render UI & kiểm thử âm tính trong Windows Sandbox | Đạt: render từ payload giải nén đủ 6 trạng thái + `dialog.png` + HTML report (`guest-ui/`), `ReadOnly`/khóa điều khiển khi `busy`, mã hóa HTML, quét thực `Invoke-EnvironmentScan` đạt `14/15` mục (chỉ chặn do `WinDefend` tắt), chặn `.NET < 4.7.2` (`461807`) và thiếu `sql-base.exe` (`install.ps1` mã thoát `1`) |
| Cài mới/nâng cấp/backup/restore từ EXE R5 | Bị chặn end-to-end trong Windows Sandbox do không có phiên `ExistingLogin` (`0x80070520`, `dwm`/`explorer` không chạy) và `WinDefend` ở trạng thái `Stopped`/`Disabled`; cần VM desktop đầy đủ nếu nghiệm thu cài đặt thực tế |

Ảnh UI dữ liệu giả và báo cáo HTML mới lưu tại `BAN-GIAO/05-HO-TRO/2026-11-15-R5-LANDING/after/`. Các ảnh mint/emerald ngày 14/11 được giữ nguyên tại `BAN-GIAO/05-HO-TRO/2026-11-14-R5/`.

## Candidate và gói bàn giao R5 Landing Page (15/11/2026)

- Thư mục truy vết candidate: `BAN-GIAO/04-CHO-NGHIEM-THU/20261115-v1.0.2-R5-LANDING-UNSIGNED-TEST/` (giữ lại `SOURCE-REVISION.json`, `stage-SHA256SUMS.txt` và `.exe.sha256` sau khi dọn `windows-stage/` và EXE trùng).
- Thư mục bàn giao hiện hành (duy nhất trong `01-HIEN-HANH/`): `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` (9 tệp, đã bổ sung `HUONG-DAN-CAI-DAT.html` tự chứa và cập nhật `SHA256SUMS.txt`).
- EXE: `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` — `790828274` byte.
- Source commit: `b42a277894b963d3a38ccc50ac924d0756f0a741`, `dirty=false` trong `SOURCE-REVISION.json` (`builtAtUtc`: `2026-11-15T05:44:47.2609258Z`).
- SHA-256 EXE: `C10A1915646381331DB3481BCA0D2F546CAF3B54440721830DC38A8FB9125D45` (khớp sidecar `.sha256` và `SHA256SUMS.txt`).
- Trạng thái ký: `NotSigned` (dựng với `-AllowUnsigned`; gói R4 cũ đã chuyển nguyên vẹn vào lưu trữ tại `02-LUU-TRU/v1.0.2-DARK-R4-UNSIGNED-TEST/`).

## Mốc candidate R5 trước đó (14/11/2026 — UI mint/emerald, đã dọn)

- Đã dọn thư mục build trung gian `BAN-GIAO/04-CHO-NGHIEM-THU/20261114-v1.0.2-R5-UNSIGNED-TEST/` (và `20261111-183841-705-v1.0.2`) để giải phóng dung lượng.
- EXE cũ (đã thay thế bởi bản 15/11): `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` — `790825037` byte.
- Source commit: `a58a2adfd75099afa41f939e78fe3a5b0a74013b`, `dirty=false`.
- SHA-256: `D3C28EB488C0C7E5A434D1CBD954A51586B9814221DFFBAD4DA080763810D4B4` (`NotSigned`).

## Môi trường thử và điều kiện còn thiếu (Sandbox ngày 14/11)

Lệnh đọc Hyper-V trước đó bị từ chối quyền. Đã bật Windows Sandbox bằng `Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All -NoRestart` qua UAC và reboot.

Sau reboot, Windows tự cập nhật client Sandbox lên `0.8.114.0`. Máy ảo khởi chạy được nhưng desktop không đăng nhập được, mất kết nối lặp lại. Guest Application log ghi `dwm.exe 10.0.26100.9818` crash trong `udwm.dll 10.0.26100.9974`, exception `0xc00001ad`. Thử cấu hình riêng `VGpu=Disable` vẫn gặp lỗi.

Đã chạy `BAN-GIAO/05-HO-TRO/2026-11-14-R5/IRM-R5-OFFLINE.wsb`: guest Windows 11 Enterprise build 26100 có 8 CPU logic, khoảng 7 GB RAM trống, hơn 76 GB đĩa trống, Framework Release 533320 và 0 adapter mạng Up. CLI Sandbox thực thi được lệnh System; `ExistingLogin` báo không có logon session (`0x80070520`). Defender `AntivirusEnabled=false`, service `WinDefend` ở trạng thái Stopped/Disabled. Thử EXE dưới System thì Inno giải nén thành công nhưng WinForms từ chối `ShowDialog` do không có `UserInteractive` session. Chưa thực hiện cài mới/nâng cấp/backup/restore/browser smoke trong VM.

## Kiểm chứng trong Windows Sandbox ngoại tuyến ngày 15/11/2026 (bản R5 Landing Page)

- Cấu hình & bằng chứng: `BAN-GIAO/05-HO-TRO/2026-11-15-R5-LANDING/IRM-R5-OFFLINE.wsb` (`Networking=Disable`, `MemoryInMB=8192`), kịch bản `sandbox-tools/run-sandbox-suite.ps1` và toàn bộ log/JSON/HTML/ảnh kết quả tại `BAN-GIAO/05-HO-TRO/2026-11-15-R5-LANDING/sandbox-results/`.
- Chẩn đoán guest (`environment.json`, `desktop-diagnostic.json`): Microsoft Windows 11 Enterprise `10.0.26100`, 8 vCPU (`Intel Core i7-1185G7`), `8190.94 MB` RAM, `77.16 GB` đĩa trống, `.NET Framework Release=533320` (`4.8.09032`), `0` adapter mạng Up (`nonLoopbackIPv4: []`); chạy dưới phiên `NT AUTHORITY\SYSTEM` (`UserInteractive=false`, `explorerRunning=false`, `dwmRunning=false`, `wsb exec -r ExistingLogin` trả lỗi `0x80070520`); dịch vụ `WinDefend` `Stopped`/`Disabled` (`AntivirusEnabled=false`).
- Giải nén EXE thực tế (`actual-extraction-verification.json`, `inno-extraction.log`): kiểm tra EXE `790828274` byte SHA-256 `C10A1915646381331DB3481BCA0D2F546CAF3B54440721830DC38A8FB9125D45` (`NotSigned`), tạm dừng tiến trình `powershell.exe` con của Inno (`installer-ui.ps1`) tại `C:\Windows\SystemTemp\is-55DBH75F9W.tmp\irm-v1.0.2`, đối chiếu khớp `406/406` tệp theo `SHA256SUMS.txt` (`407` tệp tổng cộng, `0` tệp thiếu/sai hash/dư), `SOURCE-REVISION.json` khớp commit `b42a277894b963d3a38ccc50ac924d0756f0a741` (`dirty=false`), sau khi tiếp tục Inno thoát mã `0` và dọn sạch thư mục tạm.
- AST & dựng UI từ payload giải nén (`guest-ui-verification.json`, `sandbox-results/guest-ui/`): `9/9` tệp `.ps1` qua kiểm tra AST không lỗi trên cả x64 và x86 (`SysWOW64`); dựng thành công đủ 7 PNG (`scan.png`, `configure.png`, `upgrade.png`, `busy.png`, `retry.png`, `done.png`, `dialog.png`) kèm `Bao-cao-moi-truong-sandbox-ui.html` — `2` ô mật khẩu che ký tự, `busy` khóa `TextBox` bằng `ReadOnly=true` và vô hiệu hóa `CheckBox`/nút chính, `retry` mở khóa lại, và thoát HTML an toàn với `<script>`/`&`.
- Quét môi trường thực & kiểm thử âm tính (`negative-and-scan-tests.json`, `guest-real-scan-report.html`): `Invoke-EnvironmentScan` trên guest đạt `14/15` hạng mục (bao gồm toàn vẹn checksum offline, nhận diện SQL Express + CU27 sẽ cài mới offline, các cổng `14331/5050/5443` trống) và chặn đúng ở mục `Windows Defender` (`Không đạt`); kiểm thử âm tính chặn `.NET Framework Release=461807` (< 4.7.2, `Passed=false`) và khi xóa `sql-base.exe`, cả `Invoke-EnvironmentScan` (`scanPassed=false`) lẫn `install.ps1 -NonInteractive` đều chặn đúng (mã thoát `1`, `IRM_ERROR_STEP=Kiểm tra tính toàn vẹn của bộ cài`, `IRM_ERROR_MESSAGE=Thiếu tệp trong bộ cài: sql-base.exe`).

## Các quyết định thực thi và giới hạn

- Giữ checkout `develop` theo yêu cầu workspace; không thay đổi worker/Admin/schema/WPF.
- Đọc ảnh `logo-cand.png` và `police-3d.png` qua `MemoryStream` + clone `Bitmap` để không giữ khóa tệp trong `$PayloadRoot`.
- Chuyển nguyên vẹn gói R4 vào lưu trữ tại `BAN-GIAO/02-LUU-TRU/v1.0.2-DARK-R4-UNSIGNED-TEST/`, dọn các bản build thử trung gian/trùng lặp trong `04-CHO-NGHIEM-THU/`, chỉ giữ duy nhất `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` làm gói bàn giao hiện hành (đã kèm `HUONG-DAN-CAI-DAT.html`).
- Thiếu chứng thư Code Signing nên gói vẫn gắn nhãn `UNSIGNED-TEST`, lưu ý SmartScreen khi chạy trên máy khách.

## Công việc tiếp theo (nếu phát hành chính thức có ký số)

1. Chuẩn bị VM/máy Windows thử có desktop hoạt động và Defender; nghiệm thu cài mới, nâng cấp offline bằng dữ liệu giả, DPI 100/125/150/200%, smoke ứng dụng và backup/restore.
2. Ký số bằng chứng thư Code Signing tin cậy, kiểm tra lại chữ ký và SHA-256 sau ký.
