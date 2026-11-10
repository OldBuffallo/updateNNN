# Thiết kế hợp nhất Hồ sơ Người nước ngoài và Hồ sơ Doanh nghiệp

Ngày: 10/11/2026. Trạng thái: **Chờ người dùng phê duyệt thiết kế**. Phạm vi: Hợp nhất giao diện quản lý hồ sơ người nước ngoài và hồ sơ doanh nghiệp theo Phương án 1, bảo toàn 100% tương thích database cũ và luồng import Excel.

## 1. Mục tiêu và lý do (Intent & Why)

Trong quá trình nâng cấp từ ứng dụng WPF cũ (`Employees`, `Company`) lên bản web v1.0.2 (bổ sung Định danh điện tử, Thăm thân, Du học sinh, FDI/Khu kinh tế, Cơ sở lưu trú), các tính năng mới đã được tách thành nhiều màn hình riêng lẻ trên thanh điều hướng (`Hồ sơ NNN hợp nhất`, `Nhân viên lao động`, `Du học sinh`, `Người thăm thân`, `Doanh nghiệp`, `Hồ sơ mở rộng`). Điều này làm người dùng bối rối vì:
- Một người nước ngoài (NNN) có thể vừa thuộc diện lao động, vừa có thông tin thăm thân, giấy tờ thị thực/thẻ tạm trú và định danh điện tử, nhưng các trường thông tin lại nằm rải rác ở 4 màn hình khác nhau.
- Một doanh nghiệp bị chia làm 2 màn hình (`Doanh nghiệp` cho thông tin cơ bản và `Hồ sơ mở rộng` cho FDI, MST, địa điểm KCN/KKT, người đại diện, hồ sơ pháp nhân scan).

**Mục tiêu cốt lõi**:
1. **Mỗi người nước ngoài 1 hồ sơ hợp nhất với đầy đủ các trường nhất có thể**: Gộp `Hồ sơ NNN hợp nhất`, `Nhân viên lao động`, `Du học sinh` và `Người thăm thân` vào một màn hình và một biểu mẫu (Dialog) đa năng duy nhất.
2. **Mỗi doanh nghiệp 1 hồ sơ duy nhất**: Tích hợp toàn bộ các tab của `Hồ sơ mở rộng (FDI/KKT)` vào ngay màn hình `Doanh nghiệp`.
3. **Tương thích tuyệt đối với database cũ và Import Excel**: Khi khách hàng sử dụng hoặc import từ database cũ (`ReportManagerDB` chỉ có bảng `Employees`, `Company`, mã quốc tịch biến độ dài, hồ sơ thiếu hộ chiếu hoặc chưa có bản ghi trong `ForeignPersons`), hoặc khi import file Excel theo mẫu cũ/mới, ứng dụng phải hiển thị, tìm kiếm, chỉnh sửa và lưu trữ mượt mà, không phát sinh lỗi khóa ngoại hay mất dữ liệu.

## 2. Mục tiêu, Phi mục tiêu và Chống mục tiêu (Goals, Non-Goals, Anti-Goals)

