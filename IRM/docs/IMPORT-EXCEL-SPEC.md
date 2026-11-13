# Đặc tả từng trường import Excel

Dành cho người tập huấn và người cần tra cứu lỗi tại màn hình `/import`. Có đúng 35 trường được hỗ trợ; “Bỏ qua” dùng cho cột không nhập, không phải trường thứ 36. Tên trong tiêu đề mỗi mục là tên tại màn hình ghép cột, kèm khóa để đối chiếu.

Bắt đầu thao tác theo [Hướng dẫn nhanh](HUONG-DAN-IMPORT-EXCEL-NHANH.md); quay về [mục lục tài liệu](README.md) để tìm tài liệu liên quan.

Đối chiếu ngày **13/11/2026**, theo mã ứng dụng `5c9808d1d213431d399ba51eb002e4844b9d69d5`; các commit kế tiếp trong đợt này chỉ thay Markdown. Áp dụng cho luồng import ở source này với danh mục đích đã chuẩn bị, không chứng nhận mọi workbook sẽ ghi thành công. Bộ cài R4 và các gói đã giao chưa được build lại từ source này; hướng dẫn [R4/database cũ](HUONG-DAN-IMPORT-EXCEL.md) và [kiểm chứng source trước đó](IMPORT-EXCEL-VERIFICATION-2026-11-13.md) là bằng chứng có phạm vi riêng, không đổi nhãn bộ cài cũ.

Ứng dụng ghi nhận, quản lý và phân tích dữ liệu đã có. Mọi số hộ chiếu, GPLĐ, giấy tờ, định danh trong file phải lấy từ nguồn thực tế; chương trình không cấp hoặc tự sinh các số này. Các ví dụ là dữ liệu giả để giải thích cách đọc.

## Cách đọc và điều kiện áp dụng

**Chương trình kiểm tra** mô tả hành vi hiện có, gồm cả trường hợp chương trình nhận nhưng dễ hiểu sai. **Khuyến nghị nhập liệu** giúp chuẩn bị file; không có nghĩa mọi khuyến nghị đều được chương trình cưỡng chế.

“Không ghép” nghĩa là không ánh xạ cột vào trường đó hoặc chọn Bỏ qua. “Ô trống” nghĩa là đã ghép nhưng ô không có nội dung sau khi bỏ khoảng trắng đầu/cuối. Hai trạng thái này có thể cho kết quả khác nhau khi cập nhật.

Các ví dụ có quốc tịch `CHN`/`Trung Quốc`, nghề `Kỹ sư` nhóm 1 và địa chỉ kết thúc `Hải Châu` chỉ hợp lệ nếu database đích có đúng danh mục đang hoạt động tương ứng. Đây không phải bộ mã thay thế database kế thừa. Dùng danh mục thật của đơn vị, không thêm mục tùy tiện để làm file đạt.

## File và vùng đọc

- Dùng `.xlsx`; chọn đúng công ty mặc định và nhóm nghề trước khi xem trước. Nhóm nghề áp dụng cho lần đọc, không có cột nhóm nghề riêng trong 35 trường.
- Chọn đúng sheet dữ liệu. Số dòng trên giao diện và trong lỗi là số dòng Excel, bắt đầu từ 1.
- Dòng tiêu đề `0`: tìm dòng đầu có dữ liệu. Dòng bắt đầu `0`: sau dòng tiêu đề đã chọn; nếu tiêu đề cũng tự tìm thì sau dòng đầu có dữ liệu. Dòng kết thúc để trống: đọc đến cuối vùng dùng của sheet.
- Dòng trống ở giữa vùng vẫn được kiểm tra. Đuôi vùng không có cả họ tên và hộ chiếu được bỏ, kể cả còn ghi chú; không coi hồ sơ thiếu tên/hộ chiếu ở đuôi là đã được nhập.
- Ghép theo ý nghĩa, không phụ thuộc vị trí cố định WPF. Rà lại ghép tự động và mẫu đã lưu, đặc biệt GPLĐ/Số GPLĐ, visa/giấy tờ mở rộng, hai loại địa chỉ. Cột STT chọn Bỏ qua.
- Định dạng **Text trước khi nhập/dán** các số nhận dạng và mã. Nếu Excel đã biến `001234` thành `1234`, đổi định dạng sau không khôi phục số 0; phải đối chiếu nguồn để sửa.

<a id="ngay-thang"></a>
## Ngày tháng

**Ngày lõi** gồm Ngày sinh, Hạn tạm trú, Ngày cấp thẻ và ngày nằm trong Kết quả giải quyết:

- Nhận chữ ngày/tháng/năm như `03/06/2026`, `3/6/2026`, `03-06-2026`. `03/06/2026` luôn là ngày 3 tháng 6, không phụ thuộc ngôn ngữ máy chủ.
- Năm 00–39 chuyển thành 2000–2039, 40–99 thành 1940–1999. Nếu ô có nhiều từ, lấy từ đầu tiên đọc được thành ngày, như `15/03/1980 (nam)`.
- Không nhận chữ ISO `2026-06-03` cho ngày lõi; không diễn giải theo tháng/ngày kiểu Mỹ. `31/02/2026` không hợp lệ.
- Riêng ô ngày đơn trong Excel được nhận dưới dạng Date hoặc số serial Excel rồi chuẩn hóa về `dd/MM/yyyy`; chữ là số serial cũng có thể được đọc thành ngày.

**Ngày mở rộng** gồm ngày thăm thân, diện cư trú, giấy tờ mở rộng, lịch sử lưu trú: nhận thêm chữ ISO `yyyy-MM-dd`. Ngày không trống nhưng không đọc được sẽ lỗi khi nhánh tương ứng được xử lý. Có nhánh chỉ được kiểm tra khi xác nhận ghi, không phải tất cả đều lỗi ngay ở xem trước.

**Khuyến nghị nhập liệu:** thống nhất `dd/MM/yyyy`, năm bốn chữ số; mỗi ô ngày đơn chỉ chứa một ngày, không thêm lời ghi chú hoặc nhập số serial bằng tay. Ngày hiệu lực từ giấy tờ là một ngày chung; chỉ ngày đến giấy tờ nhận danh sách `;`.

<a id="o-trong-va-cap-nhat"></a>
## Ô trống, cột không ghép và cập nhật

- Chín trường bắt buộc: Họ tên, Giới tính, Ngày sinh, Quốc tịch, Số hộ chiếu, Địa chỉ, Nghề nghiệp/Chức danh, Số Visa, Hạn tạm trú. Không ghép hay ô trống đều không đạt; cột mở rộng không thay thế chúng.
- GPLĐ, Số GPLĐ, Ghi chú, Kết quả giải quyết thuộc nhóm lõi cũ: không ghép vẫn được xử lý theo giá trị rỗng. Khi chấp nhận cập nhật có thể thay dữ liệu cũ bằng mặc định/rỗng; không hiểu là giữ nguyên.
- Ký hiệu Visa và các trường thăm thân: không ghép giữ giá trị cũ khi cập nhật; đã ghép nhưng trống có thể xóa nội dung hoặc đưa cờ về 0. Ngày cấp thẻ có quy tắc kết hợp riêng với Kết quả giải quyết.
- 12 trường mở rộng từ Diện cư trú đến Lưu trú đến ngày (V010) chỉ chuyển tiếp giá trị **không trống**. Không ghép hoặc ô trống không xóa diện cư trú, định danh, giấy tờ hay lịch sử lưu trú đã có. Có dữ liệu nhưng thiếu trường đi cùng có thể không ghi bản ghi nghiệp vụ tương ứng; không phải mọi tổ hợp thiếu đều báo lỗi.
- Các mô tả “khi cập nhật” chỉ áp dụng khi người dùng chọn cập nhật dòng thay đổi. Mặc định dòng thay đổi bị bỏ qua; bản cũ không tự bị ghi đè.

