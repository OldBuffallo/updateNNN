# Import Driver.js Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đưa hướng dẫn đúng đặc tả 35 trường và tour Driver.js lên UI import thật, hỗ trợ người nhập mà không tự thực hiện nghiệp vụ thay họ.

**Architecture:** Catalog trợ giúp chỉ đọc và component tra cứu nằm trong Web feature; wrapper JS quản lý bốn chương tour với điểm bám ổn định. Blazor gửi snapshot UI sau render và giữ các hành động import hiện tại; Driver.js phục vụ cục bộ, không dùng dịch vụ demo ngoài. Tách nội dung, adapter JS và vòng đời trang để kiểm chứng độc lập rồi thử tích hợp với thư viện thật.

**Tech Stack:** ASP.NET Core 8 Blazor Server; MudBlazor 9.3.0 hiện có; Driver.js 1.12.1 IIFE/CSS cục bộ; xUnit/HtmlRenderer và Node `node:test` hiện có. Không thêm .NET package hay npm build pipeline.

**Spec:** `docs/superpowers/specs/2026-11-13-import-driverjs-guidance-design.md`. Người dùng đã chọn live tour + Driver.js và yêu cầu viết kế hoạch; các đề xuất UI trong spec/kế hoạch cần review trước khi triển khai. Giữ cách thực hiện trực tiếp từng task (`executing-plans`) đã dùng trong phiên này; không tự chuyển sang một implementer/reviewer cho mỗi task.

## Global Constraints

- Chỉ thay UI hướng dẫn tại `/import`, static assets và test/tài liệu liên quan; không đổi parser, quy tắc import, schema, seed hoặc dữ liệu nghiệp vụ.
- Giữ Git root `NNN/`, nhánh `develop`, solution `IRM/IRM.sln`; không sửa WPF/reference, không push, không build lại hoặc đổi nhãn bộ cài cũ.
- UI tiếng Việt; ứng dụng chỉ ghi nhận số đã có, không cấp hoặc tự sinh số hộ chiếu, GPLĐ, giấy tờ hoặc định danh.
- Trợ giúp phủ đúng 35 khóa trong `SystemFields.All`, loại `skip`; chín trường bắt buộc giữ đúng như đặc tả, không thêm validator nghiệp vụ.
- Phân biệt rõ “Chương trình kiểm tra” và “Khuyến nghị nhập liệu”; ví dụ danh mục chỉ đúng khi có dữ liệu tương ứng tại database đích.
- Dùng Driver.js `1.12.1` (MIT), phục vụ JS/CSS cục bộ trong `wwwroot/vendor/driverjs/1.12.1/`; không CDN runtime, không analytics, không gửi dữ liệu import ra dịch vụ ngoài.
- Tour chỉ mở theo yêu cầu; không tự chọn file, đổi giá trị/ghép cột, bật tạo công ty, chọn cập nhật, xác nhận import hoặc hoàn tác.
- Tiến độ tour chỉ giữ trong bộ nhớ của trang; đóng/rời trang kết thúc tour, không lưu file, hộ chiếu hoặc dữ liệu hồ sơ vào storage trình duyệt.
- Trợ giúp vẫn dùng được khi tour lỗi; lỗi JS, phần tử thiếu hoặc mất kết nối Blazor không được làm hỏng luồng import thông thường.

## Review Focus