- **Goals**:
  - Rút gọn nhóm **HỒ SƠ & DỮ LIỆU** trên Sidebar từ 7 mục xuống còn **3 mục**:
    1. `🪪 Hồ sơ Người nước ngoài` (`/foreign-persons`)
    2. `🏢 Doanh nghiệp` (`/companies`)
    3. `🏠 Cơ sở lưu trú` (`/accommodations`)
  - Màn hình `Hồ sơ Người nước ngoài` hiển thị toàn bộ hồ sơ từ cả bảng mới (`ForeignPersons`) lẫn các bản ghi bảng cũ (`Employees`, `Students`) chưa được liên kết `ForeignPersonSourceLink`, kèm bộ lọc theo diện (`Tất cả`, `Lao động`, `Du học sinh`, `Thăm thân`, `Sắp hết hạn`), lọc theo Công ty, Quốc tịch, Địa bàn, GPLĐ, Xuất Excel và Xóa lưu trữ hết hạn.
  - Biểu mẫu Thêm/Sửa Hồ sơ NNN hợp nhất (chia Tab) cho phép nhập và chỉnh sửa tất cả các trường: Thông tin định danh & cư trú chung, Lao động (Công ty, Nghề nghiệp, GPLĐ, Hạn tạm trú), Du học (Trường, Ngành, Trình độ, Học bổng, Thời gian học, Trạng thái), Thăm thân (Thân nhân bảo lãnh, Quan hệ, Phân loại, CCCD/SĐT/Địa chỉ), Giấy tờ XNC (Visa, Ký hiệu Visa, Thẻ tạm trú) và Định danh điện tử (EID).
  - Màn hình `Doanh nghiệp` tích hợp cửa sổ/khung quản lý đầy đủ cả thông tin cơ bản (`Company`) và 6 tab mở rộng (`Phân loại & Pháp lý FDI/MST`, `Địa điểm & KKT`, `Cơ sở lưu trú đã thuê`, `Thân nhân NLĐ`, `Người đại diện pháp luật`, `Hồ sơ pháp nhân Scan`).
  - Giữ các route cũ (`/employees`, `/students`, `/family-visitors`, `/company-profiles`) hoạt động như các điểm vào hợp lệ (hiển thị màn hình hợp nhất với bộ lọc tương ứng) để không làm gãy link từ Dashboard, Spotlight Search, Kết quả Import Excel hoặc bộ test tự động.
- **Non-Goals**:
  - Không xóa hoặc đổi cấu trúc các bảng database hiện có (`Employees`, `Students`, `Companies`, `ForeignPersons`, `StayCases`...).
  - Không thay đổi định dạng file Excel import/export, logic rollback import hoặc công thức tính Báo cáo định kỳ / Thống kê 54 địa bàn.
- **Anti-Goals (Điều tuyệt đối tránh)**:
  - Không làm ẩn mất nhân viên/du học sinh từ database cũ chỉ vì hồ sơ cũ thiếu số hộ chiếu, trùng hộ chiếu hoặc chưa chạy backfill sang `ForeignPersons`.
  - Không ép buộc người dùng phải nhập các trường của diện Du học hay Thăm thân khi họ chỉ đang thêm/sửa một Nhân viên lao động (và ngược lại).
  - Không gây lỗi Foreign Key (quốc tịch, công ty, nghề nghiệp) khi mở và lưu lại hồ sơ được nạp từ database cũ.

## 3. Ràng buộc kỹ thuật (Constraints)

1. **Bảo toàn Baseline Schema & Kiến trúc**:
   - Giữ nguyên 43 entity và 2 migration trong `IRM.Infrastructure`; không tạo migration làm thay đổi DDL.
   - Tuân thủ hướng phụ thuộc: `IRM` (Web) → `IRM.Application` → `IRM.Core`, và `IRM.Infrastructure` → `IRM.Application` / `IRM.Core`.
2. **An toàn với dữ liệu kế thừa (Legacy Database Compatibility)**:
   - Bảng `Employees` trong DB cũ có `IDCompany` và `IDCareer` là bắt buộc (`int NOT NULL`), mã `Nationality` có thể dài bất kỳ (`USA`, `KOR`, `D`...) hoặc rỗng, và `Passport` có thể trống hoặc trùng.
   - Bảng `ForeignPersons` có thể đang trống (0 dòng) ngay sau khi khách hàng kết nối hoặc khôi phục database cũ, hoặc sau khi import file Excel chỉ chứa các cột cơ bản.
   - Mọi thao tác đọc danh sách hợp nhất phải hợp nhất (merge) dữ liệu từ `ForeignPersons`, `Employees` (đang hoạt động: `Hidden_flag == 0 && WorkingStatus == 0`) và `Students` (`Hidden_flag == 0`) thông qua `ForeignPersonSourceLinks` và chuẩn hóa số hộ chiếu (`PassportSearchKey`), đồng thời bao gồm cả các bản ghi `Employees`/`Students` chưa có liên kết.
