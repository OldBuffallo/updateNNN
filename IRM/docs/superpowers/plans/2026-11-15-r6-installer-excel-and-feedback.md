# Kế Hoạch Triển Khai: Đóng Gói Bộ Cài R6, Chuẩn Hóa File Excel Test & Phiếu Khảo Sát Khách Hàng

> **Dành cho tác nhân thực thi (Agentic workers):** KỸ NĂNG BẮT BUỘC: Sử dụng `superpowers:subagent-driven-development` (khuyến nghị) hoặc `superpowers:executing-plans` để triển khai kế hoạch này theo từng Task. Các bước sử dụng cú pháp hộp kiểm (`- [ ]`) để theo dõi tiến độ.

**Mục tiêu (Goal):** Đóng gói bộ cài Windows offline mới `IRM-v1.0.2-windows-x64-offline-setup-R6.exe` chứa đầy đủ bản sửa quy tắc WPF/quốc tịch 3 chữ cái và cảnh báo trường bắt buộc (commit `7559cb9`), chuẩn hóa file `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` nạp đạt 10/10 hồ sơ trên database cài mới mặc định, bổ sung cặp Phiếu khảo sát & góp ý (`PHIEU-KHAO-SAT-GOP-Y.html` + `PHIEU-KHAO-SAT-GOP-Y.xlsx`) trong thư mục bàn giao `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`, **và trang bị hiệu ứng UI dẫn hướng ban đầu ngay trên giao diện Web để khách hàng nhìn thấy ngay nơi gửi góp ý**.

**Kiến trúc (Architecture):**
1. **Sinh & kiểm chứng các tệp Excel R6 hoàn toàn bằng stack C#/.NET có sẵn:** Bổ sung `IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs` sử dụng thư viện `ClosedXML 0.104.2` đã có sẵn trong `IRM.Infrastructure` (kèm cờ CLI `--export-delivery-workbooks <dir>` trong `Program.cs`) để tạo đồng bộ `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` và `PHIEU-KHAO-SAT-GOP-Y.xlsx`; lưu fixture vào `IRM/IRM.Tests/Fixtures/` để `DeliverySampleWorkbookTests` kiểm chứng End-to-End với `DatabaseSeeder.SeedCatalogsIfEmptyAsync`, `SeedOnboardingGuideIfEmptyAsync`, `SqlImportStore.GeneratePreviewAsync` & `ExecuteImportAsync`.
2. **Phiếu khảo sát offline + Hiệu ứng UI chỉ dẫn chỗ Feedback ngay khi mở ứng dụng:**
   - Tạo `PHIEU-KHAO-SAT-GOP-Y.html` tự chứa (cùng bảng màu Olive/Cam `#3f5223`, `#eef5dc`, `#ea580c` của `HUONG-DAN-CAI-DAT.html`, có dải hướng dẫn 3 bước nổi bật khi mới mở file) và đặt cả cặp `.html` + `.xlsx` trong `IRM/IRM/wwwroot/` (mở được trực tiếp từ Web app qua `app.UseStaticFiles()`) lẫn trong thư mục bàn giao `BAN-GIAO/`.
   - Trên `MainLayout.razor` Topbar, thêm nút nổi bật **"Khảo sát & Góp ý"** (`#btnCustomerFeedback`) kèm menu mở bản HTML / tải bản Excel / xem lại hướng dẫn. Lần đầu mở app (`localStorage['irm-r6-feedback-seen']` chưa đặt), `irmInterop.initFeedbackSpotlight` tự kích hoạt viền sáng nhịp đập (`irm-feedback-highlight` trong `app.css`) và spotlight `Driver.js` (`wwwroot/vendor/driverjs/` đã tích hợp sẵn) chỉ thẳng vào `#btnCustomerFeedback` kèm lời giải thích chỗ phản hồi.
3. **Đóng gói `R6` sạch Git (`dirty=false`) & lưu trữ nguyên vẹn `R5`:** Cập nhật cấu hình đóng gói sang `R6`, commit sạch mã nguồn trước khi chạy `deploy/windows/build-windows-installer.ps1 -AllowUnsigned`, chuyển nguyên vẹn `R5` sang `BAN-GIAO/02-LUU-TRU/`, tập hợp đủ 11 tệp vào `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`, kiểm chứng SHA-256 và cập nhật tài liệu bàn giao.

**Công nghệ (Tech Stack — Không thêm công nghệ hay thư viện mới):**
Toàn bộ các bước đều tái sử dụng 100% nền tảng và thư viện đã có sẵn trong repository `IRM/`, **không cài đặt thêm ngôn ngữ, runtime hay gói NuGet/npm mới**:
- **`.NET 8`, `ASP.NET Core Blazor Server`, `MudBlazor 9.3.0`:** Nền tảng sẵn có của `IRM`, dùng để đặt nút `#btnCustomerFeedback` & menu thao tác trên `MainLayout.razor`.
- **`ClosedXML 0.104.2`:** Thư viện Excel **đã cài sẵn trong `IRM.Infrastructure.csproj`** (trước đây đã dùng cho `ImportSampleWorkbook.cs` và xuất báo cáo); tái sử dụng để sinh `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` và `PHIEU-KHAO-SAT-GOP-Y.xlsx` (Dropdown Data Validation + công thức `COUNTIF`), thay vì thêm công cụ bên ngoài như Python/`openpyxl`.
- **`Driver.js 1.12.1` + `irm-interop.js` + `app.css`:** Thư viện tour/spotlight đã tích hợp sẵn trong `IRM/IRM/wwwroot/vendor/driverjs/` (đang dùng cho hướng dẫn Import Excel); tái sử dụng để tạo hiệu ứng spotlight ban đầu chỉ vào nút góp ý trên Topbar mà không thêm thư viện JS mới.
- **`HTML5/CSS3/Vanilla JS` thuần offline:** Cho `PHIEU-KHAO-SAT-GOP-Y.html` mở trực tiếp qua `file:///` hoặc `/PHIEU-KHAO-SAT-GOP-Y.html`, giống định dạng của `HUONG-DAN-CAI-DAT.html` và `HUONG-DAN-IMPORT-EXCEL.html`.
- **`xUnit`, `bUnit`, `EF Core SQLite in-memory`, `node --test`:** Hệ thống kiểm thử tự động sẵn có trong `IRM.Tests/`.
- **`Inno Setup 6 (ISCC.exe)` & `Windows PowerShell 5.1`:** Bộ công cụ đóng gói bộ cài Windows offline sẵn có từ R2–R5 (`IRM/deploy/windows/`).

