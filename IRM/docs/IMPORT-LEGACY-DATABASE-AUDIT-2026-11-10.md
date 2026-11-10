# Đối chiếu import Excel với database cũ — 10/11/2026

## Kết luận đã xác minh

Database cũ `ReportManagerDB` hiện có trên máy phát triển sử dụng các mã `USA`, `KOR`, `JPN`, `CHN`, `AUS`, `GBR`, `IND`, `FRA`, `CAN`; Đức dùng mã `D`. Có 88 dòng Nationality, trong đó 87 dòng có Delete_flag = 0. Bộ mã có độ dài khác nhau; không có căn cứ ép về ISO hai hoặc ba ký tự.

Database mới `IRM` trên cả hai instance cục bộ đã đọc có 14 mã seed khác (`US`, `KR`, `JP`, `CN`, `AU`, `GB`, `IN`...). Vì vậy phải phân biệt database nghiệp vụ cũ với database IRM mới được khởi tạo. Kết luận trước đây về file gốc “sai mã” chỉ đúng đối với seed mặc định của IRM mới, không đúng khi dùng danh mục cũ làm chuẩn.

Source Seeder chỉ thêm bộ seed khi bảng Nationality chưa có dữ liệu; chưa có bằng chứng chương trình đã tự chuyển hay ghi đè các mã cũ. Trong phiên này chỉ thực hiện SELECT trên SQL Server, không sửa hoặc đồng bộ bất kỳ database thật nào.

## Nguồn bằng chứng

- Cấu hình source WPF xác định tên database là ReportManagerDB; server trong source là môi trường cũ khác máy hiện tại. Kiểm tra trực tiếp ở đây dùng Windows authentication tới các instance cục bộ, không dùng/in lại credentials từ source.
- Truy vấn chỉ đọc vào `.\SQLEXPRESS / ReportManagerDB`, `.\SQLEXPRESS / IRM` và `.\IRMEXPRESS / IRM`: metadata cột, danh mục quốc tịch và tổng hợp số hồ sơ theo mã; không lấy tên/hộ chiếu hoặc dữ liệu cá nhân.
- `immigration-reportmanager-master/ReportManager/Bases/MethodHandler.cs`: đọc danh mục bằng Delete_flag = 0; hồ sơ lưu/tham chiếu theo NationalityCode.
- `ViewModels/AddNationalityViewModel.cs`: kiểm tra không trống/trùng mã; sửa tên theo IDNationality, không đổi khóa mã trong lệnh cập nhật; xóa mềm.
- `UserControls/AddNationalityUC.xaml`: nhập mã viết hoa, không áp độ dài ISO cố định.
- `ViewModels/AddEmployeeViewModel.cs`: các trường bắt buộc, điều kiện GPLĐ và địa bàn của nghiệp vụ cũ.

## Chênh lệch cần giữ nguyên khi nâng cấp

| Nội dung | Thực tế đã đọc | Ý nghĩa |
|---|---|---|
| NationalityCode cũ | nvarchar(max), mã nghiệp vụ biến độ dài | Không tự chuẩn hóa sang mã khác |
| Ví dụ mã cũ | D; AO/CI/KE; USA/KOR/JPN/CHN... | Độ dài không phải điều kiện quyết định tính hợp lệ |
| Một tên có nhiều mã | Ví dụ AO/AUT hoặc NZD/NZL | Không gộp/xóa/đổi ID theo suy đoán; dùng mã hồ sơ cũ |
| Quốc tịch hoạt động | Delete_flag = 0 | Không tự kích hoạt lại bản ghi đã ẩn |
| Employees.IDCareer cũ | int NOT NULL | Import phải có nghề hợp lệ; model thử R4 nullable không thay thế điều kiện cũ |
| Nghề trong file mẫu | Kỹ sư, Kỹ thuật viên, Nhà đầu tư chưa khớp tên trong DB cũ đã kiểm tra; Quản lý có nhiều bản ghi | Không tự gán nghề hoặc bỏ qua giá trị thiếu; cần đối chiếu theo khóa/nghiệp vụ |
| Bảng mở rộng của web trong DB cũ | Chưa có SchemaVersions và ForeignPersons | Không coi việc đổi connection string hay nạp catalog là hoàn tất nâng cấp schema |