3. **Phân quyền & Ngôn ngữ**:
   - Tuân thủ RBAC (`ADMIN`, `DATA_EDITOR` mới được thêm/sửa/xóa; chỉ `ADMIN` hoặc `DATA_EDITOR` theo quy định mới được xóa lưu trữ hết hạn).
   - Toàn bộ nhãn giao diện và thông báo lỗi sử dụng tiếng Việt có dấu đầy đủ.

## 4. Các phương án đã cân nhắc (Approaches)

1. **Phương án 1 (Đã chọn) — Hợp nhất thực chất thành "1 Hồ sơ NNN đa năng" và "1 Hồ sơ Doanh nghiệp duy nhất"**:
   - Rút gọn Sidebar mục Hồ sơ & Dữ liệu còn 3 mục (`Hồ sơ Người nước ngoài`, `Doanh nghiệp`, `Cơ sở lưu trú`).
   - Xây dựng mô hình đọc/ghi hợp nhất (Unified Profile DTO & Service/Store): đọc gộp từ cả bảng mới và bảng cũ; khi lưu từ một form đa năng duy nhất sẽ tự động đồng bộ xuống `ForeignPerson` (kèm `StayCase`, `ResidencePeriod`, `ImmigrationDocument`, `ElectronicIdentity`, `FamilyVisitDetail`) và bảng `Employees` / `Students` tương ứng.
   - Gộp các tab của `CompanyProfiles` vào ngay trong màn hình `Companies`.
   - *Đánh đổi*: Cần xây dựng DTO/Service tổng hợp và thiết kế lại Dialog đa tab cho NNN và Doanh nghiệp, nhưng giải quyết triệt để sự phân mảnh dữ liệu và đúng yêu cầu "mỗi người 1 hồ sơ với nhiều trường nhất có thể".
2. **Phương án 2 — Gom các màn hình cũ thành các Tab con trong cùng một trang**:
   - Chỉ ẩn menu trên Sidebar và nhúng nguyên xi 4 bảng/form cũ vào 4 tab ngang trong một trang.
   - *Lý do không chọn*: Bản chất dữ liệu và form nhập vẫn bị chia cắt; một người vừa là lao động vừa có thông tin thăm thân/định danh điện tử vẫn phải nhập ở nhiều nơi.

## 5. Thiết kế chi tiết (How It Works)

### 5.1. Điều hướng Sidebar & Route tương thích
- Trong `NavMenu.razor` (nhóm `HỒ SƠ & DỮ LIỆU`), hiển thị 3 mục:
  1. `🪪 Hồ sơ Người nước ngoài` (`/foreign-persons`)
  2. `🏢 Doanh nghiệp` (`/companies`)
  3. `🏠 Cơ sở lưu trú` (`/accommodations`)
- Các route `/employees`, `/students`, `/family-visitors` được giữ làm route phụ trên cùng màn hình hợp nhất (hoặc chuyển trạng thái tab lọc tương ứng: `Lao động`, `Du học sinh`, `Thăm thân`) để mọi liên kết từ Dashboard, Spotlight Search, Import Result và kịch bản kiểm thử trình duyệt (`theme-ui.cjs`) đều hoạt động không lỗi.
- Route `/company-profiles` hiển thị màn hình Doanh nghiệp hợp nhất (mở sẵn chế độ xem/chọn hồ sơ mở rộng hoặc tích hợp trực tiếp trên trang `/companies`).

### 5.2. Hợp nhất Hồ sơ Người nước ngoài (`UnifiedForeignPerson`)

