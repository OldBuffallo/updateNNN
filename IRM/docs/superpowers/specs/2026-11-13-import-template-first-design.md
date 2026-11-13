# Thiết kế import Excel: phương án A, từng bước

## Trạng thái và quyết định của người dùng

- Ngày: 13/11/2026. Nguồn trước thay đổi: `dbf6d74`.
- Người dùng yêu cầu bắt đầu bằng tải file mẫu, hướng dẫn nạp Excel và xem trước dữ liệu theo cách hiển thị của hồ sơ.
- Đã xem bản phác thảo tương tác và chọn **A — từng bước**, với yêu cầu: “Thực hiện A, từng bước 1”.
- Phương án bố cục đã được chọn. Sau khi nhận tài liệu này, người dùng yêu cầu “executing-plans thực hiện”; đây là xác nhận triển khai đợt đầu, theo kế hoạch `../plans/2026-11-13-import-preparation-step.md`.
- Triển khai tuần tự, bắt đầu từ bước chuẩn bị file. Không coi bản phác thảo là phần mềm đã hoàn thành.

## Mục đích

Giúp người dùng biết phải làm gì tiếp theo khi nhập hồ sơ từ Excel. Họ bắt đầu với một file mẫu đúng, chỉ thấy các thiết lập cần thiết tại thời điểm sử dụng và kiểm tra dữ liệu theo bố cục hồ sơ trước khi xác nhận lưu.

Thành công là người mới có thể tải mẫu, điền dữ liệu, chọn file, hiểu dòng lỗi và đối chiếu hồ sơ mà không phải hiểu “mapping”, số dòng tự dò hoặc cấu trúc dữ liệu nội bộ.

## Các cách đã cân nhắc

| Phương án | Cách sử dụng | Đánh đổi | Quyết định |
| --- | --- | --- | --- |
| A — từng bước | Mỗi thời điểm hiển thị một bước; có quay lại và tiến trình bốn bước | Cần chuyển màn hình khi đối chiếu giữa các bước | Người dùng đã chọn; giảm lượng thông tin phải đọc cùng lúc |
| B — cùng một trang | Tải mẫu, nạp file và xem trước trên một trang dài | Đối chiếu nhanh hơn nhưng nhiều thông tin xuất hiện cùng lúc | Không chọn |

Giữ Blazor Server, MudBlazor và Driver.js đã có; không thêm thư viện giao diện hoặc dịch vụ tour bên ngoài.

## Phạm vi và giới hạn

- Chỉ đổi ứng dụng web, route `/import`, file mẫu và các DTO trình bày cần cho xem trước.
- Giữ đủ 35 trường hiện tại; chín trường bắt buộc theo `IMPORT-EXCEL-SPEC.md`. Không mở rộng thêm nghiệp vụ import du học sinh hoặc toàn bộ trường của hồ sơ.
- Giữ khả năng đọc file khác mẫu/WPF qua nhánh nâng cao; không sửa ứng dụng WPF.
- Không thay khóa nhận diện hồ sơ, quy tắc phân tích giá trị, cơ chế cập nhật phiên bản, quyền, giao dịch import và điều kiện hoàn tác.
- Không tự sinh hoặc cấp số định danh, visa, giấy phép hoặc bất kỳ số giấy tờ chính thức nào.
- Không thay đổi database schema, phiên bản thư viện hoặc bộ cài đã bàn giao.
- Các quyết định bên dưới chưa được người dùng nêu trực tiếp là đề xuất thiết kế, không phải quy tắc nghiệp vụ đã được khách hàng xác nhận.

## Luồng chính

```text
1. Chuẩn bị file      2. Nạp file Excel     3. Xem trước hồ sơ       4. Kết quả
   Tải mẫu               Chọn .xlsx           Chọn từng người          Số đã lưu
   Hướng dẫn điền        Thông tin đối chiếu    Xem lỗi/thay đổi        Mở hồ sơ
   Tôi đã chuẩn bị →     Kiểm tra →            Xác nhận lưu →          Lịch sử
```

Mỗi bước chỉ hiển thị nội dung của bước đó. Bước 2 có nhánh ghép cột nâng cao, không thêm một bước chính làm thay đổi cách người dùng hiểu tiến trình. Lịch sử và nhập thủ công vẫn truy cập được nhưng là hành động phụ. Không dùng các dấu tiến trình như nút nhảy qua kiểm tra.

