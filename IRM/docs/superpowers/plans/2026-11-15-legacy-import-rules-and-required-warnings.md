# Kế Hoạch Triển Khai: Chuẩn Hóa Quy Tắc Import Theo Dự Án Cũ (Mã Quốc Tịch 3 Chữ Cái), Cảnh Báo Trường Bắt Buộc & Unit Test Đầy Đủ

> **Dành cho tác nhân thực thi (Agentic workers):** KỸ NĂNG BẮT BUỘC: Sử dụng `superpowers:subagent-driven-development` (khuyến nghị) hoặc `superpowers:executing-plans` để triển khai kế hoạch này theo từng Task. Các bước sử dụng cú pháp hộp kiểm (`- [ ]`) để theo dõi tiến độ.

**Mục tiêu (Goal):** Đồng bộ hoàn toàn quy tắc đọc/kiểm tra dữ liệu trong `IRM/IRM.Application/Features/ImportExport/LegacyImportRules.cs`, `ImportMapping.cs`, dữ liệu danh mục khởi tạo (`DatabaseSeeder.cs`, `00-full-setup.sql`) với dự án WPF cũ (`immigration-reportmanager-master/`), áp dụng chuẩn mã quốc tịch 3 chữ cái (`CHN`, `KOR`, `JPN`, `USA`...), bổ sung cảnh báo trực quan + báo lỗi từng ô cho mọi trường bắt buộc trên giao diện (Quản trị, Nhập Excel, Nhập thủ công, Nhân viên, Công ty, Du học sinh), và kiểm chứng bằng bộ Unit Test đầy đủ.

**Kiến trúc & Giải pháp (Architecture):**
1. **Quy tắc lõi (`LegacyImportRules.cs` & `ImportMapping.cs`)**: Tập trung kiểm tra định dạng mã quốc tịch 3 chữ cái (`IsValidNationalityCode`, `NormalizeNationalityCode`), tra cứu quốc tịch từ danh mục đang hoạt động `Delete_flag == 0` (`ResolveNationalityCode`, `ResolveNationality`), giới hạn năm hợp lệ `1900..2099` trong `TryParseToken`, nhận diện nghề `"Thân nhân"` của nhóm `"Khác"` (`IsRelativeCareer` + `.Include(c => c.CareerGroup)` trong `SqlImportStore.Preview.cs`), và so sánh 13 trường nghiệp vụ không dấu/không phân biệt hoa thường (`SameAs13Fields`).
2. **Dữ liệu khởi tạo & nâng cấp an toàn khóa ngoại (`DatabaseSeeder.cs`, `00-full-setup.sql`, `TestDatabase.cs`)**: Chuẩn hóa 34 quốc tịch khởi tạo sang mã 3 chữ cái, bổ sung nhóm nghề `"Khác"` + nghề `"Thân nhân"`, và bổ sung bước nâng cấp DB đã lỡ seed mã 2 chữ cái sang 3 chữ cái theo thứ tự an toàn khóa ngoại `AK_Nationality_NationalityCode`.
3. **Cảnh báo trường bắt buộc nhất quán trên toàn UI**: Hiển thị cảnh báo ở hai tầng: (a) bảng `MudAlert` tổng hợp các trường còn thiếu và (b) viền đỏ + chữ báo lỗi `ErrorText` trực tiếp dưới từng ô bắt buộc ngay khi bấm Lưu/Thêm hoặc khi nhập sai định dạng (mã quốc tịch khác 3 chữ cái), đồng thời chặn thao tác lưu khi còn thiếu bất kỳ trường bắt buộc nào.

**Công nghệ (Tech Stack):** .NET 8, ASP.NET Core Blazor Server, MudBlazor, Entity Framework Core (SQL Server / SQLite in-memory), ClosedXML, xUnit, bUnit.

**Tài liệu đối chiếu & Mockup (Spec & Mockups):**
- Dự án WPF gốc:
  - `immigration-reportmanager-master/ReportManager/ViewModels/ReadFileViewModel.cs` (dòng 380–605, 795–855, 931–996, 1074–1111)
  - `immigration-reportmanager-master/ReportManager/Bases/MethodHandler.cs` (`checkFormatDate`, `checkSettlementResultsString`, `getCareerID`)
  - `immigration-reportmanager-master/ReportManager/Bases/Constants.cs` (`WorkPermit*`, `OtherCareerGroup = "9"`)
  - `immigration-reportmanager-master/ReportManager/ViewModels/FunctionViewModels/AddViewModels/AddNationalityViewModel.cs` (quy ước mã quốc tịch 3 chữ cái)
