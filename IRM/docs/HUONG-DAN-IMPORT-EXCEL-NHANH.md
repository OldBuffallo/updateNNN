# Hướng dẫn nhanh nhập dữ liệu từ Excel

Dành cho người chuẩn bị file và thực hiện import tại `/import`.
Tra quy tắc từng cột trong [Đặc tả 35 trường](IMPORT-EXCEL-SPEC.md).
Ứng dụng chỉ ghi nhận dữ liệu đã có; không cấp hoặc tự sinh số hộ chiếu, GPLĐ, giấy tờ, định danh.
Các quy tắc dưới đây mô tả chương trình; lời **khuyến nghị** là hướng dẫn chuẩn bị, không phải mọi điều đều đã có bộ kiểm tra.

Quy tắc đọc dữ liệu đối chiếu **13/11/2026**, theo mã nghiệp vụ `5c9808d1d213431d399ba51eb002e4844b9d69d5`; UI hướng dẫn tại chỗ cập nhật đến source `3573857`.
Áp dụng cho source tương ứng và danh mục database đích. Bộ cài R4/các gói cũ chưa được build lại từ source này, cần xác định đúng bản chạy trước khi tập huấn.
Tra [đặc tả và giới hạn áp dụng](IMPORT-EXCEL-SPEC.md), hoặc về [mục lục tài liệu](README.md).

**Hướng dẫn ngay trên màn hình:**

- Ở `/import`, chọn **Hướng dẫn từng bước** để mở bong bóng tại bước đang thao tác; dùng Tiếp/Quay lại để đọc, × hoặc Esc để đóng.
- Tour chỉ mở khi bạn chọn. Các bước chuyển sang ghép cột, xem trước và ghi dữ liệu vẫn cần bạn bấm nút thật của ứng dụng; tour không tự nhập hoặc hoàn tác.
- Tại Ghép cột, chọn **Tra cứu 35 trường** hoặc dấu ? cạnh trường đang ghép để xem kiểu dữ liệu, ví dụ, ô trống và cách sửa lỗi. Mở tra cứu sẽ dừng tour; có thể mở lại từ nút đầu trang.
- Thay file đã tải, reset hoặc chuyển nhập thủ công sẽ dừng tour. Nếu hướng dẫn chưa sẵn sàng, vẫn thao tác import bình thường và có thể mở lại sau.
- Tour dùng tài nguyên cục bộ. Kết quả kiểm chứng và các tình huống trình duyệt chưa nghiệm thu: [biên bản hướng dẫn UI](IMPORT-GUIDANCE-VERIFICATION-2026-11-13.md).

## 1. Chuẩn bị công ty và danh mục

Khi mở màn hình import, bắt đầu tại **Chuẩn bị file**. Chọn **Tải file Excel mẫu**, điền dữ liệu và bấm **Tôi đã chuẩn bị file** để sang phần nạp file. Nếu đã có file theo mẫu, có thể tiếp tục với bản đã điền.

File tải xuống gồm ba sheet. **Hồ sơ** có 35 cột và bắt đầu nhập từ dòng 2. Chín cột bắt buộc đặt trước, tô màu xanh. **Hướng dẫn** giải thích từng trường và **Danh mục tham chiếu** hướng dẫn cách đối chiếu trong ứng dụng, kèm các mã diện cư trú và loại giấy tờ được hỗ trợ. Khi điền file, tra cứu tên/mã qua màn hình danh mục theo quyền của bạn.

Các ví dụ nằm trong sheet hướng dẫn. Điền hồ sơ của bạn vào sheet **Hồ sơ**, mỗi người một dòng. Khi cập nhật hồ sơ đã có, đọc quy tắc ô trống vì một số trường có thể bị xóa hoặc đổi thông tin. Bỏ cột tùy chọn hoặc chọn **Bỏ qua** tại bước ghép cột nếu muốn bỏ ghép trường đó.

- Dùng tài khoản có quyền nhập dữ liệu; liên hệ quản trị viên nếu không truy cập được màn hình.
- Xác định công ty nhận dữ liệu và chọn **Công ty mặc định**.
- Nếu file có nhiều công ty, dùng cột **Công ty (CQ bảo lãnh)**; mỗi tên phải khớp đầy đủ công ty đang hoạt động.
- Ô Công ty trống dùng công ty mặc định. Vì vậy, hãy kiểm tra mặc định cả khi file đã có cột Công ty.
- Không bật **Cho phép tạo công ty chưa có từ cột Công ty** trừ khi thật sự cần thêm công ty mới và đã rà tên.
- Tên chưa có không được tự tạo khi tùy chọn tắt; sửa lỗi chính tả trước khi cân nhắc tạo mới.
- Chọn **Nhóm nghề (bắt buộc)** phù hợp với tên nghề trong file; nhóm này dùng cho cả lần đọc.
- Đối chiếu quốc tịch, nghề và tên phường/xã với danh mục **database đích**; bộ mã seed mới có thể khác dữ liệu kế thừa.
- Nếu nhập lịch sử lưu trú mở rộng, lấy thêm mã địa bàn từ danh mục địa bàn tương ứng, không suy mã từ tên phường.
- Khi nguồn thiếu danh mục thật sự, chuyển người quản lý danh mục xác minh; không thêm tùy tiện để hết lỗi.