### Bước 1 — Chuẩn bị file

Đây là đợt triển khai đầu tiên.

```text
Nhập hồ sơ từ Excel                         Hướng dẫn | Lịch sử
Tải mẫu, điền thông tin và kiểm tra hồ sơ trước khi lưu.

[1 Chuẩn bị] ─ [2 Nạp file] ─ [3 Xem trước] ─ [4 Kết quả]

┌ Bắt đầu với file Excel mẫu ─────────┐  ┌ Trong file mẫu có gì? ────────┐
│ Mỗi dòng là một người.              │  │ Các cột bắt buộc đặt trước.   │
│ Giữ nguyên tên cột.                 │  │ Minh họa tiêu đề, không có    │
│ Đối chiếu danh mục đang sử dụng.     │  │ dữ liệu người thật.           │
│ [Tải file Excel mẫu]                │  │ Danh sách chín trường cần có. │
│ [Tra cứu cách điền từng trường]     │  │ Cảnh báo ô trống khi cập nhật.│
└────────────────────────────────────┘  └──────────────────────────────┘
                                            [Tôi đã chuẩn bị file →]
```

- CTA chính là **Tải file Excel mẫu**. Không đưa ô chọn file, sheet, template ghép cột hoặc vùng dòng lên màn hình đầu.
- Hiển thị chín trường bắt buộc: họ tên, số hộ chiếu, giới tính, ngày sinh, quốc tịch, địa chỉ, nghề nghiệp, số visa và hạn tạm trú.
- Có chỉ dẫn ngắn về một người mỗi dòng, giữ tên cột, ngày `dd/MM/yyyy`, Nam/Nữ và để số giấy tờ/mã ở dạng Text để giữ số 0 đầu.
- Nút tra cứu mở hướng dẫn 35 trường hiện có, dùng chung nội dung đã rà soát.
- Đề xuất cho phép **Tôi đã chuẩn bị file** mà không bắt tải lại mỗi lần. Màn hình vẫn yêu cầu chuẩn bị theo mẫu trước; hệ thống chỉ biết đã phát lệnh tải, không khẳng định file đã được lưu trên máy người dùng.
- Khi tải lỗi, giữ bước 1, báo lỗi có thể hiểu được và cho tải lại. Khi tải thành công không tự chuyển bước.
- Hai cột xếp dọc trên màn hình nhỏ; thứ tự đọc giữ nguyên, thao tác được bằng bàn phím, tiêu đề và trạng thái có ngữ nghĩa truy cập.

### File Excel mẫu thuộc bước 1

File mẫu hiện tại không đủ để làm điểm bắt đầu: chỉ có 19 cột; hướng dẫn một số trường bắt buộc đang ghi tùy chọn; ví dụ GPLĐ có thể mâu thuẫn với số GPLĐ. Đợt đầu phải sửa cả file tải xuống, không chỉ đổi nút trên UI.

Đề xuất workbook mới:

1. **Hồ sơ**: đủ 35 cột có tên duy nhất, chín cột bắt buộc đứng trước và được tô màu/chú thích; không chèn dữ liệu giả vào vùng sẽ import. Bắt đầu nhập tại dòng 2.
2. **Hướng dẫn**: một dòng cho từng trường, gồm tên cột, bắt buộc/điều kiện, kiểu dữ liệu, ví dụ, ý nghĩa ô trống, cách sửa lỗi và lưu ý đối chiếu danh mục. Ví dụ chỉ nằm ở sheet này.
3. **Danh mục tham chiếu**: quốc tịch đang hoạt động, nghề và nhóm nghề, phường/xã dùng cho địa chỉ lõi, đơn vị hành chính dùng cho mã cư trú, công ty đang có và mã diện cư trú/loại giấy tờ được luồng nhập hỗ trợ. Tách rõ phường/xã lõi với mã đơn vị hành chính. Chỉ cung cấp mã/tên cần đối chiếu, không xuất dữ liệu hồ sơ người.

Danh mục lấy từ database đích khi tải; không dùng danh sách quốc gia ISO hoặc nghề giả cố định thay dữ liệu thực. Nếu danh mục rỗng thì sheet ghi rõ chưa có; không tạo danh mục hoặc thay mã thay người dùng. Ghi thời điểm xuất và nhắc rằng đây là bản tham chiếu, cần kiểm tra lại nếu danh mục đã đổi.