1. Popup MudBlazor được render ngoài điểm bám: người dùng vẫn chọn được công ty/nhóm/trường bằng chuột và bàn phím; Task 2 test allowedElements, Task 4 thử Driver thật với popup thật.
2. Server chậm, render lặp, preview lỗi hoặc thay file giữa tour: không nhảy chương sớm, không dùng điểm bám cũ hoặc tự mở lại sau đóng; Task 2 state tests và Task 3 bridge tests, Task 4 thử luồng thật.
3. Quyền ghi và hoàn tác trên dữ liệu thật: mở/đi tour không gọi ExecuteImport/Rollback/SaveTemplate, không thay quyết định cập nhật; Task 3 spy bridge và Task 4 đối chiếu dữ liệu giả, mọi ghi thử chỉ dùng database biệt lập.
4. Nội dung hợp lệ về cú pháp nhưng sai nghĩa: Nam/Nữ, nhãn GPLĐ, danh mục đích, ngày/danh sách `;`, blank/unmapped và trường có điều kiện phải đúng đặc tả; Task 1 catalog/render checks, Task 4 walkthrough với bộ golden có điều kiện.
5. Màn hình hẹp/dark, bàn phím, missing element, rời trang/circuit mất hoặc chặn mạng ngoài: hướng dẫn đọc được, đóng được và không giữ overlay/listener cũ; Task 2 cancellation/dispose/fallback tests, Task 3 dispose tests, Task 4 browser acceptance.

---

## File map và môi trường

Đường dẫn dưới đây tương đối với `IRM/`; mọi lệnh .NET chạy từ thư mục này. Git root là thư mục cha. Baseline `51cecbd`; trước triển khai kiểm tra lại diff nguồn import nếu HEAD đã đổi. Không đọc cấu hình chứa bí mật hoặc dùng database nghiệp vụ để thử ghi.

| Tệp | Trách nhiệm |
|---|---|
| Create `IRM/Features/ImportExport/Help/ImportFieldHelpCatalog.cs` | Record và 35 mục trợ giúp chỉ đọc, dùng tên từ SystemFields |
| Create `IRM/Features/ImportExport/Components/ImportFieldHelpContent.razor` | Render một mục trợ giúp bằng text Razor được encode |
| Create `IRM/Features/ImportExport/Components/ImportFieldHelpDialog.razor` | Chọn/tra cứu 35 trường, đóng và focus theo MudDialog hiện có |
| Create `IRM/Features/ImportExport/Import.Guidance.razor.cs` | Partial điều phối hộp trợ giúp và JS tour |
| Create `IRM/Features/ImportExport/Help/ImportTourSnapshot.cs` | DTO chỉ có trạng thái UI gửi sang JS |
| Modify `IRM/Features/ImportExport/Import.razor`, `Import.razor.cs` | Nút mở, root, vòng đời, dừng/đổi ngữ cảnh; không sửa hành vi ghi |
| Modify bốn `Components/Import{Upload,Mapping,Preview,Result}Section.razor` | Điểm bám tour, trợ giúp ghép cột, sửa lời rollback sai |
| Create `IRM/wwwroot/vendor/driverjs/1.12.1/` | IIFE, CSS, LICENSE, manifest nguồn/checksum |
| Create `IRM/wwwroot/js/import-tour.js`, `css/import-guidance.css` | Wrapper vòng đời Driver và CSS scope riêng |
| Modify `IRM/Components/App.razor` | Đăng ký asset cục bộ đúng thứ tự |
| Create `IRM.Tests/ImportFieldHelpTests.cs`, `ImportGuidanceRenderingTests.cs`, `ImportGuidanceInteropTests.cs` | Nội dung, markup và bridge theo mẫu HtmlRenderer sẵn có |
| Create `IRM.Tests/Browser/import-tour-fixture.cjs`, `import-tour.test.cjs` | Hành vi wrapper bằng DOM/Driver spy, không thay nghiệm thu browser thật |
| Modify hai hướng dẫn import và `docs/README.md` | Cách mở tour/tra cứu và mốc áp dụng source mới |
| Create `docs/IMPORT-GUIDANCE-VERIFICATION-2026-11-13.md` | Kết quả kiểm chứng thực tế và phần chưa nghiệm thu nếu có |