## 2. Chuẩn bị file `.xlsx`

- Lưu file dạng `.xlsx`; đổi tên đuôi một file khác thành `.xlsx` không chuyển được định dạng.
- Đặt cột hộ chiếu, số GPLĐ, visa, CCCD người thân, định danh và mã địa bàn thành **Text trước khi nhập/dán**.
- Kiểm tra số 0 đầu. Nếu Excel đã làm mất số 0, phải sửa theo nguồn, đổi định dạng sau không phục hồi được.
- **Khuyến nghị:** dùng ngày `dd/MM/yyyy`, năm bốn chữ số; ví dụ `03/06/2026` là ngày 3 tháng 6.
- Ngày sinh, Hạn tạm trú và Ngày cấp thẻ không nhận chữ ISO `2026-06-03`; ô Date thật của Excel vẫn được đọc.
- Các ngày mở rộng nhận thêm ISO, nhưng dùng chung một cách ghi sẽ dễ rà hơn. Xem [quy tắc ngày](IMPORT-EXCEL-SPEC.md#ngay-thang).
- Dùng `Nam`/`Nữ` cho Giới tính. Chương trình nhận mọi chữ khác Nam thành Nữ, nên chữ sai có thể không báo lỗi.
- Bảng xem trước không hiển thị giới tính; rà file nguồn trước khi nhập và đối chiếu giới tính trong hồ sơ nhân viên sau khi nhập.
- Chưa có GPLĐ: để trống cả GPLĐ và Số GPLĐ. Miễn: ghi `Miễn` ở GPLĐ và để trống số riêng.
- Đã có GPLĐ: nhập số thật ở Số GPLĐ khi GPLĐ trống, hoặc cùng số ở hai cột; không dùng nhãn `Đã có` đi cùng số riêng.
- Không nhập `Chưa có` vào GPLĐ: chương trình có thể lưu chính chữ đó như một số giấy phép.
- **Khuyến nghị:** một dòng là một hồ sơ cần nhập, tránh lặp cùng hộ chiếu/công ty trong file và bỏ dòng tổng khỏi vùng nhập.

## 3. Chọn sheet, vùng dòng và ghép cột

1. Tải file lên màn hình import và chọn **Sheet** chứa dữ liệu người.
2. Nhập **Dòng tiêu đề** theo số dòng Excel; `0` nghĩa là tìm dòng đầu có dữ liệu.
3. Nhập **Dòng dữ liệu đầu**; `0` nghĩa là sau tiêu đề. Với file có nhiều dòng giới thiệu, nên nhập rõ số dòng.
4. Nhập **Dòng dữ liệu cuối**, hoặc để trống để đọc hết sheet; không chọn dòng cuối trước dòng đầu.
5. Tiếp tục đến màn hình ghép cột và rà từng cột, kể cả cột được tự ghép hoặc lấy từ mẫu đã lưu.
6. Chọn **Bỏ qua** cho STT và cột không cần nhập. Không ghép hai cột khác nghĩa vào cùng trường.
7. Kiểm tra đủ chín trường bắt buộc ở bảng dưới, rồi **Tiếp tục** để xem trước.

Số dòng trong thông báo lỗi là số dòng thật trong Excel, bắt đầu từ 1.
Dòng trống giữa vùng có thể báo lỗi; các dòng đuôi không có cả họ tên và hộ chiếu được bỏ, kể cả còn ghi chú.
Nếu sửa file trên máy sau khi đã tải lên, hãy tải lại bản đã lưu rồi ghép/xem trước lại.
Không chỉ sửa file và bấm xác nhận trên bản xem trước cũ.

## 4. Kiểm tra các trường bắt buộc và tùy chọn

Cả chín trường sau phải được ghép và có dữ liệu trên mỗi dòng, kể cả dòng cập nhật:

| Trường trên màn hình | Chuẩn bị và kiểm tra |
|---|---|
| Họ tên | Tên đúng theo hồ sơ, không chỉ có khoảng trắng. |
| Giới tính | Dùng Nam hoặc Nữ; kiểm tra đúng nghĩa. |
| Ngày sinh | Ngày hợp lệ, khuyến nghị `dd/MM/yyyy`. |
| Quốc tịch | Tên hoặc mã có trong danh mục đang hoạt động tại đích. |
| Số hộ chiếu | Số đã có, dạng Text, giữ số 0 đầu; đúng người/công ty. |
| Địa chỉ | Kết thúc bằng tên phường/xã có trong danh mục đích. |
| Nghề nghiệp/Chức danh | Đúng tên có dấu và đúng nhóm nghề đã chọn. |
| Số Visa | Số visa/thẻ tạm trú lõi đã có; không để trống dù có giấy tờ mở rộng. |
| Hạn tạm trú | Ngày hợp lệ, khớp hạn cuối trong Kết quả giải quyết nếu có. |

Các trường khác là tùy chọn hoặc cần cột đi cùng để ghi nhận:

- Kết quả giải quyết không phải ghi chú tự do. Ví dụ `TTT: DN1 03/06/2026-29/05/2028` cần Số Visa `DN1`, Hạn tạm trú `29/05/2028`.
- Nếu nhập Ngày cấp thẻ riêng trong ví dụ trên thì phải là `03/06/2026`.
- Mã diện cư trú cần đúng tập mã; ngày từ/đến của diện chỉ có tác dụng khi nhập diện tương ứng.
- Định danh chỉ nhập số đã có; ngày từ cho định danh mới dùng cột Diện cư trú từ ngày hoặc mặc định ngày import.
- Loại/ngày giấy tờ cần đi cùng Số thị thực/gia hạn/thẻ; không thay được Số Visa bắt buộc.
- Nhiều giấy tờ dùng `;` theo cùng thứ tự số/loại/hạn đến. Ngày từ là **một ngày chung**, không phải danh sách.
- **Khuyến nghị:** danh sách cùng số phần, không có phần rỗng giữa các dấu `;`; phần rỗng bị bỏ và có thể làm lệch vị trí.
- Lịch sử lưu trú cần cả Mã xã/phường/đặc khu và Địa chỉ lưu trú; thiếu một có thể không ghi kỳ lưu trú mà không báo lỗi.
- Không ghép cột và ghép nhưng để trống có thể khác nhau khi cập nhật. Đọc [quy tắc ô trống](IMPORT-EXCEL-SPEC.md#o-trong-va-cap-nhat) trước khi nhập lại hồ sơ cũ.

## 5. Đọc xem trước và quyết định cập nhật

| Trạng thái | Thao tác cần làm |
|---|---|
| Mới | Kiểm tra đúng người, công ty và dữ liệu; dự kiến được thêm nếu ghi thành công. |
| Thông tin đã có sẵn | Không khác nội dung được so sánh với bản hiện hành; bỏ qua. |
| Nhân viên có thay đổi | Đối chiếu bản cũ; mặc định bỏ qua. Chỉ chọn **Cập nhật** khi chấp nhận dữ liệu mới. |
| Lỗi | Đọc số dòng và nội dung, sửa nguồn/ghép cột rồi xem trước lại. |

- Khác một trường mở rộng cũng có thể thành dòng thay đổi, không chỉ khác trường bắt buộc.
- Chọn cập nhật tạo phiên bản mới và ẩn bản hiện hành cùng hộ chiếu/công ty; nội dung bản cũ được giữ.
- Phiên bản nhân viên mới ở trạng thái đang làm, ngày vào/nghỉ trống theo luồng import; đối chiếu trước khi dùng để cập nhật hồ sơ đã nghỉ.
- Cảnh báo đang làm ở công ty khác không chặn dòng; vẫn cần xác minh đúng công ty.
- Cảnh báo người còn hạn không có trong file không có nghĩa hệ thống sẽ tự xóa hay đánh dấu nghỉ người đó.
- Tên công ty kèm “Tạo mới khi xác nhận import” là công ty dự kiến thêm; rà từng tên trước khi tiếp tục.
- Xem trước chưa bảo đảm ghi thành công: một số mã/ngày/ràng buộc phần mở rộng chỉ kiểm tra khi ghi.

Checklist trước khi bấm **Bắt đầu Import**:

- [ ] Đúng file đã lưu, sheet, dòng tiêu đề và vùng dữ liệu.
- [ ] Đúng công ty mặc định, công ty từng dòng và nhóm nghề.
- [ ] Đủ chín trường bắt buộc, số nhận dạng còn nguyên số 0 đầu.
- [ ] Đã rà GPLĐ hai cột, số visa/kết quả và các cặp/danh sách tùy chọn.
- [ ] Đã sửa lỗi cần sửa, hiểu các cảnh báo và chủ động chọn các dòng cập nhật.

## 6. Sửa lỗi, xác nhận kết quả và hoàn tác

| Dấu hiệu hoặc thông báo | Nguyên nhân thường gặp và cách xử lý |
|---|---|
| Lỗi đọc ngày sinh/hạn/ngày cấp thẻ | Ngày sai hoặc dùng ISO ở cột lõi. Sửa ngày hợp lệ `dd/MM/yyyy`, không đảo tháng/ngày. |
| Quốc tịch chưa có | Mã/tên không thuộc danh mục đích đang hoạt động. Tra đúng danh mục và sửa, không tự đổi sang mã seed khác. |
| Nghề nghiệp chưa có / chưa chọn nhóm | Sai tên có dấu hoặc nhóm. Chọn đúng nhóm, chép đúng tên; không thêm nghề tùy tiện. |
| Địa chỉ không có phường/xã trong lưu trữ | Đuôi địa chỉ không khớp danh mục Wards. Kiểm tra phường/xã và phần chữ sau tên đó. |
| GPLĐ và Số GPLĐ không thống nhất | Nhãn miễn/đã có hoặc hai số mâu thuẫn. Đối chiếu và sửa theo quy tắc GPLĐ, không bỏ số thật để né lỗi. |
| Kết quả sai định dạng hoặc lệch thẻ/hạn | Chuỗi kết quả không đúng mẫu, số/hạn khác cột lõi. Đối chiếu cả ba theo cùng giấy tờ nguồn. |
| Đọc dòng tiêu đề như người / số dòng bất thường | Sai sheet hoặc vùng dòng. Quay lại chọn đúng tiêu đề/dòng đầu/dòng cuối và ghép lại. |
| Lỗi đọc tên/hộ chiếu hoặc thiếu trường | Thiếu dữ liệu, ghép sai hoặc dòng trống trong vùng. Sửa dữ liệu/ghép cột; không dùng trường mở rộng thay lõi. |
| Dữ liệu đã thay đổi sau xem trước | Có thay đổi hồ sơ/danh mục trong lúc chờ. Tạo xem trước mới, đối chiếu lại và chọn lại cập nhật. |
| Import thất bại dù xem trước không lỗi | Có thể lỗi phần mở rộng hoặc lưu trữ làm hủy cả giao dịch. Đọc lỗi, sửa mã/ngày/khoảng theo đặc tả; nhờ quản trị kiểm tra nếu cần. |

Khi chạy xong, đọc số **thêm mới, cập nhật, bỏ qua, lỗi** và kiểm tra lịch sử phiên.
Thành công có thể vẫn có dòng lỗi bị bỏ qua; không chỉ nhìn thông báo thành công.
Lỗi ghi có thể hoàn tác **toàn bộ giao dịch**, không chỉ dòng sai; số thêm/cập nhật trả về 0 khi thất bại.
Nếu mất kết nối ở bước cuối, kiểm tra lịch sử trước khi nhập lại bằng thao tác mới.
Đối chiếu một số hồ sơ thực tế, nhất là trường mở rộng; trạng thái thay đổi không chứng minh mọi giá trị đều đã được ghi.

Muốn hoàn tác, quản trị viên có quyền dùng **Lịch sử Import**, chọn đúng phiên.
Cập nhật phiên bản: xóa bản mới và hiện bản cũ; thêm mới thông thường: ẩn hồ sơ đã thêm.
Hoàn tác có thể bị chặn khi có sửa thủ công hoặc phiên/phụ thuộc mới hơn; xử lý các phiên liên quan từ mới về cũ.
Nếu vẫn bị chặn, chuyển người quản lý kiểm tra; không xóa dữ liệu để vượt chốt.
Xem [giới hạn ghi dữ liệu và hoàn tác](IMPORT-EXCEL-SPEC.md#xem-truoc-giao-dich-hoan-tac).

**Bài thực hành đã có, chỉ dùng trên database test chuẩn bị đúng:**

- File `BAN-GIAO/03-DU-LIEU-TEST/2026-11-12/IRM-IMPORT-WPF-GOLDEN-20261112.xlsx`.
- Cần CHN/Trung Quốc, Kỹ sư nhóm 1, Thân nhân nhóm 9, phường Hải Châu; INV-01…INV-04 đã nằm trong đầu tư đúng công ty test.
- Chọn sheet **Du lieu thu**, tiêu đề **8**, dữ liệu **9–35**, nhóm nghề **1**; không nhập sheet **Ket qua mong doi**.
- Lần đầu: 12 dòng hợp lệ được thêm và 15 lỗi có chủ ý. Nhập lại: 12 dòng đã có sẵn, 15 dòng vẫn lỗi.
- Tập sửa tên một hồ sơ, xem trước, chủ động cập nhật, rồi hoàn tác; không dùng dữ liệu giả này để nhập vào database nghiệp vụ.
- Quy tắc chi tiết và phạm vi bộ thử nằm trong [đặc tả](IMPORT-EXCEL-SPEC.md).
