# IRM v2.0 — Hướng dẫn Triển khai Hệ thống

> **Hệ thống Quản lý Báo cáo Lao động Nước ngoài**  
> Phiên bản: 2.0 | Nền tảng: ASP.NET Core 8 + Blazor Server + MudBlazor  
> Ngày cập nhật: 17/04/2026

---

## 1. Tổng quan Kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                    MÁY CHỦ (Server)                        │
│                                                             │
│  ┌──────────────┐    ┌─────────────────────────────────┐   │
│  │  SQL Server   │◄──│  IRM v2.0 (Windows Service)     │   │
│  │  2014/2019    │    │  Port: 5050                     │   │
│  │              │    │  Self-contained (.NET 8)         │   │
│  │  Database:   │    │  Auto-start, Auto-recovery      │   │
│  │ ReportManager│    └──────────┬──────────────────────┘   │
│  │    DB        │               │ http://0.0.0.0:5050      │
│  └──────────────┘               │                          │
│                    Firewall ────┤── Port 5050 OPEN         │
└────────────────────────────────┼───────────────────────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              │                  │                  │
       ┌──────┴──────┐   ┌──────┴──────┐   ┌──────┴──────┐
       │  Client 1   │   │  Client 2   │   │  Client 3   │
       │  Browser    │   │  Browser    │   │  Browser    │
       │ Chrome/Edge │   │ Chrome/Edge │   │ Chrome/Edge │
       └─────────────┘   └─────────────┘   └─────────────┘
```

**Mô hình:** 1 Server + 3 Clients qua mạng LAN

---

## 2. Yêu cầu Hệ thống

### Máy chủ (Server)
| Thành phần | Yêu cầu |
|-----------|---------|
| HĐH | Windows Server 2012 R2+ / Windows 10+ |
| CPU | 2 cores+ |
| RAM | 4 GB+ |
| Ổ cứng | 10 GB trống |
| SQL Server | 2014 trở lên (đã có sẵn) |
| .NET | Không cần (app self-contained) |
| Mạng | IP tĩnh trên LAN |

### Máy client
| Thành phần | Yêu cầu |
|-----------|---------|
| Trình duyệt | Chrome 90+ / Edge 90+ / Firefox 90+ |
| Mạng | Cùng LAN với server |

---

## 3. Quy trình Triển khai

### Tổng quan 6 bước

```
 ① Backup DB cũ  →  ② Đóng gói USB  →  ③ Copy .bak vào USB
       ↓                                        ↓
 (Máy cũ)          (Máy dev)              (Bỏ vào deploy\data\)
                                                 ↓
 ⑥ Truy cập     ←  ⑤ Cắm USB,        ←  ④ Mang USB sang
    từ Client       chạy install.ps1       máy chủ mới
```

---

### Bước 1: Backup Database trên máy cũ

**Cách 1 — Dùng script (khuyến nghị):**
```powershell
# Trên máy chủ CŨ, mở PowerShell (Admin)
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Nếu dùng SQL Server Authentication
.\backup-old-server.ps1 -SqlInstance ".\BIRDIEPO" -SqlUser "sa" -SqlPassword "123456"

# Nếu dùng Windows Authentication  
.\backup-old-server.ps1 -SqlInstance ".\SQLEXPRESS"

# Hoặc với instance mặc định
.\backup-old-server.ps1 -SqlInstance "."
```

**Cách 2 — Dùng SSMS (SQL Server Management Studio):**
1. Mở SSMS → kết nối SQL Server
2. Click phải database `ReportManagerDB` → **Tasks** → **Back Up...**
3. Backup type: **Full**
4. Destination: chọn đường dẫn → **OK**
5. Copy file `.bak` ra USB

### Bước 2: Đóng gói trên máy Dev

```powershell
cd d:\ATTT\immigration-reportmanager-master
.\deploy\build-package.ps1

# Hoặc tạo luôn file ZIP
.\deploy\build-package.ps1 -Zip
```

Output tại: `deploy-package\`

### Bước 3: Chuẩn bị USB

Copy file `.bak` vào đúng thư mục:

```
USB:\
└── deploy\
    ├── install.ps1              ← File cài đặt chính
    ├── uninstall.ps1            ← Gỡ bỏ
    ├── backup-old-server.ps1    ← Backup (đã dùng ở bước 1)
    ├── DOC-TRUOC.txt            ← Hướng dẫn nhanh
    ├── data\
    │   └── ReportManagerDB_20260417.bak   ← ĐẶT FILE .BAK VÀO ĐÂY
    ├── sql\
    │   ├── 01-add-new-tables.sql
    │   └── 02-add-indexes.sql
    └── app\
        ├── IRM.exe
        ├── appsettings.json
        ├── wwwroot\
        └── *.dll
