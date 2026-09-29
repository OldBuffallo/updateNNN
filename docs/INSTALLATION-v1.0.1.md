# Hướng dẫn cài đặt và nghiệm thu IRM v1.0.1

Tài liệu này dành cho người đóng gói, kỹ thuật viên cài đặt và người nghiệm thu IRM. Hai bộ cài dùng cùng ứng dụng, migration và database SQL Server; khác biệt chỉ nằm ở cơ chế hệ điều hành.

## Cấu hình tối thiểu

| Thành phần | Yêu cầu bắt buộc |
|---|---|
| CPU | x64/amd64, ít nhất 2 core và 2 GHz |
| RAM | Ít nhất 2 GB khả dụng trước khi cài |
| Ổ đĩa | Ít nhất 10 GB trống trước khi cài |
| Sau cài đặt | Phải còn ít nhất 4 GB |
| Windows | Windows 11 hoặc Windows Server 2019/2022/2025 64-bit còn hỗ trợ |
| Ubuntu | Ubuntu 22.04 LTS amd64, ext4 hoặc XFS |
| Quyền | Local Administrator hoặc sudo/root |

Cấu hình khuyến nghị là 4 GB RAM khả dụng và 20 GB ổ đĩa trống. Bộ cài chặn cài nếu không đạt mức tối thiểu.

## Thành phần trong bộ cài

- IRM v1.0.1 self-contained cho đúng hệ điều hành.
- Microsoft SQL Server 2022 Express CU27 build `16.0.4295.3`.
- Database migration và dữ liệu danh mục, không có hồ sơ nghiệp vụ mẫu.
- Defender adapter trên Windows hoặc ClamAV cùng chữ ký ngoại tuyến trên Ubuntu.
- Chứng thư HTTPS tự ký cho truy cập LAN.
- Service, health check, lịch backup, công cụ repair/restore/uninstall, SBOM và SHA-256.

| Nhóm | Windows | Ubuntu |
|---|---|---|
| Ứng dụng | IRM `win-x64` self-contained, static assets và Chart.js nội bộ | IRM `linux-x64` self-contained, cùng static assets |
| Database | SQL Server 2022 Express engine + CU27 | `mssql-server` Express `16.0.4295.3-1` |
| Công cụ SQL | Công cụ provisioning tích hợp trong IRM | `mssql-tools18`, `msodbcsql18` |
| Quét tệp | Microsoft Defender có sẵn trong Windows | ClamAV on-demand và chữ ký offline ban đầu |
| Vận hành | Windows Service, Scheduled Task, repair/restore/uninstall | systemd service/timer, repair/restore |
| Kiểm kê | SBOM .NET, migration SQL, manifest và SHA-256 | SBOM .NET + DEB, migration SQL, manifest và SHA-256 |

Không cài SSMS, LocalDB, SQLite, Machine Learning, PolyBase, Reporting Services hoặc ClamAV daemon thường trực.

## Cài trên Windows

1. Sao chép `IRM-v1.0.1-windows-x64-setup.exe` vào máy đích và đối chiếu SHA-256.
2. Chạy file với quyền Administrator.
3. Nhập `YES` để chấp nhận điều khoản SQL Server.
4. Nhập tên và mật khẩu quản trị IRM; mật khẩu phải có ít nhất 12 ký tự.
5. Nhập mật khẩu phục hồi `irm_dba` có ít nhất 16 ký tự; mật khẩu bootstrap SQL được sinh ngẫu nhiên.
6. Chọn có mở truy cập LAN qua HTTPS hay không.
7. Chờ health check hoàn tất. Shortcut IRM sẽ mở `http://localhost:5050`.

Nếu bật LAN, máy trạm dùng `https://TEN-MAY-CHU:5443`. Chứng thư tự ký có thể tạo cảnh báo lần đầu; chỉ chấp nhận sau khi đối chiếu đúng máy chủ.

## Cài trên Ubuntu 22.04

1. Mount ISO và đối chiếu SHA-256.
2. Bấm `Install IRM v1.0.1`; Ubuntu có thể yêu cầu chọn Allow Launching lần đầu.
3. Cấp quyền quản trị và nhập các mật khẩu giống quy trình Windows.
4. Bộ cài dùng local APT repository trong ISO, không truy cập Internet.
5. Khi health check đạt, mở IRM từ menu ứng dụng hoặc `http://localhost:5050`.

## Tài khoản và kết nối database

| Tài khoản | Phạm vi |
|---|---|
| Quản trị IRM | Do người cài đặt đặt, dùng đăng nhập giao diện |
| `irm_app` | Sinh ngẫu nhiên; chỉ có quyền runtime và backup cần thiết |
| `irm_dba` | Do người cài đặt đặt, dùng phục hồi và bảo trì |
| `sa` | Chỉ dùng bootstrap và bị vô hiệu hóa sau khi cài thành công |

SQL Server chỉ lắng nghe loopback cổng `14331`; tuyệt đối không mở cổng này trên firewall LAN.

Schema v1.0.1 có SHA-256 `B737B48CE90D924B78303E3E22C9B5D465E880E37B0F6C93A7FA15809CA46CA2`, giống nhau trên Windows và Ubuntu.

## Backup và dung lượng

- Backup tự động lúc khoảng 02:00 mỗi ngày.
- Backup được tạo dạng `IRM_yyyyMMdd_HHmmss.bak.gz`, giữ tối đa 7 bản và tổng dung lượng tối đa 2 GB.
- Khi vượt hạn mức, xóa bản cũ nhất nhưng luôn giữ một bản phục hồi hợp lệ.
- Upload bị chặn khi ổ đĩa còn dưới 1 GB; log cảnh báo bắt đầu dưới 2 GB.
- Khi cần lưu lâu hơn, cấu hình thư mục backup ngoài trước khi dung lượng đạt ngưỡng.
- Windows phục hồi bằng `C:\ProgramData\IRM\tools\restore.ps1 -ArchivePath <tệp.bak.gz>` trong PowerShell Administrator; kiểm tra bằng `repair.ps1`.
- Ubuntu phục hồi bằng `sudo restore-irm <tệp.bak.gz>`; kiểm tra bằng `sudo repair-irm`.

## Checklist nghiệm thu

- [ ] Cài khi máy không có Internet.
- [ ] Xác nhận SQL Server version `16.0.4295.3`, edition Express.
- [ ] `/health/ready` trả trạng thái `ready`, database `sqlserver`, schema `1.0.1`.
- [ ] Đăng nhập bằng mật khẩu đã đặt; `admin/123456` không hoạt động.
- [ ] Thêm, sửa, tìm kiếm và xuất báo cáo.
- [ ] Import Excel và rollback import.
- [ ] Upload tệp sạch thành công; tệp nhiễm hoặc antivirus lỗi bị chặn.
- [ ] Reboot máy và xác nhận IRM tự chạy.
- [ ] Chạy backup và thử phục hồi trên môi trường nghiệm thu.
- [ ] Sau cài đặt còn ít nhất 4 GB ổ đĩa.
- [ ] Artifact, SBOM và SHA-256 được lưu cùng biên bản bàn giao.

Không bàn giao nếu bất kỳ mục bắt buộc nào chưa đạt.
