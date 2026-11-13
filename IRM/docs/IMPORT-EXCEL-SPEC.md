# Nhập Excel theo nghiệp vụ WPF

## Chuẩn bị

Dùng file `.xlsx`. Chọn công ty, nhóm nghề, sheet, dòng tiêu đề và vùng dữ liệu trước khi ghép cột. Số dòng là số hiển thị trong Excel, bắt đầu từ 1; sheet mặc định là sheet đầu.

- Dòng tiêu đề `0`: tự tìm dòng đầu có dữ liệu. Dòng dữ liệu đầu `0`: bắt đầu sau dòng tiêu đề đã chọn, hoặc sau dòng tự tìm nếu chưa chọn.
- Với file xuất WPF có nhiều dòng tiêu đề: nhập đúng dòng tiêu đề (ví dụ 8), dòng dữ liệu đầu (ví dụ 9), dòng cuối hoặc để trống để đọc hết sheet.
- Chọn lại ánh xạ nếu tiêu đề không nhận đúng. Cột STT chọn “Bỏ qua”. Có thể lưu mẫu ánh xạ theo công ty.
- Dòng trống giữa vùng được báo lỗi. Các dòng cuối không có cả họ tên và hộ chiếu được bỏ qua, kể cả khi còn ghi chú hoặc định dạng.
- Danh mục đang hoạt động của database đích là chuẩn. Cần có quốc tịch, nhóm nghề/nghề và phường/xã phù hợp trước khi nhập. Chương trình không tự đổi mã danh mục.

## Cột dữ liệu

| Trường | Cột mặc định WPF | Bắt buộc | Quy tắc |
|---|---:|:---:|---|
| Họ và tên | 2 | Có | Bỏ khoảng trắng đầu/cuối |
| Giới tính | 3 | Có | “NAM” không phân biệt hoa/thường là Nam (1), chữ khác là Nữ (2) |
| Ngày sinh | 4 | Có | Ngày hợp lệ theo quy tắc bên dưới |
| Quốc tịch | 5 | Có | Tên quốc tịch so khớp không dấu, không phân biệt hoa/thường; lưu đúng mã của danh mục đích. IRM cũng nhận mã hiện có |
| Số hộ chiếu | 6 | Có | Nhận diện hồ sơ trong công ty |
| Địa chỉ tạm trú | 7 | Có | Kết thúc bằng tên phường/xã đang có trong danh mục; so khớp không dấu, bỏ ký tự ẩn |
| Nghề nghiệp | 8 | Có | Tra đúng nhóm nghề đã chọn; không tự gán nghề khi thiếu hoặc không khớp |
| Số GPLĐ/Thời hạn | 9 | Không | Quy tắc GPLĐ bên dưới |
| Số visa/Thẻ tạm trú | 10 | Có | Bỏ khoảng trắng đầu/cuối |
| Thời hạn tạm trú | 11 | Có | Ngày hợp lệ |
| Ghi chú | 12 | Không | Ô trống lưu một dấu cách để tương thích DB cũ |
| Kết quả giải quyết | 20 | Không | Kiểm tra số thẻ và hạn tạm trú |
| Ngày cấp thẻ | Tùy chọn IRM | Không | Nếu có, phải khớp ngày trong kết quả giải quyết |

Vị trí cột thay đổi được bằng màn hình ghép cột. Tiêu đề “Số GPLĐ/Thời hạn” được ghép vào ô nghiệp vụ GPLĐ; “Số GPLĐ” riêng được ghép vào trường số GPLĐ. Nếu ô GPLĐ trống nhưng cột số riêng có số, IRM dùng số đó để nhận “Đã có”. Nếu cả hai có nội dung mâu thuẫn, dòng bị từ chối; không âm thầm bỏ số riêng.

## Ngày tháng

Ô ngày Excel, số serial Excel và chữ dạng `dd/MM/yyyy`, `d/M/yyyy`, `dd-MM-yyyy`, `d-M-yyyy` đều được nhận. `03/06/2026` luôn là ngày 3 tháng 6, kể cả máy chủ chạy tiếng Anh.

