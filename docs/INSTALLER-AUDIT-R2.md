# Kiểm tra gói bàn giao mới nhất và lỗi quét Windows

Ngày rà soát: 02/11/2026. Chọn theo LastWriteTime trong các thư mục build/bàn giao hiện có, theo yêu cầu người dùng; chưa có hash hoặc log từ máy khách để xác nhận file khách thực nhận.

## Gói mới nhất tìm thấy

- Gói: `build-output/file cài bài giao.rar`, sửa cuối 29/09/2026 lúc 22:27:45.
- Bên trong: `v1.0.2-UNSIGNED-TEST-r2/IRM-v1.0.2-windows-x64-offline-setup-R2-UNSIGNED-TEST.exe`, sửa cuối 29/09/2026 lúc 22:18:00.
- EXE: 789424062 byte; SHA-256 `3045BC13AFE295ABA8D159D8FFDC2699AA0BA4AD652DDE544147DE463BC0E029`.
- Hash EXE trong RAR trùng file R2 bên ngoài và tệp `.sha256`; đọc archive không lỗi. Các script install/UI lấy trực tiếp từ EXE khớp checksum nội bộ. Không thấy bằng chứng hỏng file ở bản local.
- EXE chưa ký số, đúng với tên UNSIGNED-TEST. Thông tin này độc lập với kết quả dòng kiểm tra Windows bên trong giao diện.
- Thư mục BAN-GIAO-IRM-v1.0.2_2026-09-28 còn checksum của bản cũ (`59C4...`), không phải hash R2. Hiện thư mục offline đó chỉ còn hướng dẫn, không có EXE. RAR R2 là ứng viên mới nhất tìm thấy; không dùng manifest bàn giao cũ để đối chiếu R2.
- R2 có từ 29/09, chưa chứa phần tách bốn project thực hiện ngày 02/11. Không thay payload ứng dụng của R2 trong lần kiểm tra này.

## Điều đã xác nhận trong EXE R2

`installer-ui.ps1` nhúng trong EXE, dòng 107–108, gộp `Is64BitOperatingSystem` và ngưỡng build Windows vào một dòng tên “Hệ điều hành Windows 64-bit”. Bởi vậy Windows x64 build thấp vẫn bị báo lỗi dưới nhãn 64-bit. `install.ps1` nhúng cũng dùng `Environment.OSVersion.Version.Build` để chặn phiên bản.

Điều kiện hiện có là Windows máy trạm build từ 19044 hoặc Windows Server build từ 17763. Ví dụ Windows 10 x64 build 19043 sẽ bị chặn dù kiến trúc đúng. Máy kiểm tra hiện tại Windows 11 x64 build 26200 nên qua điều kiện này.

Ngoài ra `Environment.OSVersion` trên .NET Framework có thể chịu ảnh hưởng của chế độ tương thích/manifest. Microsoft nêu rõ giới hạn này: https://learn.microsoft.com/en-us/dotnet/api/system.environment.osversion?view=netframework-4.8.1 . Đây là rủi ro xác định được trong mã, chưa phải bằng chứng máy khách đang gặp chế độ tương thích. Thử đọc trên máy hiện tại không tái hiện sai lệch: .NET và CIM cùng báo 26200.

Chưa có phiên bản/build hoặc báo cáo quét máy khách, nên chưa kết luận nguyên nhân cụ thể là Windows quá cũ, API báo phiên bản tương thích hay một mục kiểm tra khác không đạt.

## Sửa trong nguồn và kiểm chứng

- Một helper dùng chung cho UI và worker: tách kiến trúc OS, kiến trúc PowerShell và phiên bản Windows; lấy build từ CIM, không dựa vào số phiên bản .NET để cho phép/chặn.
- Ban đầu giữ ngưỡng R2 để sửa nhận diện; sau yêu cầu rà soát mốc tối thiểu, source được chỉnh thành Windows 10 1607 / Server 2016 x64 (build 14393), đồng thời kiểm tra .NET Framework 4.7.2 riêng. Điều kiện cũ trong EXE R2 không tự thay đổi.
- UI tự chuyển từ PowerShell x86 sang native trước khi đọc registry; worker dùng đúng đường dẫn System32/Sysnative và báo riêng lỗi tiến trình x86 nếu được gọi trực tiếp.
- Helper và công cụ chẩn đoán được đưa vào stage, checksum và thư mục tools khi dựng/cài bộ cài mới.
- Bộ test chỉ đọc thông tin: Windows cũ x64, Windows 11, Server 2016/2019, domain controller, CIM thiếu/sai build, .NET báo 9200 trong khi CIM báo 19045, x86 OS, x86 process trên x64 OS, và khởi chạy PowerShell native thực tế.
- Chạy thành công trên Windows PowerShell x64 và x86; tất cả script thay đổi qua parser. CI bổ sung cả hai host.