File không có macro, công thức hoặc liên kết ngoài. Đặt định dạng Text cho mã/số giấy tờ; ngày có định dạng rõ ràng, không tự thay số giấy tờ. Các cột có mô tả phải có thể xem bằng bộ đọc Excel mà ứng dụng đang dùng, không phụ thuộc người dùng mở đúng một phần mềm Excel cụ thể.

Tên cột có thể dùng nguyên nhãn hiện tại hoặc một nhãn ổn định riêng, nhưng phải có kiểm chứng round-trip: toàn bộ 35 tiêu đề tự ghép về đúng 35 key khác nhau, không nhận nhầm GPLĐ/Số GPLĐ, Visa/Ký hiệu Visa, địa chỉ/lưu trú hoặc các ngày mở rộng. Không sửa parser nghiệp vụ để làm ví dụ sai trở thành đúng.

Không tự coi ô trống là “giữ dữ liệu cũ”: hướng dẫn giải thích khác biệt giữa bỏ cột/không ghép và ghép một cột có ô trống. Cảnh báo tại bước chuẩn bị và xem trước rằng một số ô trống có thể xóa hoặc đổi thông tin khi chọn cập nhật. Muốn không ghép một trường tùy chọn, người dùng có thể bỏ cột đó hoặc dùng nhánh ghép cột; không bỏ chín cột bắt buộc.

Nguồn nội dung phải dùng chung giữa hướng dẫn trên màn hình và workbook ở mức phù hợp với ranh giới Application/Infrastructure/Web; không sao chép rồi để hai bộ quy tắc mâu thuẫn. Không đưa UI component hoặc MudBlazor vào Infrastructure.

### Bước 2 — Nạp file Excel

- Vùng chọn/kéo thả `.xlsx`, giới hạn 100 MB hiện có; báo lỗi đọc file ngay tại bước này.
- Sau khi chọn hiển thị tên file, sheet dữ liệu và số dòng dữ liệu thực, không đếm sheet hướng dẫn hoặc hàng trống thành hồ sơ.
- Công ty dùng khi dòng không có công ty và nhóm nghề được trình bày với lời giải thích ngắn; không tự chọn công ty đầu tiên. Không bắt chọn công ty mặc định nếu mọi dòng đã phân giải được công ty theo cơ chế hiện có.
- Khi nhận đúng cấu trúc mẫu, tự ghép theo tiêu đề một cách xác định; không buộc người dùng đi qua bảng ghép cột. Không suy đoán dữ liệu từ tên file.
- Nếu thiếu trường bắt buộc, có tiêu đề trùng/không rõ hoặc dùng file khác mẫu, chỉ ra vấn đề và mở nhánh **Thiết lập nâng cao / Ghép cột**. Giữ sheet, dòng tiêu đề, phạm vi dòng, template và tạo công ty chủ động trong nhánh này.
- Không âm thầm nhận nhầm cột chỉ vì có từ khóa giống nhau. File mẫu đầy đủ và mẫu đã bỏ cột tùy chọn đều cần được xử lý rõ ràng.
- Đổi file, sheet, phạm vi đọc, ghép cột hoặc lựa chọn ảnh hưởng kết quả làm mất hiệu lực preview và quyết định cập nhật cũ. Có thể quay lại bước 1 và tải lại mẫu.

### Bước 3 — Xem trước theo hồ sơ

```text
Tất cả · Thêm mới · Có thay đổi · Cần sửa · Đã có sẵn
┌ Danh sách người ───────┐  ┌ Hồ sơ đang chọn ────────────────────────────┐
│ Dòng Excel / Họ tên   │  │ Tên, hộ chiếu, dòng nguồn, trạng thái       │
│ Hộ chiếu / Trạng thái │  │ Thông tin chung | Lao động | Thăm thân | XNC│
│ ...                   │  │ Nhãn trường → dữ liệu dự kiến               │
│                       │  │ Giá trị cũ → mới khi xác định có thay đổi   │
└───────────────────────┘  └────────────────────────────────────────────┘
[Chọn lại file]                     Tổng hợp lựa chọn · [Xác nhận lưu]
```

