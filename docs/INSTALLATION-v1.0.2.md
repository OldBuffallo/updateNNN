# Hướng dẫn cài đặt IRM v1.0.2

## Hai hình thức bàn giao

1. **Online/VPS:** IRM chạy bằng Docker Compose phía sau HTTPS. Quy trình triển khai nằm tại `deploy/deploy-vps.ps1`.
2. **Offline/Windows:** khách hàng chỉ chạy `IRM-v1.0.2-windows-x64-offline-setup.exe`. Gói đã chứa ứng dụng .NET self-contained, SQL Server 2022 Express Core và CU27; máy cài không cần Internet.

## Cài offline trên Windows

Yêu cầu tối thiểu: Windows 10 21H2/Windows 11 hoặc Windows Server 2019 trở lên, 64-bit, 2 CPU logic từ 2 GHz, 2 GB RAM khả dụng, 10 GB ổ đĩa trống và Windows Defender đang hoạt động để quét tệp tải lên.

1. Sao chép file `IRM-v1.0.2-windows-x64-offline-setup.exe` vào máy cần cài.
2. Nhấp đúp file và chọn **Yes** khi Windows hỏi quyền Administrator.
3. Ở **Bước 1**, nhấn **Quét môi trường**. Trình cài đặt hiển thị báo cáo tickbox và lưu bản HTML tại `C:\ProgramData\IRM\logs`.
4. Nếu thiếu SQL Server/CU27, báo cáo ghi **Sẽ cài bổ sung**. Các thành phần này được lấy ngay trong bộ cài, không tải Internet.
5. Ở **Bước 2**, đặt tên tài khoản và mật khẩu quản trị IRM, chọn quyền truy cập LAN nếu cần, chấp nhận điều khoản SQL Server rồi nhấn **Cài đặt IRM**.
6. Chỉ khi database, đăng nhập, upload/quét file, xuất báo cáo và health check đều đạt, màn hình mới chuyển sang **Bước 3 — Hoàn tất**.

Sau khi cài:

- Trên máy chủ: `http://localhost:5050`
- Trong mạng LAN khi đã bật tùy chọn LAN: `https://TEN-MAY:5443`
- Dữ liệu và cấu hình: `C:\ProgramData\IRM`
- Công cụ backup/restore/repair/uninstall: `C:\ProgramData\IRM\tools`
- Mã khôi phục database: `C:\ProgramData\IRM\MA-KHOI-PHUC.txt` (chỉ Administrators và SYSTEM đọc được)

## Kiểm tra sau cài

- Mở shortcut **IRM** trên Desktop.
- Đăng nhập bằng tài khoản đã tạo trong bộ cài.
- Kiểm tra `http://localhost:5050/health/ready` trả về trạng thái `ready`.
- Chạy `C:\ProgramData\IRM\tools\backup.ps1`, sau đó xác nhận có tệp backup mới.

## Gỡ cài đặt

Chạy PowerShell với quyền Administrator:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\ProgramData\IRM\tools\uninstall.ps1"
```

Lệnh gỡ dịch vụ nhưng giữ lại database và dữ liệu để tránh mất dữ liệu ngoài ý muốn.

## Triển khai VPS

Quy trình VPS tạo backup trước khi thay đổi, tạo thư mục đích nếu chưa có, nạp image đã khóa version, kiểm tra health và tự rollback container khi health check thất bại. Không ghi mật khẩu mặc định trong source; truyền mật khẩu bằng biến môi trường `IRM_SQL_SA_PASSWORD` hoặc tham số SecureString.