- Mockup HTML trực quan (mở trực tiếp bằng trình duyệt): [`../specs/2026-11-15-required-field-warnings-mockup.html`](../specs/2026-11-15-required-field-warnings-mockup.html)

## Ràng Buộc Chung (Global Constraints)

- **Mã quốc tịch chuẩn 3 chữ cái:** Mọi mã quốc tịch tạo mới trong Quản trị hoặc seed mặc định phải gồm đúng 3 chữ cái ASCII (`^[A-Za-z]{3}$`), chuẩn hóa viết hoa (`CHN`, `KOR`, `JPN`, `USA`...).
- **Giữ nguyên thông báo lỗi dòng của WPF:** Các thông báo lỗi đọc dòng trong `ImportMapping.cs` và `LegacyImportRules.cs` giữ nguyên câu chữ tiếng Việt của dự án cũ (`"Lỗi đọc <trường> tại dòng <n>"`, `"Quốc tịch <x> chưa có trong dữ liệu. Vui lòng thêm quốc tịch tại dòng <n>"`, `"Địa chỉ không có phường/ xã trong lưu trữ. Vui lòng kiểm tra lại tại dòng <n>"`, `"Nghề nghiệp chưa có trong dữ liệu. Vui lòng thêm nghề nghiệp tại dòng <n>"`).
- **Tương thích khóa ngoại (`AK_Nationality_NationalityCode`):** Các bảng `Employees`, `Students`, `ForeignPersons` đều tham chiếu khóa ngoại tới `Nationality.NationalityCode`. Khi nâng cấp mã 2 chữ cái (`CN`, `KR`...) sang 3 chữ cái (`CHN`, `KOR`...), bắt buộc chèn bản ghi 3 chữ cái trước, cập nhật các bảng con, lưu DB rồi mới xóa bản ghi 2 chữ cái.
- **Không phụ thuộc Culture của máy chủ:** Mọi phép đọc ngày (`dd/MM/yyyy`, `dd-MM-yyyy`) và chuẩn hóa chuỗi không dấu phải hoạt động giống hệt nhau trên `vi-VN`, `en-US` và `InvariantCulture`.
- **Hiển thị tiếng Việt có dấu đầy đủ trên UI:** Không để sót chữ không dấu (`"Luu"`, `"Them"`, `"Da xoa..."`) trên các form Quản trị.

## Trọng Tâm Kiểm Tra (Review Focus)

1. **Năm 4 chữ số nằm ngoài khoảng `1900..2099` (VD: `"01/01/0001"`, `"01/01/1899"`, `"01/01/2100"`):** Không được áp dụng quy tắc cộng `2000` của năm 2 chữ số cho chuỗi `"0001"`; phải từ chối mọi năm ngoài `1900..2099` đúng như regex `(19|20)\d\d` của WPF.
2. **So sánh 13 trường nghiệp vụ (`SameAs13Fields`) khi khác hoa/thường, khác dấu hoặc chuỗi rỗng `" "` vs `""`:** Trong WPF, khi GPLĐ trống hoặc Miễn thì `WorkPermitNumber = " "`, còn `Note` trống là `""`; `SameAs13Fields` phải coi `null`, `""`, `" "` ở `WorkPermitNumber` và `Note` là bằng nhau, và so sánh không dấu/không phân biệt hoa thường cho các trường chuỗi.
3. **Nhận diện nghề `"Thân nhân"` khi nhóm nghề `"Khác"` không mang `IDCG = 9`:** Trong DB khởi tạo mới của IRM, nhóm `"Khác"` là nhóm thứ 5 (`IDCG = 5`), trong khi DB cũ cố định `IDCG = 9`. `IsRelativeCareer` và truy vấn EF trong `SqlImportStore.Preview.cs` phải nhận diện được cả `IDCG == 9` lẫn `CareerGroup.CareerGroupName == "Khác"`.
4. **Dòng Excel có `GPLĐ = "Khác"` và `Nghề nghiệp = "Thân nhân"` khi người dùng đang chọn nhóm nghề khác (VD: `IDCG = 1`):** Phải tự động khớp về nghề `"Thân nhân"` của nhóm `"Khác"` thay vì báo lỗi `"Nghề nghiệp chưa có trong dữ liệu"`.
5. **Bấm nút Thêm/Lưu khi người dùng chưa chạm vào các ô trống bắt buộc:** Trong MudBlazor, `Required="true"` chỉ kích hoạt `RequiredError` sau khi blur. Mọi form có trường bắt buộc (`Employees`, `Import` thủ công, `Admin` danh mục, `Companies`, `Students`) phải theo dõi cờ submit (hoặc kiểm tra giá trị khi bấm Lưu) để bật `Error="true"` + `ErrorText="..."` trực tiếp dưới từng ô bị thiếu và chặn gửi yêu cầu lưu lên server.