**Tài liệu thiết kế (Spec):** [`../specs/2026-11-15-r6-installer-excel-and-feedback-design.md`](../specs/2026-11-15-r6-installer-excel-and-feedback-design.md)

## Ràng Buộc Chung (Global Constraints)

- **Phải có hiệu ứng UI ban đầu để khách hàng biết chỗ để feedback:** Ngay lần đầu người dùng đăng nhập mở ứng dụng Web, UI phải tự động làm nổi bật nút **"Khảo sát & Góp ý"** (`#btnCustomerFeedback` trên Topbar) bằng viền sáng nhịp đập (`irm-feedback-highlight`, tôn trọng `prefers-reduced-motion: reduce`) kèm hộp spotlight hướng dẫn 1 bước chỉ rõ nơi đánh giá chức năng, giao diện & định dạng Excel; người dùng có thể bấm xem lại hướng dẫn bất kỳ lúc nào. Bên trong `PHIEU-KHAO-SAT-GOP-Y.html` cũng phải có dải dẫn hướng ban đầu nổi bật 3 bước thao tác khi mới mở tệp.
- **Không thêm công nghệ hay phụ thuộc ngoài:** Tuyệt đối không cài thêm Python package, npm package, NuGet package hoặc tải tài nguyên qua CDN (`https://...`); mọi tệp HTML/Web/Excel đều chạy 100% offline.
- **Không sửa/ghi đè gói R5 đã giao:** Thư mục `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` phải được chuyển nguyên vẹn sang `BAN-GIAO/02-LUU-TRU/v1.0.2-R5-UNSIGNED-TEST/`, không chỉnh sửa bất kỳ tệp nào bên trong R5 để bảo toàn `SHA256SUMS.txt` cũ.
- **Đóng gói từ Git tree sạch:** Script `IRM/deploy/windows/build-windows-installer.ps1` từ chối build nếu `git status --porcelain` còn tệp chưa commit; toàn bộ thay đổi cấu hình `R6`, mã nguồn hiệu ứng feedback, tài liệu nguồn và test phải được commit trước khi đóng gói `.exe`.
- **Trung thực trạng thái ký số & phạm vi test:** Chỉ ghi đã kiểm chứng cho các lệnh và bước thực sự đã chạy; ghi rõ `-AllowUnsigned` (`NotSigned`) và giới hạn nghiệm thu thực tế nếu chưa chạy cài đặt trên VM desktop.
- **Khớp tuyệt đối danh mục seed mặc định của R6:** Khi database được khởi tạo bởi `DatabaseSeeder.SeedCatalogsIfEmptyAsync`, danh mục hiện có là Công ty `CÔNG TY MẪU - HÃY ĐỔI TÊN`, nhóm nghề `Kỹ thuật` (nghề `Kỹ sư`), nhóm `Khác` (nghề `Thân nhân`), 34 quốc tịch 3 chữ cái (`USA`, `KOR`, `JPN`, `CHN`, `AUS`, `GBR`, `IND`...), và 3 phường (`Phường Hồng Gai`, `Phường Bãi Cháy`, `Phường Cẩm Trung`).

## Trọng Tâm Kiểm Tra (Review Focus)

1. **File `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` trên DB mới cài đặt R6:** Được kiểm chứng tự động trong `DeliverySampleWorkbookTests` (Task 1) với đúng DB tạo mới qua `DatabaseSeeder.SeedCatalogsIfEmptyAsync` + `SeedOnboardingGuideIfEmptyAsync`, không thêm tay bất kỳ phường hay nghề nào khác.
2. **Dòng 10 (`Nghề nghiệp = Thân nhân`, `GPLĐ = Khác`) khi chọn `Nhóm nghề = Kỹ thuật`:** Được kiểm chứng trong `DeliverySampleWorkbookTests` (Task 1) xác nhận `IDCareer` khớp nghề `"Thân nhân"` thuộc nhóm `"Khác"` và `WorkPermit == WorkPermitType.Other` (`6`).
3. **Bảo toàn số 0 đầu ở cột mã định danh trong `.xlsx`:** Cột `Số hộ chiếu`, `Số GPLĐ`, `Số Visa`, `CMND người thân` có định dạng ô Text (`@`) và dữ liệu `000000000001`, `000000000004`, `000000000008`, `000000000010` không bị biến thành số mất số 0 (kiểm chứng tại Task 1).
4. **Hiệu ứng UI chỉ dẫn Feedback trên `MainLayout.razor`, `irm-interop.js` & Phiếu khảo sát offline/Excel:** Kiểm chứng trong Task 2 bằng: (a) test `node --test` cho `irmInterop.initFeedbackSpotlight` (chạy tự động lần đầu khi chưa có `irm-r6-feedback-seen`, đánh dấu đã xem khi đóng, phát lại được khi `forceReplay = true`), (b) test C# kiểm tra `MainLayout.razor` chứa `#btnCustomerFeedback` và `App.razor` khớp regex `app.css?v=mui-gov-...`, (c) test `ClosedXML` xác nhận `PHIEU-KHAO-SAT-GOP-Y.xlsx` có 3 sheet, Data Validation ở `E2:E23` và công thức `COUNTIF`, và (d) `PHIEU-KHAO-SAT-GOP-Y.html` không gọi CDN ngoài / JS không lỗi cú pháp.
5. **Đối chiếu sạch Git trước build & checksum SHA-256 sau build:** Kiểm chứng trong Task 3 (`environment.Tests.ps1` x64/x86, `dotnet test`, `node --test`, `git status` sạch) và Task 4 (đối chiếu 100% hash trong `SHA256SUMS.txt` và `SOURCE-REVISION.json` của gói R6).