Không tạo code hoặc tải vendor trước khi kế hoạch được chấp thuận. Nếu đang chạy app local giữ đúng source/runtime; build có thể cần dừng tiến trình đúng app rồi khởi động lại, không giết tiến trình .NET khác. Bằng chứng thử UI không chứa thông tin cá nhân; artifact cần lưu đặt dưới `BAN-GIAO/05-HO-TRO/` sau khi đọc hướng dẫn delivery.

### Task 1: Tra cứu đủ 35 trường tại bước ghép cột

**Files:** Create catalog, HelpContent, HelpDialog, `Import.Guidance.razor.cs`, hai test `ImportFieldHelpTests.cs`/`ImportGuidanceRenderingTests.cs`; modify `ImportMappingSection.razor`. Read `SystemFields.cs`, `docs/IMPORT-EXCEL-SPEC.md`, parser/Comparison/ExtensionHistory và mẫu `ImportPreviewRenderingTests.cs`.

**Interfaces:**
- Namespace nội dung `IRM.Features.ImportExport.Help`; record public `ImportFieldHelp(string Key, string Group, bool IsRequired, string Condition, string ExcelType, string Summary, string Checks, string EmptyBehavior, string ValidExample, string Pitfall, string Troubleshooting, string Recommendation)`; property `Label: string` lấy từ `SystemFields.All[Key]`.
- `ImportFieldHelpCatalog.All: IReadOnlyDictionary<string, ImportFieldHelp>`; `Get(string? key): ImportFieldHelp?`, trả null cho null/unknown/skip.
- `ImportFieldHelpContent` ở namespace `IRM.Components.Pages`, parameter `Entry: ImportFieldHelp`; `ImportFieldHelpDialog` cùng namespace, parameter `FieldKey: string?`, null/skip khởi đầu StaffName, dropdown lấy toàn bộ 35 mục.
- Partial `Import` cung cấp `internal Task ShowFieldHelpAsync(string? fieldKey)` qua `IDialogService`, chưa phụ thuộc Driver. Task 3 bổ sung stop trước khi mở hộp.

- [ ] **Step 1: Viết test catalog và content trước.** Trong `ImportFieldHelpTests`: `Catalog_CoversExactlySupportedFields` so sánh tập khóa với `SystemFields.All.Keys` trừ skip, count 35; `Catalog_RequiredFieldsAreExactlyNine` so sánh với StaffName/Passport/Gender/Birthday/Nationality/Address/Career/VisaNumber/TemporaryStay; `Catalog_UsesSystemLabelsAndHasCompleteGuidance` kiểm tra tên và các phần không trống, Condition chỉ trống khi không cần điều kiện; `Lookup_UnknownAndSkipReturnNull` thử null/skip/unknown. HtmlRenderer `HelpContent_EncodesExamples` dùng Entry với ValidExample `<img src=x onerror=alert(1)>`, assert text được encode, không có thẻ img thực thi. Dùng cách DI MudServices/Navigation/StaticJs của test hiện tại, không thêm bUnit.