---

## Mockup Giao Diện (Visual Mockups)

- **Bản HTML trực quan đầy đủ (mở bằng trình duyệt):** `IRM/docs/superpowers/specs/2026-11-15-required-field-warnings-mockup.html`

### Mockup 1 — Quản trị → Danh mục Quốc tịch & Các danh mục bắt buộc
```text
+------------------------------------------------------------------------------------+
| [!] Cảnh báo: Mã quốc tịch phải gồm đúng 3 chữ cái (VD: CHN, KOR, JPN, USA).       |
+------------------------------------------------------------------------------------+
| Mã quốc tịch *                  | Tên quốc tịch *                  | [Thêm] [Hủy]  |
| [ CN                          ] | [ (Trống)                      ] |               |
|  ^ Viền đỏ                      |  ^ Viền đỏ                       |               |
|  Lỗi: Mã quốc tịch phải gồm     |  Lỗi: Bắt buộc nhập tên quốc     |               |
|  đúng 3 chữ cái (VD: CHN, KOR)  |  tịch                            |               |
+------------------------------------------------------------------------------------+
| Bảng: CHN -> Trung Quốc | KOR -> Hàn Quốc | JPN -> Nhật Bản | USA -> Hoa Kỳ         |
+------------------------------------------------------------------------------------+
| Đồng bộ cho Phường/Xã *, Ngành nghề *, Nhóm ngành *, Lĩnh vực *, Quận/Huyện *,     |
| Địa bàn hành chính (Mã *, Tên *), Tài khoản (Username *, Họ tên *, Mật khẩu *)     |
| + Sửa nút "Luu"/"Them" -> "Lưu"/"Thêm"                                             |
+------------------------------------------------------------------------------------+
```

### Mockup 2 — Nhập Excel (Bước 1 Chọn file & Bước 2 Ghép cột)
```text
[Bước 1: Cấu hình đọc file]
+------------------------------------------------------------------------------------+
| Công ty mặc định *              | Nhóm nghề * (bắt buộc)                           |
| [ — Chưa chọn —           ▾ ]  | [ — Chưa chọn —           ▾ ] (Viền đỏ)         |
|                                 | Lỗi: Bắt buộc chọn nhóm nghề                     |
+------------------------------------------------------------------------------------+
| [!] Cảnh báo trường bắt buộc: Vui lòng chọn Nhóm nghề trước khi tiếp tục.          |
| [!] Cảnh báo trường bắt buộc: Chưa chọn Công ty mặc định (nếu không cho tạo mới).  |
+------------------------------------------------------------------------------------+

[Bước 2: Ghép cột]
+------------------------------------------------------------------------------------+
| [!] Cảnh báo trường bắt buộc: Còn thiếu 2/9 trường bắt buộc chưa được ghép cột:    |
|     Quốc tịch, Địa chỉ tạm trú. Nếu tiếp tục, các dòng thiếu sẽ báo lỗi ở Preview. |
+------------------------------------------------------------------------------------+
| Cột A: "Họ và tên"    -> [ Họ tên * (Bắt buộc)        ▾ ]  [Tự động] [Bắt buộc]   |
| Cột D: "Nationality"  -> [ Quốc tịch * (Bắt buộc)     ▾ ]  [Tự động] [Bắt buộc]   |
|   (?) Mã 3 chữ cái (CHN, KOR, USA) hoặc tên quốc tịch có trong Quản trị -> Quốc tịch|
| Cột F: "Nơi ở"        -> [ — Bỏ qua —                 ▾ ]  [Bỏ qua]               |
+------------------------------------------------------------------------------------+
```

