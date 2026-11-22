# Hướng dẫn import Excel và giữ đúng dữ liệu nghiệp vụ cũ

Cập nhật: 10/11/2026. Áp dụng cho màn hình **Import dữ liệu từ Excel** của IRM v1.0.2. Tài liệu phân biệt bộ cài R4 đã giao, source nâng cấp hiện tại và danh mục trong database cũ; cùng nhãn phiên bản không có nghĩa ba phần này đã đồng nhất dữ liệu.

## 1. Điều phải xác nhận trước khi import

**Danh mục của database nghiệp vụ đang sử dụng là chuẩn. Không tự đổi mã quốc tịch sang một bộ mã khác chỉ để import thành công.**

Kiểm tra chỉ đọc trên máy phát triển cho thấy database cũ `ReportManagerDB` có 88 quốc tịch, 87 đang hoạt động; các database mới tên `IRM` đang có bộ 14 mã mặc định khác. Chưa truy cập database của máy khách để xác nhận danh mục tại đó.

| Quốc tịch | Mã đang có trong database cũ đã kiểm tra | Mã mặc định trong database IRM mới |
|---|---|---|
| Mỹ | USA | US |
| Hàn Quốc | KOR | KR |
| Nhật Bản | JPN | JP |
| Trung Quốc | CHN | CN |
| Anh | GBR | GB |
| Úc | AUS | AU |
| Ấn Độ | IND | IN |
| Đức | D | Chưa có |
| Pháp | FRA | Chưa có |
| Canada | CAN | Chưa có |

Database cũ có cả mã một ký tự như `D`, hai ký tự như `AO`, `CI`, `KE`, và nhiều mã ba ký tự. Không áp điều kiện “tất cả mã phải có hai/ba ký tự”. Giữ nguyên mã, ID, trạng thái hoạt động và các hồ sơ đang tham chiếu đến chúng.

Trong file gốc `IRM-MAU-IMPORT-TEST.xlsx`, 9/10 mã quốc tịch khớp danh mục cũ đã kiểm tra. Riêng ô **D6** ghi `DEU`, trong khi database cũ dùng `D` cho Đức. Việc đối chiếu này chỉ xác nhận quốc tịch; các trường nghề nghiệp và quy tắc bắt buộc vẫn phải kiểm tra riêng.

File `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` trước đây chỉ được sửa để thử với **database IRM mới có 14 mã mặc định**. File đó đã đổi ba quốc tịch mẫu sang quốc gia khác vì danh mục mặc định thiếu mã. Nó không phải file chuẩn để kế thừa database cũ và không được dùng làm quy tắc sửa dữ liệu thật.

## 2. Chuẩn bị tài khoản, công ty và danh mục

1. Đăng nhập bằng tài khoản có quyền nhập dữ liệu (`ADMIN` hoặc `DATA_EDITOR`). Hoàn tác yêu cầu `ADMIN`.
2. Mở **Doanh nghiệp**, kiểm tra công ty cần nhập đã tồn tại và đang hoạt động. Nếu chỉ có một công ty, hệ thống có thể chọn sẵn; vẫn phải kiểm tra tên.
3. Nhờ người quản trị mở **Quản trị → Quốc tịch** và **Ngành nghề** để đối chiếu mã/tên thực tế trong database ứng dụng đang kết nối. Không lấy bộ mã mặc định của một database mới làm chuẩn cho database cũ.
4. Nếu danh mục đích thiếu mã cũ hợp lệ, ghi nhận chênh lệch để xử lý việc kế thừa/đồng bộ danh mục. Đổi quốc tịch của người trong file là thay đổi nghiệp vụ, không phải cách sửa lỗi danh mục.
5. Với dữ liệu thật, thử một nhóm nhỏ và kiểm tra hồ sơ sau khi nhập trước khi nhập số lượng lớn. Dữ liệu test nên dùng công ty/môi trường thử riêng.

**Kiểm tra nghề nghiệp:** database cũ yêu cầu `Employees.IDCareer` có giá trị. File mẫu có các tên `Kỹ sư`, `Kỹ thuật viên`, `Nhà đầu tư` chưa khớp tên nghề trong database cũ đã kiểm tra. Tên `Quản lý` xuất hiện nhiều hơn một lần. Vì vậy cần chọn/đối chiếu đúng nghề nghiệp, không chấp nhận ô nghề nghiệp trống hoặc tự chọn một tên gần giống.

