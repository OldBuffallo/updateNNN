# Biên bản sẵn sàng phát hành IRM v1.0.1

Ngày rà soát: 21/09/2026.

## Kết luận hiện tại

Mã nguồn và quy trình đóng gói đã sẵn sàng để tạo release candidate, nhưng **chưa được phép gửi khách hàng** cho tới khi tạo đủ hai artifact chính thức và hoàn tất nghiệm thu ngoại tuyến trên hai máy sạch.

## Bằng chứng đã đạt

- Build Release: 0 lỗi, 0 cảnh báo.
- Unit/integration tests: 84/84 đạt.
- Publish self-contained thành công cho `win-x64` và `linux-x64`.
- Kích thước ứng dụng publish: Windows khoảng 131 MiB; Linux khoảng 127 MiB.
- Không có PDB, source C# hoặc SQLite trong thư mục publish.
- `dotnet list package --vulnerable --include-transitive`: không phát hiện package có lỗ hổng theo nguồn NuGet tại thời điểm kiểm tra.
- PowerShell scripts và Bash scripts đều qua kiểm tra cú pháp.
- Tài liệu Word đã render và kiểm tra trực quan 15 trang.
- Migration SQL SHA-256: `B737B48CE90D924B78303E3E22C9B5D465E880E37B0F6C93A7FA15809CA46CA2`.
- SQL Server CU27 Windows SHA-256 bắt buộc: `675E3CEDD6C7A3D0FFBE713E46E54A86A92AF7B96CA4A6BEB987AE99C79B96F5`.

## Thành phần chặn phát hành

1. Máy build hiện tại chưa có Inno Setup 6 và chưa có hai media Microsoft trong `deploy/vendor/windows/`:
   - `SQLEXPR_x64_ENU.exe`
   - `SQLServer2022-KB5104824-x64.exe`
2. ISO Ubuntu phải được dựng trên Ubuntu 22.04 amd64 đã cấu hình repository Microsoft; máy hiện tại không có môi trường Ubuntu build hoạt động.
3. Chưa thực hiện bài test cài đặt ngoại tuyến trên Windows sạch và Ubuntu sạch với đúng 2 GB RAM khả dụng/10 GB trống.
4. Chưa có số đo thực tế RAM, dung lượng sau cài, reboot, backup/restore và cài đặt bị gián đoạn trên hai máy nghiệm thu.

## Lệnh tạo artifact

Windows, sau khi đặt đúng media và cài Inno Setup 6:

```powershell
./deploy/windows/build-windows-installer.ps1
```

Ubuntu 22.04 amd64, sau khi cấu hình repository chính thức của Microsoft:

```bash
sudo ./deploy/linux/build-ubuntu-offline.sh
```

Đầu ra bắt buộc:

- `build-output/v1.0.1/IRM-v1.0.1-windows-x64-setup.exe`
- `build-output/v1.0.1/IRM-v1.0.1-windows-x64-setup.exe.sha256`
- `build-output/v1.0.1/IRM-v1.0.1-ubuntu22.04-amd64-offline.iso`
- `build-output/v1.0.1/IRM-v1.0.1-ubuntu22.04-amd64-offline.iso.sha256`

Chỉ chuyển trạng thái sang **ĐƯỢC PHÉP BÀN GIAO** khi checklist trong `INSTALLATION-v1.0.1.md` đạt đầy đủ trên cả hai máy sạch và checksum của artifact bàn giao trùng khớp.