#### A. Luồng đọc dữ liệu an toàn với DB cũ (Unified Read)
- Khi tải trang `Hồ sơ Người nước ngoài`, service đọc:
  1. Danh sách `ForeignPersons` (`!IsDeleted`) cùng `StayCases`, `FamilyVisitDetail`, `ResidencePeriods`, `ImmigrationDocuments`, `ElectronicIdentities`, `SourceLinks`.
  2. Danh sách `Employees` (`Hidden_flag == 0 && WorkingStatus == 0`) kèm `Company`, `Career`, `NationalityNav`.
  3. Danh sách `Students` (`Hidden_flag == 0`) kèm `NationalityNav`.
- **Quy tắc ghép (Merge Rules)**:
  - Ghép ưu tiên theo `ForeignPersonSourceLink` (`SourceType == "EMPLOYEE"` / `"STUDENT"`).
  - Nếu bản ghi `Employee` hoặc `Student` từ DB cũ chưa có `ForeignPersonSourceLink`, ghép tạm thời theo `PassportSearchKey` (nếu có hộ chiếu hợp lệ và khớp duy nhất).
  - Nếu vẫn chưa khớp (hoặc bản ghi DB cũ không có số hộ chiếu / trùng số hộ chiếu), vẫn tạo một dòng `UnifiedForeignPersonDto` đại diện trực tiếp từ bản ghi `Employee` / `Student` đó (với `EmployeeId` / `StudentId` gốc) để **không một hồ sơ nào từ DB cũ bị bỏ sót**.
- **Bộ lọc & Thao tác trên danh sách**:
  - Các chip/tab lọc nhanh theo diện: `Tất cả`, `Lao động`, `Du học sinh`, `Thăm thân`, `Sắp hết hạn` (tổng hợp cả sắp hết hạn tạm trú NLĐ, hạn visa DHS, hạn giấy tờ XNC).
  - Bộ lọc chi tiết: Tìm kiếm từ khóa (họ tên, hộ chiếu, công ty, trường học, thân nhân), Công ty, Quốc tịch (nạp động từ danh mục `CatalogService.GetNationalitiesAsync()`), Địa bàn xã/phường, Trạng thái GPLĐ.
  - Giữ nguyên các nút nghiệp vụ: **📥 Xuất Excel** (xuất theo diện đang lọc hoặc danh sách NLĐ/DHS), **🗑️ Xóa hết hạn tạm trú**, **🗑️ Xóa hết hạn thăm thân**, và **+ Thêm hồ sơ NNN**.

#### B. Biểu mẫu (Dialog) Hồ sơ NNN đa năng (5 Tab trong 1 cửa sổ)
Khi bấm **Thêm hồ sơ NNN** hoặc **Sửa** bất kỳ dòng nào (dù nguồn gốc từ DB cũ hay mới), mở một Dialog hợp nhất gồm 5 tab:
1. **Tab 1 — Thông tin cá nhân & Cư trú chung**:
   - Họ và tên (`*`), Giới tính, Ngày sinh, Quốc tịch (chọn từ danh mục `Nationality` đang hoạt động; nếu hồ sơ DB cũ có mã quốc tịch chưa nằm trong danh sách active thì vẫn hiển thị giữ nguyên mã đó để không mất dữ liệu), Số hộ chiếu.
   - Địa bàn xã/phường (`AdministrativeUnit`), Địa chỉ tạm trú cụ thể (`Address` / `ResidenceAddress`), Ghi chú chung.
   - Các nút bật/tắt (hoặc checkbox diện hồ sơ áp dụng): `[x] Diện Lao động`, `[ ] Diện Du học`, `[ ] Diện Thăm thân` (mặc định tự động bật dựa theo dữ liệu hiện có của hồ sơ hoặc tab đang đứng).
2. **Tab 2 — Lao động & Việc làm** *(hiển thị/kích hoạt khi có diện Lao động)*:
   - Công ty làm việc (`IDCompany`), Nghề nghiệp (`IDCareer`), Giấy phép lao động (`WorkPermit`: Đã có / Miễn / Chưa có), Số GPLĐ (`WorkPermitNumber`), Hạn tạm trú (`TemporaryStay`).