## 3. Chuẩn bị cấu trúc file Excel

- Dùng **Excel Workbook `.xlsx`**. Nếu đang dùng `.xls`, vào **Save As → Excel Workbook (.xlsx)**; đổi đuôi tên file không chuyển được định dạng. Giao diện hiện có nhận cả hai đuôi, nhưng lần kiểm tra này chỉ xác nhận `.xlsx`.
- Đặt sheet dữ liệu làm **sheet đầu tiên**. Ứng dụng đọc sheet đầu, không tự tìm sheet theo tên “Mẫu Import”.
- Hàng đầu tiên là tiêu đề cột; dữ liệu bắt đầu từ hàng tiếp theo. Không đặt tên báo cáo, logo hoặc dòng giải thích phía trên tiêu đề.
- Mỗi người một dòng. Không gộp ô trong vùng dữ liệu và không chèn một dòng hoàn toàn trống giữa danh sách: bộ đọc hiện tại dừng khi gặp dòng trống.
- Để hộ chiếu, số visa, số GPLĐ và CCCD/CMND ở kiểu **Text** để giữ số 0 đầu và tránh ký hiệu khoa học.
- Ngày nên là ô Date thực của Excel, hiển thị ngày/tháng/năm. Nếu phải dùng văn bản, ưu tiên dạng rõ ràng `yyyy-MM-dd`; đối chiếu lại ngày sau khi nhập.
- Không lặp một hộ chiếu trong cùng file. Giữ cách viết hộ chiếu thống nhất với dữ liệu đang có.

Các cột cơ bản:

| Cột Excel | Ghép với trường hệ thống | Nội dung cần kiểm tra |
|---|---|---|
| Họ tên | Họ tên | Có giá trị, đúng người |
| Giới tính | Giới tính | Nam/Nữ |
| Ngày sinh | Ngày sinh | Ngày hợp lệ, không nhầm tháng/ngày |
| Quốc tịch | Quốc tịch | Mã thực tế đang hoạt động trong database cũ/đích đã thống nhất |
| Số hộ chiếu | Số hộ chiếu | Giữ nguyên chuỗi mã; dùng để nhận diện trùng |
| Địa chỉ | Địa chỉ | Đầy đủ, đúng địa bàn theo hồ sơ |
| Nghề nghiệp | Nghề nghiệp/Chức danh | Khớp danh mục; kiểm tra trường hợp trùng tên |
| GPLĐ | GPLĐ | Dùng đúng giá trị ở bảng bên dưới |
| Số GPLĐ | Số GPLĐ | Có khi hồ sơ thuộc trường hợp đã có GPLĐ |
| Số Visa | Số Visa | Giá trị đúng hồ sơ |
| Ký hiệu Visa | Ký hiệu Visa | Ghép riêng nếu file có cột này |
| Hạn tạm trú | Hạn tạm trú | Ngày hợp lệ |
| Ghi chú | Ghi chú | Không nhầm với ghi chú thăm thân |
| Thăm thân | Thăm thân (Có/Không) | Có hoặc Không |
| Tên người thân | Tên người thân | Điền theo hồ sơ nếu có thăm thân |
| Quan hệ | Quan hệ người thân | Vợ/Chồng, Con… theo hồ sơ |
| CMND người thân | CMND/CCCD người thân | Kiểu Text |
| Ngày bắt đầu thăm thân | Ngày bắt đầu thăm thân | Ngày hợp lệ |
| Hạn thăm thân | Hạn thăm thân | Ngày hợp lệ |
| Ghi chú thăm thân | Ghi chú thăm thân | Cần kiểm tra và ghép thủ công nếu bị Bỏ qua |

Giá trị GPLĐ được bộ đọc nhận diện:

| Nội dung nên ghi trong file | Ý nghĩa |
|---|---|
| Đã có GPLĐ | Người lao động đã có giấy phép |
| Chưa có | Người lao động chưa có giấy phép |
| Miễn GPLĐ | Người lao động miễn giấy phép |
| Nhà đầu tư miễn | Nhà đầu tư miễn giấy phép |
| Nhà đầu tư có | Nhà đầu tư đã có giấy phép |
| Nhà đầu tư chưa | Nhà đầu tư chưa có giấy phép |

