# Hướng dẫn cài đặt IRM v1.0.2

## Hai hình thức bàn giao

1. **Online/VPS:** IRM chạy bằng Docker Compose phía sau HTTPS. Quy trình triển khai nằm tại `deploy/deploy-vps.ps1`.
2. **Offline/Windows R6:** tên artifact là `IRM-v1.0.2-windows-x64-offline-setup-R6.exe`. Gói chứa ứng dụng .NET self-contained, SQL Server 2022 Express Core và CU27; máy cài không cần Internet nếu đã có .NET Framework 4.7.2 trở lên cho SQL Server. Kiểm tra biên bản, trạng thái ký số và checksum đi kèm để phân biệt candidate thử với bản được phép bàn giao chính thức.

## Cài offline trên Windows

Bộ cài R6 dùng giao diện đồng bộ màn hình đăng nhập (Landing Page) của ứng dụng: bố cục 2 cột với nền xanh olive vát khối đa giác (`#C2D67C → #9AB55C → #6D8446`), Công an hiệu ở phần đầu, nhân vật 3D Công an Nhân dân đứng trên bệ ở cột phải, khung bảng kiểm tra/biểu mẫu sáng rõ ở cột trái và nút chính màu cam (`#EA580C`). Bảng quét, hộp thông báo và báo cáo môi trường HTML dùng chung bảng màu olive sáng; bản in dùng nền trắng. Theme chỉ áp dụng cho bộ cài; không đổi theme Windows. Hộp thoại bảo mật của Windows do hệ điều hành hiển thị. Điều kiện Windows, RAM, SQL và hành vi nâng cấp giữ nguyên. Gói R4/R5 cũ vẫn giữ nội dung và checksum cũ.

> Bộ cài bàn giao chính thức phải có chữ ký Authenticode từ chứng thư Code Signing tin cậy. Không bàn giao artifact được tạo với `-AllowUnsigned`, vì Windows SmartScreen hoặc chính sách doanh nghiệp có thể chặn hoàn toàn.

Yêu cầu tối thiểu: Windows 10 phiên bản 1607 x64 trở lên (build tối thiểu 14393, bao gồm Windows 11) hoặc Windows Server 2016 x64 trở lên (build tối thiểu 14393; bộ cài giao diện cần Desktop Experience), đã có .NET Framework 4.7.2 trở lên, 2 CPU logic từ 2 GHz, 2 GB RAM khả dụng, 10 GB ổ đĩa trống và Windows Defender đang hoạt động để quét tệp tải lên.

1. Sao chép file `IRM-v1.0.2-windows-x64-offline-setup-R6.exe` và checksum đi kèm vào máy cần cài; xác minh SHA-256 trước khi chạy.
2. Nhấp đúp file và chọn **Yes** khi Windows hỏi quyền Administrator.
3. Ở **Bước 1**, nhấn **Quét môi trường**. Trình cài đặt hiển thị báo cáo tickbox và lưu bản HTML tại `C:\ProgramData\IRM\logs`.
4. Nếu thiếu SQL Server/CU27, báo cáo ghi **Sẽ cài bổ sung**. Các thành phần này được lấy ngay trong bộ cài, không tải Internet.
5. Ở **Bước 2**, đặt tên tài khoản và mật khẩu quản trị IRM, chọn quyền truy cập LAN nếu cần, chấp nhận điều khoản SQL Server rồi nhấn **Cài đặt IRM**.
6. Chỉ khi database, đăng nhập, upload/quét file, xuất báo cáo và health check đều đạt, màn hình mới chuyển sang **Bước 3 — Hoàn tất**.

Database mới được tạo sẵn một công ty tên **CÔNG TY MẪU - HÃY ĐỔI TÊN** để toàn bộ chức năng có khóa ngoại công ty sử dụng được ngay. Sau lần đăng nhập đầu tiên, mở **Công ty**, sửa bản ghi này thành thông tin đơn vị thực tế. Khi hệ thống chỉ có một công ty, màn hình Import tự chọn công ty đó.

Khi chạy lại bộ cài để nâng cấp, database nghiệp vụ được giữ nguyên. Username và mật khẩu quản trị nhập trong trình cài đặt sẽ được tạo hoặc đặt lại với quyền Admin trước khi chạy self-test.

Sau khi cài:

- Trên máy chủ: `http://localhost:5050`
- Trong mạng LAN khi đã bật tùy chọn LAN: `https://TEN-MAY:5443`
- Dữ liệu và cấu hình: `C:\ProgramData\IRM`
- Công cụ backup/restore/repair/uninstall: `C:\ProgramData\IRM\tools`
- Mã khôi phục database: `C:\ProgramData\IRM\MA-KHOI-PHUC.txt` (chỉ Administrators và SYSTEM đọc được)

## Kiểm tra sau cài