```csharp
[Fact]
public void Catalog_CoversExactlySupportedFields()
{
    var expected = SystemFields.All.Keys.Where(key => key != "skip").OrderBy(key => key).ToArray();
    Assert.Equal(35, ImportFieldHelpCatalog.All.Count);
    Assert.Equal(expected, ImportFieldHelpCatalog.All.Keys.OrderBy(key => key).ToArray());
}

[Fact]
public void Catalog_RequiredFieldsAreExactlyNine()
{
    string[] expected = ["StaffName", "Passport", "Gender", "Birthday", "Nationality", "Address", "Career", "VisaNumber", "TemporaryStay"];
    Assert.Equal(expected.OrderBy(key => key).ToArray(), ImportFieldHelpCatalog.All.Values
        .Where(entry => entry.IsRequired).Select(entry => entry.Key).OrderBy(key => key).ToArray());
}
```
- [ ] **Step 2: Chạy RED.** `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~ImportFieldHelpTests|FullyQualifiedName~ImportGuidanceRenderingTests.HelpContent"`. Expected: chưa có type/component mới nên build FAIL; không tiếp tục nếu lỗi do môi trường chứ không do thiếu phần cần xây.
- [ ] **Step 3: Viết catalog và content renderer theo interface.** Chuyển đúng 35 mục từ đặc tả, gồm cả ví dụ có điều kiện danh mục; chín IsRequired đúng tập trên. Giữ các phần “Chương trình kiểm tra”, “Không ghép / ô trống”, “Ví dụ”, “Lỗi và cách sửa”, “Khuyến nghị nhập liệu” tách rõ. Nội dung không dùng MarkupString/HTML từ file. Kiểm tra riêng Gender preview thiếu cột, GPLĐ hai cột/nhà đầu tư, ngày lõi/mở rộng, danh sách ;, V010 cần cặp/chủ và không xóa khi trống; không tạo validator mới.
- [ ] **Step 4: Chạy GREEN cho Step 2.** Expected: toàn bộ test mới đạt, không thiếu/trùng trường.
- [ ] **Step 5: Viết test mapping rồi chạy RED.** `Mapping_ExposesLookupAndHelpForMappedFields` render step 2 có mapping Passport và skip; assert nút Tra cứu 35 trường, mô tả Text/giữ số 0 đầu, nút trợ giúp có accessible label cho Passport, không có trợ giúp cho skip. `HelpContent_RendersPermitDatesAndConditionalGroups` render các entry Gender/WorkPermit/WorkPermitNumber/CardCreationDate/DocumentValidFrom/DocumentValidTo/ResidenceAddress, assert nội dung mang đúng các ý ở Step 3, không chỉ kiểm tra có component. Run filter `FullyQualifiedName~ImportGuidanceRenderingTests`; Expected: mapping chưa có UI mới nên FAIL.
- [ ] **Step 6: Thêm hộp tra cứu và ghép UI.** Dialog có selector cả 35 trường, content và đóng; options MaxWidth.Medium/FullWidth/CloseButton/CloseOnEscape theo mẫu hiện có. Partial thêm DI và ShowFieldHelpAsync; mapping thêm nút tra cứu chung và ? cho trường khác skip, Summary theo entry. Không thay OnMappingChanged, selected values hoặc template. Focus trở lại trigger qua hành vi dialog, kiểm tra thật ở Task 4.
- [ ] **Step 7: Kiểm chứng và commit.** Run `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~ImportFieldHelpTests|FullyQualifiedName~ImportGuidanceRenderingTests|FullyQualifiedName~ImportPreviewRenderingTests"`. Expected: PASS. Review staged diff chỉ phạm vi Task 1; commit `feat: add contextual help for Excel import fields`.

### Task 2: Wrapper tour Driver.js và asset cục bộ

**Files:** Create vendor folder, JS/CSS, Browser fixture/test; modify App.razor.

**Interfaces:**
- Consumes bốn chương/điểm bám trong spec; rootId là ID instance do Task 3 tạo, DOM bên trong root dùng `data-import-tour` như bảng spec; điểm `notice` là live-region trống.
- Produces `window.irmImportTour.start(rootId: string, snapshot: {stage:number,isBusy:boolean,isManual:boolean,contextVersion:number}): boolean`; `.sync(rootId, snapshot): void`; `.stop(rootId): void`; `.dispose(rootId): void`. start false khi thiếu library/root hoặc đang busy/manual; sync không bật một tour đã đóng. Không có callback để gọi .NET nghiệp vụ.
- Thư viện lấy factory `window.driver.js.driver`; gọi bằng API của version khóa, không dùng API từ phiên bản cũ. Registry theo root instance; không storage/network/file data.