---

## Sơ Đồ Bố Cục Bàn Giao R6 & Giao Diện Chỉ Dẫn Feedback

```text
BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/   (11 tệp)
├── IRM-v1.0.2-windows-x64-offline-setup-R6.exe
├── IRM-v1.0.2-windows-x64-offline-setup-R6.exe.sha256
├── IRM-MAU-IMPORT-TEST-DA-SUA.xlsx              (10/10 hồ sơ chuẩn 3 chữ cái + Nhóm Kỹ thuật)
├── PHIEU-KHAO-SAT-GOP-Y.html                    (Phiếu khảo sát tương tác offline, xuất CSV/JSON, in PDF)
├── PHIEU-KHAO-SAT-GOP-Y.xlsx                    (Phiếu khảo sát Excel 3 sheet + Dropdown)
├── DOC-TRUOC-KHI-CAI.txt
├── HUONG-DAN-CAI-DAT.html
├── HUONG-DAN-CAI-DAT.md
├── SHA256SUMS.txt
├── SOURCE-REVISION.json
└── VERIFICATION.json
```

```text
[1. Trên giao diện Web IRM ngay lần đầu mở (MainLayout.razor Topbar)]
+-----------------------------------------------------------------------------------+
| [☰] IRM v1.0.2  [Tìm kiếm Ctrl+K] ... [Thêm nhanh▾]  [ * Khảo sát & Góp ý (R6) * ] |
+--------------------------------------------------------^--------------------------+
                                                         | (Vòng sáng pulse + Spotlight)
   +-----------------------------------------------------+------------------------+
   | 📍 Phiếu Khảo Sát & Góp Ý Nghiệm Thu (R6)                                    |
   | Nhấn nút "Khảo sát & Góp ý" trên thanh tiêu đề bất kỳ lúc nào để đánh giá   |
   | từng chức năng, giao diện & định dạng Excel (HTML trực tiếp hoặc file Excel).|
   |                                           [ Đã hiểu — Bắt đầu dùng thử ]     |
   +------------------------------------------------------------------------------+

[2. Bên trong PHIEU-KHAO-SAT-GOP-Y.html (mở từ Web hoặc trực tiếp trong BAN-GIAO/)]
+-----------------------------------------------------------------------------------+
|  PHIẾU KHẢO SÁT & GÓP Ý NGHIỆM THU HỆ THỐNG IRM (v1.0.2 - R6)                     |
|  [Dải hướng dẫn nổi bật khi mở: Bước 1 Chọn đánh giá -> Bước 2 Góp ý -> Bước 3 Xuất]|
|  Tiến độ: [ 0 / 22 hạng mục ]   Đạt: 0 | Cần sửa: 0 | Lỗi: 0                      |
|  [Xuất CSV (Excel)]  [Xuất JSON]  [Sao chép tóm tắt]  [In / Lưu PDF]  [Xóa nháp] |
+-----------------------------------------------------------------------------------+
|  I. THÔNG TIN ĐƠN VỊ | II. 8 PHÂN HỆ CHỨC NĂNG | III. GIAO DIỆN | IV. EXCEL      |
+-----------------------------------------------------------------------------------+
```

---

### Task 1: Bộ Sinh File Excel Bằng `ClosedXML` & Kiểm Chứng File Test `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` (TDD)

**Files:**
- Create: `IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs`
- Modify: `IRM/IRM/Program.cs:1-20` (bổ sung nhánh `--export-delivery-workbooks <outputDir>` gọi `DeliveryWorkbooks.WriteAll(...)` trước khi mở DB)
- Create: `IRM/IRM.Tests/DeliverySampleWorkbookTests.cs`
- Modify: `IRM/IRM.Tests/IRM.Tests.csproj:24-29`
- Generate & Track Fixture: `IRM/IRM.Tests/Fixtures/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`

**Interfaces:**
- Produces trong `IRM.Infrastructure.Excel.DeliveryWorkbooks`:
  - `public static byte[] CreateSampleImportWorkbook()`
  - `public static void WriteSampleImportWorkbook(string path)`
  - `public static void WriteAll(string directoryPath)` (sẽ gọi cả `CreateSampleImportWorkbook` và `CreateFeedbackSurveyWorkbook` sau Task 2)

- [ ] **Step 1: Thêm cấu hình copy `Fixtures/**/*` vào `IRM/IRM.Tests/IRM.Tests.csproj` và viết test thất bại trước (RED)**

Cập nhật `<ItemGroup>` cuối cùng trong `IRM/IRM.Tests/IRM.Tests.csproj` thêm dòng:
`<None Update="Fixtures/**/*" CopyToOutputDirectory="PreserveNewest" />`