## Nhóm lõi và giấy tờ cũ — 15 trường

### `StaffName` — Họ tên

- **Ý nghĩa, kiểu Excel:** tên người được quản lý; Text. **Bắt buộc.**
- **Chương trình kiểm tra:** bỏ khoảng trắng đầu/cuối, phải còn nội dung; không tự sửa chính tả hoặc đối chiếu tên theo hộ chiếu.
- **Không ghép / ô trống:** đều lỗi, kể cả cập nhật; không lấy tên cũ bù vào.
- **Ví dụ đúng:** `TEST USER` trong hồ sơ giả có đủ tám trường bắt buộc còn lại. **Sai:** ô chỉ có khoảng trắng.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc tên nhân viên tại dòng N`; kiểm tra ô và ghép cột. Thay tên cùng hộ chiếu/công ty có thể đưa dòng vào trạng thái thay đổi.
- **Khuyến nghị nhập liệu:** ghi đúng tên theo nguồn chứng từ; không dùng dòng tiêu đề hoặc dòng tổng làm tên người.

### `Gender` — Giới tính

- **Ý nghĩa, kiểu Excel:** giới tính ghi nhận; Text. **Bắt buộc.**
- **Chương trình kiểm tra:** `NAM` không phân biệt hoa/thường, sau khi bỏ khoảng trắng, lưu Nam (1); mọi nội dung không trống khác lưu Nữ (2).
- **Không ghép / ô trống:** đều lỗi; cập nhật không giữ giới tính cũ để bù.
- **Ví dụ đúng:** `Nam`, `Nữ`. **Dễ hiểu sai:** `Male`, `1`, `không rõ` đều thành Nữ, không phải lỗi giá trị.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc giới tính tại dòng N` khi thiếu. Nếu sai nghĩa nhưng vẫn được nhận, sửa file và xem trước lại.
- **Khuyến nghị nhập liệu:** chỉ dùng Nam hoặc Nữ, rà từng giá trị trong file nguồn trước khi nhập. Bảng xem trước hiện không hiển thị giới tính; sau khi nhập, đối chiếu lại trên hồ sơ nhân viên đã lưu.

### `Birthday` — Ngày sinh