- Năm 00–39 là 2000–2039; 40–99 là 1940–1999.
- Nếu ô có nhiều từ, lấy từ đầu tiên đọc được thành ngày, ví dụ `15/03/1980 (nam)`.
- Ngày lõi không nhận `yyyy-MM-dd` hoặc cách hiểu tháng/ngày của Mỹ. Ngày sai như `31/02/2026` bị báo lỗi.
- Ngày thăm thân và V010 nhận thêm ISO `yyyy-MM-dd`, đọc độc lập culture. Giá trị không trống nhưng sai định dạng bị báo lỗi; không dùng định dạng mở rộng này để đổi nghĩa ngày lõi.

## Giấy phép lao động

| Nội dung ô GPLĐ | Người lao động | Nhà đầu tư | Số GPLĐ lưu |
|---|---:|---:|---|
| Trống | 2 — Chưa có | 5 | Một dấu cách |
| Chứa “Miễn” (nhận cả không dấu) | 0 — Miễn | 3 | Một dấu cách |
| Chứa “Khác” | 6 — Thân nhân sống cùng | 6 | Một dấu cách |
| Chữ khác | 1 — Đã có | 4 | Chính nội dung ô |

Nhà đầu tư được xác định bằng hộ chiếu trong bảng đầu tư của đúng công ty, không bằng chữ “Nhà đầu tư” trong ô GPLĐ. Thứ tự nhận: Miễn, Khác, còn lại.

Nghề “Thân nhân” thuộc nhóm Khác (nhóm 9 theo dữ liệu cũ) buộc GPLĐ = 6 và số GPLĐ rỗng. Nếu GPLĐ = 6, hệ thống chọn nghề Thân nhân của nhóm Khác nếu có.

## Kết quả giải quyết

| Tiền tố | Giá trị lưu | Nội dung lưu |
|---|---:|---|
| `GHTT:` | 0 | Phần sau dấu hai chấm |
| `Visa:` | 1 | Phần sau dấu hai chấm |
| `TTT:` | 2 | Phần sau dấu hai chấm |
| Chữ khác | 3 | Toàn bộ ô |
| Trống | 3 | Một dấu cách |

Ví dụ: `TTT: DN1 03/06/2026-29/05/2028`. Số thẻ `DN1` phải khớp cột số visa; ngày sau phải bằng hạn tạm trú; ngày trước lưu thành ngày cấp thẻ. Sai định dạng hoặc lệch dữ liệu bị từ chối theo dòng.

## Xem trước và xác nhận

- **Mới:** sẽ thêm hồ sơ.
- **Thông tin đã có sẵn:** 13 trường nghiệp vụ và các cột mở rộng được ánh xạ giống bản hiện hành trong cùng công ty; bỏ qua. Chỉ khác cột mở rộng cũng được đưa vào nhánh “Nhân viên có thay đổi”. Cột mở rộng không được ánh xạ giữ dữ liệu cũ.
- **Nhân viên có thay đổi:** mặc định bỏ qua. Chọn cập nhật nếu chấp nhận thêm phiên bản mới và ẩn các phiên bản hiện hành của cùng hộ chiếu trong công ty. Bản cũ được giữ nguyên nội dung.
- **Lỗi:** dòng không được ghi. Thông báo chỉ rõ số dòng Excel thật, trường thiếu, ngày sai hoặc danh mục không khớp. Các dòng hợp lệ khác vẫn được xử lý.
- Cảnh báo nhân viên đang làm ở công ty khác không chặn dòng. Hệ thống cũng cảnh báo nếu còn người chưa hết hạn tạm trú nhưng không có trong file.

Cột Công ty có thể chỉ định nhiều công ty. Tên phải khớp chính xác, không ghép theo một phần tên. Công ty chưa có chỉ được tạo nếu bạn bật rõ “Cho phép tạo công ty”; kiểm tra tên tại xem trước. Ô Công ty trống dùng công ty mặc định đã chọn.