- [ ] **Step 1: Viết fixture và test hành vi trước.** Dùng Node vm theo `theme-fixture.cjs`, DOM fixture root/targets/MutationObserver/fake clock, Driver spy có lifecycle hooks. Test: `start uses current chapter without application clicks`; `sync never starts a closed tour`; `same render preserves active hint`; `stage changes start matching chapter`; `busy pauses and resumes without user-close semantics`; `failed request stays in chapter`; `manual mode and context change stop`; `close and escape survive later sync`; `dispose cancels waits and late callbacks and is idempotent`; `missing required target times out at 1500ms with text notice`; `optional result errors may be absent`; `missing library leaves import untouched`; `dropdown portals are allowed`. Assert zero synthetic clicks, zero network/storage calls, no auto-advance on request start/click alone; check state/counts rather than source-string snapshots.

Fixture export `browser()` trả `{api, rootId, driverConfigs, clickCount, networkCount, storageCount, activeHint(), userClose(), advanceHint(), tick(ms), removeTarget(name)}`. Thêm test assertions cụ thể, ví dụ:

```javascript
test('sync never starts a closed tour', () => {
  const b = browser();
  const state = { stage: 2, isBusy: false, isManual: false, contextVersion: 0 };
  assert.equal(b.api.start(b.rootId, state), true);
  b.userClose();
  const starts = b.driverConfigs.length;
  b.api.sync(b.rootId, state);
  assert.equal(b.driverConfigs.length, starts);
  assert.equal(b.activeHint(), null);
  assert.equal(b.clickCount, 0);
  assert.equal(b.networkCount, 0);
  assert.equal(b.storageCount, 0);
});
```
- [ ] **Step 2: Run RED.** `node --test IRM.Tests/Browser/import-tour.test.cjs`; Expected: FAIL vì wrapper chưa tồn tại/không có API, không phải fixture lỗi.
- [ ] **Step 3: Đóng gói dependency đã khóa.** Lấy tarball `https://registry.npmjs.org/driver.js/-/driver.js-1.12.1.tgz`; kiểm tra SHA-512 với integrity trong spec trước khi dùng. Chép đúng `dist/driver.js.iife.js`, `dist/driver.css`, LICENSE; giữ nguyên vendor, thêm `manifest.json` ghi version, license, source URL, tarball integrity và SHA-256 ba tệp. Không cài npm toàn cục hoặc thêm package build; nếu artifact/metadata khác spec thì dừng phần vendor để xác minh nguồn, không tự chọn @latest.
- [ ] **Step 4: Viết wrapper theo interface và spec.** Dùng Driver thật cho highlight/position/focus, wrapper chỉ điều phối state/chapter và waits. Các điểm chờ hành động dùng action instruction, không gọi click hay hàm Blazor; chương chuyển bởi sync sau render. Dùng allowedElements cho `.mud-popover-open`, copy Việt cố định, không đưa tên file/header vào HTML. Khi busy tạm destroy có cờ nội bộ phân biệt với close; dispose hủy observer/timer và vô hiệu callback thế hệ cũ. Thiếu điểm bắt buộc sau 1500ms: stop và ghi notice bằng textContent theo câu spec; lỗi root/library lúc start trả false. Tôn trọng reduced motion.
- [ ] **Step 5: Gắn CSS/assets và chạy GREEN.** CSS riêng dùng palette MudBlazor, responsive, focus-visible; bảo đảm popup được phép ở trên spotlight. App.razor load Driver IIFE rồi import-tour.js trước blazor.web.js; CSS Driver rồi import-guidance.css, tất cả URL local có version cố định. Không đổi token cũ app.css gây fail test theme. Run Step 2 và `node --test IRM.Tests/Browser/*.test.cjs` (PowerShell nếu wildcard không mở thì liệt kê Get-ChildItem rồi truyền mảng FullName). Expected: PASS tất cả JS. Chạy `dotnet build IRM.sln -c Release`; Expected 0 error.
- [ ] **Step 6: Review staged diff/manifest và commit.** Đối chiếu checksum và chỉ asset/library được phép, không log/tarball/package cache. Commit `feat: add local Driver.js import tour adapter`.

