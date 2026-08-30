# Hướng dẫn Deploy IRM lên Production

## Yêu cầu hệ thống

| Thành phần | Yêu cầu |
|---|---|
| **OS** | Windows Server 2016+ hoặc Windows 10+ |
| **SQL Server** | 2017 trở lên (Express, Standard, hoặc Enterprise) |
| **RAM** | Tối thiểu 4GB |
| **Disk** | 500MB cho app + dung lượng database |
| **.NET Runtime** | Không cần — app là self-contained |

## Các bước triển khai

### Bước 1: Giải nén package

Giải nén file `IRM-production-deploy.zip` vào thư mục cài đặt, ví dụ:
```
C:\IRM\
├── app\           ← Chứa IRM.exe và các file chương trình
├── migration.sql  ← Script tạo database
└── DEPLOY-GUIDE-PRODUCTION.md
```

### Bước 2: Tạo database trên SQL Server

**Cách 1 — Tự động (khuyên dùng):**
App sẽ tự tạo schema khi khởi động lần đầu nếu database tồn tại nhưng trống.

```sql
CREATE DATABASE IRM_Production;
```

**Cách 2 — Thủ công:**
Chạy file `migration.sql` trên SQL Server Management Studio (SSMS):
1. Mở SSMS, kết nối SQL Server
2. Tạo database mới: `IRM_Production`
3. Mở file `migration.sql`
4. Chọn database `IRM_Production`
5. Execute

### Bước 3: Cấu hình Connection String

Sửa file `app\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_SERVER;Database=IRM_Production;User Id=TEN_USER;Password=MAT_KHAU;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "DataProtection": {
    "KeysPath": "C:\\IRM\\keys"
  }
}
```

> ⚠️ **Quan trọng:** Thay `TEN_SERVER`, `TEN_USER`, `MAT_KHAU` bằng thông tin SQL Server thực tế.

### Bước 4: Chạy ứng dụng

**Chạy trực tiếp (test):**
```powershell
cd C:\IRM\app
.\IRM.exe --urls "http://0.0.0.0:5000"
```

**Đăng ký Windows Service (production):**
```powershell
sc.exe create "IRM" binpath="C:\IRM\app\IRM.exe --urls http://0.0.0.0:5000" start=auto
sc.exe start IRM
```

**Chạy qua IIS (nếu đã cài IIS):**
1. Cài ASP.NET Core Hosting Bundle (nếu chưa có)
2. Tạo Application Pool mới (No Managed Code)
3. Tạo Website trỏ vào `C:\IRM\app`
4. Cấu hình binding (port, HTTPS)

### Bước 5: Kiểm tra

1. Mở trình duyệt: `http://localhost:5000`
2. Đăng nhập: `admin` / `123456`
3. Đổi mật khẩu admin ngay sau lần đăng nhập đầu tiên

## Tài khoản mặc định

| Username | Mật khẩu | Quyền |
|---|---|---|
| `admin` | `123456` | Admin |

> ⚠️ **Bảo mật:** Đổi mật khẩu admin ngay sau khi deploy!

## Backup & Recovery

### Backup database
```sql
BACKUP DATABASE IRM_Production
TO DISK = 'C:\Backup\IRM_Production.bak'
WITH FORMAT, COMPRESSION;
```

### Restore
```sql
RESTORE DATABASE IRM_Production
FROM DISK = 'C:\Backup\IRM_Production.bak'
WITH REPLACE;
```

## Troubleshooting

| Lỗi | Giải pháp |
|---|---|
| Không kết nối SQL Server | Kiểm tra connection string, firewall, SQL Server Browser service |
| Port 5000 bị chiếm | Đổi port: `--urls http://0.0.0.0:5001` |
| Lỗi migration | Chạy `migration.sql` thủ công qua SSMS |
| App không khởi động | Kiểm tra log trong thư mục app |