Tạo tệp `IRM/IRM.Tests/DeliverySampleWorkbookTests.cs` với bài test `SampleTestWorkbook_ImportsTenRowsCleanlyOnFreshR6SeededDatabase()`:
- Khởi tạo `TestDatabase` với `seedBaseline: false`.
- Gọi `await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db.Context, "admin_r6", "AdminR6@123456!");` và `await DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync(db.Context);`.
- Mở tệp `Path.Combine(AppContext.BaseDirectory, "Fixtures", "IRM-MAU-IMPORT-TEST-DA-SUA.xlsx")`.
- Kiểm tra 4 cột mã định danh (`Số hộ chiếu`, `Số GPLĐ`, `Số Visa`, `CMND người thân`) có định dạng Text `"@"` và nội dung file khớp chính xác cấu trúc `DeliveryWorkbooks.CreateSampleImportWorkbook()`.
- Gọi `ExcelReaderHelper.ReadSheetNames(stream)` (khẳng định sheet đầu tiên là `"Mẫu Import"`), `ExcelReaderHelper.ReadHeaders(stream)` (khẳng định đủ 19 cột), `ImportMapping.AutoMapColumns(headers)` (khẳng định **0 cột** `"skip"` và `ImportMapping.GetMissingRequiredFields(mappings)` rỗng).
- Lấy `companyId` của `DatabaseSeeder.InitialCompanyName` và `careerGroupId` của nhóm `"Kỹ thuật"`.
- Gọi `SqlImportStore.GeneratePreviewAsync(stream, mappings, companyId, new ImportReadOptions { CareerGroupId = careerGroupId })`:
  - `Assert.Equal(10, preview.Count);`
  - `Assert.All(preview, row => Assert.Equal("new", row.Status));`
  - `Assert.All(preview, row => Assert.Null(row.ErrorMessage));`
  - Kiểm tra mọi `row.ParsedEmployee!.Nationality` dài đúng 3 chữ cái ASCII (`LegacyImportRules.IsValidNationalityCode(...)`).
  - Kiểm tra dòng thứ 10 (`IRMTEST26010`) có `WorkPermit == WorkPermitType.Other` và `IDCareer == relativeCareerId` (`"Thân nhân"`).
  - Kiểm tra dòng thứ 1 (`IRMTEST26001`) giữ nguyên `FamilyVisitRelativeIdCard == "000000000001"`.
- Gọi `await store.ExecuteImportAsync(preview, [], companyId, "IRM-MAU-IMPORT-TEST-DA-SUA.xlsx")`:
  - `Assert.True(result.Success); Assert.Equal(10, result.AddedCount); Assert.Equal(0, result.ErrorCount);`
- Nạp lại cùng file lần 2 với `GeneratePreviewAsync`:
  - `Assert.All(secondPreview, row => Assert.Equal("unchanged", row.Status));`

- [ ] **Step 2: Chạy bài test vừa viết để xác nhận RED**

Run: `dotnet test IRM\IRM.Tests\IRM.Tests.csproj --filter "FullyQualifiedName~DeliverySampleWorkbookTests"`
Expected: FAIL biên dịch/chạy vì chưa có `DeliveryWorkbooks` và tệp `Fixtures\IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`.

- [ ] **Step 3: Viết `DeliveryWorkbooks.CreateSampleImportWorkbook()` bằng `ClosedXML`, thêm cờ CLI trong `Program.cs` và xuất file fixture**

Trong `IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs` (dùng `ClosedXML.Excel`):
- Cài đặt `CreateSampleImportWorkbook()` tạo 2 sheet:
  - Sheet 1 `"Mẫu Import"`: 19 tiêu đề cột ở dòng 1 và 10 dòng dữ liệu `IRMTEST26001`–`IRMTEST26010` ở dòng 2–11 đúng bảng trong Spec (mã quốc tịch 3 chữ cái `USA`, `KOR`, `JPN`, `CHN`, `AUS`, `GBR`, `IND`; địa chỉ kết thúc bằng `Phường Hồng Gai` / `Phường Bãi Cháy` / `Phường Cẩm Trung`; nghề `Kỹ sư` cho dòng 1–9 và `Thân nhân` + `GPLĐ = Khác` cho dòng 10; đặt `Style.NumberFormat.Format = "@"` cho toàn bộ cột `Số hộ chiếu`, `Số GPLĐ`, `Số Visa`, `CMND người thân` để giữ nguyên số 0 đầu; đặt ô ngày dạng DateTime chuẩn định dạng `dd/MM/yyyy`).
  - Sheet 2 `"Hướng dẫn"`: giải thích điều kiện chạy trên DB cài mới (`Công ty: CÔNG TY MẪU - HÃY ĐỔI TÊN`, `Nhóm nghề: Kỹ thuật`), quy tắc WPF (quốc tịch 3 chữ cái, đuôi phường/xã mặc định, `GPLĐ = Số / Miễn / trống / Khác → Thân nhân`), và cách thử nhập lại phát hiện trùng & hoàn tác.
- Cài đặt nhánh đầu `IRM/IRM/Program.cs` trước `WebApplication.CreateBuilder(args)`:
  ```csharp
  var exportIdx = Array.FindIndex(args, a => a.Equals("--export-delivery-workbooks", StringComparison.OrdinalIgnoreCase));
  if (exportIdx >= 0 && exportIdx + 1 < args.Length)
  {
      IRM.Infrastructure.Excel.DeliveryWorkbooks.WriteAll(args[exportIdx + 1]);
      return;
  }
  ```
- Xuất file fixture bằng chính `.NET CLI`:
  `dotnet run --project IRM\IRM\IRM.csproj -- --export-delivery-workbooks IRM\IRM.Tests\Fixtures` (và xóa tạm `PHIEU-KHAO-SAT-GOP-Y.xlsx` trong `Fixtures` nếu chưa tới Task 2 hoặc tách `WriteSampleImportWorkbook` / chưa xuất bản khảo sát tại bước này).

- [ ] **Step 4: Chạy lại test để xác nhận GREEN**