Mỗi lần import có mã phiên riêng. Có thể hoàn tác từ lịch sử bằng tài khoản được phân quyền. Với cập nhật phiên bản, hoàn tác xóa phiên bản mới và hiện lại bản cũ. Các lần thêm mới thông thường được ẩn khi hoàn tác theo cơ chế lịch sử của IRM. Năm số liệu công ty được tính lại sau import và hoàn tác; loại trừ hồ sơ ẩn, hết hạn hoặc đã nghỉ việc.

Nếu dữ liệu thay đổi sau xem trước, hệ thống yêu cầu xem trước lại. Nếu hồ sơ, dữ liệu mở rộng hoặc công ty đã phát sinh thay đổi sau phiên import, hoàn tác bị chặn để giữ dữ liệu mới hơn; hoàn tác các phiên liên quan theo thứ tự từ mới đến cũ trước. Khi đường truyền lỗi ở bước xác nhận giao dịch, hệ thống kiểm tra mã phiên đã lưu để tránh nhập hai lần.

Các cột VisaSymbol, thăm thân, diện cư trú, giấy tờ, định danh điện tử và lưu trú V010 vẫn là tùy chọn. Không có cột mở rộng không làm mất các kiểm tra lõi.

Đối với giấy tờ hoặc diện cư trú V010 đã có nhưng thời hạn trong file khác với hồ sơ, hệ thống báo lỗi yêu cầu chỉnh sửa hồ sơ trước; không báo thành công rồi bỏ qua thời hạn mới.

## Đối chiếu file mẫu ngày 10/11/2026

Đã đọc bản `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` mà không sửa file. SHA-256: `5DC0518B9FA3086253B4BC7372D849C5A987409B858368E477084567DFBEE3CA`.

File có 10 dòng dữ liệu, 19 cột và một sheet hướng dẫn. Các cột bắt buộc có dữ liệu, nhưng chưa thể coi là bộ mẫu chuẩn cho nghiệp vụ cũ:

- Quốc tịch dùng bộ seed IRM mới; không thay cho mã của database cũ.
- Ô GPLĐ chứa các nhãn `Chưa có`, `Đã có GPLĐ`, `Nhà đầu tư có/chưa/miễn`; theo WPF, chữ khác “Miễn”/“Khác” được hiểu là số GPLĐ. Không dùng file này để nghiệm thu GPLĐ cũ.
- File có cột Số GPLĐ riêng; cần kiểm tra cách ghép và quy tắc mẫu trước khi nhập.
- Không có cột Kết quả giải quyết/ngày cấp thẻ; không kiểm chứng được nội dung này.
- Nghề và đuôi địa chỉ vẫn cần khớp danh mục thật của database đích.

## Bộ thử ngày 12/11/2026

File mới: `BAN-GIAO/03-DU-LIEU-TEST/2026-11-12/IRM-IMPORT-WPF-GOLDEN-20261112.xlsx`. Chọn sheet **Du lieu thu**, dòng tiêu đề **8**, dữ liệu **9–35**. Sheet **Ket qua mong doi** ghi kết quả cho từng dòng; không nhập sheet này.

Đây là dữ liệu giả cho database test: CHN/Trung Quốc, Kỹ sư nhóm 1, Thân nhân nhóm 9, phường Hải Châu; INV-01…INV-04 đã có trong bảng đầu tư của công ty test. Khi dùng danh mục khác, đối chiếu các giá trị trước khi thử; không tự thêm hoặc thay danh mục nghiệp vụ để làm test đạt.

Lần đầu mong đợi 12 hồ sơ mới và 15 dòng lỗi. Nhập lại cùng file có 12 dòng đã có sẵn; sửa tên một hồ sơ để kiểm tra nhánh thay đổi và hoàn tác. Test tự động đối chiếu từng cột, số dòng, thông điệp, hồ sơ thân nhân/nhà đầu tư và phiên bản trên SQLite và SQL Server riêng biệt.