- **Ý nghĩa, kiểu Excel:** ngày sinh; Date hoặc Text theo quy tắc ngày lõi. **Bắt buộc.**
- **Chương trình kiểm tra:** phải đọc được ngày hợp lệ theo [Ngày tháng](#ngay-thang); không có kiểm tra tuổi trong parser import.
- **Không ghép / ô trống:** đều lỗi, cả khi cập nhật.
- **Ví dụ đúng:** `03/06/1990`. **Sai:** chữ `1990-06-03`, `31/02/1990`; `03/06/90` được nhận thành 1990 nhưng dễ nhầm.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc ngày sinh nhật tại dòng N`; sửa đúng ngày/tháng/năm, không đảo theo thiết lập máy chủ.
- **Khuyến nghị nhập liệu:** dùng năm bốn chữ số, đối chiếu hồ sơ gốc.

### `Nationality` — Quốc tịch

- **Ý nghĩa, kiểu Excel:** quốc tịch; Text chứa tên hoặc mã thực tế trong danh mục đích. **Bắt buộc.**
- **Chương trình kiểm tra:** chỉ danh mục đang hoạt động; ưu tiên khớp mã rồi tên, không phân biệt hoa/thường/dấu, chuẩn hóa khoảng trắng. Lưu mã của mục tìm được, không tự đổi sang ISO.
- **Không ghép / ô trống:** đều lỗi; cập nhật cũng phải cung cấp.
- **Ví dụ đúng có điều kiện:** `CHN`, `Trung Quốc` khi đích có mục tương ứng; database cũ có thể dùng mã khác. **Sai:** mã seed mới không tồn tại tại đích.
- **Liên quan, lỗi/cách sửa:** `Quốc tịch ... chưa có trong dữ liệu... tại dòng N`; tra danh mục đích, sửa mã/tên hoặc chuyển người quản lý danh mục xử lý nguồn thiếu thật sự.
- **Khuyến nghị nhập liệu:** lấy danh sách tên/mã từ database sẽ nhập; không thay mã hàng loạt theo suy đoán.

### `Passport` — Số hộ chiếu

- **Ý nghĩa, kiểu Excel:** số hộ chiếu đã có; **Text**, giữ số 0 đầu. **Bắt buộc.**
- **Chương trình kiểm tra:** bỏ khoảng trắng đầu/cuối, yêu cầu nội dung; không xác minh tính hợp pháp hay mẫu ký tự. Kết hợp công ty để tìm hồ sơ hiện hành; phần mở rộng còn liên kết hồ sơ người theo hộ chiếu chuẩn hóa.
- **Không ghép / ô trống:** đều lỗi; không tự sinh hoặc lấy số từ tên người.
- **Ví dụ đúng:** `GOLD-01` cho dữ liệu giả. **Dễ sai:** nhập `001234` kiểu Number làm mất số 0 có thể khiến chương trình hiểu thành hộ chiếu khác.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc số hộ chiếu tại dòng N`; sửa ô/ghép cột. Sửa số hoặc đổi công ty có thể tạo hồ sơ mới thay vì cập nhật hồ sơ dự định.
- **Khuyến nghị nhập liệu:** đối chiếu đúng người, số và công ty; không dùng tên làm khóa thay hộ chiếu.

### `Address` — Địa chỉ

- **Ý nghĩa, kiểu Excel:** địa chỉ tạm trú hồ sơ lõi; Text. **Bắt buộc.**
- **Chương trình kiểm tra:** phải kết thúc bằng tên phường/xã đang hoạt động trong Wards; so khớp đuôi không dấu/không phân biệt hoa/thường, bỏ ký tự ẩn. Không bắt buộc dấu phẩy trong parser.
- **Không ghép / ô trống:** đều lỗi; Địa chỉ lưu trú mở rộng không thay trường này.
- **Ví dụ đúng có điều kiện:** `12 Lê Duẩn, Hải Châu` nếu có phường `Hải Châu`. **Dễ sai:** thêm tên tỉnh sau phường khiến đuôi không còn khớp.
- **Liên quan, lỗi/cách sửa:** `Địa chỉ không có phường/ xã trong lưu trữ. Vui lòng kiểm tra lại tại dòng N`; đối chiếu địa chỉ và tên phường/xã thật ở đích. Mã xã/phường/đặc khu là cơ chế khác, không chữa được lỗi này.
- **Khuyến nghị nhập liệu:** ghi địa chỉ đầy đủ, kết thúc rõ bằng tên phường/xã đúng danh mục, giữ đúng ý nghĩa nguồn.

### `Career` — Nghề nghiệp/Chức danh

- **Ý nghĩa, kiểu Excel:** tên nghề trong nhóm chọn cho lần nhập; Text. **Bắt buộc.**
- **Chương trình kiểm tra:** tên khớp đầy đủ, không phân biệt hoa/thường nhưng **có phân biệt dấu**, trong đúng nhóm và chưa bị xóa; không tự gán khi thiếu. Trùng tên trong nhóm thì chọn ID nhỏ nhất.
- **Không ghép / ô trống:** đều lỗi, kể cả cập nhật.
- **Ví dụ đúng có điều kiện:** `Kỹ sư` khi có nghề đó trong nhóm đã chọn. **Sai:** `Ky su`, nghề chỉ có ở nhóm khác, nhập mã ID thay tên.
- **Liên quan, lỗi/cách sửa:** `Nghề nghiệp chưa có trong dữ liệu... tại dòng N`; kiểm tra tên lẫn nhóm. Nghề Thân nhân nhóm 9 buộc GPLĐ = 6 và số GPLĐ rỗng; GPLĐ Khác có thể chuyển nghề sang Thân nhân nhóm 9 nếu có.
- **Khuyến nghị nhập liệu:** chọn nhóm trước khi ghép; tách lần nhập theo nhóm nếu cần các nhóm khác nhau.

### `WorkPermit` — GPLĐ

- **Ý nghĩa, kiểu Excel:** ô nghiệp vụ GPLĐ tương thích dữ liệu cũ, chứa số đã có hoặc chỉ báo miễn/khác; Text. **Tùy chọn**, nhưng trống vẫn xác định trạng thái.
- **Chương trình kiểm tra:** theo [bảng GPLĐ](#quy-tac-gpld), không nhận mã enum như lựa chọn số trên form. Trống nhưng Số GPLĐ riêng có nội dung thì dùng số riêng để đọc.
- **Không ghép / ô trống:** nếu số riêng cũng thiếu/trống thì thành Chưa có (2), hoặc 5 với nhà đầu tư; số lưu một dấu cách. Cập nhật không mặc nhiên giữ GPLĐ cũ.
- **Ví dụ đúng:** `Miễn`, `Khác`, `WP-001` theo hồ sơ thật. **Dễ sai:** `Chưa có`, `Đã có`, `2` đều bị coi là số GPLĐ nếu không chứa chữ Miễn/Khác.
- **Liên quan, lỗi/cách sửa:** số riêng mâu thuẫn gây `Cột GPLĐ và Số GPLĐ không thống nhất tại dòng N`. Nhà đầu tư dựa trên bảng đầu tư đúng công ty, không dựa chữ trong ô. Nghề Thân nhân nhóm 9 có thể ghi đè kết quả đọc GPLĐ.
- **Khuyến nghị nhập liệu:** chưa có thì trống cả hai; đã có thì số thật; miễn/khác không kèm số riêng. Kiểm tra trạng thái tại xem trước.

### `WorkPermitNumber` — Số GPLĐ

- **Ý nghĩa, kiểu Excel:** số GPLĐ đã có, tách riêng ô GPLĐ; **Text**. **Tùy chọn.**
- **Chương trình kiểm tra:** nếu GPLĐ trống thì dùng số này đọc như GPLĐ; nếu cả hai có dữ liệu, GPLĐ phải thành Đã có (1/4) và số phải giống chính xác, có phân biệt hoa/thường.
- **Không ghép / ô trống:** không cung cấp số bổ sung; vẫn đọc GPLĐ, không giữ số cũ độc lập khi cập nhật.
- **Ví dụ đúng:** GPLĐ trống, Số GPLĐ `WP-001`; hoặc cả hai `WP-001`. **Sai:** GPLĐ `Miễn`, số riêng `WP-001`; GPLĐ `WP-001`, số riêng `WP-002`.
- **Liên quan, lỗi/cách sửa:** `Cột GPLĐ và Số GPLĐ không thống nhất tại dòng N`; đối chiếu và sửa hai cột. Nghề Thân nhân nhóm 9 cuối cùng đưa số về rỗng.
- **Khuyến nghị nhập liệu:** không ghép nhãn Đã có với số riêng; tránh số thật có chữ Miễn/Khác bị đọc thành chỉ báo.

### `VisaNumber` — Số Visa

- **Ý nghĩa, kiểu Excel:** số visa/thẻ tạm trú ghi trên hồ sơ lõi; **Text**. **Bắt buộc.**
- **Chương trình kiểm tra:** bỏ khoảng trắng đầu/cuối, phải có nội dung; không xác thực mẫu số. Đối chiếu số trong Kết quả giải quyết nếu chuỗi đó có số thẻ.
- **Không ghép / ô trống:** đều lỗi; DocumentNumber không thay thế Số Visa.
- **Ví dụ đúng:** `DN1` với kết quả cùng số. **Sai:** `DN2` khi kết quả là `TTT: DN1 03/06/2026-29/05/2028`.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc số visa tại dòng N` hoặc `Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau tại dòng N`; đối chiếu cùng giấy tờ, không sửa một phía tùy ý.
- **Khuyến nghị nhập liệu:** giữ đủ chữ và số 0 của số đã có; giấy tờ mở rộng là cơ chế riêng, không tự đồng bộ trường này.

### `VisaSymbol` — Ký hiệu Visa

- **Ý nghĩa, kiểu Excel:** ký hiệu visa hồ sơ lõi; Text. **Tùy chọn.**
- **Chương trình kiểm tra:** nhận chuỗi bỏ khoảng trắng đầu/cuối, không ép tập mã ký hiệu trong parser.
- **Không ghép:** mới rỗng, cập nhật giữ giá trị cũ. **Ô trống:** giá trị mới rỗng, có thể xóa ký hiệu trên phiên bản cập nhật.
- **Ví dụ đúng:** `LĐ2` theo giấy tờ đã có. **Dễ hiểu sai:** `VISA_EXTENSION` là loại giấy tờ mở rộng, không tự chuyển thành ký hiệu visa.
- **Liên quan, lỗi/cách sửa:** không có lỗi riêng cho ký hiệu sai nghiệp vụ; đối chiếu nguồn và dữ liệu đã ghi. Thay ký hiệu có thể thành dòng thay đổi.
- **Khuyến nghị nhập liệu:** dùng ký hiệu trên giấy tờ, không ghép loại giấy tờ vào đây.

### `TemporaryStay` — Hạn tạm trú

- **Ý nghĩa, kiểu Excel:** ngày hết hạn tạm trú hồ sơ lõi; Date hoặc Text ngày lõi. **Bắt buộc.**
- **Chương trình kiểm tra:** ngày hợp lệ; ngày cuối Kết quả giải quyết phải bằng ngày này nếu có kết quả. Không bắt buộc ngày còn ở tương lai trong parser.
- **Không ghép / ô trống:** đều lỗi dù đã có ResidenceValidTo hoặc DocumentValidTo.
- **Ví dụ đúng:** `29/05/2028` với kết quả kết thúc cùng ngày. **Sai:** `28/05/2028` với kết quả trên; chữ ISO không đạt ngày lõi.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc thời hạn tạm trú tại dòng N` hoặc `Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau tại dòng N`; đối chiếu giấy tờ. Ngày này ảnh hưởng cảnh báo/số liệu công ty.
- **Khuyến nghị nhập liệu:** phân biệt hạn tạm trú với hạn GPLĐ, giấy tờ mở rộng và lưu trú tại địa chỉ.

### `Note` — Ghi chú

- **Ý nghĩa, kiểu Excel:** ghi chú hồ sơ lõi; Text. **Tùy chọn.**
- **Chương trình kiểm tra:** bỏ khoảng trắng đầu/cuối; trống lưu một dấu cách để tương thích database cũ.
- **Không ghép / ô trống:** đều thành trống nói trên; nếu chọn cập nhật, ghi chú cũ có thể bị thay bằng trống.
- **Ví dụ đúng:** `Đã đối chiếu bản gốc`. **Dễ hiểu sai:** không ghép để mong giữ ghi chú cũ là không đúng với trường lõi này.
- **Liên quan, lỗi/cách sửa:** không có kiểm tra nội dung nghiệp vụ riêng; rà quyết định cập nhật và chuẩn bị đủ nội dung muốn giữ.
- **Khuyến nghị nhập liệu:** chỉ ghi thông tin liên quan hồ sơ, không dùng ghi chú thay trường bắt buộc.

### `SettlementResults` — Kết quả giải quyết

- **Ý nghĩa, kiểu Excel:** kết quả và chuỗi số thẻ/khoảng ngày nghiệp vụ cũ; Text. **Tùy chọn.**
- **Chương trình kiểm tra:** tiền tố `GHTT:` → 0, `Visa:` → 1, `TTT:` → 2, bỏ tiền tố khi lưu; không tiền tố → 3 nhưng không trống **vẫn phải đọc được khoảng ngày**. Không phải ghi chú tự do.
- **Không ghép / ô trống:** kết quả = 3, nội dung một dấu cách. Cập nhật không giữ nội dung cũ; ghép kết quả nhưng trống có thể làm Ngày cấp thẻ về null nếu không có ngày riêng.
- **Ví dụ đúng có điều kiện:** `TTT: DN1 03/06/2026-29/05/2028`, Số Visa `DN1`, Hạn tạm trú `29/05/2028`; suy ra Ngày cấp thẻ `03/06/2026`. **Sai:** `Đã giải quyết`, ISO trong khoảng, số/hạn lệch cột lõi.
- **Liên quan, lỗi/cách sửa:** `Sai định dạng kết quả giải quyết`, lỗi lệch thẻ/hạn, kèm `tại dòng N`; dùng mẫu và đối chiếu giấy tờ. Ngày cấp thẻ riêng phải khớp ngày suy ra. Parser chưa kiểm tra thứ tự đầu/cuối khoảng ngày.
- **Khuyến nghị nhập liệu:** dùng tiền tố, một số thẻ và khoảng ngày liền nhau, không chèn khoảng trắng quanh dấu nối; tự kiểm tra thứ tự ngày.

### `CardCreationDate` — Ngày cấp thẻ

- **Ý nghĩa, kiểu Excel:** ngày cấp thẻ **đã có**, không phải thao tác cấp thẻ; Date hoặc Text ngày lõi. **Tùy chọn.**
- **Chương trình kiểm tra:** không trống phải là ngày lõi hợp lệ, bằng ngày đầu Kết quả giải quyết nếu kết quả đã suy ra ngày.
- **Không ghép:** lấy ngày suy ra từ kết quả; cập nhật chỉ giữ ngày cũ nếu **cả cột ngày lẫn kết quả không ghép**. **Ô trống:** lấy ngày suy ra nếu có, nếu không thì null, có thể xóa ngày cũ.
- **Ví dụ đúng:** `03/06/2026` với kết quả mẫu trên. **Sai:** `04/06/2026` trong cùng dòng, chữ `2026-06-03` không đạt ngày lõi.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc ngày cấp thẻ` hoặc `Ngày cấp thẻ và kết quả giải quyết là khác nhau`, kèm số dòng; sửa theo nguồn và kiểm tra kết quả.
- **Khuyến nghị nhập liệu:** dùng cùng ngày nếu nhập cả hai cột; không dùng cho ngày cấp định danh hoặc GPLĐ.

<a id="quy-tac-gpld"></a>
## Bảng chuyển đổi GPLĐ dùng khi tập huấn

| Nội dung GPLĐ sau khi lấy số riêng nếu cần | Người lao động | Nhà đầu tư | Số GPLĐ lưu |
|---|---|---|---|
| Trống | 2 — Chưa có | 5 — Nhà đầu tư chưa có | Một dấu cách |
| Chứa Miễn (nhận cả không dấu) | 0 — Miễn | 3 — Nhà đầu tư miễn | Một dấu cách |
| Chứa Khác | 6 — Thân nhân sống cùng | 6 | Một dấu cách, trừ quy tắc nghề Thân nhân đưa về rỗng |
| Nội dung khác | 1 — Đã có | 4 — Nhà đầu tư đã có | Chính nội dung ô |

Thứ tự nhận: Miễn → Khác → còn lại. Nhà đầu tư phải có hộ chiếu trong bảng đầu tư **đúng công ty của dòng**. Tạo công ty mới không tự tạo đầu tư. Nghề Thân nhân nhóm 9 buộc GPLĐ = 6, số rỗng; GPLĐ = 6 còn có thể chọn nghề Thân nhân nhóm 9 đang hoạt động nếu có.

## Nhóm thăm thân — 7 trường

Các trường này lưu trên hồ sơ nhân viên, khác diện cư trú mở rộng FAMILY_VISIT. Luồng Excel không buộc nhập tên người thân chỉ vì bật cờ thăm thân; không lấy điều kiện form nhập tay áp sang Excel.

### `FamilyVisit` — Thăm thân (Có/Không)

- **Ý nghĩa, kiểu Excel:** cờ thăm thân hồ sơ lõi; Text. **Tùy chọn.**
- **Chương trình kiểm tra:** chuyển chữ thường: `có`, `co`, `1`, `yes`, `x` → 1; nội dung khác → 0. Không có kiểm tra tập giá trị khác ngoài quy tắc chuyển này.
- **Không ghép:** mới = 0, cập nhật giữ cờ cũ. **Ô trống:** = 0, có thể tắt cờ khi cập nhật, không tự xóa các cột người thân khác.
- **Ví dụ đúng:** `Có`, `Không`. **Dễ hiểu sai:** `true` thành 0, không báo lỗi.
- **Liên quan, lỗi/cách sửa:** không có lỗi riêng cho từ không nhận; sửa Có/Không, xem lại. Cờ không tự tạo diện cư trú hoặc chi tiết thăm thân V010.
- **Khuyến nghị nhập liệu:** dùng Có/Không nhất quán; nhập thông tin người thân khi thực tế có dù parser chưa bắt buộc.

### `FamilyVisitRelativeName` — Tên người thân

- **Ý nghĩa, kiểu Excel:** tên người thân liên quan thăm thân; Text. **Tùy chọn**, không ép bắt buộc theo cờ trong Excel.
- **Chương trình kiểm tra:** nhận chuỗi ô đã bỏ khoảng trắng đầu/cuối, không tra danh mục tên.
- **Không ghép:** mới chưa có, cập nhật giữ tên cũ. **Ô trống:** ghi rỗng trên phiên bản cập nhật.
- **Ví dụ đúng:** `NGUYEN VAN A` cho người thân giả. **Dễ hiểu sai:** tên công ty ghép nhầm cũng có thể được nhận.
- **Liên quan, lỗi/cách sửa:** không có lỗi xác minh tên; rà ghép và nguồn. Không tự nối sang hồ sơ người thân V010.
- **Khuyến nghị nhập liệu:** dùng tên người thân đúng nguồn, đối chiếu cùng giấy tờ người đó.

### `FamilyVisitRelationship` — Quan hệ người thân

- **Ý nghĩa, kiểu Excel:** mô tả quan hệ; Text. **Tùy chọn.**
- **Chương trình kiểm tra:** nhận chuỗi, không ép danh mục mã quan hệ trong parser Excel.
- **Không ghép:** mới chưa có, cập nhật giữ quan hệ cũ. **Ô trống:** ghi rỗng nếu chọn cập nhật.
- **Ví dụ đúng:** `Vợ/chồng` theo hồ sơ thật. **Dễ hiểu sai:** `1` nhận như chữ, không tự giải mã thành quan hệ.
- **Liên quan, lỗi/cách sửa:** không có lỗi mã quan hệ riêng; sửa nội dung/ghép nếu nhầm. Không thay cho Nghề nghiệp Thân nhân.
- **Khuyến nghị nhập liệu:** ghi rõ nghĩa, thống nhất cách ghi của đơn vị, không dùng mã tự quy ước chưa được giải thích.

### `FamilyVisitRelativeIdCard` — CMND/CCCD người thân

- **Ý nghĩa, kiểu Excel:** số giấy tờ đã có của người thân; **Text**. **Tùy chọn.**
- **Chương trình kiểm tra:** nhận chuỗi, không kiểm tra số chữ số hoặc hợp lệ pháp lý trong parser.
- **Không ghép:** mới chưa có, cập nhật giữ số cũ. **Ô trống:** ghi rỗng khi cập nhật, không phát sinh số thay thế.
- **Ví dụ đúng về kiểu:** `001234567890` cho dữ liệu giả. **Dễ sai:** Number làm mất `00`, chương trình không phục hồi.
- **Liên quan, lỗi/cách sửa:** không có lỗi mẫu số riêng; đối chiếu giấy tờ người thân, tránh ghép vào định danh điện tử của người được quản lý.
- **Khuyến nghị nhập liệu:** đặt Text trước khi dán, chỉ nhập số trong nguồn được giao.

### `FamilyVisitStartDate` — Ngày bắt đầu thăm thân

- **Ý nghĩa, kiểu Excel:** ngày bắt đầu đợt thăm thân; Date hoặc Text ngày mở rộng. **Tùy chọn.**
- **Chương trình kiểm tra:** không trống phải đọc được ngày; nhận ngày/tháng/năm và ISO. Parser chưa kiểm tra ngày bắt đầu ≤ hạn thăm thân.
- **Không ghép:** mới null, cập nhật giữ ngày cũ. **Ô trống:** null, có thể xóa ngày cũ.
- **Ví dụ đúng:** `03/06/2026`, `2026-06-03`. **Sai:** `31/02/2026`.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc ngày bắt đầu thăm thân tại dòng N`; sửa ngày thật, đối chiếu Hạn thăm thân.
- **Khuyến nghị nhập liệu:** dùng năm bốn chữ số và tự kiểm tra thứ tự hai ngày.

### `FamilyVisitEndDate` — Hạn thăm thân

- **Ý nghĩa, kiểu Excel:** ngày kết thúc thăm thân; Date hoặc Text ngày mở rộng. **Tùy chọn.**
- **Chương trình kiểm tra:** không trống phải là ngày hợp lệ; không buộc khớp Hạn tạm trú, không kiểm tra thứ tự với ngày bắt đầu trong parser.
- **Không ghép:** mới null, cập nhật giữ ngày cũ. **Ô trống:** null, có thể xóa ngày cũ.
- **Ví dụ đúng về cú pháp:** `29/05/2028`, `2028-05-29`. **Sai:** `29/02/2027`; ngày trước ngày bắt đầu chưa bị chặn vì thứ tự.
- **Liên quan, lỗi/cách sửa:** `Lỗi đọc hạn thăm thân tại dòng N`; sửa theo nguồn. Không thay Hạn tạm trú bắt buộc.
- **Khuyến nghị nhập liệu:** đối chiếu hai hạn độc lập, không mặc nhiên sao chép cho nhau.

### `FamilyVisitNote` — Ghi chú thăm thân

- **Ý nghĩa, kiểu Excel:** ghi chú riêng thông tin thăm thân; Text. **Tùy chọn.**
- **Chương trình kiểm tra:** nhận nội dung ô, không kiểm tra nội dung nghiệp vụ riêng.
- **Không ghép:** mới chưa có, cập nhật giữ ghi chú cũ. **Ô trống:** ghi rỗng khi cập nhật.
- **Ví dụ đúng:** `Đã đối chiếu quan hệ`. **Dễ hiểu sai:** nội dung không thay Ghi chú lõi hoặc ngày thăm thân.
- **Liên quan, lỗi/cách sửa:** không có lỗi nội dung riêng; rà đúng cột và nguồn nếu thông tin hiển thị sai.
- **Khuyến nghị nhập liệu:** ghi ngắn, rõ, liên quan đợt thăm thân.

<a id="mo-rong-v010"></a>
## Nhóm diện cư trú và định danh — 4 trường

V010 là phần hồ sơ người, diện cư trú, giấy tờ, lịch sử lưu trú mở rộng. Trong 12 trường sau, **không ghép và ô trống không xóa bản ghi mở rộng cũ**, kể cả cập nhật. Có giá trị mở rộng có thể tạo/liên kết hồ sơ người, nhưng chỉ tổ hợp nêu dưới đây mới tạo diện/giấy tờ/kỳ lưu trú. Lỗi khi ghi có thể hủy cả giao dịch.

### `StayPurposeCode` — Diện cư trú (WORK/STUDY/FAMILY_VISIT/TOURISM/OTHER)

- **Ý nghĩa, kiểu Excel:** mã diện cư trú chính; Text. **Tùy chọn**, có mã kích hoạt ghi diện.
- **Chương trình kiểm tra:** chuyển hoa, nhận đủ sáu mã `WORK`, `FAMILY_VISIT`, `STUDY`, `TOURISM`, `BORDER_VISA_EXEMPT`, `OTHER`. Nhãn ghép cột chưa liệt kê BORDER_VISA_EXEMPT nhưng mã xử lý có hỗ trợ.
- **Không ghép / ô trống:** không tạo/sửa diện, không xóa diện cũ. Có mã thì ngày từ mặc định ngày tạo phiên bản nhân viên (ngày import), ngày đến mặc định null.
- **Ví dụ đúng có điều kiện:** `WORK` với khoảng không xung đột hồ sơ hiện có. **Sai:** `LAO_DONG`, hoặc diện chính khác chồng lấn khoảng đã có.
- **Liên quan, lỗi/cách sửa:** `Diện cư trú không hợp lệ`, `Khoảng hiệu lực diện cư trú không hợp lệ`, `Diện cư trú import chồng lấn một diện chính khác`, hoặc yêu cầu chỉnh hồ sơ khi cùng diện nhưng thời hạn khác. Kiểm tra mã/ngày/diện hiện có; lỗi có thể khi xác nhận ghi.
- **Khuyến nghị nhập liệu:** điền rõ khoảng thật thay vì dựa ngày import; FAMILY_VISIT không tự tạo chi tiết người thân và không thay cờ Thăm thân.

### `StayValidFrom` — Diện cư trú từ ngày

- **Ý nghĩa, kiểu Excel:** ngày bắt đầu diện, đồng thời làm ngày từ cho định danh mới nếu có số định danh; Date hoặc Text ngày mở rộng. **Chỉ có tác dụng nghiệp vụ với diện hoặc định danh tương ứng.**
- **Chương trình kiểm tra:** đọc ngày khi nhánh tương ứng chạy; với diện, ngày đến không được trước ngày từ. Không có cột ngày từ định danh riêng trong danh sách import.
- **Không ghép / ô trống:** không xóa ngày cũ; bản ghi mới dùng ngày tạo phiên bản nhân viên (ngày import) khi thiếu ngày này.
- **Ví dụ đúng:** `2026-06-03` cùng WORK hoặc số định danh đã có từ nguồn. **Sai:** `31/02/2026` khi được xử lý; ngày một mình không đủ tạo diện.
- **Liên quan, lỗi/cách sửa:** `Ngày không hợp lệ`, lỗi khoảng diện hoặc khác thời hạn đã có; sửa ngày/điền trường chủ hoặc chỉnh hồ sơ. Định danh đã trùng không được cập nhật ngày bằng cột này.
- **Khuyến nghị nhập liệu:** lấy ngày thật từ nguồn; không dùng cùng ngày cho diện và định danh nếu hai ý nghĩa thực tế khác nhau.

### `StayValidTo` — Diện cư trú đến ngày

- **Ý nghĩa, kiểu Excel:** ngày hết hiệu lực diện; Date hoặc Text ngày mở rộng. **Chỉ ghi diện khi có StayPurposeCode.**
- **Chương trình kiểm tra:** khi ghi diện, ngày hợp lệ, không trước ngày từ; có thể lỗi chồng lấn hoặc khác khoảng đã có.
- **Không ghép / ô trống:** không xóa ngày cũ; diện mới có hạn null (không chỉ định), không tự lấy Hạn tạm trú.
- **Ví dụ đúng:** `29/05/2028` với diện/ngày từ phù hợp. **Sai:** `02/06/2026` khi ngày từ `03/06/2026`; chỉ nhập ngày đến không tạo diện.
- **Liên quan, lỗi/cách sửa:** `Khoảng hiệu lực diện cư trú không hợp lệ` hoặc yêu cầu chỉnh hồ sơ trước; đối chiếu diện hiện hành. Không phải ngày hết hạn định danh.
- **Khuyến nghị nhập liệu:** điền hạn thật nếu biết; không dùng trống để yêu cầu xóa hạn cũ.

### `ElectronicIdentityNumber` — Số định danh điện tử

- **Ý nghĩa, kiểu Excel:** số định danh **đã được cấp ở nguồn có thẩm quyền**, ghi nhận vào ứng dụng; **Text**. **Tùy chọn.**
- **Chương trình kiểm tra:** nhận số cung cấp, tạo khóa tìm kiếm chuẩn hóa, kiểm tra đã có trên cùng người; đã có thì bỏ qua. Nhánh này không xác thực mẫu số hoặc quyền cấp của nguồn, không sinh số.
- **Không ghép / ô trống:** không tạo số, không xóa định danh cũ. Ngày từ định danh mới lấy StayValidFrom hoặc ngày tạo phiên bản nhân viên nếu thiếu.
- **Ví dụ đúng về kiểu:** `001234567890` là số giả; thực tế chỉ nhập khi nguồn đã có số. **Dễ hiểu sai:** `Tự cấp` có thể được nhận như chuỗi, không phải yêu cầu hệ thống cấp số.
- **Liên quan, lỗi/cách sửa:** ngày từ sai có thể gây `Ngày không hợp lệ` khi ghi định danh mới; sửa ngày và nguồn số. Số khác có thể thêm bản ghi khác, không phải lệnh thay số cũ.
- **Khuyến nghị nhập liệu:** giữ số 0 đầu, kiểm chứng đúng người; không đặt số để lấp ô hoặc dùng CCCD người thân.

## Nhóm giấy tờ mở rộng — 4 trường

<a id="danh-sach-giay-to"></a>
Số giấy tờ, loại và ngày đến có thể chứa nhiều phần, phân cách `;`, ghép theo vị trí. Bộ đọc bỏ phần rỗng: `VISA;;OTHER` thành hai loại, không giữ chỗ thứ hai. Thiếu loại ở một vị trí mặc định VISA; thiếu hạn đến mặc định null; phần loại/hạn thừa không tạo giấy tờ thêm. Chương trình không buộc ba danh sách bằng nhau. **Khuyến nghị:** danh sách không có lỗ và cùng số phần; nếu không biết hạn cho một số giấy tờ, tách lần nhập để tránh lệch vị trí.

### `DocumentTypeCode` — Loại giấy tờ (phân cách ; nếu nhiều)

- **Ý nghĩa, kiểu Excel:** loại từng giấy tờ mở rộng; Text, danh sách `;` nếu nhiều. **Chỉ có tác dụng khi có DocumentNumber.**
- **Chương trình kiểm tra:** loại được dùng chuyển hoa; nhận `VISA`, `VISA_EXTENSION`, `TEMPORARY_RESIDENCE_CARD`, `VISA_EXEMPTION`, `OTHER`.
- **Không ghép / ô trống:** không xóa loại cũ; số mới thiếu loại ở vị trí nào thì mặc định VISA ở đó.
- **Ví dụ đúng:** `VISA;TEMPORARY_RESIDENCE_CARD` cùng hai số tương ứng. **Sai:** `TTT`; `VISA;;OTHER` dễ lệch loại, không phải ba vị trí.
- **Liên quan, lỗi/cách sửa:** `Loại giấy tờ không hợp lệ: ...` khi ghi; sửa mã và vị trí cùng số/hạn. Có loại nhưng thiếu số không tạo giấy tờ, có thể không báo lỗi.
- **Khuyến nghị nhập liệu:** ghi rõ loại cho từng số, không dùng nhãn tiếng Việt hoặc ký hiệu Visa thay mã.

### `DocumentNumber` — Số thị thực/gia hạn/thẻ (phân cách ;)

- **Ý nghĩa, kiểu Excel:** số giấy tờ đã có cần ghi vào hồ sơ mở rộng; **Text**, danh sách `;` nếu nhiều. **Tùy chọn**, có số kích hoạt nhánh giấy tờ.
- **Chương trình kiểm tra:** tách phần không rỗng, bỏ khoảng trắng từng phần; tra cùng người/loại/số. Không có kiểm tra mẫu pháp lý số trong nhánh này.
- **Không ghép / ô trống:** không thêm giấy tờ, không xóa số cũ; loại/ngày đơn độc không thay số. Ô chỉ có `;` không có số hữu ích sau tách.
- **Ví dụ đúng về cách ghép:** `V-001;T-001` với `VISA;TEMPORARY_RESIDENCE_CARD`. **Dễ sai:** dấu phẩy bị coi là một phần của một số; ghép vào đây thay Số Visa lõi thì lõi vẫn thiếu.
- **Liên quan, lỗi/cách sửa:** cùng loại/số nhưng ngày cung cấp khác thì lỗi `Giấy tờ đã có thời hạn khác. Hãy chỉnh sửa giấy tờ trong hồ sơ trước khi import lại.`; kiểm tra hồ sơ, không đổi số giả để né lỗi.
- **Khuyến nghị nhập liệu:** giữ số 0 đầu, thứ tự danh sách, số đúng nguồn; không coi đây là thao tác cấp/gia hạn giấy tờ.

### `DocumentValidFrom` — Giấy tờ hiệu lực từ

- **Ý nghĩa, kiểu Excel:** **một ngày chung cho tất cả giấy tờ trong dòng**; Date hoặc Text ngày mở rộng. **Chỉ ghi với DocumentNumber.**
- **Chương trình kiểm tra:** đọc ngày khi nhánh giấy tờ chạy, không nhận danh sách ngày từ. Nhánh ghi import chưa kiểm tra ngày đến ≥ ngày từ cho giấy tờ mới.
- **Không ghép / ô trống:** không xóa ngày cũ; giấy tờ mới dùng ngày tạo phiên bản nhân viên (ngày import). Giấy tờ đã có không bị đổi ngày từ nếu không cung cấp.
- **Ví dụ đúng:** `03/06/2026` cùng một/nhiều số có cùng ngày từ. **Sai:** `03/06/2026;04/06/2026` không phải cú pháp nhiều ngày từ.
- **Liên quan, lỗi/cách sửa:** `Ngày không hợp lệ` hoặc giấy tờ đã có thời hạn khác; sửa ngày, tách lần nhập nếu các giấy tờ có ngày từ khác nhau; rà hồ sơ trước khi cập nhật.
- **Khuyến nghị nhập liệu:** nhập rõ ngày thật; tự kiểm tra thứ tự hiệu lực vì nhánh này không chặn mọi khoảng đảo ngược.

### `DocumentValidTo` — Giấy tờ hiệu lực đến (phân cách ;)

- **Ý nghĩa, kiểu Excel:** ngày hết hiệu lực từng giấy tờ; Date cho một ngày hoặc Text danh sách ngày mở rộng `;`. **Chỉ có tác dụng với DocumentNumber.**
- **Chương trình kiểm tra:** đọc ngày ở vị trí số tương ứng; bỏ phần rỗng nên có thể lệch. Nhánh ghi import chưa kiểm tra thứ tự ngày đến/ngày từ cho giấy tờ mới.
- **Không ghép / ô trống:** không xóa hạn cũ; giấy tờ mới mặc định null. Danh sách ngắn hơn số thì số phía sau hạn null; giấy tờ đã có giữ hạn nếu không có phần tương ứng.
- **Ví dụ đúng:** `29/05/2028;30/05/2028` với hai số/loại đúng thứ tự. **Sai:** `31/02/2028`; `;29/05/2028` áp ngày cho số thứ nhất, không giữ chỗ.
- **Liên quan, lỗi/cách sửa:** `Ngày không hợp lệ` hoặc giấy tờ đã có thời hạn khác; kiểm tra vị trí, chỉnh trong hồ sơ khi cần đổi hạn. Không tự đồng bộ Hạn tạm trú lõi.
- **Khuyến nghị nhập liệu:** không để lỗ giữa các phần, đối chiếu ngày từ/đến; tách lần nhập nếu danh sách khó biểu diễn đúng.

## Nhóm địa bàn và lịch sử lưu trú — 4 trường

Chỉ ghi kỳ lưu trú khi **cả Mã xã/phường/đặc khu và Địa chỉ lưu trú đều có nội dung**. Thiếu một bên có thể không báo lỗi nhưng không tạo kỳ. Danh mục AdministrativeUnits này khác kiểm tra đuôi Address lõi theo Wards.

### `AdministrativeUnitCode` — Mã xã/phường/đặc khu

- **Ý nghĩa, kiểu Excel:** mã địa bàn kỳ lưu trú mở rộng; **Text**, giữ số 0 đầu. **Cần cùng ResidenceAddress để ghi kỳ.**
- **Chương trình kiểm tra:** khớp mã trong AdministrativeUnits chưa bị xóa; nhánh tra mã không kiểm tra khoảng ngày hiệu lực danh mục. Không suy mã từ tên trong Address.
- **Không ghép / ô trống:** không xóa địa bàn cũ; thiếu mã thì không tạo kỳ dù có địa chỉ mới.
- **Ví dụ đúng có điều kiện:** mã chép nguyên vẹn từ danh mục đích, `00123` chỉ hợp lệ nếu thực sự có. **Sai:** dùng tên `Hải Châu` thay mã, mã khác bộ danh mục.
- **Liên quan, lỗi/cách sửa:** `Không tìm thấy mã địa bàn: ...` khi ghi đủ cặp; tra mã thật. Có mã không miễn kiểm tra Địa chỉ lõi.
- **Khuyến nghị nhập liệu:** đối chiếu mã/tên với người quản lý danh mục; không bịa mã hoặc coi Wards và AdministrativeUnits là cùng một bộ.

### `ResidenceAddress` — Địa chỉ lưu trú

- **Ý nghĩa, kiểu Excel:** địa chỉ cụ thể của kỳ lưu trú mở rộng; Text. **Cần cùng AdministrativeUnitCode.**
- **Chương trình kiểm tra:** nhận chuỗi không trống để ghi cùng mã; không áp kiểm tra đuôi phường/xã của Address lõi tại nhánh này.
- **Không ghép / ô trống:** không xóa kỳ cũ; chỉ có mã không có địa chỉ thì không tạo kỳ.
- **Ví dụ đúng có điều kiện:** `12 Lê Duẩn` với mã địa bàn thật tương ứng. **Dễ hiểu sai:** chỉ nhập ở đây vẫn không thay Địa chỉ bắt buộc.
- **Liên quan, lỗi/cách sửa:** lỗi mã hoặc khoảng ngày khi cặp được xử lý. Đã có cùng người/địa bàn/địa chỉ/ngày từ thì bỏ qua, **kể cả hạn đến mới khác**; không dùng import để chắc chắn sửa hạn kỳ đó.
- **Khuyến nghị nhập liệu:** ghi địa chỉ đúng mã; nếu cần sửa kỳ cũ, kiểm tra và chỉnh trong hồ sơ thay vì mong nhập lại sẽ ghi đè.

### `ResidenceValidFrom` — Lưu trú từ ngày

- **Ý nghĩa, kiểu Excel:** ngày bắt đầu kỳ lưu trú tại địa chỉ; Date hoặc Text ngày mở rộng. **Chỉ ghi với đủ mã/địa chỉ lưu trú.**
- **Chương trình kiểm tra:** khi nhánh chạy phải đọc được ngày, ngày đến không trước ngày từ. Ngày từ tham gia nhận diện kỳ đã có.
- **Không ghép / ô trống:** không xóa ngày cũ; kỳ mới dùng ngày tạo phiên bản nhân viên (ngày import).
- **Ví dụ đúng:** `2026-06-03` với đủ cặp. **Sai:** `31/02/2026`; ngày một mình không tạo kỳ lưu trú.
- **Liên quan, lỗi/cách sửa:** `Ngày không hợp lệ` hoặc `Khoảng lịch sử lưu trú không hợp lệ`; sửa ngày và đối chiếu hạn đến. Đổi ngày từ có thể tạo kỳ khác thay vì sửa kỳ cũ.
- **Khuyến nghị nhập liệu:** nhập ngày bắt đầu thật, tránh dựa mặc định khi nhập dữ liệu quá khứ.

### `ResidenceValidTo` — Lưu trú đến ngày

- **Ý nghĩa, kiểu Excel:** ngày kết thúc kỳ lưu trú; Date hoặc Text ngày mở rộng. **Chỉ ghi với đủ mã/địa chỉ lưu trú.**
- **Chương trình kiểm tra:** ngày hợp lệ, không trước ngày từ khi tạo kỳ. Đã có cùng người/địa bàn/địa chỉ/ngày từ thì bỏ qua thay vì cập nhật hạn đến.
- **Không ghép / ô trống:** không xóa hạn cũ; kỳ mới hạn null, không tự lấy Hạn tạm trú hoặc hạn diện.
- **Ví dụ đúng:** `29/05/2028`, ngày từ `03/06/2026`, đủ cặp. **Sai:** `02/06/2026` trong trường hợp đó; đổi hạn kỳ cũ có thể không được ghi dù xem trước báo thay đổi.
- **Liên quan, lỗi/cách sửa:** `Khoảng lịch sử lưu trú không hợp lệ` hoặc `Ngày không hợp lệ`; sửa nguồn. Muốn sửa hạn kỳ cũ thì kiểm tra và chỉnh tại hồ sơ phù hợp.
- **Khuyến nghị nhập liệu:** phân biệt thêm kỳ với sửa kỳ; trạng thái thay đổi không chứng minh mọi cột đã được ghi.

## Nhóm công ty — 1 trường

### `CompanyName` — Công ty (CQ bảo lãnh)

- **Ý nghĩa, kiểu Excel:** tên công ty áp dụng cho dòng; Text. **Tùy chọn nếu có công ty mặc định; mọi dòng cần giải quyết được công ty.**
- **Chương trình kiểm tra:** tên không trống khớp đầy đủ tên công ty hoạt động, không phân biệt hoa/thường, bỏ khoảng trắng ngoài; không khớp một phần hoặc tự bỏ dấu. Chưa có chỉ được tạo nếu người dùng bật rõ cho phép tạo công ty; lúc ghi còn cần lĩnh vực hoạt động để gán mặc định.
- **Không ghép / ô trống:** dùng công ty mặc định; chưa chọn thì có thể tự dùng khi database chỉ có đúng một công ty hoạt động. Không giải quyết được thì lỗi, không tự lấy theo tên người.
- **Ví dụ đúng có điều kiện:** tên chép đầy đủ từ danh sách đích. **Sai:** viết tắt chưa có, ô trống khi không giải quyết được công ty mặc định.
- **Liên quan, lỗi/cách sửa:** công ty chưa có, thiếu tên/công ty mặc định hoặc chưa chọn công ty; chọn lại/sửa tên. Chỉ bật tạo mới nếu thật sự muốn thêm. Công ty ảnh hưởng khóa hộ chiếu/công ty và nhà đầu tư; đổi đích có thể tạo hồ sơ ở công ty khác.
- **Khuyến nghị nhập liệu:** ưu tiên công ty có sẵn, rà tên từng dòng tại xem trước; không bật tạo mới chỉ để né lỗi chính tả.

<a id="xem-truoc-giao-dich-hoan-tac"></a>
## Xem trước, ghi dữ liệu và hoàn tác

| Trạng thái | Ý nghĩa và hành động |
|---|---|
| Mới | Chưa có theo hộ chiếu/công ty, dự kiến thêm nếu giao dịch thành công. |
| Thông tin đã có sẵn | So sánh 13 trường lõi cùng phần mở rộng liên quan thấy không đổi; bỏ qua. |
| Nhân viên có thay đổi | Cùng hộ chiếu/công ty, khác nội dung so sánh; mặc định bỏ qua. Chỉ cập nhật khi đã đối chiếu. |
| Lỗi | Thiếu/sai khi đọc; xem thông báo với số dòng Excel thật. |

Cập nhật tạo phiên bản mới, ẩn phiên bản hiện hành cùng hộ chiếu/công ty, không ghi đè nội dung bản cũ. Phiên bản mới đang làm, không ẩn, ngày vào/nghỉ trống theo luồng import; không phải mọi trường database đều được nhập qua Excel.

Cảnh báo đang làm ở công ty khác không chặn dòng. Cảnh báo người còn hạn không có trong file không có nghĩa tự xóa hoặc đánh dấu họ nghỉ. Số liệu công ty tính lại sau import/hoàn tác, loại trừ hồ sơ ẩn, hết hạn hoặc đã nghỉ theo ngày.

**Phân biệt các loại lỗi:**

- Lỗi dòng đã biết từ xem trước: không ghi dòng lỗi; các dòng hợp lệ có thể được ghi. Kết quả thành công vẫn có thể có dòng lỗi, phải đọc cả bộ đếm và danh sách lỗi.
- Quốc tịch được kiểm tra lại trước ghi; danh mục thay đổi có thể dừng trước khi ghi. Hồ sơ thay đổi sau xem trước có thể làm giao dịch bị từ chối, yêu cầu xem trước lại.
- Kiểm tra phần mở rộng có thể chỉ chạy khi ghi hoặc khi so sánh hồ sơ đã có. Sai mã diện/giấy tờ/địa bàn, ngày, chồng lấn, thời hạn khác cũ hoặc lỗi lưu trữ/audit có thể **hoàn tác cả giao dịch**, kể cả dòng trước đã xử lý trong bộ nhớ. Không hứa mọi lỗi đều được cô lập theo dòng.
- Ghi thất bại trả kết quả thất bại, số thêm/cập nhật về 0. Mất xác nhận giao dịch có cơ chế kiểm tra mã phiên đã lưu để tránh nhập hai lần; kiểm tra lịch sử trước khi nhập lại bằng thao tác mới.

Mỗi lần import có mã phiên. Tài khoản có quyền có thể hoàn tác từ lịch sử: cập nhật phiên bản thì xóa phiên bản mới, hiện bản cũ; thêm mới thông thường thì ẩn nhân viên theo cơ chế lịch sử. Bản ghi mở rộng do phiên tạo được xử lý theo dữ liệu hoàn tác đã lưu.

Hoàn tác bị chặn khi hồ sơ, dữ liệu mở rộng hoặc công ty có thay đổi/phụ thuộc mới hơn. Hoàn tác các phiên liên quan từ mới về cũ; sửa thủ công cần người quản lý kiểm tra, không chắc chỉ đổi thứ tự là đủ. Không sửa database hoặc xóa dữ liệu để vượt chốt an toàn.

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

Trên database test chuẩn bị đúng, lần đầu mong đợi 12 hồ sơ mới và 15 dòng lỗi có chủ ý. Nhập lại cùng file có 12 dòng đã có sẵn, 15 dòng vẫn lỗi; sửa tên một hồ sơ để kiểm tra nhánh thay đổi và hoàn tác. Test tự động đối chiếu từng cột, số dòng, thông điệp, hồ sơ thân nhân/nhà đầu tư và phiên bản trên SQLite và SQL Server riêng biệt.

SHA-256 workbook golden: `D9A5C839E99AD48990684B475739C2C74CDCEF5E9E8E066A086C35F66E967576`. Bộ thử này không bao phủ toàn bộ 35 trường được hỗ trợ, đặc biệt các nhóm mở rộng.

## Nguồn tra cứu và giới hạn

Các đường dẫn dưới đây tương đối từ `IRM/`. Căn cứ là nguồn đọc/ghi và test, không lấy mọi điều kiện form nhập tay làm luật Excel:

- `IRM.Application/Features/ImportExport/Models/SystemFields.cs`: 35 khóa và tên ghép cột.
- `IRM.Application/Features/ImportExport/ImportMapping.cs`, `LegacyImportRules.cs` cùng thư mục: bắt buộc, parse, đối chiếu, danh sách và quy tắc tương thích.
- `IRM.Infrastructure/Excel/ExcelReaderHelper.cs`: ô ngày/số/chữ, sheet, dòng thật, cắt đuôi vùng.
- `IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs`, `SqlImportStore.Comparison.cs`: công ty, danh mục, trạng thái, giữ cột không ghép.
- `SqlImportStore.Execution.cs`, `SqlImportStore.ExtensionHistory.cs`, `SqlImportStore.Rollback.cs` cùng thư mục store: giao dịch, ghi mở rộng, kiểm tra và hoàn tác.
- `IRM.Core/Entities/ForeignPersons/StayPurposeCodes.cs`, `ImmigrationDocumentTypeCodes.cs`: tập mã thực nhận.
- `IRM.Tests/LegacyImportRulesTests.cs`, `LegacyImportLookupRulesTests.cs`, `LegacyExcelReaderTests.cs`, `LegacyImportEndToEndTests.cs`, `LegacyImportFinalReviewTests.cs`, `LegacyGoldenWorkbookTests.cs`, `NationalityImportTests.cs`: bằng chứng hành vi, không chứng nhận mọi dữ liệu thực đều hợp lệ.

Không bổ sung độ dài, regex hoặc chuẩn pháp lý không được luồng này kiểm tra. Lưu trữ vẫn có thể từ chối theo ràng buộc database; không có lỗi xem trước không đồng nghĩa đủ điều kiện ghi. Nếu mã, schema hoặc danh mục thay đổi, cần đối chiếu lại trước khi tập huấn.