Giá trị lạ không được bộ đọc báo lỗi đầy đủ và có thể rơi về giá trị mặc định. Luôn đối chiếu cột GPLĐ tại bước xem trước.

Theo form WPF cũ, họ tên, ngày sinh, quốc tịch, hộ chiếu, địa chỉ, số visa và hạn tạm trú là các trường bắt buộc; trường hợp đã có GPLĐ còn cần số GPLĐ. Form cũ cũng kiểm tra địa bàn/phường xã. Màn hình import hiện tại chưa thực thi đầy đủ tất cả điều kiện này: “xem trước không báo lỗi” không thay thế kiểm tra nghiệp vụ.

## 4. Thao tác bốn bước trong ứng dụng

### Bước 1 — Upload file

1. Mở **Import dữ liệu từ Excel** (`/import`).
2. Chọn công ty trong ô **Công ty mặc định (dùng khi file không có cột Công ty)**. File mẫu 10 người đang kiểm tra không có cột Công ty, nên cần một công ty mặc định đúng.
3. Chọn **— Không dùng template —** ở lần đầu để tự kiểm tra các cột.
4. Nhấn **Chọn file Excel**, chọn đúng file `.xlsx`.
5. Kiểm tra thông báo tên file, tên sheet và số dòng. File mẫu phải có 10 dòng dữ liệu.
6. Nhấn **Tiếp tục**.

Nếu file có cột Công ty/CQ bảo lãnh, hệ thống có thể tìm công ty theo từng dòng. Cần đối chiếu cột Công ty ở bước xem trước vì cơ chế tìm hiện tại có cả so khớp gần theo chuỗi tên; tên gần giống nhau có thể gây chọn nhầm.

### Bước 2 — Ghép cột

1. Đọc từng hàng **Cột Excel → Trường hệ thống**. Tự động nhận diện là gợi ý, không phải xác nhận dữ liệu đã đúng.
2. Ghép đúng ít nhất các trường nghiệp vụ cần nhập và hộ chiếu để nhận diện trùng.
3. Mỗi trường hệ thống chỉ nên nhận từ một cột Excel. Chọn **— Bỏ qua —** cho cột không cần nhập.
4. Kiểm tra riêng **Ghi chú thăm thân**. Thử nghiệm với DLL R4 cho thấy cột này bị tự động đặt là Bỏ qua; chọn lại thủ công thành **Ghi chú thăm thân** nếu muốn lưu.
5. Nếu sẽ dùng lại đúng cấu trúc file, chọn **Lưu template** và đặt tên dễ nhận biết, ví dụ “Lao động – danh mục DB cũ”. Template chỉ lưu cách ghép cột; nó không sửa mã quốc tịch, tạo nghề nghiệp hay bảo đảm dữ liệu hợp lệ.
6. Nhấn **Tiếp tục**.

### Bước 3 — Xem trước và xử lý trùng

1. Đối chiếu tổng số dòng với Excel. Dòng 1 trên màn hình là người đầu tiên; nếu tiêu đề ở hàng 1 Excel thì người đó nằm ở hàng 2 Excel.
2. Kiểm tra họ tên, quốc tịch, hộ chiếu, công ty, nghề nghiệp và GPLĐ. Với ngày sinh/hạn tạm trú, kiểm tra thêm sau khi nhập hoặc qua file đối chiếu vì bảng xem trước không hiển thị toàn bộ trường.
3. Dòng **Mới** dự kiến được thêm mới; dòng **Trùng** đã có hộ chiếu trong database; dòng **Lỗi** cần sửa trước khi tiếp tục.
4. Với dòng trùng, chọn **Bỏ qua** nếu không chủ ý cập nhật. Hiện tại lựa chọn mặc định của giao diện là **Cập nhật**.
5. Khi chọn Cập nhật, bộ xử lý ghi đè nhiều trường của hồ sơ, không chỉ cột ghi chú. Ô trống hoặc cột thiếu có thể làm mất giá trị cũ ở một số trường. Chỉ cập nhật bằng file đầy đủ đã đối chiếu.
6. Nếu không khớp danh mục, sửa bản sao file theo mã đã được xác nhận hoặc xử lý danh mục trước, tải lại và kiểm tra lại. R4 cũ có thể vẫn hiện Mới dù mã quốc tịch không tồn tại; chỉ đến lúc lưu mới báo lỗi database.
7. Nhấn **Bắt đầu Import** sau khi các dòng cần ghi đã được kiểm tra.

