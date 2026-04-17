# 📦 Hướng dẫn Deploy — IRM v2.0 (Máy chủ không có Internet)

## Tổng quan

Ứng dụng IRM v2.0 được build dạng **self-contained** — gộp toàn bộ .NET runtime vào cùng thư mục.  
**Máy chủ KHÔNG CẦN cài .NET SDK hoặc .NET Runtime.**

---

## Bước 1: Build trên máy dev (có internet)

Mở terminal tại thư mục `IRM/`, chạy:

```powershell
dotnet publish -c Release --self-contained -r win-x64 -o ./publish
```

> Kết quả tạo thư mục `publish/` khoảng **80–100MB**, chứa:
> - `IRM.exe` — File chạy chính
> - `appsettings.json` — Cấu hình
> - `wwwroot/` — Tài nguyên web (CSS, JS)
> - Các file .dll của .NET Runtime

### Nếu máy chủ chạy Windows 32-bit:
```powershell
dotnet publish -c Release --self-contained -r win-x86 -o ./publish
```

### Nếu máy chủ chạy Linux:
```bash
dotnet publish -c Release --self-contained -r linux-x64 -o ./publish
```

---

## Bước 2: Copy ra USB

1. Copy **toàn bộ** thư mục `publish/` vào USB
2. Cắm USB vào máy chủ
3. Copy thư mục `publish/` vào vị trí mong muốn, ví dụ: `C:\IRM\`

---

## Bước 3: Chạy trên máy chủ

### Cách 1: Chạy trực tiếp (đơn giản nhất)

```powershell
cd C:\IRM
.\IRM.exe --urls "http://0.0.0.0:5050"
```

> Truy cập từ máy khác trong mạng LAN: `http://<IP-máy-chủ>:5050`

### Cách 2: Cài đặt làm Windows Service (tự khởi động)

```powershell
# Chạy CMD với quyền Administrator
sc create IRM binPath="C:\IRM\IRM.exe --urls http://0.0.0.0:5050" start=auto
sc description IRM "Immigration Report Manager v2.0"
sc start IRM
```

Để gỡ service:
```powershell
sc stop IRM
sc delete IRM
```

---

## Bước 4: Mở Firewall (nếu cần truy cập từ máy khác)

```powershell
# Chạy CMD với quyền Administrator
netsh advfirewall firewall add rule name="IRM Web" dir=in action=allow protocol=tcp localport=5050
```

---

## Cấu hình

### Đổi port
Sửa file `appsettings.json`:
```json
{
  "Urls": "http://0.0.0.0:8080"
}
```

### Kết nối Database
Sửa file `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=IRM;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Cập nhật phiên bản mới

1. Build lại trên máy dev: `dotnet publish -c Release --self-contained -r win-x64 -o ./publish`
2. Stop service: `sc stop IRM`
3. Copy đè thư mục `publish/` lên máy chủ
4. Start service: `sc start IRM`

---

## Kiểm tra

| Kiểm tra | Lệnh |
|----------|-------|
| App đang chạy? | Mở browser: `http://localhost:5050` |
| Service status | `sc query IRM` |
| Xem log | `Get-Content C:\IRM\logs\*.log -Tail 50` |
| Port đang mở? | `netstat -an | findstr 5050` |

---

## Yêu cầu máy chủ tối thiểu

- **OS**: Windows Server 2016+ hoặc Windows 10+
- **RAM**: 2GB (khuyến nghị 4GB)
- **Disk**: 200MB trống
- **CPU**: 2 cores
- **Không cần**: .NET SDK, IIS, Node.js, hoặc bất kỳ phần mềm nào khác