Run: `dotnet test IRM\IRM.Tests\IRM.Tests.csproj --filter "FullyQualifiedName~DeliverySampleWorkbookTests"`
Expected: `Passed: 1, Failed: 0, Skipped: 0`.

- [ ] **Step 5: Commit Task 1**

```powershell
git add IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs IRM/IRM/Program.cs IRM/IRM.Tests/IRM.Tests.csproj IRM/IRM.Tests/DeliverySampleWorkbookTests.cs IRM/IRM.Tests/Fixtures/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx
git commit -m "test(import): add ClosedXML R6 sample workbook builder and fresh-seed verification"
```

---

### Task 2: Phiếu Khảo Sát (`HTML` & `Excel ClosedXML`) + Hiệu Ứng UI Spotlight Ban Đầu Trên Ứng Dụng Web (TDD)

**Files:**
- Modify: `IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs` (bổ sung `CreateFeedbackSurveyWorkbook()` và hoàn thiện `WriteAll(directoryPath)`)
- Create: `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.html`
- Generate & Track: `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.xlsx`
- Create (copy đồng bộ từ `wwwroot` cho tài liệu nguồn): `IRM/docs/PHIEU-KHAO-SAT-GOP-Y.html`
- Modify: `IRM/IRM/Components/Shared/MainLayout.razor:48-115,224-281`
- Modify: `IRM/IRM/wwwroot/irm-interop.js:355-403` (bổ sung `initFeedbackSpotlight(forceReplay)` và `dismissFeedbackSpotlight()`)
- Modify: `IRM/IRM/wwwroot/app.css` (bổ sung style nút `.irm-feedback-btn`, hiệu ứng `@keyframes irmFeedbackPulse`, `.irm-feedback-highlight`, `.irm-feedback-fallback-tip`, hỗ trợ `[data-theme="dark"]` và `@media (prefers-reduced-motion: reduce)`)
- Modify: `IRM/IRM/Components/App.razor:10` (cập nhật cache token `app.css?v=mui-gov-20261115-r6` và `irm-interop.js?v=mui-gov-20261115-r6`)
- Modify: `IRM/IRM.Tests/DeliverySampleWorkbookTests.cs` (thêm test kiểm chứng `PHIEU-KHAO-SAT-GOP-Y.xlsx`, `MainLayout.razor`, `wwwroot/PHIEU-KHAO-SAT-GOP-Y.html`)
- Create: `IRM/IRM.Tests/Browser/feedback-spotlight.test.cjs` (kiểm thử JS bằng `node --test` cho `irmInterop.initFeedbackSpotlight`)

**Interfaces:**
- Produces trong `IRM.Infrastructure.Excel.DeliveryWorkbooks`:
  - `public static byte[] CreateFeedbackSurveyWorkbook()`
- Produces trong `window.irmInterop`:
  - `initFeedbackSpotlight(forceReplay?: boolean): boolean`
  - `dismissFeedbackSpotlight(): void`
- Produces trên `MainLayout.razor`:
  - Nút Topbar `#btnCustomerFeedback` ("Khảo sát & Góp ý") + gọi `JS.InvokeVoidAsync("irmInterop.initFeedbackSpotlight", false)` trong `OnAfterRenderAsync(true)`

- [ ] **Step 1: Viết các bài kiểm thử thất bại trước (RED) cho Excel Khảo sát và JS Spotlight**

1. Thêm test vào `IRM/IRM.Tests/DeliverySampleWorkbookTests.cs`:
   - `FeedbackSurveyWorkbook_HasThreeSheetsValidationAndCountIfFormulas()`: kiểm tra `DeliveryWorkbooks.CreateFeedbackSurveyWorkbook()` tạo ra 3 sheet (`"Tổng hợp & Thông tin"`, `"Chi tiết khảo sát"`, `"Đề xuất cột Excel thực tế"`), Sheet 2 có 22 dòng hạng mục (`rows 2..23`), `Sheet2.DataValidations` chứa danh sách `"\"Đạt yêu cầu,Cần chỉnh sửa nhỏ,Không đạt / Lỗi,Chưa thử\""`, và Sheet 1 chứa công thức `COUNTIF` trỏ tới `'Chi tiết khảo sát'!$E$2:$E$23`.
   - `StaticFeedbackAssets_AndMainLayoutTrigger_ArePresentOffline()`: kiểm tra `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.html`, `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.xlsx` tồn tại; file HTML không chứa URL ngoài `https?://` trong thẻ `<script>`/`<link>`; và `MainLayout.razor` chứa `id="btnCustomerFeedback"` cùng `initFeedbackSpotlight`.
2. Tạo `IRM/IRM.Tests/Browser/feedback-spotlight.test.cjs` (tái sử dụng `theme-fixture.cjs` hoặc mock DOM/localStorage + mock `window.driver.js.driver`) kiểm tra:
   - Lần đầu gọi `initFeedbackSpotlight(false)` khi `localStorage['irm-r6-feedback-seen']` chưa có → trả về `true`, gắn class `irm-feedback-highlight` vào `#btnCustomerFeedback`, gọi `driver.drive(0)`.
   - Khi bấm đóng (`dismissFeedbackSpotlight()`), `localStorage.getItem('irm-r6-feedback-seen') === '1'`, gỡ class `irm-feedback-highlight`.
   - Lần tiếp theo gọi `initFeedbackSpotlight(false)` → trả về `false` (không tự bật lại); gọi `initFeedbackSpotlight(true)` → trả về `true` (cho phép xem lại).

Run:
- `dotnet test IRM\IRM.Tests\IRM.Tests.csproj --filter "FullyQualifiedName~DeliverySampleWorkbookTests"` → Expected: FAIL.
- `node --test IRM\IRM.Tests\Browser\feedback-spotlight.test.cjs` → Expected: FAIL (`initFeedbackSpotlight` chưa tồn tại).

