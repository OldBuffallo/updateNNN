# Immigration Report Manager v1.0.1

IRM là ứng dụng nội bộ quản lý hồ sơ người nước ngoài, doanh nghiệp, lưu trú, kiểm tra, import và báo cáo. Bản v1.0.1 sử dụng một mã nguồn và một schema SQL Server trên cả Windows lẫn Ubuntu.

## Trạng thái phát hành

- Runtime phát hành chỉ hỗ trợ Microsoft SQL Server 2022 Express CU27, build `16.0.4295.3`.
- Không có SQLite hoặc LocalDB trong gói ứng dụng.
- Hai artifact dự kiến: `IRM-v1.0.1-windows-x64-setup.exe` và `IRM-v1.0.1-ubuntu22.04-amd64-offline.iso`.
- Bộ cài hoạt động ngoại tuyến sau khi được build với đầy đủ media Microsoft.
- Chưa được coi là sẵn sàng bàn giao cho đến khi hai artifact được tạo và vượt nghiệm thu trên máy sạch.

## Cấu hình tối thiểu

| Thành phần | Yêu cầu |
|---|---|
| CPU | x64/amd64, 2 core, 2 GHz |
| RAM | 2 GB khả dụng trước khi cài |
| Ổ đĩa | 10 GB trống trước khi cài và tối thiểu 4 GB sau khi cài |
| Windows | Windows 11 hoặc Windows Server 2019/2022/2025 64-bit còn hỗ trợ |
| Linux | Ubuntu 22.04 LTS amd64, ext4 hoặc XFS |
| Internet | Không cần khi cài đặt hoặc vận hành |

Mốc RAM và ổ đĩa là tài nguyên còn trống trên máy đã có hệ điều hành, không phải tổng cấu hình máy.

## Thành phần chính

- ASP.NET Core 8 Blazor Interactive Server, publish self-contained.
- Entity Framework Core SQL Server 8.0.31.
- SQL Server 2022 Express CU27 với database `IRM`.
- ClosedXML cho import và xuất Excel.
- Microsoft Defender trên Windows, ClamAV quét theo yêu cầu trên Ubuntu.
- Database migration, health check, backup nén và giới hạn lưu trữ.

## Phát triển và kiểm thử

Máy phát triển cần một SQL Server tương thích. Đặt connection string qua biến môi trường, không lưu mật khẩu vào Git:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=127.0.0.1,14331;Database=IRM;User Id=irm_app;Password=...;Encrypt=True;TrustServerCertificate=True'
dotnet run --project IRM/IRM.csproj
```

SQLite chỉ được dùng trong test in-memory để kiểm thử nhanh và nằm riêng trong `IRM.Tests`; nó không được tham chiếu bởi project phát hành.

```powershell
dotnet test IRM.Tests/IRM.Tests.csproj -c Release
```

## Tạo bộ cài

### Windows

1. Đặt media Microsoft đã ký vào `deploy/vendor/windows/`:
   - `SQLEXPR_x64_ENU.exe`
   - `SQLServer2022-KB5104824-x64.exe`
2. Cài Inno Setup 6 trên máy build.
3. Chạy:

```powershell
./deploy/windows/build-windows-installer.ps1
```

### Ubuntu

Chạy trên máy build Ubuntu 22.04 amd64 đã cấu hình repository chính thức của Microsoft:

```bash
sudo ./deploy/linux/build-ubuntu-offline.sh
```

Build tạo local APT repository, app self-contained, chữ ký ClamAV, SBOM và checksum trong ISO.

Chi tiết cài đặt, bảo mật và checklist nghiệm thu nằm trong [hướng dẫn phát hành](docs/INSTALLATION-v1.0.1.md). Trạng thái và các điều kiện còn thiếu được ghi tại [biên bản sẵn sàng](docs/RELEASE-READINESS-v1.0.1.md).

## Bảo mật mặc định

- Không có mật khẩu quản trị mặc định.
- Bộ cài chỉ yêu cầu đặt mật khẩu quản trị IRM và mật khẩu phục hồi `irm_dba`; mật khẩu bootstrap SQL được sinh ngẫu nhiên rồi tài khoản `sa` bị vô hiệu hóa.
- Runtime dùng login `irm_app` với quyền đọc, ghi, thực thi và backup; không có quyền sysadmin.
- Login `sa` bị vô hiệu hóa sau bootstrap; `irm_dba` được dùng cho phục hồi và bảo trì.
- SQL Server chỉ lắng nghe tại loopback cổng `14331`.
- HTTP chỉ chấp nhận trên loopback; truy cập LAN dùng HTTPS cổng `5443`.
- Upload bị chặn nếu antivirus không hoạt động hoặc ổ đĩa còn dưới 1 GB.

## Dữ liệu và backup

Backup SQL được tạo hằng ngày, kiểm tra checksum rồi nén thành `IRM_*.bak.gz`. Hệ thống giữ tối đa 7 bản và tổng dung lượng tối đa 2 GB, nhưng không xóa bản hợp lệ cuối cùng. Công cụ phục hồi nằm trong `C:\ProgramData\IRM\tools` trên Windows và lệnh `restore-irm` trên Ubuntu.

- Database nghiệp vụ: `IRM`.
- File đính kèm lưu ngoài database.
- SQL Server Express giới hạn 10 GB mỗi database.