3. **Tab 3 — Du học sinh** *(hiển thị/kích hoạt khi có diện Du học)*:
   - Trường học (`SchoolName`), Ngành học (`Major`), Trình độ (`EducationLevel`), Loại học bổng (`ScholarshipType`), Ngày nhập học (`EnrollmentDate`), Ngày tốt nghiệp dự kiến (`GraduationDate`), Trạng thái học tập (`StudentStatus`), Số điện thoại (`PhoneNumber`).
4. **Tab 4 — Thăm thân** *(hiển thị/kích hoạt khi có diện Thăm thân)*:
   - Họ tên thân nhân bảo lãnh (`RelativeName`), Mối quan hệ (`Relationship`), Phân loại thân nhân (`RelativeTypeCode`: Công dân VN / Gốc VN / Việt kiều 2 quốc tịch), Quốc tịch thân nhân, Số CCCD/CMND/Hộ chiếu thân nhân, Số điện thoại thân nhân, Địa chỉ thường trú thân nhân, Thời hạn thăm thân (`ValidFrom` – `ValidTo`), Doanh nghiệp/NNN bảo lãnh (nếu có).
5. **Tab 5 — Giấy tờ XNC & Định danh điện tử**:
   - Thị thực / Thẻ tạm trú: Loại giấy tờ (`Visa`, `Gia hạn tạm trú`, `Thẻ tạm trú TRC`), Số Visa/Số thẻ (`VisaNumber` / `DocumentNumber`), Ký hiệu Visa (`VisaSymbol`), Ngày cấp / Từ ngày (`ValidFrom`), Hạn đến ngày (`ValidTo` / `VisaExpiry`).
   - Định danh điện tử: Mã số định danh điện tử (`ElectronicIdentityNumber`), Ngày kích hoạt (`ValidFrom`), Hạn hiệu lực (`ValidTo`).
   - Danh sách lịch sử giấy tờ XNC và định danh điện tử đã cấp của người đó (cho phép thêm mới ngay trong tab).

#### C. Luồng Lưu đồng bộ (Unified Write)
Khi người dùng bấm **Lưu hồ sơ**:
- Kiểm tra hợp lệ rõ ràng bằng tiếng Việt có dấu (Họ tên bắt buộc; mã quốc tịch phải tồn tại trong danh mục `Nationality`; nếu bật diện Lao động thì kiểm tra Công ty và Nghề nghiệp hợp lệ để không bao giờ văng lỗi FK `IDCompany`/`IDCareer`; nếu bật diện Thăm thân thì kiểm tra Họ tên thân nhân và Quan hệ).
- Trong cùng luồng xử lý an toàn:
  1. Tạo mới hoặc cập nhật `ForeignPerson` (họ tên, giới tính, ngày sinh, quốc tịch, hộ chiếu).
  2. Nếu có địa bàn + địa chỉ tạm trú: cập nhật/thêm `ResidencePeriod`.
  3. Nếu có thông tin Giấy tờ XNC hoặc Định danh điện tử: cập nhật/thêm `ImmigrationDocument` và `ElectronicIdentity`.
  4. **Nếu có diện Lao động** (hoặc hồ sơ gốc có `EmployeeId`):
     - Tạo mới hoặc cập nhật bản ghi trong bảng `Employees` (kèm thông tin thăm thân trên `Employee` nếu người đó vừa là lao động vừa có thông tin thăm thân), đảm bảo liên kết `ForeignPersonSourceLink (SourceType = "EMPLOYEE")` và `StayCase (PurposeCode = "WORK")`.
  5. **Nếu có diện Du học** (hoặc hồ sơ gốc có `StudentId`):
     - Tạo mới hoặc cập nhật bản ghi trong bảng `Students`, đảm bảo liên kết `ForeignPersonSourceLink (SourceType = "STUDENT")` và `StayCase (PurposeCode = "STUDY")`.
  6. **Nếu có diện Thăm thân**:
     - Tạo mới hoặc cập nhật `StayCase (PurposeCode = "FAMILY_VISIT")` kèm `FamilyVisitDetail`.