- [ ] **Step 2: Cài đặt `CreateFeedbackSurveyWorkbook()` trong `DeliveryWorkbooks.cs` & xuất `PHIEU-KHAO-SAT-GOP-Y.xlsx` vào `wwwroot`**

Trong `IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs`:
- Định nghĩa danh sách chuẩn 22 hạng mục đồng bộ 1-1 với bản HTML (`STT`, `Nhóm`, `Hạng mục`, `Nội dung kiểm tra`).
- Tạo 3 sheet bằng `ClosedXML`:
  1. `"Tổng hợp & Thông tin"`: tiêu đề `PHIẾU KHẢO SÁT & GÓP Ý NGHIỆM THU IRM v1.0.2 (R6)`, 6 dòng thông tin đơn vị/người đánh giá, và bảng tổng hợp với công thức `=COUNTIF('Chi tiết khảo sát'!$E$2:$E$23,"...")` và `=COUNTA('Chi tiết khảo sát'!$E$2:$E$23)`.
  2. `"Chi tiết khảo sát"`: 22 dòng dữ liệu (dòng 2–23), gắn `ws.Range("E2:E23").CreateDataValidation().List("\"Đạt yêu cầu,Cần chỉnh sửa nhỏ,Không đạt / Lỗi,Chưa thử\"", true);`, đóng băng dòng 1, bật `WrapText`.
  3. `"Đề xuất cột Excel thực tế"`: bảng 10 dòng trống (`STT | Áp dụng cho (Import/Export) | Tên cột trong file đơn vị | Bắt buộc / Tùy chọn | Ví dụ dữ liệu | Yêu cầu xử lý`).
- Xuất file vào `IRM/IRM/wwwroot/` qua `--export-delivery-workbooks` (bỏ/không ghi đè fixture nếu chỉ lấy file khảo sát, hoặc đồng bộ cả `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` và `PHIEU-KHAO-SAT-GOP-Y.xlsx` vào thư mục tạm rồi chép `PHIEU-KHAO-SAT-GOP-Y.xlsx` vào `IRM/IRM/wwwroot/`).

- [ ] **Step 3: Tạo `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.html` (và đồng bộ sang `IRM/docs/PHIEU-KHAO-SAT-GOP-Y.html`)**

- Giao diện 100% offline theo tông Olive/Cam (`--bg: #eef5dc`, `--olive-primary: #3f5223`, `--accent: #ea580c`), không có `<script src>` hoặc `<link>` trỏ ra ngoài Internet.
- **Hiệu ứng dẫn hướng ban đầu khi mở trang:** dải banner hướng dẫn 3 bước ở ngay dưới header kèm hiệu ứng viền sáng nổi bật trong 2,5 giây ở thanh nút thao tác (tôn trọng `prefers-reduced-motion: reduce`).
- Đầy đủ Phần I (Thông tin đơn vị), Phần II (10 mục chức năng thuộc 8 phân hệ), Phần III (4 mục giao diện & cảnh báo trường bắt buộc `*`), Phần IV (8 mục quy tắc/định dạng Excel + Bảng 8 dòng đề xuất cột thực tế của đơn vị), Phần V (Kết luận & ưu tiên), tự lưu `localStorage['irm_r6_feedback_v1']`, đếm tiến độ live `X/22`, nút **Xuất CSV (Excel)** có `\uFEFF`, **Xuất JSON**, **Sao chép tóm tắt**, **In / Lưu PDF**, **Xóa nháp**.

- [ ] **Step 4: Tích hợp nút & hiệu ứng spotlight ban đầu trên `MainLayout.razor`, `irm-interop.js`, `app.css` và `App.razor`**

1. **`IRM/IRM/Components/Shared/MainLayout.razor`:**
   - Trong `.topbar-actions` (trước nút Table Density Toggle), thêm `<MudMenu>` với `ActivatorContent` là nút nổi bật `id="btnCustomerFeedback"`, icon `Icons.Material.Outlined.RateReview`, chữ `"Khảo sát & Góp ý"`, badge `"R6"`.
   - Menu gồm 3 mục:
     - *"Mở phiếu khảo sát trực tuyến (HTML)"* (gọi JS `open` `/PHIEU-KHAO-SAT-GOP-Y.html` ở tab mới + gọi `dismissFeedbackSpotlight`)
     - *"Tải phiếu khảo sát Excel (.xlsx)"* (thẻ `<a href="/PHIEU-KHAO-SAT-GOP-Y.xlsx" download="PHIEU-KHAO-SAT-GOP-Y.xlsx">`)
     - *"Xem lại hướng dẫn vị trí góp ý"* (gọi `JS.InvokeVoidAsync("irmInterop.initFeedbackSpotlight", true)`)
   - Trong `OnAfterRenderAsync(bool firstRender)`: khi `firstRender`, gọi `await JS.InvokeVoidAsync("irmInterop.initFeedbackSpotlight", false);` trong khối `try/catch (JSException)`.
2. **`IRM/IRM/wwwroot/irm-interop.js`:**
   - Bổ sung `initFeedbackSpotlight(forceReplay)` và `dismissFeedbackSpotlight()`: kiểm tra `localStorage['irm-r6-feedback-seen']`, bật class `.irm-feedback-highlight` trên `#btnCustomerFeedback`, ưu tiên chạy 1-step `window.driver.js.driver` trỏ tới `#btnCustomerFeedback` (nếu `window.driver?.js?.driver` tồn tại), đồng thời có fallback CSS callout badge nếu Driver.js không khả dụng trong trình duyệt cũ; gọi `dismissFeedbackSpotlight()` khi đóng hoặc click mở menu.