- Bên trái là danh sách có lọc trạng thái; bên phải là hồ sơ chỉ đọc. Trên màn hình nhỏ danh sách nằm trước phần chi tiết; không mất lựa chọn khi đổi tab.
- Dùng nhóm/nhãn tương ứng màn hình hồ sơ người nước ngoài: Thông tin chung & Cư trú, Lao động & Việc làm, Thăm thân, Giấy tờ XNC & Định danh ĐT. Chỉ hiển thị những phần import thực sự hỗ trợ; không hiển thị tab Du học sinh trống như thể file đã nạp dữ liệu đó.
- Thể hiện giới tính, ngày sinh, quốc tịch, hộ chiếu, visa/hạn tạm trú và các trường hỗ trợ khác, thay vì chỉ vài cột như hiện tại.
- Phân biệt giá trị trong file, giá trị đã chuẩn hóa sẽ lưu, trường không ghép và ô ghép nhưng trống. Không gộp địa chỉ lõi với địa chỉ cư trú bổ sung. Không hiển thị “sẽ giữ nguyên” nếu writer không có hành vi đó.
- Dòng lỗi có thể chưa có đối tượng đã phân tích: chỉ hiện dữ liệu nguồn đã đọc được; không dựng hồ sơ hoàn chỉnh từ giá trị mặc định. Báo dòng/cột và cách sửa khi bộ đọc cung cấp vị trí; nếu chỉ có lỗi cấp dòng thì hiển thị đúng cấp dòng, không đoán ô lỗi.
- Cách sửa chính là sửa file Excel rồi nạp lại, không thêm trình sửa bảng tính trong trình duyệt.
- Hồ sơ có thay đổi mặc định **Bỏ qua**, chỉ cập nhật khi người dùng chủ động chọn. Khi có dữ liệu cũ để so sánh, hiện cũ/mới theo trường, bao gồm thay đổi do ô trống. Cần đọc trạng thái cũ qua service/DTO có quyền, không bind entity theo dõi của EF lên UI.
- Phải cho biết đây là bản xem trước; một số kiểm tra phần mở rộng chỉ xảy ra khi ghi. Không gọi preview là bảo đảm import thành công; giữ kiểm tra dữ liệu đã thay đổi kể từ preview.
- Quy tắc xử lý dòng lỗi: giữ khả năng xử lý hiện có của engine. Mockup có mô phỏng chặn đến khi sửa hết lỗi; quy tắc này **không** được suy thành yêu cầu đổi nghiệp vụ. UI ghi rõ số dòng hợp lệ sẽ xử lý, số dòng lỗi/bỏ qua và chỉ cho lưu khi có thao tác hợp lệ. Không vô hiệu hóa toàn bộ phiên chỉ vì còn một dòng lỗi nếu engine hiện tại cho xử lý các dòng hợp lệ.
- Trước khi ghi, xác nhận rõ số thêm mới/cập nhật/bỏ qua/lỗi, công ty liên quan và tác động cập nhật. Tour không bấm xác nhận thay người dùng.

### Bước 4 — Kết quả

- Hiển thị kết quả thực tế từ engine, không dùng số dự kiến thay số đã ghi.
- Tách phiên bị hủy giao dịch với phiên đã ghi nhưng có dòng lỗi/bỏ qua; chỉ gọi thành công theo kết quả thật.
- Cho mở dữ liệu đã nhập khi có định danh kết quả đủ để lọc chính xác; không hứa dẫn đến danh sách “vừa nhập” bằng bộ lọc suy đoán. Nếu chưa có định danh, dùng liên kết tới danh sách hồ sơ hiện có và lời dẫn đúng nghĩa.
- Giữ lịch sử và hoàn tác với điều kiện hiện tại; không hứa hoàn tác mọi lúc.

## Hướng dẫn trong lúc thao tác

Giữ Driver.js cục bộ và nội dung có dấu. Bước chuẩn bị có bong bóng cho tải mẫu, cách điền và chuyển sang nạp file; các bước sau dùng đúng anchor mới. Người dùng tự mở/đóng hướng dẫn, có thể tiếp tục thao tác bình thường. Không để tour chờ một anchor bị ẩn trong nhánh nâng cao, không ghi dữ liệu hay gửi dữ liệu ra ngoài. Nếu JS/tour lỗi, luồng import vẫn hoạt động.