Các bước upload, ghép cột và xem trước chưa ghi hồ sơ người lao động vào database. Tuy nhiên, nếu chọn Lưu template thì template được lưu khi chuyển sang xem trước.

### Bước 4 — Kết quả và đối chiếu hồ sơ

1. Đọc trạng thái **Import hoàn tất!** hoặc **Import thất bại!**, mã phiên và các số **Thêm mới/Cập nhật/Lỗi/Bỏ qua**.
2. Nếu hoàn tất nhưng có dòng lỗi hoặc bỏ qua, những dòng đó chưa được nhập. Không chỉ nhìn dấu thành công để kết luận đã đủ dữ liệu.
3. Nếu thất bại do lỗi SQL và có thông báo toàn bộ giao dịch đã hoàn tác, dữ liệu của phiên đó không được lưu. Hai phép thử file lỗi trong lần kiểm tra này đều còn 0 hồ sơ sau rollback.
4. Nhấn **Xem danh sách NLĐ**, tìm theo hộ chiếu và kiểm tra chi tiết hồ sơ. Với dữ liệu thử, tìm tiền tố `IRMTEST26`.
5. Mở **Lịch sử Import**, đối chiếu đúng tên file, công ty, mã phiên, số lượng và trạng thái **Đã lưu**.

Trên database thử mới với danh mục mặc định R4, file DA-SUA đã nhập 10 hồ sơ, 0 cập nhật, 0 lỗi. Đây là kết quả cho danh mục mặc định, không phải xác nhận file đó phù hợp database cũ.

## 5. Quản lý danh mục bằng tính năng đang có

Mở **Quản trị → Quốc tịch** (`/admin/nationalities`) bằng tài khoản quản trị. Hiện có các thao tác Thêm, Sửa và Xóa.

- Xem mã và tên đang có trước khi chuẩn bị Excel.
- Khi bổ sung một quốc tịch thực sự thiếu, dùng mã nghiệp vụ đã thống nhất với database cũ; kiểm tra trùng mã và tên trước khi lưu.
- Với mã đã có hồ sơ tham chiếu, giữ nguyên mã và ID; sửa tên khi có yêu cầu nghiệp vụ hợp lệ. Việc đổi khóa mã cần quy trình riêng, không sửa trực tiếp để chữa lỗi import.
- Xóa là thao tác ẩn bằng `Delete_flag`; quốc tịch bị ẩn không thuộc danh mục hoạt động để import.
- Nếu một tên có nhiều mã, ví dụ `AO`/`AUT`, dùng đúng mã đang được hồ sơ cũ sử dụng; không tự gộp hai bản ghi chỉ vì trùng tên.

Chức năng thêm/sửa danh mục hiện có không đồng nghĩa ứng dụng đã có quy trình chuyển toàn bộ database cũ sang database IRM mới. Hai việc này cần được quản lý riêng.

## 6. Lịch sử và hoàn tác

Mở **Lịch sử Import**, tìm đúng phiên và dùng **Hoàn tác** bằng tài khoản ADMIN. Bản ghi thêm mới được đánh dấu ẩn; các bản cập nhật được xử lý từ dữ liệu backup của phiên. Kiểm tra lại hồ sơ và trạng thái **Đã hoàn tác** sau thao tác.

Các giới hạn đã xác định trong mã và kiểm tra trước đây: rollback bản cập nhật chưa khôi phục đầy đủ một số giá trị vốn là NULL; mã phiên import có độ phân giải theo giây, nên hai phiên đồng thời có thể trùng mã. Do đó chưa dùng lời hứa “hoàn tác bất cứ lúc nào” để thay thế backup và nghiệm thu trước khi nhập dữ liệu nghiệp vụ hàng loạt. Tránh chạy nhiều phiên xác nhận cùng lúc và giữ bản dữ liệu trước cập nhật.