3. **`IRM/IRM/wwwroot/app.css` & `IRM/IRM/Components/App.razor`:**
   - Định nghĩa `.irm-feedback-btn`, `@keyframes irmFeedbackPulse`, `.irm-feedback-highlight`, `.irm-feedback-popover` (sáng/tối `[data-theme="dark"]` và `@media (prefers-reduced-motion: reduce)`).
   - Cập nhật cache token trong `IRM/IRM/Components/App.razor`: `app.css?v=mui-gov-20261115-r6` và `irm-interop.js?v=mui-gov-20261115-r6` (đúng regex `/^app\.css\?v=mui-gov-\d{8}(?:-[\w-]+)?$/` của `theme-preference.test.cjs`).

- [ ] **Step 5: Chạy toàn bộ test kiểm chứng Task 2 (GREEN) & Commit**

Run:
1. `dotnet test IRM\IRM.Tests\IRM.Tests.csproj --filter "FullyQualifiedName~DeliverySampleWorkbookTests"` → Expected: ALL PASS.
2. `node --test IRM\IRM.Tests\Browser\*.test.cjs` → Expected: ALL PASS (cả 4 file cũ + `feedback-spotlight.test.cjs`).
3. Kiểm tra JS inline trong `PHIEU-KHAO-SAT-GOP-Y.html` không có lỗi cú pháp qua `node --check` trên file JS tạm (nhớ xóa tệp tạm sau kiểm tra).

```powershell
git add IRM/IRM.Infrastructure/Excel/DeliveryWorkbooks.cs IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.html IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.xlsx IRM/docs/PHIEU-KHAO-SAT-GOP-Y.html IRM/IRM/Components/Shared/MainLayout.razor IRM/IRM/Components/App.razor IRM/IRM/wwwroot/irm-interop.js IRM/IRM/wwwroot/app.css IRM/IRM.Tests/DeliverySampleWorkbookTests.cs IRM/IRM.Tests/Browser/feedback-spotlight.test.cjs
git commit -m "feat(feedback): add offline survey assets and initial UI spotlight guidance for R6"
```

---

### Task 3: Cập Nhật Cấu Hình & Tài Liệu Đóng Gói `R6`, Kiểm Thử Toàn Bộ & Commit Sạch

**Files:**
- Modify: `IRM/deploy/release-manifest.json:34`
- Modify: `IRM/deploy/windows/irm.iss:22`
- Modify: `IRM/docs/INSTALLATION-v1.0.2.md:6,10,16`
- Create: `IRM/docs/HUONG-DAN-CAI-DAT.html` (đồng bộ từ bản R5 HTML, cập nhật sang `R6`, bổ sung hướng dẫn thử file `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` với công ty mẫu + nhóm `Kỹ thuật` và hướng dẫn phản hồi qua nút **Khảo sát & Góp ý** trên Web hoặc `PHIEU-KHAO-SAT-GOP-Y.html`/`.xlsx`)

- [ ] **Step 1: Cập nhật manifest, Inno Setup & hướng dẫn cài đặt sang `R6`**

- `IRM/deploy/release-manifest.json`: đổi `"artifact"` sang `"IRM-v1.0.2-windows-x64-offline-setup-R6.exe"`.
- `IRM/deploy/windows/irm.iss`: đổi `OutputBaseFilename=IRM-v1.0.2-windows-x64-offline-setup-R6`.
- `IRM/docs/INSTALLATION-v1.0.2.md` & `IRM/docs/HUONG-DAN-CAI-DAT.html`: cập nhật tên file & hướng dẫn từ `R5` sang `R6`, bổ sung hướng dẫn kiểm thử nhanh `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` và điền `PHIEU-KHAO-SAT-GOP-Y.html`/`.xlsx`.

- [ ] **Step 2: Chạy toàn bộ bộ kiểm thử trước khi commit**

Run:
1. `dotnet build IRM\IRM.sln -c Release`
   Expected: `0 Warning(s), 0 Error(s)`.
2. `dotnet test IRM\IRM.Tests\IRM.Tests.csproj -c Release --no-build`
   Expected: Toàn bộ test SQLite/unit/bUnit đạt (`0 Failed`; ghi rõ số lượng `SqlServerFact` skipped nếu không đặt `IRM_TEST_SQLSERVER`).
3. `powershell -NoProfile -ExecutionPolicy Bypass -File IRM\deploy\windows\tests\environment.Tests.ps1`
   Expected: `PASS: 46 assertions...`.
4. `& "$env:SystemRoot\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File IRM\deploy\windows\tests\environment.Tests.ps1`
   Expected: `PASS: 47 assertions...`.
5. `node --test IRM\IRM.Tests\Browser\*.test.cjs`
   Expected: Toàn bộ đạt (`0 Fail`).

- [ ] **Step 3: Rà soát diff & Commit sạch Git trước khi đóng gói bộ cài**

```powershell
git diff --stat
git add IRM/deploy/release-manifest.json IRM/deploy/windows/irm.iss IRM/docs/INSTALLATION-v1.0.2.md IRM/docs/HUONG-DAN-CAI-DAT.html
git commit -m "chore(release): prepare installer configuration and guides for R6"
git status -sb
```
Expected: `nothing to commit, working tree clean` (`build-windows-installer.ps1` sẽ đọc ra `dirty=false`).

---

### Task 4: Lưu Trữ `R5`, Đóng Gói Bộ Cài `R6`, Hoàn Thiện Thư Mục Bàn Giao & Cập Nhật Truy Vết