### Task 3: Đồng bộ tour với wizard Blazor và sửa lời hướng dẫn sai

**Files:** Create snapshot record và `ImportGuidanceInteropTests.cs`; extend partial Guidance và rendering tests; modify Import.razor/Import.razor.cs cùng bốn section.

**Interfaces:**
- Consumes catalog/dialog Task 1 và `window.irmImportTour` Task 2.
- `ImportTourSnapshot` public record trong namespace `IRM.Features.ImportExport.Help`: `(int Stage, bool IsBusy, bool IsManual, int ContextVersion)`; JSInterop serialize thành camelCase đúng Task 2. Không thêm thông tin file/hồ sơ vào DTO.
- Partial Import có `_tourRootId: string` duy nhất per instance, `_tourContextVersion: int`; `internal Task StartImportTourAsync()`; `protected override Task OnAfterRenderAsync(bool firstRender)` gửi sync; `public ValueTask DisposeAsync()` thay Dispose hiện có. ShowFieldHelpAsync từ Task 1 gọi stop trước dialog. Chỉ thêm bộ đếm ngữ cảnh trong HandleFileUpload/ResetWizard, không sửa parse/ghi.

- [ ] **Step 1: Viết bridge/render tests và chạy RED.** Dùng subclass Import expose OnAfterRenderAsync, recording IJSRuntime và reflection tối thiểu theo test hiện có. `StartTour_SendsOnlyUiSnapshotAndNeverRunsImport` assert đúng 4 JSON properties, stage hiện tại, root riêng; ImportSvc chưa cần gán vẫn không bị gọi. `RepeatedRender_SyncsWithoutRestart` assert gọi sync, không start. `Dispose_DisconnectedCircuitIsSafe` fake JSDisconnectedException, stream được dispose và gọi lặp an toàn. `MappingAndWizard_ExposeStableTourAnchors` render từng section theo stage, kiểm tra các điểm bảng spec. Run `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~ImportGuidanceInteropTests|FullyQualifiedName~ImportGuidanceRenderingTests"`; Expected FAIL trước code.
- [ ] **Step 2: Nối root/nút/điểm bám.** Import.razor bọc trang bằng root instance, có live-region notice, nút Hướng dẫn từng bước chỉ trong Excel mode và disabled lúc busy. Giữ nguyên các handler nghiệp vụ. Thêm data-import-tour theo đúng bảng spec, wrapper bao trọn control có popup; result-errors chỉ hiện khi có lỗi. Không đặt dấu ? trong lựa chọn dropdown gây thay value khi bấm.
- [ ] **Step 3: Nối lifecycle trước, chạy GREEN Step 1.** Start gọi JS start, false/JSException báo Snackbar hướng dẫn chưa sẵn sàng, không chặn import. OnAfterRender gửi snapshot, bắt các lỗi disconnect/JS cho phần hướng dẫn; manual mode sync dừng. DisposeAsync hủy stream và JS registry, bỏ IDisposable cũ để không có hai đường disposal trùng; callback muộn của instance cũ không ảnh hưởng root mới. Render không tự bật tour. Chưa sửa counter thay file/reset và stop khi mở dialog ở bước này.
- [ ] **Step 4: Viết thêm test chuyển ngữ cảnh rồi chạy RED/GREEN.** `ReplacingLoadedFileOrResetInvalidatesTourButFirstUploadDoesNot` dùng upload stream giả theo API InputFileChangeEventArgs/IBrowserFile, fake ExcelReader; assert bộ đếm chỉ thay đúng lúc và dữ liệu wizard vẫn reset như trước. `OpeningHelpStopsTourBeforeDialog` dùng spy JS và IDialogService để assert thứ tự, không gọi ghi/hoàn tác. Chạy `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~ImportGuidanceInteropTests"`; Expected FAIL vì counter/stop chưa nối. Sau đó ShowFieldHelp gọi stop trước dialog; ResetWizard và thay file đã tải tăng contextVersion, lần tải đầu không tăng. Chạy lại cùng lệnh, Expected PASS. Helper fixture nhỏ trong test được phép, không thêm port nghiệp vụ chỉ để test.
- [ ] **Step 5: Kiểm tra rồi sửa lời rollback.** Viết `Result_RollbackCopyStatesGuards`: HtmlRenderer với kết quả thành công, assert không còn “bất cứ lúc nào”, có điều kiện phiên/phụ thuộc. Run `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~ImportGuidanceRenderingTests.Result_RollbackCopyStatesGuards"`; Expected FAIL trên copy hiện tại. Dùng đúng câu spec thay lời sai, không đổi quyền/nút/logic. Run `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~ImportGuidanceInteropTests|FullyQualifiedName~ImportGuidanceRenderingTests|FullyQualifiedName~ImportPreviewRenderingTests|FullyQualifiedName~LegacyImportFinalReviewTests|FullyQualifiedName~ImportRetryTests"`; bật SQL tests chỉ với generated `IRM_RefactorTest_*` theo hướng dẫn workspace. Expected PASS. Chạy lại JS suite, Expected PASS.
- [ ] **Step 6: Review staged diff và commit.** Xác nhận mọi lệnh ExecuteImport/Rollback/SaveTemplate vẫn chỉ từ handler hiện tại khi người dùng thao tác; Snapshot không chứa dữ liệu thật; không sửa Application/Infrastructure/Core. Commit `feat: connect import guidance to the live Blazor wizard`.

