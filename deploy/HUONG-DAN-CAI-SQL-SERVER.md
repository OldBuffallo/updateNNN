# Hướng dẫn cài SQL Server 2014 Express + Tạo Database Test

## Cách 1: Cài tự động bằng Script (Khuyến nghị)

```powershell
# Mở PowerShell với quyền Administrator
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

cd "d:\Dự án outsource\NNN\immigration-reportmanager-master\deploy"
.\setup-sqlserver-localdb.ps1
```

Script sẽ tự động:
1. Kiểm tra SQL Server trên máy
2. Tải và cài SQL Server 2014 Express (nếu chưa có)
3. Tạo database `ReportManagerDB` với đầy đủ bảng + dữ liệu mẫu
4. Cập nhật connection string cho ứng dụng

---

## Cách 2: Cài thủ công

### Bước 1: Tải SQL Server 2014 Express

Tải từ Microsoft:
- **SQL Server 2014 Express with Tools** (có SSMS):
  https://www.microsoft.com/en-us/download/details.aspx?id=42299
  → Chọn file: `SQLEXPRWT_x64_ENU.exe` (~800MB)

- **Hoặc SQL Server 2019 Express** (mới hơn, vẫn tương thích):
  https://www.microsoft.com/en-us/sql-server/sql-server-downloads
  → Chọn Express → Download → Basic

### Bước 2: Cài đặt

1. Chạy file installer
2. Chọn **New SQL Server stand-alone installation**
3. Instance name: `SQLEXPRESS`
4. Authentication: **Mixed Mode**
5. Sa password: đặt password (vd: `IRM@2026!Dev`)
6. Hoàn tất cài đặt

### Bước 3: Tạo Database

**Cách A — Dùng SSMS:**
1. Mở SQL Server Management Studio
2. Kết nối: `.\SQLEXPRESS` (Windows Authentication)
3. File → Open → chạy lần lượt:
   - `deploy/sql/00-full-setup.sql`
   - `deploy/sql/04-family-visit.sql`

**Cách B — Dùng sqlcmd:**
```powershell
sqlcmd -S .\SQLEXPRESS -E -i "deploy\sql\00-full-setup.sql"
sqlcmd -S .\SQLEXPRESS -E -i "deploy\sql\04-family-visit.sql"
```

### Bước 4: Chạy ứng dụng

```powershell
cd IRM
dotnet run
# Mở http://localhost:5050
```

### Tài khoản test
| Username | Password | Quyền |
|----------|----------|-------|
| admin | 123456 | Quản trị viên |
| nguyenvana | 123456 | User |
| tranthib | 123456 | User |

---

## Connection String

Trong `IRM/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ReportManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Nếu dùng SQL Authentication:
```
Server=.\SQLEXPRESS;Database=ReportManagerDB;User Id=sa;Password=IRM@2026!Dev;TrustServerCertificate=True;
```
