# Bộ cài R4 mặc định dark mode

R4 giữ nguyên payload ứng dụng, SQL, schema và worker cài đặt của R3; cập nhật giao diện và tài liệu. Không đưa refactor ứng dụng mới vào gói.

- EXE: `build-output/v1.0.2-DARK-R4-UNSIGNED-TEST/IRM-v1.0.2-windows-x64-offline-setup-R4-DARK-UNSIGNED-TEST.exe`.
- SHA-256: `5B591C720C4E99D6FDDA647C3955F87AB16806825B699F8F9B1399DC1C0B0BD3`; 789648104 byte.
- Giải nén lại EXE và xác minh 394/394 checksum. Toàn bộ 378 tệp ứng dụng, SQL, schema, worker, backup/restore/repair/uninstall trùng R3 từng byte.
- Inno Setup dùng `WizardStyle=modern dark includetitlebar`; WinForms có palette cố định, bảng và thanh tiến trình tự vẽ, ô nhập tối, nút/checkbox bị khóa vẫn rõ chữ. Không sửa registry/theme Windows. Thanh tiêu đề WinForms yêu cầu DWM hỗ trợ; hộp thoại bảo mật của Windows do hệ điều hành hiển thị.
- Đã xem ảnh các màn hình quét, cài mới, nâng cấp, đang cài, thông báo và resize; xác minh khóa ô nhập khi cài và mở lại khi thử lại. Preview không chạy scan thật, worker hoặc ghi vào database/dịch vụ.
- PowerShell preflight: x64 46 assertion, x86 47 assertion. Helper theme biên dịch trên Windows PowerShell 5.1. CI đã bao gồm parser helper mới.
- Tài liệu cài TXT/HTML và Excel mẫu được đưa vào thư mục R4. Bản R3 và RAR cũ không thay đổi.
- EXE R4 chưa ký số; chưa chạy cài đặt đầy đủ R4 trên máy sạch/máy khách. Xác nhận chạy tốt trước đó của người dùng áp dụng cho R3.

Nguồn cú pháp dark mode của Inno: https://jrsoftware.org/ishelp/topic_setup_wizardstyle.htm .