## 7. Tra lỗi theo triệu chứng

| Triệu chứng | Cần kiểm tra và xử lý |
|---|---|
| `FK_Employees_Nationality_Nationality` | Mã trong dòng Excel không có ở `Nationality` của đúng database đích. Đối chiếu danh mục cũ/đích, không mặc định đổi USA thành US. |
| Lỗi ở Dòng 1 với USA | Ảnh lỗi trước đây phù hợp database đích đang có US thay cho USA; kiểm tra lại database kết nối và việc kế thừa danh mục. |
| Lỗi DEU khi dùng danh mục cũ đã kiểm tra | Database cũ dùng D cho Đức; trong bản sao file mẫu, đối chiếu ô D6. |
| Lỗi IDCareer hoặc nghề nghiệp trống | Nghề chưa khớp danh mục, tên bị trùng hoặc chưa chọn đúng nghề. Database cũ yêu cầu IDCareer có giá trị. |
| Chưa có/chưa chọn công ty | Tạo hoặc chọn đúng công ty đang hoạt động trước khi tiếp tục. |
| Không đọc được file | Dùng Save As thành .xlsx, không đổi đuôi; kiểm tra file không hỏng hoặc có mật khẩu. |
| Số dòng bị thiếu | Kiểm tra sheet đầu, vị trí tiêu đề, dòng trắng giữa dữ liệu và các dòng thiếu họ tên. |
| Ghi chú thăm thân không lưu | Ghép lại cột này thủ công thay vì để Bỏ qua. |
| Hộ chiếu hiện Trùng | Xem hồ sơ hiện có; chọn Bỏ qua hoặc Cập nhật theo chủ ý nghiệp vụ. |
| Không có quyền import/hoàn tác | Kiểm tra vai trò tài khoản; hoàn tác cần ADMIN. |
| Lỗi ngày/thông tin ngày bị trống | Sử dụng ô Date hoặc văn bản ngày rõ ràng; kiểm tra lại ngày sau nhập. |

Khi cần hỗ trợ, ghi lại: tên file và bản sao file đang dùng; bộ cài/commit đang chạy; bước bị lỗi; toàn bộ thông báo; mã phiên; tên database đích và danh sách mã quốc tịch liên quan. Không gửi mật khẩu kết nối. Database khách hàng chưa được truy cập trong lần kiểm tra này, nên nếu khách vẫn lỗi với file khác cần đối chiếu dữ liệu tại đúng máy đó.

## 8. Kết quả kiểm tra và phạm vi xác nhận

Đã dùng đúng DLL R4 đã bàn giao để đọc hai file `.xlsx` và chạy import trong SQLite in-memory tách biệt. Không ghi vào ba database SQL Server được đối chiếu bằng SELECT.

| Dữ liệu thử | Danh mục quốc tịch trong database thử | Kết quả |
|---|---|---|
| File gốc | 14 mã mặc định R4 | Lỗi dòng dữ liệu 1; 0 hồ sơ lưu |
| File DA-SUA | 14 mã mặc định R4 | 10 hồ sơ lưu |
| File gốc | 87 mã hoạt động lấy từ DB cũ | Chỉ DEU không khớp; lỗi dòng dữ liệu 5; 0 hồ sơ lưu |
| Dữ liệu file gốc, thay DEU bằng D trong bộ kiểm tra | 87 mã hoạt động lấy từ DB cũ | Qua kiểm tra quốc tịch và lưu 10 hồ sơ trong model thử |

**Giới hạn:** phép thử cuối chỉ kiểm chứng danh mục quốc tịch với model thử của R4, không tái tạo toàn bộ schema và ràng buộc database cũ. Nó không xác nhận file đã đáp ứng IDCareer, địa bàn, mọi quy tắc WPF hoặc nghiệm thu trực tiếp trên SQL Server của khách.

Source nâng cấp hiện đã có validation quốc tịch dựa trên danh mục đang hoạt động, không khóa độ dài mã ở hai ký tự. Bộ cài R4 chưa chứa bản sửa source đó. Việc hoàn thiện kiểm tra nghiệp vụ cũ, hỗ trợ sửa tại bước xem trước và quản lý danh mục có kiểm soát là phần nâng cấp cần chốt riêng trước khi triển khai.