Không áp migration/DDL, không đổi khóa, không chuyển dữ liệu thực trong lần đối chiếu này. Việc nâng cấp database cũ cần được thiết kế và nghiệm thu riêng.

## Kiểm chứng bằng binary R4 và file thực tế

DLL R4 được lấy từ payload đã xác minh của bộ cài bàn giao, không dùng DLL source mới để thay thế. Probe riêng chạy trong Docker Linux, SQLite in-memory, mạng tắt; chỉ bản sao file test và danh mục tham chiếu được đưa vào probe.

SHA-256 phục vụ đối chiếu:

- EXE R4 hiện hành: `5B591C720C4E99D6FDDA647C3955F87AB16806825B699F8F9B1399DC1C0B0BD3`.
- DLL R4 được thử: `27DFD9867100C8FD8168C16578E4CBC9099CC2CEBFF3E6527BCC992C04C709BC`.
- Workbook gốc: `EC0E39DCC64F297F0E18EAED0830B927521C284E02250BB7ACFD31CE455C9FB7`.

1. File gốc + 14 mã mặc định: 10 mã không khớp; preview cũ vẫn hiện Mới; lỗi foreign key khi ghi dòng 1, còn 0 hồ sơ sau rollback.
2. File DA-SUA + 14 mã mặc định: thành công, 10 hồ sơ; chỉ có ý nghĩa với danh mục mặc định mới.
3. File gốc + danh mục cũ: chỉ DEU không khớp (dòng dữ liệu 5, ô D6); lỗi foreign key tại dòng 5 và còn 0 hồ sơ sau rollback.
4. Danh mục cũ + thay DEU bằng D trong bộ dữ liệu probe: qua phần kiểm tra quốc tịch, 10 hồ sơ được lưu trong model thử R4. Không sửa workbook gốc.

Cả 10 ngày sinh và 10 hạn tạm trú đều được đọc. Cột “Ghi chú thăm thân” bị automap thành skip và cần ghép thủ công. Probe cuối không sao chép toàn bộ schema/career/địa bàn của database cũ, nên không phải xác nhận import đầy đủ nghiệp vụ cũ hoặc nghiệm thu trên SQL Server khách hàng.

Lần chạy bốn tình huống trên đã kết thúc thành công trong phiên chẩn đoán này. Khi rà soát cuối tài liệu, lệnh chạy lại không kết nối được `dockerDesktopLinuxEngine` vì Docker daemon không còn hoạt động; không ghi nhận lần chạy lại này là đạt. Không có thay đổi mã ứng dụng sau phép thử. Kiểm tra cuối xác nhận đủ 161 tệp bàn giao khớp kích thước/SHA-256, 160 mục lịch sử giữ nguyên và workbook gốc không thay đổi.

## Hướng nâng cấp cần chốt

Danh mục database cũ là chuẩn. Ưu tiên hỗ trợ import: xác minh mã theo database đích, báo đầy đủ lỗi từng dòng, đối chiếu điều kiện bắt buộc cũ, chọn lại mã từ danh mục và xem thay đổi trước khi ghi. Một hướng khác là hoàn thiện quản lý danh mục: khóa mã đã dùng, thêm/sửa có kiểm tra trùng, phân biệt ngừng dùng với xóa, xuất danh mục và lưu lịch sử thay đổi.

Các khả năng này chưa được triển khai trong lần viết hướng dẫn. Không thay thế quy trình kiểm soát nghiệp vụ bằng màn hình chạy SQL/đổi schema tùy ý. Source và bộ cài đã giao phải được phân biệt khi nghiệm thu.