### Mockup 3 — Nhập Thủ Công (`Import.razor`)
```text
[Trạng thái ngay khi bấm "Thêm nhân viên" mà chưa điền đủ]
+------------------------------------------------------------------------------------+
| Họ tên *                | Giới tính *        | Ngày sinh * (Viền đỏ)               |
| [ NGUYỄN VĂN AN       ] | [ Nam          ▾ ] | [ dd/MM/yyyy                    ]   |
|                         |                    | Lỗi: Bắt buộc nhập ngày sinh        |
+-------------------------+--------------------+-------------------------------------+
| Quốc tịch * (Viền đỏ)   | Số hộ chiếu *      | Công ty * (Viền đỏ)                 |
| [ — Chọn —        ▾ ]   | [ E12345678      ] | [ — Chọn công ty —          ▾ ]     |
| Lỗi: Bắt buộc chọn QT   |                    | Lỗi: Bắt buộc chọn công ty          |
+-------------------------+--------------------+-------------------------------------+
| Nghề nghiệp * (Viền đỏ) | Số Visa * (Viền đỏ)| Hạn tạm trú * (Viền đỏ)             |
| Lỗi: Bắt buộc chọn nghề | Lỗi: Nhập số Visa  | Lỗi: Bắt buộc nhập hạn tạm trú      |
+-------------------------+--------------------+-------------------------------------+
| Địa chỉ tạm trú * (Viền đỏ)                                                        |
| [ VD: 12 Lê Duẩn, Hải Châu                                                       ] |
| Lỗi: Bắt buộc nhập địa chỉ tạm trú (nên kết thúc bằng tên Phường/Xã)               |
+------------------------------------------------------------------------------------+
| [!] Cảnh báo các trường bắt buộc chưa nhập (*): Ngày sinh, Quốc tịch, Công ty...   |
|                          [ Xóa form ]   [ Thêm nhân viên ]                         |
+------------------------------------------------------------------------------------+
```

### Mockup 4 — Hộp Thoại Thêm / Sửa Nhân Viên (`Employees.razor`)
```text
+------------------------------------------------------------------------------------+
| Thêm nhân viên                                           Đủ 9 trường bắt buộc + CT |
+------------------------------------------------------------------------------------+
| Họ tên * [       ] | Giới tính * [Nam▾] | Ngày sinh * [dd/MM/yyyy]                 |
| Quốc tịch * [▾]    | Số hộ chiếu * [ ]  | Công ty * [▾]                            |
| Nghề nghiệp * [▾]  | Hạn tạm trú * [  ] | Số Visa * [ ]                            |
| Địa chỉ tạm trú * [ BỔ SUNG MỚI VÀO DIALOG ] | Ký hiệu Visa [ ]                    |
| (Mọi ô * còn trống bật viền đỏ + ErrorText ngay bên dưới khi bấm Thêm/Cập nhật)    |
+------------------------------------------------------------------------------------+
| [!] Cảnh báo các trường bắt buộc chưa nhập (*): ...                                |
|                                                        [ Hủy ]   [ Thêm / Lưu ]    |
+------------------------------------------------------------------------------------+
```

---

## Cấu Trúc File Tác Động (File Structure)

- **Quy tắc nghiệp vụ & Ánh xạ Import (`IRM.Application`):**
  - `IRM/IRM.Application/Features/ImportExport/LegacyImportRules.cs`
  - `IRM/IRM.Application/Features/ImportExport/ImportMapping.cs`
  - `IRM/IRM.Application/Features/ImportExport/ImportFieldHelpCatalog.cs`
- **Tầng Hạ tầng & Cơ sở dữ liệu (`IRM.Infrastructure` & `deploy/sql`):**
  - `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs`
  - `IRM/IRM.Infrastructure/Maintenance/DatabaseSeeder.cs`
  - `IRM/deploy/sql/00-full-setup.sql`
- **Giao diện người dùng (`IRM`):**
  - `IRM/IRM/Features/Administration/Admin.Catalogs.cs`
  - `IRM/IRM/Features/Administration/Admin.Accounts.cs`
  - `IRM/IRM/Features/Administration/Components/AdminNationalitiesSection.razor`
  - `IRM/IRM/Features/Administration/Components/AdminWardsSection.razor`
  - `IRM/IRM/Features/Administration/Components/AdminCareersSection.razor`
  - `IRM/IRM/Features/Administration/Components/AdminFieldsSection.razor`
  - `IRM/IRM/Features/Administration/Components/AdminDistrictsSection.razor`
  - `IRM/IRM/Features/Administration/Components/AdminAdministrativeUnitsSection.razor`
  - `IRM/IRM/Features/Administration/Components/AdminAccountsSection.razor`
  - `IRM/IRM/Features/ImportExport/Import.razor`
  - `IRM/IRM/Features/ImportExport/Import.razor.cs`
  - `IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor`
  - `IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor`
  - `IRM/IRM/Features/Employees/Employees.razor`
  - `IRM/IRM/Features/Companies/Companies.razor`
  - `IRM/IRM/Features/Students/Students.razor`
- **Kiểm thử tự động (`IRM.Tests`):**
  - `IRM/IRM.Tests/TestDatabase.cs`
  - `IRM/IRM.Tests/LegacyImportRulesTests.cs`
  - `IRM/IRM.Tests/LegacyImportLookupRulesTests.cs`
  - `IRM/IRM.Tests/LegacyImportEndToEndTests.cs`
  - `IRM/IRM.Tests/NationalityImportTests.cs`
  - `IRM/IRM.Tests/EnumAndReleaseFeatureTddTests.cs`
  - `IRM/IRM.Tests/ImportGuidanceRenderingTests.cs`
  - `IRM/IRM.Tests/VietnameseUiTextTests.cs`