```

### Bước 4–5: Cài đặt trên máy chủ mới

```powershell
# Cắm USB, mở PowerShell với quyền Administrator
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
cd E:\deploy           # hoặc đường dẫn USB

# Cài đặt 1 lần — tất cả tự động
.\install.ps1
```

**Script tự động thực hiện:**
1. ✅ Tạo thư mục `C:\IRM`
2. ✅ Copy ứng dụng
3. ✅ Restore database từ file `.bak`
4. ✅ Chạy SQL migration (tạo bảng mới)
5. ✅ Cấu hình `appsettings.json`
6. ✅ Tạo Windows Service (auto-start)
7. ✅ Mở Firewall port 5050
8. ✅ Khởi động ứng dụng

**Tùy chỉnh:**
```powershell
# Đổi port
.\install.ps1 -Port 8080

# Chỉ định SQL instance
.\install.ps1 -SqlInstance ".\SQLEXPRESS"
.\install.ps1 -SqlInstance "SERVERNAME\INSTANCENAME"

# Bỏ qua restore (nếu DB đã có sẵn trên server)
.\install.ps1 -SkipRestore
```

### Bước 6: Truy cập từ Client

| Vị trí | URL |
|--------|-----|
| Trên máy chủ | `http://localhost:5050` |
| Client 1 | `http://<IP-máy-chủ>:5050` |
| Client 2 | `http://<IP-máy-chủ>:5050` |
| Client 3 | `http://<IP-máy-chủ>:5050` |

> **Ví dụ:** Nếu IP máy chủ là `192.168.1.100`, client mở trình duyệt → `http://192.168.1.100:5050`

---

## 4. Quản trị Vận hành

### Lệnh quản lý

```powershell
# Kiểm tra trạng thái
powershell C:\IRM\status.ps1

# Backup database thủ công
powershell C:\IRM\backup-db.ps1

# Dừng / Khởi động / Restart
sc.exe stop IRM
sc.exe start IRM
Restart-Service IRM

# Xem log
Get-Content C:\IRM\logs\*.log -Tail 50
```

### Backup tự động
- **Lịch:** Hàng ngày lúc 02:00
- **Vị trí:** `C:\IRM\backups\`
- **Giữ lại:** 30 ngày gần nhất
- **Task Name:** `IRM-DailyBackup`

### Cấu hình
File `C:\IRM\appsettings.json`:
```json
{
  "Urls": "http://0.0.0.0:5050",
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ReportManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **Lưu ý:** Sau khi sửa `appsettings.json`, chạy `Restart-Service IRM`

---

## 5. Gỡ bỏ

```powershell
# Chạy script gỡ bỏ
.\uninstall.ps1

# Hoặc thủ công
sc.exe stop IRM
sc.exe delete IRM
netsh advfirewall firewall delete rule name="IRM Web Application"
Unregister-ScheduledTask -TaskName "IRM-DailyBackup" -Confirm:$false
```

> **Lưu ý:** Script gỡ bỏ KHÔNG xóa database. Database `ReportManagerDB` vẫn an toàn trên SQL Server.

---

## 6. Khắc phục sự cố

| Vấn đề | Giải pháp |
|--------|----------|
| Không mở được trang web | Kiểm tra service: `sc.exe query IRM` |
| Client không truy cập được | Kiểm tra firewall: `netsh advfirewall firewall show rule name="IRM Web Application"` |
| Lỗi kết nối database | Kiểm tra connection string trong `appsettings.json` |
| Service crash liên tục | Xem log: `C:\IRM\logs\` |
| Restore .bak thất bại | Dùng SSMS restore thủ công, rồi chạy `.\install.ps1 -SkipRestore` |

---

## 7. Cấu trúc Database

### Bảng cũ (giữ nguyên từ hệ thống WPF)
| Bảng | Mô tả |
|------|-------|
| `Accounts` | Tài khoản người dùng |
| `Companies` | Công ty sử dụng LĐ nước ngoài |
| `Employees` | Lao động nước ngoài |
| `Fields` | Lĩnh vực hoạt động |
| `Careers` | Ngành nghề |
| `CareerGroups` | Nhóm ngành nghề |
| `Nationality` | Quốc tịch |
| `Investment` | Vốn đầu tư |
| `PhoneNumbers` | SĐT liên hệ |
| `Emails` | Email liên hệ |
| `Districts` | Quận/Huyện |
| `Wards` | Phường/Xã |
| `Attach` | File đính kèm |

### Bảng mới (thêm bởi IRM v2.0)
| Bảng | Mô tả |
|------|-------|
| `AuditLogs` | Nhật ký hoạt động hệ thống |
| `ImportHistories` | Lịch sử import Excel |