### Task 4: Nghiệm thu browser thật và cập nhật hướng dẫn

**Files:** Modify `docs/HUONG-DAN-IMPORT-EXCEL-NHANH.md`, `IMPORT-EXCEL-SPEC.md`, `README.md`; create `docs/IMPORT-GUIDANCE-VERIFICATION-2026-11-13.md`; app/test chỉ sửa nếu phát hiện lỗi có bằng chứng trong phạm vi hướng dẫn. Không thêm workbook hoặc bộ cài.

**Interfaces:** Consumes toàn bộ UI/adapter/metadata Tasks 1–3; produces hướng dẫn sử dụng và bằng chứng nghiệm thu của source mới, không tuyên bố bộ cài R4 có tính năng này.

- [ ] **Step 1: Chuẩn bị môi trường thử đúng source.** Chạy build Release. Dùng local app để xem/read-only ở trang được phép; luồng có ghi/hoàn tác phải khởi động instance riêng trên SQL database test biệt lập với dữ liệu giả và tài khoản test. Không lấy connection string runtime hay ghi trên IRM/nghiệp vụ; tận dụng hạ tầng IsolatedImportSqlDatabase/seed test và golden hiện có, không thêm endpoint test vào production. Nếu chưa có môi trường cho UI ghi, ghi rõ phần chưa nghiệm thu, không tự thay database local.
- [ ] **Step 2: Thử với Driver thật.** Theo spec: mở ở cả bốn chương; Next/Previous chỉ đổi lời, action step chờ nút thật; đọc file/preview lỗi không nhảy chương; render lặp giữ vị trí; Close/Esc không tự mở lại; tour không tự ghi dữ liệu. Thử dropdown công ty/nhóm/ghép cột bằng chuột và bàn phím, mở/đóng hộp tra cứu rồi mở lại tour, thay file/reset/manual mode/rời trang. Thiếu điểm bám/thiếu asset không treo import. Ghi kết quả từng trường hợp; sửa lỗi bằng regression test fail trước rồi pass trong task sở hữu, không mở rộng nghiệp vụ.
- [ ] **Step 3: Kiểm tra giao diện và nội dung.** Light/dark, viewport 390×844 và 1280×800, zoom 200%, Tab/Shift+Tab/Enter/Esc, reduced-motion; đọc đủ nội dung, focus/scroll trở lại hợp lý, popup không kẹt dưới overlay. Chặn mạng ngoài trong browser nhưng giữ kết nối app/circuit: tour/tra cứu vẫn hoạt động bằng asset local, không request Guideflow/CDN/analytics. Không ghi/capture PII. Khi dùng golden, đúng điều kiện CHN/Kỹ sư nhóm 1/Thân nhân nhóm 9/Hải Châu và INV-01…04 trong đầu tư công ty test; kỳ vọng 12 mới/15 lỗi, không dùng bộ này để chứng nhận toàn bộ V010.
- [ ] **Step 4: Cập nhật tài liệu sau khi đã thử.** Ghi cách mở tour, tra cứu ?, các chương, đóng/mở lại, điều kiện thao tác thật, nguồn trợ giúp và fallback. Ghi mốc source mới cho UI, giữ mốc nghiệp vụ và checksum workbook/bộ cài cũ. Tạo báo cáo nghiệm thu ngắn `docs/IMPORT-GUIDANCE-VERIFICATION-2026-11-13.md` ghi bằng chứng/tồn tại; log/screenshot không PII ở BAN-GIAO/05-HO-TRO, không commit runtime dữ liệu.
- [ ] **Step 5: Chạy kiểm chứng cuối.** Từ IRM: `dotnet build IRM.sln -c Release`, `dotnet test IRM.Tests/IRM.Tests.csproj -c Release` với SQL test nếu có, và toàn bộ Node browser tests như Task 2. Expected 0 error/0 failed; đọc số skipped và báo đúng hạn chế. Audit tập khóa catalog = 35, link tài liệu mới và manifest checksum; `git diff --check`. Chỉ mở rộng test nếu có lỗi/thay đổi mới cần chứng minh.
- [ ] **Step 6: Review staged diff/commit.** Không binary bộ cài, DB/keys/log/credentials, không thay nghiệp vụ. Commit `docs: document and verify live Excel import guidance`. Khi thực thi inline, nhờ một reviewer fresh-context rà toàn bộ diff theo Review Focus và spec; sửa findings quan trọng rồi kiểm chứng. Giữ develop và commit local, không push.