---

### Task 1: Chuẩn Hóa `LegacyImportRules.cs` Theo Đúng Quy Tắc Dự Án Cũ

**Files:**
- Modify: `IRM/IRM.Application/Features/ImportExport/LegacyImportRules.cs:45-69,226-354`
- Test: `IRM/IRM.Tests/LegacyImportRulesTests.cs`
- Test: `IRM/IRM.Tests/LegacyImportLookupRulesTests.cs`

**Interfaces:**
- Produces:
  - `LegacyImportRules.IsValidNationalityCode(string? code) -> bool`
  - `LegacyImportRules.NormalizeNationalityCode(string? code) -> string`
  - `LegacyImportRules.ResolveNationalityCode(string? value, IReadOnlyCollection<NationalityEntity> nationalities) -> string?`
  - `LegacyImportRules.ResolveNationality(string? raw, IReadOnlyCollection<NationalityEntity> nationalities, int rowNumber) -> string`
  - `LegacyImportRules.IsRelativeCareer(Career career, int otherCareerGroupId = 9) -> bool`
  - `LegacyImportRules.SameAs13Fields(Employee a, Employee b) -> bool`

- [x] **Bước 1: Viết/kiểm tra unit test cho 4 quy tắc của `LegacyImportRules`**

```csharp
[Theory]
[InlineData("CHN", true)]
[InlineData("kor", true)]
[InlineData("CN", false)]
[InlineData("CHINA", false)]
[InlineData("C1N", false)]
[InlineData("", false)]
public void IsValidNationalityCode_RequiresExactlyThreeAsciiLetters(string? code, bool expected)
    => Assert.Equal(expected, LegacyImportRules.IsValidNationalityCode(code));

[Theory]
[InlineData("01/01/1899")]
[InlineData("01/01/2100")]
[InlineData("01/01/0001")]
public void Date_RejectsYearsOutside1900To2099(string text)
    => Assert.False(LegacyImportRules.TryParseLegacyDate(text, out _));
```

- [x] **Bước 2: Chạy test kiểm chứng**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~LegacyImportRulesTests|FullyQualifiedName~LegacyImportLookupRulesTests"`
Expected: Các test về mã 3 chữ cái, khoảng năm `1900..2099`, `SameAs13Fields`, `IsRelativeCareer` khớp kỳ vọng.

- [x] **Bước 3: Rà soát/hoàn thiện mã trong `LegacyImportRules.cs`**
- `TryParseToken`: chỉ mở rộng năm 2 chữ số khi `parts[2].Length <= 2`, kiểm tra `year >= 1900 && year <= 2099`.
- `ResolveNationalityCode` / `ResolveNationality`: ưu tiên khớp mã hoặc tên trong danh mục đang hoạt động (`Delete_flag == 0`), hỗ trợ ánh xạ 2 chữ cái sang 3 chữ cái nếu mã 3 chữ cái đích có trong danh mục đang hoạt động.
- `IsRelativeCareer`: khớp khi `(career.IDCG == otherCareerGroupId || RemoveDiacritics(career.CareerGroup?.CareerGroupName ?? "").Equals("KHAC", StringComparison.OrdinalIgnoreCase))` và tên nghề không dấu là `"THAN NHAN"`.
- `SameAs13Fields`: so sánh không dấu, không phân biệt hoa thường cho trường chuỗi; `WorkPermitNumber` và `Note` coi `null`/`""`/`" "` là như nhau.

- [x] **Bước 4: Chạy lại bộ test Task 1**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~LegacyImportRulesTests|FullyQualifiedName~LegacyImportLookupRulesTests"`
Expected: PASS 100%.

---

### Task 2: Cập Nhật `ImportMapping.cs` & `SqlImportStore.Preview.cs` (9 Trường Bắt Buộc & Nghề Thân Nhân)

