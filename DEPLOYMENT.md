# Hướng Dẫn Triển Khai IRM - Immigration Report Manager v2.0

## Mục Lục
1. [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
2. [Triển khai trên máy chủ vật lý (Windows)](#triển-khai-trên-máy-chủ-vật-lý-windows)
3. [Triển khai trên Render (Cloud)](#triển-khai-trên-render-cloud)
4. [Cấu hình Database](#cấu-hình-database)
5. [Backup & Restore](#backup--restore)
6. [Khắc phục sự cố](#khắc-phục-sự-cố)

---

## Yêu Cầu Hệ Thống

### Phần mềm bắt buộc
| Thành phần | Phiên bản | Ghi chú |
|---|---|---|
| .NET Runtime | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Hệ điều hành | Windows Server 2016+ / Windows 10+ | Hoặc Linux (Docker) |
| Database | SQL Server 2014+ HOẶC SQLite | SQLite tự động khi không có SQL Server |
| Trình duyệt | Chrome/Edge/Firefox bản mới nhất | Hỗ trợ WebSocket cho Blazor Server |

### Phần cứng tối thiểu
- **CPU:** 2 cores
- **RAM:** 4 GB
- **Ổ đĩa:** 2 GB trống (ứng dụng + database)

---

## Triển Khai Trên Máy Chủ Vật Lý (Windows)

### Bước 1: Cài đặt .NET 8 Runtime

```powershell
# Download và cài đặt ASP.NET Core 8.0 Runtime
# https://dotnet.microsoft.com/download/dotnet/8.0
# Chọn: ASP.NET Core Runtime 8.0.x (Hosting Bundle) cho Windows
```

### Bước 2: Build ứng dụng

```powershell
# Trên máy phát triển
cd IRM
dotnet publish -c Release -o ./publish
```

### Bước 3: Sao chép lên server

Sao chép toàn bộ thư mục `publish/` lên server, ví dụ:
```
C:\IRM\
```

### Bước 4A: Chạy trực tiếp (Kestrel)

```powershell
cd C:\IRM
dotnet IRM.dll --urls "http://0.0.0.0:5000"
```

Truy cập: `http://<IP_SERVER>:5000`

### Bước 4B: Cấu hình IIS (Production)

1. Cài đặt **ASP.NET Core Hosting Bundle**
2. Tạo Application Pool mới:
   - Tên: `IRM_Pool`
   - .NET CLR Version: **No Managed Code**
   - Pipeline: **Integrated**
3. Tạo Website:
   - Physical Path: `C:\IRM`
   - Application Pool: `IRM_Pool`
   - Port: `80` hoặc `443` (nếu có SSL)
4. Đảm bảo `web.config` đã có trong thư mục publish

### Bước 5: Cấu hình chạy như Windows Service

```powershell
# Tạo Windows Service
sc.exe create IRM binPath="C:\IRM\IRM.exe --urls http://0.0.0.0:5000"
sc.exe config IRM start=auto
sc.exe start IRM
```

---

## Triển Khai Trên Render (Cloud)

### Yêu cầu
- Tài khoản [Render](https://render.com)
- Repository GitHub đã push code

### Các bước

1. **Tạo Web Service** trên Render Dashboard
2. **Kết nối GitHub** repository
3. **Cấu hình:**
   - Environment: `Docker`
   - Dockerfile Path: `Dockerfile`
   - Plan: Free hoặc Starter
4. **Deploy** — Render sẽ tự build từ Dockerfile

### Biến môi trường quan trọng
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000
DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
DOTNET_USE_POLLING_FILE_WATCHER=true
```

---

## Cấu Hình Database

### Chế độ SQLite (Mặc định)
- Database tự tạo tại: `<app_dir>/ReportManagerDB.db`
- Không cần cấu hình thêm
- Phù hợp demo và triển khai đơn giản

### Chế độ SQL Server
Thêm vào `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ReportManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Hoặc dùng tài khoản:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100;Database=ReportManagerDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

---

## Backup & Restore

### SQLite
```powershell
# Backup
Copy-Item "C:\IRM\ReportManagerDB.db" "C:\Backup\ReportManagerDB_$(Get-Date -Format 'yyyyMMdd').db"

# Restore
Copy-Item "C:\Backup\ReportManagerDB_20260423.db" "C:\IRM\ReportManagerDB.db" -Force
# Restart ứng dụng
```

### SQL Server
```powershell
# Backup
sqlcmd -S localhost -Q "BACKUP DATABASE ReportManagerDB TO DISK='C:\Backup\ReportManagerDB.bak'"

# Restore
sqlcmd -S localhost -Q "RESTORE DATABASE ReportManagerDB FROM DISK='C:\Backup\ReportManagerDB.bak' WITH REPLACE"
```

---

## Thông Tin Đăng Nhập

| Tài khoản | Mật khẩu |
|---|---|
| admin | demo |

> ⚠️ **Lưu ý:** Đổi mật khẩu trước khi đưa vào sản xuất thực tế.

---

## Khắc Phục Sự Cố

### Lỗi "inotify instances has been reached" (Linux/Docker)
```
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
```

### Lỗi kết nối SQL Server
- Kiểm tra SQL Server đang chạy: `Get-Service MSSQLSERVER`
- Kiểm tra firewall port 1433
- Kiểm tra connection string trong `appsettings.json`

### Lỗi trắng trang / WebSocket
- Đảm bảo trình duyệt hỗ trợ WebSocket
- Nếu dùng proxy (nginx), thêm cấu hình WebSocket:
```nginx
location / {
    proxy_pass http://localhost:5000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
}
```

### Lỗi import Excel
- File phải là `.xlsx` (Excel 2007+)
- Dòng đầu tiên phải là tiêu đề cột
- Tối đa 10MB

---

## Liên Hệ Hỗ Trợ

- 📧 Email: support@irm.vn
- 📞 Điện thoại: 0123.456.789
- 📖 Phiên bản: **v2.0** — Cập nhật: Tháng 04/2026