- **Khi Xóa hồ sơ trên màn hình hợp nhất**: Thực hiện xóa mềm đồng bộ trên tất cả các bản ghi liên kết của hồ sơ đó (`ForeignPerson.IsDeleted = true`, `StayCase.StatusCode = "DELETED"`, và nếu hồ sơ liên kết với `Employee` hoặc `Student` thì đặt `Hidden_flag = 1`) để hồ sơ đã xóa không xuất hiện lại từ bảng cũ.
- **Khi Import Excel tại `/import`**: Giữ nguyên hoàn toàn cơ chế ghi vào bảng `Employees` và backup/rollback hiện tại của `SqlImportStore`. Nhờ cơ chế Unified Read ở mục 5.2.A, mọi nhân viên vừa import từ file Excel mẫu cũ (kể cả chưa có bản ghi `ForeignPerson`) sẽ lập tức hiển thị đầy đủ trên màn hình `Hồ sơ Người nước ngoài`, và khi người dùng mở sửa/bổ sung thông tin sẽ tự động tạo liên kết `ForeignPersonSourceLink`.

### 5.3. Hợp nhất Hồ sơ Doanh nghiệp (`/companies`)
- Trên bảng danh sách `Doanh nghiệp` (`/companies`):
  - Giữ nguyên bảng danh sách công ty, tìm kiếm, lọc lĩnh vực, Xuất Excel, Import Excel và nút `+ Thêm công ty`.
  - Khi bấm **Xem chi tiết / Hồ sơ mở rộng** hoặc **Sửa công ty**, mở Dialog Doanh nghiệp hợp nhất có các Tab:
    - **Tab 1 — Thông tin chung & Liên hệ** (bảng `Company` cũ: Tên công ty, Loại hình, Lĩnh vực, Địa chỉ, Quận/Huyện/Thị xã, Xã/Phường, SĐT, Cán bộ theo dõi, Ghi chú, Thống kê số lượng lao động).
    - **Tab 2 — Phân loại FDI & Pháp lý** (`CompanyProfile`: Loại hình vốn Trong nước / FDI, Mã số thuế, Số ĐKKD).
    - **Tab 3 — Địa điểm & Khu kinh tế** (`CompanySite` & `EconomicZone`: Danh sách nhà xưởng/chi nhánh, gắn địa bàn xã/phường `AdministrativeUnit`, gắn KCN/KKT, tạo mới Khu/Cụm kinh tế).
    - **Tab 4 — Cơ sở lưu trú đã thuê** (`CompanyAccommodationAgreement`).
    - **Tab 5 — Người đại diện & Thân nhân NLĐ** (`CompanyRepresentative` & `SponsoredFamilyVisitors`).
    - **Tab 6 — Hồ sơ pháp nhân Scan** (`CompanyLegalDocument`: tải lên và tải về file PDF/ảnh scan ĐKKD, giấy phép).
- Công ty từ database cũ chưa có bản ghi `CompanyProfile` sẽ tự động được khởi tạo đối tượng mặc định khi mở các tab mở rộng, không gây lỗi null.

## 6. Phần để người triển khai quyết định (Left to the Builder)

- Cách tổ chức tách các partial class hoặc component con cho Dialog Hồ sơ NNN hợp nhất (`UnifiedForeignPersonDialog.razor`) và Dialog Hồ sơ Doanh nghiệp hợp nhất (`CompanyUnifiedDialog.razor`) để giữ mỗi file gọn gàng, dễ bảo trì.
- Thứ tự cột hiển thị mặc định trên lưới `Hồ sơ Người nước ngoài` sao cho cân đối khi đổi giữa các chip lọc (`Tất cả`, `Lao động`, `Du học sinh`, `Thăm thân`).
- Cách nối tham số query string hoặc route phụ (`/employees`, `/students`, `/family-visitors`, `/company-profiles`) để tự động chọn sẵn tab/bộ lọc tương ứng.