**Files:**
- Modify: `IRM/IRM.Application/Features/ImportExport/ImportMapping.cs:8-28,145-165,206-208`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs:35-42`
- Test: `IRM/IRM.Tests/LegacyImportEndToEndTests.cs`
- Test: `IRM/IRM.Tests/NationalityImportTests.cs`

**Interfaces:**
- Consumes: `LegacyImportRules.ResolveNationalityCode`, `LegacyImportRules.IsRelativeCareer`, `LegacyImportRules.ParseWorkPermit`
- Produces:
  - `ImportMapping.RequiredCoreFields -> IReadOnlyList<string>`
  - `ImportMapping.GetMissingRequiredFields(IEnumerable<ColumnMapping>? mappings) -> IReadOnlyList<(string Key, string Label)>`

- [x] **Bước 1: Viết unit test cho trường hợp dòng Excel có `WorkPermit = "Khác"` và `Career = "Thân nhân"` khi đang chọn nhóm nghề khác (`IDCG != 9`)**

```csharp
[Fact]
public void RelativePermitWithRelativeCareerName_ResolvesEvenWhenSelectedCareerGroupIsNotOther()
{
    var row = ValidRow();
    row["WorkPermit"] = "Khác";
    row["Career"] = "Thân nhân";
    var employee = Parse(row); // careerGroupId = 1, "Thân nhân" thuộc nhóm "Khác" (IDCG = 9)
    Assert.Equal(6, (int)employee.WorkPermit);
    Assert.Equal(14, employee.IDCareer);
}
```

- [x] **Bước 2: Chạy test mới để xác nhận lỗi trước khi sửa**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~RelativePermitWithRelativeCareerName_ResolvesEvenWhenSelectedCareerGroupIsNotOther"`
Expected: FAIL với `"Nghề nghiệp chưa có trong dữ liệu"` trước khi cập nhật `ImportMapping.ParseRowToEmployee`.

- [x] **Bước 3: Cập nhật `ImportMapping.ParseRowToEmployee` và `SqlImportStore.Preview.cs`**
- Trong `ParseRowToEmployee`: khi `careerId` chưa tìm thấy trong `careerGroupId`, kiểm tra xem ô `WorkPermit` có phân tích ra `WorkPermitOther` (`6`) và tên nghề không dấu là `"THAN NHAN"` hay không; nếu đúng, tìm nghề khớp `LegacyImportRules.IsRelativeCareer` trong `careers`.
- Đảm bảo `SqlImportStore.Preview.cs` giữ `.Include(c => c.CareerGroup)`.

- [x] **Bước 4: Chạy lại bộ test Task 2**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~LegacyImportEndToEndTests|FullyQualifiedName~NationalityImportTests"`
Expected: PASS 100%.

---

### Task 3: Chuẩn Hóa Dữ Liệu Seed Quốc Tịch 3 Chữ Cái & Nhóm Nghề `Khác` / `Thân Nhân`

**Files:**
- Modify: `IRM/IRM.Infrastructure/Maintenance/DatabaseSeeder.cs`
- Modify: `IRM/deploy/sql/00-full-setup.sql`
- Modify: `IRM/IRM.Tests/TestDatabase.cs`
- Test: `IRM/IRM.Tests/EnumAndReleaseFeatureTddTests.cs`

**Interfaces:**
- Produces:
  - Danh mục 34 mã quốc tịch 3 chữ cái (`CHN`, `KOR`, `JPN`, `TWN`, `IND`, `MYS`, `PHL`, `THA`, `IDN`, `SGP`, `MMR`, `LAO`, `KHM`, `BRN`, `USA`, `GBR`, `FRA`, `DEU`, `RUS`, `ITA`, `ESP`, `CAN`, `NLD`, `SWE`, `DNK`, `AUS`, `NZL`, `BGD`, `PAK`, `LKA`, `NPL`, `NGA`, `ZAF`, `BRA`)
  - Nhóm nghề `"Khác"` + nghề `"Thân nhân"`
  - Cơ chế nâng cấp an toàn khóa ngoại từ mã 2 chữ cái sang 3 chữ cái

- [x] **Bước 1: Xác nhận test nâng cấp khóa ngoại và seed 34 mã 3 chữ cái trong `EnumAndReleaseFeatureTddTests.cs`**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~DatabaseSeeder_SeedCatalogsIfEmpty"`
Expected: PASS 100%, không lỗi khóa ngoại với `Employees`, `Students`, `ForeignPersons`.

---

### Task 4: Bổ Sung Cảnh Báo Trường Bắt Buộc Trực Tiếp Trên Từng Ô & Ô `Địa Chỉ *` Ở Quản Trị, Nhân Viên, Công Ty, Du Học Sinh

