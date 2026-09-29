# Windows offline vendor media

Đặt hai tệp Microsoft chính thức sau trong thư mục này trước khi chạy `deploy/windows/build-windows-installer.ps1`:

| Tệp | Nguồn | SHA-256 |
|---|---|---|
| `SQLEXPR_x64_ENU.exe` | `https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SQLEXPR_x64_ENU.exe` | `2E61C8BBDE6021F9026C54AD9DB4BBB1227E68761D4C00A6A50A2C70FE7AFE05` |
| `SQLServer2022-KB5104824-x64.exe` | `https://download.microsoft.com/download/a89001cb-9c99-48d3-9f14-ded054b35fe4/SQLServer2022-KB5104824-x64.exe` | `675E3CEDD6C7A3D0FFBE713E46E54A86A92AF7B96CA4A6BEB987AE99C79B96F5` |

Builder kiểm tra cả Authenticode signer Microsoft và SHA-256 trước khi tạo artifact. Các binary `.exe` không được commit vào Git.