- Mở shortcut **IRM** trên Desktop.
- Đăng nhập bằng tài khoản đã tạo trong bộ cài.
- Kiểm tra `http://localhost:5050/health/ready` trả về trạng thái `ready`.
- Chạy `C:\ProgramData\IRM\tools\backup.ps1`, sau đó xác nhận có tệp backup mới.
- **Kiểm thử nhanh Import Excel mẫu R6 (`IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`):** Vào mục **Import Excel**, ở Bước 1 chọn Công ty **CÔNG TY MẪU - HÃY ĐỔI TÊN** và Nhóm nghề **Kỹ thuật**, tải lên tệp `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` đi kèm trong thư mục bàn giao — hệ thống tự động ghép đủ 19 cột và nạp thành công 10/10 hồ sơ mẫu (gồm cả mã quốc tịch 3 chữ cái, số định danh có số `0` đầu và hồ sơ diện `Thân nhân`).
- **Gửi phản hồi nghiệm thu (`Khảo sát & Góp ý`):** Ngay lần đầu mở ứng dụng Web, hiệu ứng spotlight trên thanh tiêu đề sẽ chỉ dẫn nút **Khảo sát & Góp ý (R6)**. Khách hàng có thể bấm nút này để mở trực tiếp phiếu khảo sát HTML hoặc tải phiếu Excel (`PHIEU-KHAO-SAT-GOP-Y.html` / `PHIEU-KHAO-SAT-GOP-Y.xlsx`, cũng có sẵn ngay trong thư mục bàn giao).

## Gỡ cài đặt

Chạy PowerShell với quyền Administrator:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\ProgramData\IRM\tools\uninstall.ps1"
```

Lệnh gỡ dịch vụ nhưng giữ lại database và dữ liệu để tránh mất dữ liệu ngoài ý muốn.

## Triển khai VPS

Quy trình VPS tạo backup trước khi thay đổi, tạo thư mục đích nếu chưa có, nạp image đã khóa version, kiểm tra health và tự rollback container khi health check thất bại. Không ghi mật khẩu mặc định trong source; truyền mật khẩu bằng biến môi trường `IRM_SQL_SA_PASSWORD` hoặc tham số SecureString.

## Chẩn đoán mục Windows khi quét môi trường

Windows 64-bit và phiên bản Windows là hai yêu cầu độc lập. Máy trạm phải có Windows 10 phiên bản 1607 x64, build 14393 trở lên hoặc Windows 11; máy chủ phải từ Windows Server 2016 x64, build 14393. SQL Server 2022 cần thêm .NET Framework 4.7.2 trở lên (Release registry tối thiểu 461808). Không bỏ qua yêu cầu phiên bản chỉ vì máy có CPU/Windows 64-bit.

Mã nguồn sau bản R2 tách hai dòng kết quả và dùng `Win32_OperatingSystem.BuildNumber`/`Version` qua CIM, không dùng `Environment.OSVersion` để quyết định đủ điều kiện. Nếu tiến trình PowerShell là 32-bit trên Windows 64-bit, giao diện tự mở lại bằng PowerShell native trước khi quét registry/khởi chạy worker. Các sửa đổi này chỉ có trong bộ cài được dựng lại; EXE R2 đã phát hành không tự cập nhật.

Để lấy thông tin máy khách mà không cài hoặc thay đổi cấu hình:

```powershell
Get-CimInstance Win32_OperatingSystem | Select-Object Caption,OSArchitecture,Version,BuildNumber,ProductType
```

Tại máy có source hoặc bộ công cụ mới, chạy `diagnose-environment.ps1 -OutputPath IRM-environment.json` để ghi báo cáo JSON. Báo cáo quét của bộ cài nằm tại `C:\ProgramData\IRM\logs\Bao-cao-moi-truong-*.html`. Không sửa script trong bộ cài R2 rồi bỏ qua checksum.

Kiểm thử hồi quy (chỉ đọc thông tin, không chạy cài đặt):

```powershell
./deploy/windows/tests/environment.Tests.ps1
& "$env:SystemRoot\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -File deploy/windows/tests/environment.Tests.ps1
```

## Căn cứ mốc Windows tối thiểu

| Thành phần | Yêu cầu liên quan |
|---|---|
| SQL Server 2022 Express | Windows 10 1607 / Windows Server 2016 trở lên, CPU x64; .NET Framework 4.7.2 trở lên |
| IRM .NET 8 self-contained | Runtime đi kèm ứng dụng; nền tảng .NET 8 có Windows 10 1607 và Windows Server 2016 trong ma trận tương thích theo edition/vòng đời |
| Bộ cài IRM | `MinVersion=10.0.14393`; UI và worker cùng dùng build thật từ CIM; kiểm tra .NET Framework riêng |

Đây là mốc kỹ thuật để xét điều kiện cài, không phải cam kết mọi edition Windows cũ còn trong vòng đời hỗ trợ của Microsoft. Phiên bản Windows/edition còn được Microsoft hỗ trợ được khuyến nghị cho triển khai chính thức.

Media hiện có không kèm bộ cài .NET Framework. Nếu Windows cũ chưa có 4.7.2 hoặc mới hơn, trình quét phải báo đúng thành phần này đang thiếu và chặn trước khi chạy SQL Setup; không báo nhầm lỗi Windows 64-bit. Cần chuẩn bị/cài .NET Framework offline trước, không tải ngầm khi máy khách đang offline.

Nguồn chính thức:

- [Yêu cầu SQL Server 2022](https://learn.microsoft.com/en-us/sql/sql-server/install/hardware-and-software-requirements-for-installing-sql-server-2022?view=sql-server-ver16)
- [.NET 8: hệ điều hành](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md)

Mốc 19044/Windows Server 2019 của R2 là điều kiện cũ, cao hơn mốc kỹ thuật thành phần yêu cầu. Chỉ EXE dựng lại từ định nghĩa mới mới áp dụng ngưỡng 14393; EXE R2 đã bàn giao vẫn giữ điều kiện cũ.