**Files:**
- Modify: `IRM/IRM/Features/Administration/Admin.Catalogs.cs`
- Modify: `IRM/IRM/Features/Administration/Admin.Accounts.cs`
- Modify: `IRM/IRM/Features/Administration/Components/AdminNationalitiesSection.razor`
- Modify: `IRM/IRM/Features/Administration/Components/AdminWardsSection.razor`
- Modify: `IRM/IRM/Features/Administration/Components/AdminCareersSection.razor`
- Modify: `IRM/IRM/Features/Administration/Components/AdminFieldsSection.razor`
- Modify: `IRM/IRM/Features/Administration/Components/AdminDistrictsSection.razor`
- Modify: `IRM/IRM/Features/Administration/Components/AdminAdministrativeUnitsSection.razor`
- Modify: `IRM/IRM/Features/Administration/Components/AdminAccountsSection.razor`
- Modify: `IRM/IRM/Features/Employees/Employees.razor`
- Modify: `IRM/IRM/Features/Companies/Companies.razor`
- Modify: `IRM/IRM/Features/Students/Students.razor`
- Test: `IRM/IRM.Tests/VietnameseUiTextTests.cs`

**Interfaces:**
- Consumes: `LegacyImportRules.IsValidNationalityCode`, `LegacyImportRules.NormalizeNationalityCode`

- [x] **Bước 1: Viết bUnit/UI test cho các màn hình Quản trị và Nhân viên**
- Kiểm tra `AdminNationalitiesSection` báo cảnh báo khi nhập mã 2 chữ cái (`"CN"`) và hiện lỗi trực tiếp dưới ô khi bấm Lưu/Thêm mà thiếu trường bắt buộc.
- Kiểm tra dialog `Employees.razor` có đủ ô **`Địa chỉ tạm trú *`**, bật lỗi đỏ dưới từng ô khi bấm Lưu mà thiếu trường bắt buộc (`Họ tên`, `Ngày sinh`, `Quốc tịch`, `Số hộ chiếu`, `Công ty`, `Nghề nghiệp`, `Địa chỉ`, `Hạn tạm trú`, `Số Visa`), và nút lưu không bỏ qua các trường này.

- [x] **Bước 2: Cập nhật các form Quản trị (`Admin*.razor` & `Admin.*.cs`)**
- Gắn cờ `_submitted...` cho từng form danh mục (reset khi bấm `Hủy` / `Reset...Form()` hoặc mở `Edit...()`).
- Gắn `Error` + `ErrorText` tiếng Việt trực tiếp dưới từng ô bắt buộc:
  - `AdminNationalitiesSection`: `Mã quốc tịch *` (kiểm tra trống + kiểm tra `IsValidNationalityCode`), `Tên quốc tịch *`.
  - `AdminWardsSection`: `Tên phường/xã *`; sửa nút `"Luu"` / `"Them"` -> `"Lưu"` / `"Thêm"`.
  - `AdminCareersSection`: `Tên ngành nghề *`, `Nhóm ngành *`; sửa `"Ten nganh nghe"` / `"Nhom"` / `"Luu"` / `"Them"` sang tiếng Việt có dấu.
  - `AdminFieldsSection`: `Tên lĩnh vực *`; sửa `"Ten linh vuc"` / `"Mo ta"` / `"Luu"` / `"Them"` sang tiếng Việt có dấu.
  - `AdminDistrictsSection`: `Tên quận/huyện *`; sửa `"Luu"` / `"Them"` sang `"Lưu"` / `"Thêm"`.
  - `AdminAdministrativeUnitsSection`: `Mã *`, `Tên địa bàn *`.
  - `AdminAccountsSection` & `Admin.Accounts.cs`: `Username *`, `Họ tên *`, `Mật khẩu *`; sửa `"Luu"` / `"Them"` và thông báo không dấu (`"Vui long nhap..."`, `"Loi luu..."`, `"Da xoa..."`) sang tiếng Việt có dấu.

- [x] **Bước 3: Cập nhật dialog Nhân viên (`Employees.razor`), Công ty (`Companies.razor`), Du học sinh (`Students.razor`)**
- Trong `Employees.razor`:
  - Thêm ô `Địa chỉ tạm trú *` (`@bind-Value="_f.Address"`) vào dialog Thêm/Sửa nhân viên.
  - Thêm cờ `_dlgSubmitted`; bỏ `Disabled="@(string.IsNullOrWhiteSpace(_f.StaffName))"` trên nút `Thêm`/`Cập nhật` để người dùng bấm là thấy ngay toàn bộ ô thiếu sáng đỏ (`Error` + `ErrorText`) kèm `MudAlert` tổng hợp.
  - Trong `SaveEmployeeAsync()`: kiểm tra đủ `GetMissingDialogRequiredFields()`, đặt `_dlgSubmitted = true` và cảnh báo `Snackbar` nếu còn thiếu bất kỳ trường bắt buộc nào.
- Trong `Companies.razor`: thêm `RequiredError="Bắt buộc nhập tên công ty"`, `Error` + `ErrorText` cho `Tên công ty *`, cho phép bấm nút lưu để hiện cảnh báo ô trống.
- Trong `Students.razor`: thêm `RequiredError` + `Error` + `ErrorText` tiếng Việt cho `Họ tên *` và `Trường *`, kiểm tra cả `SchoolName` trong `SaveAsync()`.