File RAR/EXE R2 gốc được giữ nguyên. Ngày 03/11/2026 đã dựng R3 UNSIGNED-TEST bằng cách giải nén payload R2, đối chiếu checksum rồi chỉ thay script kiểm tra/cài đặt và tài liệu/manifest; không publish lại ứng dụng từ source đang refactor. R3 dùng để kiểm thử nội bộ, chưa phải bản phát hành ký số.

## Thông tin tối thiểu cần từ khách

Lấy `Caption`, `OSArchitecture`, `Version`, `BuildNumber`, `ProductType` bằng lệnh CIM trong hướng dẫn cài đặt, hoặc báo cáo `C:\ProgramData\IRM\logs\Bao-cao-moi-truong-*.html`. Không cần mật khẩu, database hay dữ liệu nghiệp vụ.

## Bổ sung: mốc kỹ thuật thấp nhất

Đã đối chiếu yêu cầu SQL Server 2022 và .NET 8: source mới cho phép Windows 10 1607 x64 hoặc Server 2016 x64 (build 14393) trở lên, với .NET Framework 4.7.2+ đã có cho SQL Setup. Inno MinVersion, kiểm tra UI/worker và release-manifest thống nhất mốc này. Không đồng nhất ngưỡng kỹ thuật với vòng đời hỗ trợ từng edition Windows. Bản R2 gốc vẫn giữ 19044/17763.


## R3 kiểm thử được dựng ngày 03/11/2026

- Artifact: `build-output\v1.0.2-ENV-R3-UNSIGNED-TEST\IRM-v1.0.2-windows-x64-offline-setup-R3-ENV-UNSIGNED-TEST.exe`.
- Kích thước: 789417762 byte; SHA-256 `99333703656CADC5917BB93254744BB56BA91E20CD36C6810A4174333F749351`. Tệp `.exe.sha256`, `HUONG-DAN-TEST.txt` và `VERIFICATION.json` nằm cùng thư mục.
- Đã kiểm tra checksum 390 tệp gốc R2 trước khi đóng gói; giải nén EXE R3 vừa biên dịch và kiểm tra đủ 393 checksum payload. Toàn bộ 378 tệp `app/`, hai media SQL, schema, SBOM và script backup/restore/repair/uninstall giữ nguyên từng byte so với R2.
- Chỉ thay các tệp có sẵn `install.ps1`, `installer-ui.ps1`, hướng dẫn và manifest; bổ sung helper môi trường, công cụ chẩn đoán và thông tin revision. Tạo lại checksum; Inno entry gate dùng `MinVersion=10.0.14393`.
- Test logic môi trường đạt 45 assertion trên Windows PowerShell x64 và 46 trên x86; Inno biên dịch thành công. Không chạy bộ cài trên máy đang có dữ liệu/dịch vụ.
- Chưa cài thử trên Windows 10 1607 / Server 2016 sạch hoặc máy khách gặp lỗi. R3 chưa ký số, không thay thế bản phát hành chính thức cần ký số và nghiệm thu môi trường thực tế.


## Bổ sung ảnh máy khách: Windows 10 Pro build 19043

Ảnh người dùng cung cấp ngày 03/11/2026 hiển thị `Microsoft Windows 10 Pro, build 19043`, dòng gộp `Hệ điều hành Windows 64-bit` không đạt. Đây là bố cục kiểm tra cũ; chỉ từ ảnh chưa xác nhận được hash EXE hay bitness thật qua CIM.

Build 19043 nhỏ hơn ngưỡng 19044 trong script R2, nên điều kiện phiên bản chắc chắn không đạt dù giả định Windows x64. Đã tái hiện phép kiểm tra với ProductType=1, build/version=19043 và OS/process x64: R2 không đạt, helper R3 đạt (mốc 14393). Regression build 19043 có sẵn trong bộ test và chạy lại thành công 45 assertion trên PowerShell x64.

Ảnh cho thấy CPU, RAM và dung lượng ổ C đạt; SQL Server Express/CU ghi sẽ cài bổ sung, không phải lỗi SQL. Thanh tiến trình đang ở bước checksum; một ảnh tĩnh chưa đủ chứng minh bị treo. Nếu checksum không kết thúc cần lấy báo cáo/log hoặc thời gian chờ thực tế để kiểm tra riêng.

R3 đã dựng xử lý đúng trường hợp build 19043; cần dùng đúng EXE R3 để kiểm thử, không sửa hoặc bỏ qua checksum của R2. Chưa xác minh .NET Framework trên máy khách và chưa cài thử trực tiếp; các điều kiện khác vẫn phải qua bước quét.
