# Rà soát bộ cài R3 trước bàn giao

Ngày kiểm tra: 03/11/2026. Phạm vi: EXE R3 đã dựng, payload giải nén từ EXE và điều kiện phát hành hiện có trong repository.

## Kết quả

Ứng dụng và SQL được đóng gói đầy đủ từ R2. Nhãn `UNSIGNED-TEST` biểu thị trạng thái phát hành: bộ cài chưa ký số và chưa qua kiểm thử cài đặt thực tế trọn quy trình. Chưa đủ bằng chứng để xác nhận đây là bản bàn giao chính thức đã nghiệm thu.

## Bằng chứng đã xác minh

| Nội dung | Kết quả |
|---|---|
| Artifact | `build-output/v1.0.2-ENV-R3-UNSIGNED-TEST/IRM-v1.0.2-windows-x64-offline-setup-R3-ENV-UNSIGNED-TEST.exe` |
| Kích thước | 789417762 byte |
| SHA-256 EXE, đối chiếu lại với sidecar | `99333703656CADC5917BB93254744BB56BA91E20CD36C6810A4174333F749351` — trùng |
| Checksum nội dung đã giải nén | 393/393 đạt, kiểm tra lại trong lần rà soát này |
| Ứng dụng, schema và SQL | Toàn bộ 378 tệp ứng dụng cùng schema, SBOM và hai media SQL trùng R2 từng byte |
| Bộ cài SQL base/CU | Chữ ký Microsoft hợp lệ ở cả hai tệp |
| Bộ cài IRM bên ngoài | Authenticode `NotSigned` |
| Chứng thư ký hiện có | Không có Code Signing certificate trong CurrentUser/My hoặc LocalMachine/My; biến cấu hình thumbprint/timestamp chưa đặt |
| Thư viện kiểm thử và dữ liệu đóng gói | Không thấy IRM.Tests, xUnit, testhost, PFX/P12, backup .bak, database .db hoặc log .log trong payload; đây là kiểm tra theo tên tệp, không phải kiểm toán toàn bộ bí mật |
| Cấu hình kết nối mặc định | DefaultConnection rỗng ở cả appsettings.json và appsettings.Production.json; bộ cài tạo cấu hình khi triển khai |
| Runtime ứng dụng | .NET và ASP.NET Core 8.0.27 self-contained |
| Logic kiểm tra môi trường | Lần chạy gần nhất: x64 45 assertion, x86 46 assertion; có trường hợp Windows 10 Pro build 19043 |

## Những việc còn thiếu trước khi chốt bản chính thức

1. Cài thử trên máy Windows x64 sạch hoặc máy thử được phép thay đổi: quét môi trường, cài SQL/CU, tạo tài khoản, đăng nhập, health, upload/quét Defender, báo cáo và backup/restore bằng dữ liệu thử. Kiểm tra riêng nâng cấp và giữ dữ liệu thử từ R2. Chưa thực hiện các bước này với EXE R3.
2. Kiểm tra lại trên máy khách build 19043. Nếu tiếp tục công bố Windows 10 1607/Server 2016 là mốc thấp nhất, cần xác minh cài đặt trên môi trường tương ứng; hiện mới kiểm tra logic ngưỡng và đối chiếu yêu cầu nền tảng.
3. Theo quy trình hiện tại trong `docs/INSTALLATION-v1.0.2.md` và `deploy/windows/build-windows-installer.ps1`, bản phát hành cần ký Authenticode bằng chứng thư tin cậy, có timestamp. Máy hiện tại chưa có chứng thư. Không tạo chứng thư tự ký để giả trạng thái tin cậy.
4. Sau nghiệm thu, tạo revision phát hành với tên artifact/manifest/tài liệu thống nhất; giữ nguyên payload R2 cho hotfix installer, ký EXE, xác minh chữ ký, tạo SHA-256 cuối cùng sau ký và đối chiếu lại gói bàn giao.

Hyper-V có trên máy nhưng lệnh liệt kê VM bị từ chối do quyền truy cập; chưa có môi trường máy sạch có thể dùng để kiểm tra cài đặt trong phiên này. Không chạy cài đè hoặc backup/restore lên dịch vụ/database hiện có của máy làm việc.

## Thông tin phải ghi cho khách

- Windows 10 1607 x64 / Windows Server 2016 x64 (Desktop Experience), build 14393 trở lên là mốc kỹ thuật đang cấu hình; Windows 10 Pro build 19043 vượt ngưỡng này.
- Đã có .NET Framework 4.7.2+ cho SQL Server. Gói hiện không kèm bộ cài Framework, nên khả năng cài hoàn toàn offline phụ thuộc thành phần này đã có trên máy.
- Tài khoản `admin` dùng mật khẩu người cài tự đặt, tối thiểu 12 ký tự. Khi nâng cấp, tài khoản được nhập sẽ được tạo hoặc đặt lại mật khẩu và quyền Admin.
- EXE/RAR R2 cũ và checksum của chúng không phải gói R3. Dùng đúng tên revision và checksum đi kèm khi bàn giao.

Không đổi tên hoặc thay EXE đã dựng trong lần rà soát này; báo cáo này không xác nhận nghiệm thu cài đặt thực tế.


## Cập nhật từ người bàn giao ngày 03/11/2026

Người dùng xác nhận gói đã chạy tốt trên máy của mình và yêu cầu thêm tài liệu để tự bàn giao khách hàng. Đây là xác nhận từ người dùng; chưa có thông tin chi tiết về phiên bản Windows hoặc phạm vi kiểm thử. Đã bổ sung `HUONG-DAN-CAI-DAT.txt` và `HUONG-DAN-CAI-DAT.html` ngay cạnh EXE R3, cập nhật tệp ghi chú bên ngoài. EXE và checksum được giữ nguyên; trạng thái chưa ký số và chưa xác minh trên máy sạch/minimum OS không thay đổi.