## Self-review của kế hoạch

- Coverage: 35 trường/tra cứu ở Task 1; tour/offline/stop/waits ở Task 2; root/lifecycle/action boundaries/lời rollback ở Task 3; UI thật, bàn phím, responsive/theme và tài liệu ở Task 4. Không thiếu phần spec, không tạo màn hình nghiệp vụ khác.
- Interfaces: namespace model cố định; snapshot chỉ bốn properties camelCase; JS exports start/sync/stop/dispose khớp cả hai phía; điểm bám duy nhất theo bảng spec; Task 1 chưa phụ thuộc adapter Task 2.
- Step scan: mỗi code step có nguồn/interface/copy chính xác, test bước trước và lệnh kiểm chứng; không chép body nguyên chương trình, không thêm unit test cho checklist Markdown.
- Review Focus: năm nhóm đều có test owning task và browser acceptance cho phần fixture không chứng minh được; không gọi spy wrapper là đã kiểm chứng Driver/MudBlazor thật.
- Scope: một feature UI với bốn task có deliverable riêng; không refactor kiến trúc, sửa luật hoặc dựng pipeline package mới. Đóng gói vendor gắn với wrapper, docs gắn nghiệm thu.

## Handoff

Kế hoạch hoàn tất để người dùng review cùng spec; chưa triển khai source hay tải dependency. Giữ phương thức inline (`executing-plans`) đã được dùng trước đó. Sau khi người dùng xác nhận kế hoạch phản ánh đúng mong muốn, thực hiện từng task và một lượt review độc lập cuối; không hỏi lại lựa chọn thư viện hoặc phương thức đã chốt.
