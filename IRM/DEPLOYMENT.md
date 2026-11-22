# Triển khai IRM v1.0.2

Tài liệu triển khai chính thức đã được hợp nhất tại [docs/INSTALLATION-v1.0.2.md](docs/INSTALLATION-v1.0.2.md).

Không dùng các gói ZIP, script LocalDB, SQLite hoặc SQL Server 2014 cũ để bàn giao khách hàng. Bản v1.0.2 được phát hành bằng artifact có checksum:

- `IRM-v1.0.2-windows-x64-offline-setup.exe`

Artifact phải chứa SQL Server 2022 Express CU27 build `16.0.4295.3` và vượt kiểm thử ngoại tuyến trên máy sạch. Gói Ubuntu cũ không phải artifact bàn giao vì chưa chứa đủ Docker `.deb` để cài trên máy hoàn toàn ngoại tuyến.