**Files (ngoài Git — `BAN-GIAO/`):**
- Move (nguyên vẹn): `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` → `BAN-GIAO/02-LUU-TRU/v1.0.2-R5-UNSIGNED-TEST/`
- Generate build: `BAN-GIAO/04-CHO-NGHIEM-THU/20261115-v1.0.2-R6-UNSIGNED-TEST/` → tập hợp thành phẩm vào `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`
- Update archive test data: `BAN-GIAO/03-DU-LIEU-TEST/2026-11-15/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` + `README.md`
- Update: `BAN-GIAO/README.md`, `BAN-GIAO/DANH-MUC-TEP.json`

**Files (trong Git — `IRM/docs/`):**
- Create: `IRM/docs/RELEASE-READINESS-R6.md`
- Modify: `IRM/docs/DELIVERY-LAYOUT.md`, `IRM/docs/DELIVERY-INVENTORY.json`

- [ ] **Step 1: Chuyển nguyên vẹn `R5` vào lưu trữ trước khi đóng gói `R6`**

Run:
```powershell
Move-Item -LiteralPath "BAN-GIAO\01-HIEN-HANH\v1.0.2-R5-UNSIGNED-TEST" -Destination "BAN-GIAO\02-LUU-TRU\v1.0.2-R5-UNSIGNED-TEST"
```
Verify: Đối chiếu SHA-256 của `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` trong `BAN-GIAO\02-LUU-TRU\v1.0.2-R5-UNSIGNED-TEST\` vẫn khớp `C10A1915646381331DB3481BCA0D2F546CAF3B54440721830DC38A8FB9125D45`.

- [ ] **Step 2: Chạy `build-windows-installer.ps1 -AllowUnsigned` từ commit sạch**

Run từ thư mục `IRM`:
```powershell
Set-Location IRM
.\deploy\windows\build-windows-installer.ps1 -OutputDirectory "..\BAN-GIAO\04-CHO-NGHIEM-THU\20261115-v1.0.2-R6-UNSIGNED-TEST" -AllowUnsigned
```
Expected:
- Cảnh báo `-AllowUnsigned`, biên dịch Inno Setup thoát mã `0`.
- Sinh `IRM-v1.0.2-windows-x64-offline-setup-R6.exe` và `.exe.sha256`.
- `windows-stage\SOURCE-REVISION.json` có `"dirty": false` và `"commit"` khớp đúng `git rev-parse HEAD`.

- [ ] **Step 3: Tập hợp đủ 11 tệp vào `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/` & tạo dữ liệu test ngày `2026-11-15`**

1. Tạo thư mục `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/` và `BAN-GIAO/03-DU-LIEU-TEST/2026-11-15/`.
2. Di chuyển `IRM-v1.0.2-windows-x64-offline-setup-R6.exe` và sao chép `.exe.sha256` sang `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`; giữ `SOURCE-REVISION.json`, `SHA256SUMS.txt` của stage (đổi tên thành `stage-SHA256SUMS.txt`) và `.exe.sha256` tại `BAN-GIAO/04-CHO-NGHIEM-THU/20261115-v1.0.2-R6-UNSIGNED-TEST/` rồi dọn thư mục `windows-stage/` nặng.
3. Chép các tài sản nguồn đồng bộ vào `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/`:
   - `SOURCE-REVISION.json`
   - `HUONG-DAN-CAI-DAT.md` (từ `IRM/docs/INSTALLATION-v1.0.2.md`)
   - `HUONG-DAN-CAI-DAT.html` (từ `IRM/docs/HUONG-DAN-CAI-DAT.html`)
   - `PHIEU-KHAO-SAT-GOP-Y.html` (từ `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.html`)
   - `PHIEU-KHAO-SAT-GOP-Y.xlsx` (từ `IRM/IRM/wwwroot/PHIEU-KHAO-SAT-GOP-Y.xlsx`)
   - `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` (từ `IRM/IRM.Tests/Fixtures/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`; đồng thời chép sang `BAN-GIAO/03-DU-LIEU-TEST/2026-11-15/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` kèm `README.md`)
4. Tạo mới `DOC-TRUOC-KHI-CAI.txt` và `VERIFICATION.json` cho R6 (ghi đúng commit nguồn, kích thước EXE, SHA-256 EXE, kết quả toàn bộ test `.NET`/`PowerShell`/`Node JS`, `signed: false`, `fullR6InstallPerformed: false`).
5. Tạo `SHA256SUMS.txt` cho 10 tệp còn lại trong `BAN-GIAO/01-HIEN-HANH/v1.0.2-R6-UNSIGNED-TEST/` và xác minh từng dòng hash khớp 100%.

- [ ] **Step 4: Cập nhật tài liệu tổng hợp & Commit release docs**

1. Viết `IRM/docs/RELEASE-READINESS-R6.md` ghi rõ các thay đổi trong `R6` (quy tắc WPF/quốc tịch 3 chữ cái, cảnh báo trường bắt buộc, hiệu ứng spotlight nút feedback trên Web, file Excel mẫu 10/10, cặp phiếu khảo sát HTML/Excel), commit nguồn EXE, SHA-256, kết quả kiểm chứng và các giới hạn còn lại.
2. Cập nhật `BAN-GIAO/README.md`, `IRM/docs/DELIVERY-LAYOUT.md`, `IRM/docs/DELIVERY-INVENTORY.json` và `BAN-GIAO/DANH-MUC-TEP.json`.
3. Commit tài liệu bàn giao:
```powershell
git add IRM/docs/RELEASE-READINESS-R6.md IRM/docs/DELIVERY-LAYOUT.md IRM/docs/DELIVERY-INVENTORY.json
git commit -m "docs(release): publish R6 delivery package, sample workbook, and feedback survey"
git status -sb
```
Expected: Working tree sạch, `BAN-GIAO/01-HIEN-HANH/` chỉ có duy nhất thư mục `v1.0.2-R6-UNSIGNED-TEST/` với đủ 11 tệp khớp SHA-256.