## Tổ chức thực hiện tuần tự

1. **Chuẩn bị file**: màn hình đầu + workbook đúng + liên kết hướng dẫn + tour cho bước đầu. Kết nối với luồng nạp/ghép/xem trước hiện tại để đợt đầu vẫn sử dụng được; ghi rõ xem trước dạng hồ sơ chưa thuộc đợt đầu.
2. **Nạp file**: nhận mẫu, tự ghép xác định, thu gọn thiết lập nâng cao, vô hiệu hóa preview cũ.
3. **Xem trước hồ sơ**: dữ liệu trình bày, các nhóm thông tin, lỗi và so sánh; xác nhận ghi.
4. **Kết quả và nghiệm thu toàn luồng**: liên kết hồ sơ/kết quả thật, tour toàn luồng, cập nhật hướng dẫn và kiểm thử hồi quy.

Đây là các đợt tích hợp của cùng một luồng, không phải bốn sản phẩm độc lập. Mỗi đợt có kiểm tra và commit riêng. Không thay bước chưa làm bằng mô phỏng trong ứng dụng thật.

## Ranh giới triển khai và kiểm chứng

- Web điều phối các bước bằng các component và code-behind hiện có. Application giữ hợp đồng dịch vụ/DTO; Infrastructure tạo workbook và đọc dữ liệu. Không truy cập DbContext từ UI.
- Có thể dùng thêm trạng thái chuẩn bị nội bộ thay vì đổi ý nghĩa các chỉ số đang dùng trong import/tour. Bốn bước trình bày và nhánh ghép cột phải được ánh xạ tường minh, có kiểm chứng chuyển/quay lại/reset.
- Không đưa dữ liệu người thật, số giấy tờ, tên file hoặc lỗi động vào HTML không mã hóa của tour.
- Kiểm thử file mẫu: đủ 35 trường, đủ chín bắt buộc, không có hồ sơ giả trong vùng import; ghép đúng từng trường; định dạng Text cho số/mã; hướng dẫn không mâu thuẫn; danh mục đúng database và không lộ dữ liệu hồ sơ.
- Kiểm thử UI: chuẩn bị là trạng thái đầu/reset, không bắt tải lại, lỗi download vẫn thao tác lại được, chỉ chuyển khi người dùng bấm, nhánh cũ vẫn sử dụng được trong đợt đầu, tour bước đầu chạy và dừng an toàn.
- Các đợt sau kiểm tra file khác mẫu, thiếu/trùng cột, đổi file sau preview, dòng lỗi thiếu dữ liệu đã parse, ô trống cập nhật, nhiều giấy tờ và ngày, danh sách lớn và màn hình nhỏ. Giữ kiểm thử giao dịch/hoàn tác hiện có.
- Chạy build Release, .NET tests và bộ Node browser tests phù hợp; SQL integration chỉ dùng database sinh tên `IRM_RefactorTest_*`. Báo rõ phần UI chỉ kiểm tra tự động và phần đã thử trực tiếp.
- Review staged diff và commit cục bộ; không push. File workbook nghiệm thu/log nằm dưới `BAN-GIAO/`, không commit dữ liệu chạy thật hoặc bộ cài.

## Phần giao người triển khai quyết định

Tên file/component/DTO cụ thể, tổ chức metadata dùng chung, kích thước cột workbook, cách nén danh sách khi dữ liệu nhiều, số mục mỗi trang và chi tiết CSS theo MudBlazor hiện có. Các quyết định này phải giữ các hành vi/giới hạn nêu trên và được ghi trong kế hoạch, không yêu cầu người dùng chọn thư viện hoặc cấu trúc code.

## Tài liệu nguồn

- [Đặc tả 35 trường](../../IMPORT-EXCEL-SPEC.md).
- [Hướng dẫn nhanh](../../HUONG-DAN-IMPORT-EXCEL-NHANH.md).
- [Bằng chứng hướng dẫn import hiện tại](../../IMPORT-GUIDANCE-VERIFICATION-2026-11-13.md).
- Bản phác thảo được chọn: `.superpowers/brainstorm/957-1794564632/content/import-template-profile-v1.html` tại Git root; là scratch bị ignore, không phải mã sản phẩm hoặc tài liệu bàn giao.