- [x] **Bước 4: Chạy test xác nhận Task 4**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~VietnameseUiTextTests"`
Expected: PASS 100%.

---

### Task 5: Hoàn Thiện Cảnh Báo Trường Bắt Buộc Trên Nhập Excel & Nhập Thủ Công (`Import`)

**Files:**
- Modify: `IRM/IRM.Application/Features/ImportExport/ImportFieldHelpCatalog.cs`
- Modify: `IRM/IRM/Features/ImportExport/Import.razor`
- Modify: `IRM/IRM/Features/ImportExport/Import.razor.cs`
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor`
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor`
- Test: `IRM/IRM.Tests/ImportGuidanceRenderingTests.cs`

**Interfaces:**
- Consumes: `ImportMapping.RequiredCoreFields`, `ImportMapping.GetMissingRequiredFields`, `ImportFieldHelpCatalog`

- [x] **Bước 1: Mở rộng test bUnit trong `ImportGuidanceRenderingTests.cs`**
- Kiểm tra tab Nhập thủ công bật `Error` + `ErrorText` cho các ô bắt buộc bị thiếu khi bấm `Thêm nhân viên` và không gọi `CreateAsync` khi còn thiếu bất kỳ trường bắt buộc nào.
- Kiểm tra `ImportUploadSection` hiển thị lỗi dưới ô `Nhóm nghề *` và `Công ty mặc định`.
- Kiểm tra `ImportMappingSection` hiển thị cảnh báo thiếu trường trong 9 trường bắt buộc, hậu tố `* (Bắt buộc)` và chip `Bắt buộc`.

- [x] **Bước 2: Cập nhật `Import.razor`, `Import.razor.cs`, `ImportUploadSection.razor`, `ImportMappingSection.razor`**
- Thêm `_manualSubmitted` trong `Import.razor.cs`; reset trong `ResetManualForm()`.
- Trên tab Nhập thủ công (`Import.razor`): gắn `Error` + `ErrorText` cho tất cả 9 trường bắt buộc + `Công ty *`, bỏ `Disabled` trên nút `Thêm nhân viên` để khi bấm thiếu trường hệ thống bật viền đỏ dưới từng ô thiếu, giữ nguyên `MudAlert` tổng hợp và chặn lưu trong `SaveManualEntryAsync()` nếu `GetMissingManualEntryFields().Any()`.
- Trên `ImportUploadSection.razor` & `ImportMappingSection.razor`: giữ `MudAlert` cảnh báo và bổ sung `Error` / `ErrorText` rõ ràng cho `Nhóm nghề *`.

- [x] **Bước 3: Chạy test bUnit kiểm chứng**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportGuidanceRenderingTests"`
Expected: PASS 100%.

---

### Task 6: Kiểm Chứng Toàn Bộ Solution, Tạo Bản Test & Commit Sạch Sẽ

**Files:**
- Verify: Toàn bộ `IRM/IRM.sln` và `IRM/IRM.Tests/IRM.Tests.csproj`

- [x] **Bước 1: Build toàn bộ Solution cấu hình Release**
Run: `dotnet build IRM/IRM.sln -c Release`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [x] **Bước 2: Chạy toàn bộ bộ Unit Test**
Run: `dotnet test IRM/IRM.Tests/IRM.Tests.csproj -c Release --no-build`
Expected: Toàn bộ test PASS (`Failed: 0`, trừ các `SqlServerFact` nếu không bật LocalDB trong môi trường hiện tại — ghi rõ số lượng Skip nếu có).

- [x] **Bước 3: Rà soát `git status` & `git diff`, cập nhật commit trên nhánh `develop`**
Run: `git status -sb` và `git diff --stat`
Expected: Chỉ gồm các file thuộc phạm vi kế hoạch; tạo commit hoàn chỉnh (hoặc amend theo yêu cầu người dùng).

---

## Trạng Thái Thực Thi Hiện Tại

- File kế hoạch tiếng Việt: `IRM/docs/superpowers/plans/2026-11-15-legacy-import-rules-and-required-warnings.md`
- File HTML Mockup trực quan: `IRM/docs/superpowers/specs/2026-11-15-required-field-warnings-mockup.html`
- Đã hoàn tất toàn bộ Task 1 → Task 6, build Release (`0 Warning(s), 0 Error(s)`) và chạy toàn bộ `IRM.Tests` (`360 Passed, 0 Failed, 6 SqlServerFact Skipped`).
