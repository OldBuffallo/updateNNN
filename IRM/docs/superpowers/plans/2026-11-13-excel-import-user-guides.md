# Excel Import User Guides Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hoàn thiện hướng dẫn nhanh và đặc tả chi tiết 35 trường để người dùng chuẩn bị Excel đúng với luồng import đang có.

**Architecture:** Mở rộng đặc tả hiện có làm nguồn tra cứu duy nhất, tạo một hướng dẫn nhanh liên kết tới đặc tả. Đối chiếu trực tiếp parser/preview/persistence và test; không thay đổi ứng dụng để làm cho lời hướng dẫn trở thành đúng.

**Tech Stack:** Markdown tiếng Việt; mã C#/.NET 8, ClosedXML và EF Core làm nguồn đối chiếu; các test xUnit hiện có dùng kiểm chứng hành vi. Không thêm thư viện hoặc định dạng xuất mới.

**Spec:** `docs/superpowers/specs/2026-11-13-excel-import-user-guides-design.md`.

## Global Constraints

- Chỉ viết tài liệu; không sửa mã ứng dụng, schema, seed, cấu hình chạy hoặc dữ liệu nghiệp vụ.
- Phủ đúng 35 trường trong `SystemFields.All`, không tính `skip`, không thêm cột import không được hỗ trợ.
- Phân biệt “Chương trình kiểm tra” và “Khuyến nghị nhập liệu”; không trình bày khuyến nghị như lỗi bắt buộc.
- Danh mục database đích là chuẩn; không tự đổi mã quốc tịch, nhóm nghề, nghề hoặc địa bàn sang một bộ mã khác.
- Ngày lõi ưu tiên hướng dẫn `dd/MM/yyyy`; ngày mở rộng có thể nhận thêm ISO theo mã; số nhận dạng được ghi nhận từ dữ liệu đã có, không tự sinh.
- Không hứa mọi file đều import thành công; nêu đúng lỗi theo dòng, lỗi danh mục thay đổi và lỗi có thể hoàn tác cả giao dịch.
- Giữ Git root và nhánh `develop`; không push, không sửa WPF/reference và không đổi nhãn bộ cài cũ.

## Review Focus

1. Danh mục seed mới khác danh mục kế thừa: ví dụ chỉ hợp lệ khi database đích có đúng mã/tên/nhóm; Task 1 đối chiếu `NationalityImportTests` và `Career_IsResolvedInsideTheChosenGroup`.
2. Ngày mơ hồ, số 0 đầu và danh sách `;`: không làm đổi nghĩa dữ liệu; Task 1 đối chiếu test ngày/ô số và mã đọc danh sách, Task 2 kiểm tra quy trình Excel.
3. GPLĐ trống, nhãn trạng thái và số riêng: mô tả đúng giá trị lưu, nhà đầu tư và xung đột; Task 1 đối chiếu test GPLĐ và `SeparatePermitNumber_IsConsumed`/`ConflictingPermitColumns_AreRejected`.
4. Không ghép cột khác ghép nhưng ô trống, trường tùy chọn cần trường đi cùng: ghi đúng hành vi, không gán điều kiện của biểu mẫu cho Excel; Task 1 đọc parser/Comparison/ExtensionHistory và test mở rộng.
5. Preview đúng nhưng giao dịch lỗi hoặc rollback không còn an toàn: người dùng hiểu giới hạn và biết khi nào cần xem trước lại; Task 2 đối chiếu test stale preview/rollback và nhánh catch của Execution.

---

## File map và cách kiểm chứng

Mọi đường dẫn dự án dưới đây tương đối với `IRM/`; chạy lệnh .NET từ thư mục này. Git root là thư mục cha. Mốc nguồn đã đọc `5c9808d`; nếu nguồn import đổi, rà lại trước khi chốt tài liệu.

| Tệp | Trách nhiệm |
|---|---|
| Modify `docs/IMPORT-EXCEL-SPEC.md` | Đặc tả 35 trường, quy tắc thực tế, lỗi/cách sửa, phụ lục nguồn và giới hạn |
| Create `docs/HUONG-DAN-IMPORT-EXCEL-NHANH.md` | Quy trình ngắn cho người nhập, 9 trường bắt buộc, checklist và lỗi thường gặp |
| Modify `docs/README.md:5` | Thêm liên kết hai mức hướng dẫn cạnh phần import; không sửa phần lịch sử không liên quan |

Đây là tác vụ tài liệu. Không viết unit test kiểm tra prose, không tạo bài test FAIL giả vì tài liệu chưa có. Dùng kiểm tra phủ trường, rà soát ví dụ theo mã/test sẵn có và kiểm tra liên kết. Không cần build/restart ứng dụng chỉ vì Markdown thay đổi.

### Task 1: Đặc tả đầy đủ 35 trường theo mã hiện tại

**Files:** Modify `docs/IMPORT-EXCEL-SPEC.md`; read the source/test paths listed in spec section 4.

**Interfaces:** Consumes toàn bộ danh sách khóa trong spec section 3 và `SystemFields.All`. Produces 35 mục ``### `FieldKey` — Tên hiển thị``, mẫu mỗi mục theo spec section 2; các quy tắc chung dùng anchor Markdown được hướng dẫn nhanh liên kết đến.

- [ ] **Step 1: Đối chiếu từng trường với nguồn đọc và nguồn ghi.** Với mỗi khóa, xác định bắt buộc, giá trị nhận, ô trống, không ánh xạ, danh mục, điều kiện trường đi cùng, lỗi thực tế và tác động khi cập nhật. Đọc cả nhánh ExtensionHistory, không chỉ `SystemFields` hoặc biểu mẫu UI.
- [ ] **Step 2: Kiểm chứng các ví dụ rủi ro bằng test hiện có.** Run: `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~LegacyImportRulesTests|FullyQualifiedName~LegacyImportLookupRulesTests|FullyQualifiedName~LegacyExcelReaderTests|FullyQualifiedName~LegacyImportEndToEndTests|FullyQualifiedName~NationalityImportTests|FullyQualifiedName~LegacyImportFinalReviewTests.SeparatePermitNumber|FullyQualifiedName~LegacyImportFinalReviewTests.ConflictingPermitColumns|FullyQualifiedName~LegacyImportFinalReviewTests.FamilyExtensionDate"`. Expected: 0 failed; ghi rõ môi trường nếu không chạy được. Đây là kiểm chứng nguồn, không phải test prose.
- [ ] **Step 3: Viết lại đặc tả theo các nhóm trong spec.** Mỗi mục đủ tên/ý nghĩa, kiểu, bắt buộc, cách nhập, giá trị, hai trạng thái thiếu/trống, ví dụ có tiền đề, ràng buộc, lỗi/cách sửa, khuyến nghị riêng. Không tự đặt độ dài/regex/chuẩn pháp lý. Giữ phần đối chiếu file cũ và phân biệt workbook/source/bộ cài.
- [ ] **Step 4: Kiểm tra đủ và không trùng mục trường.** Chạy đoạn kiểm tra bên dưới từ `IRM/`. Expected: `35 supported fields; 35 documented fields; coverage OK`.

```powershell
@'
from pathlib import Path
import re
source = Path('IRM.Application/Features/ImportExport/Models/SystemFields.cs').read_text(encoding='utf-8-sig').split('AutoMapKeywords')[0]
supported = set(re.findall(r'\{\s*"([^"]+)"\s*,', source)) - {'skip'}
text = Path('docs/IMPORT-EXCEL-SPEC.md').read_text(encoding='utf-8-sig')
documented = re.findall(r'^### `([A-Za-z][A-Za-z0-9]*)`', text, re.M)
assert len(supported) == 35
assert len(documented) == len(set(documented)) == 35
assert set(documented) == supported, (sorted(supported-set(documented)), sorted(set(documented)-supported))
print('35 supported fields; 35 documented fields; coverage OK')
'@ | python -
```

- [ ] **Step 5: Rà soát từng mẫu trường thủ công.** Expected: đủ thành phần; những hành vi chưa được kiểm tra hoặc chưa báo lỗi phải nói rõ, không viết như đã có validator. Chỉ tài liệu mới, không sửa C# để làm ví dụ đạt.
- [ ] **Step 6: Review staged diff và commit.** Run từ Git root: `git add IRM/docs/IMPORT-EXCEL-SPEC.md`, `git diff --cached --check`, `git diff --cached`, `git commit -m "docs: specify all supported Excel import fields"`. Expected: chỉ đặc tả thay đổi, không có runtime/data/binary.

### Task 2: Hướng dẫn nhanh cho người nhập

**Files:** Create `docs/HUONG-DAN-IMPORT-EXCEL-NHANH.md`; read `docs/IMPORT-EXCEL-SPEC.md`, `IRM/Features/ImportExport/Import.razor.cs`, `Components/ImportUploadSection.razor`/`ImportPreviewSection.razor` dưới cùng thư mục feature, và các nhánh Preview/Execution/Rollback đã nêu trong spec.

**Interfaces:** Consumes 35 mục trường và quy tắc chung từ Task 1. Produces quy trình 6 phần theo spec section 2, bảng 9 trường bắt buộc và liên kết tương đối `IMPORT-EXCEL-SPEC.md`.

- [ ] **Step 1: Viết quy trình 120–180 dòng.** Gồm chuẩn bị danh mục, Text cho số nhận dạng, `.xlsx`, sheet/tiêu đề/vùng dòng, ghép cột, kiểm tra preview, xác nhận và hoàn tác. Không lặp 35 mục chi tiết. Nêu cả chọn công ty mặc định và cột Công ty; tạo công ty mới chỉ khi người dùng bật rõ.
- [ ] **Step 2: Viết checklist và khoảng 10 lỗi thường gặp.** Phải có ngày, quốc tịch, nghề/nhóm, địa chỉ, GPLĐ hai cột, visa/kết quả, sai sheet/dòng, ô trống, dữ liệu thay đổi sau preview và lỗi giao dịch. Nêu nguyên nhân và hành động sửa cụ thể; không hướng dẫn thêm danh mục tùy tiện để làm file đạt.
- [ ] **Step 3: Walkthrough ví dụ đã biết.** Đọc `LegacyGoldenWorkbookTests` và hướng dẫn workbook ngày 12/11; ghi đúng tiền đề database test và các kỳ vọng 12 dòng hợp lệ/15 lỗi. Không sửa hoặc tạo workbook. Nếu dùng ví dụ nhỏ hơn, chọn các giá trị đã được test và ghi rõ điều kiện danh mục.
- [ ] **Step 4: Kiểm chứng diễn giải luồng lỗi.** Run: `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter "FullyQualifiedName~LegacyImportEndToEndTests|FullyQualifiedName~LegacyImportFinalReviewTests.StalePreview|FullyQualifiedName~LegacyImportFinalReviewTests.FailedTransaction|FullyQualifiedName~LegacyImportFinalReviewTests.Rollback_RequiresReverseOrder|FullyQualifiedName~ImportRetryTests"`. Expected: 0 failed. Sau đó đọc nhánh catch Execution và xác nhận hướng dẫn phân biệt lỗi theo dòng với hoàn tác cả giao dịch.
- [ ] **Step 5: Rà soát từ góc nhìn người mới và commit.** Expected: người đọc biết thao tác theo thứ tự, xác định 9 trường bắt buộc, nhận diện dữ liệu thiếu và không nhầm “đã ghi nhận số” với “cấp số”. Run từ Git root: `git add IRM/docs/HUONG-DAN-IMPORT-EXCEL-NHANH.md`, `git diff --cached --check`, `git diff --cached`, `git commit -m "docs: add practical Excel import quick start"`.

### Task 3: Liên kết và nghiệm thu bộ tài liệu

**Files:** Modify `docs/README.md:5` và phần mở đầu của hai tài liệu vừa hoàn thiện.

**Interfaces:** Consumes tên tệp/tiêu đề/anchor Task 1–2. Produces bộ hai mức có đường đi rõ từ mục lục, có mốc source đã đối chiếu và giới hạn áp dụng.

- [ ] **Step 1: Thêm liên kết hai chiều và mục lục.** Từ docs README thêm “Hướng dẫn nhanh” và “Đặc tả từng trường”. Từ hướng dẫn nhanh trỏ sang đặc tả, từ đặc tả trỏ về hướng dẫn nhanh. Giữ liên kết hướng dẫn R4/database cũ như tài liệu lịch sử, không nói bộ cài cũ có luật source mới.
- [ ] **Step 2: Rà soát link và phạm vi.** Kiểm tra mọi tệp/anchor được liên kết có thật; chạy lại kiểm tra 35 trường Task 1 và `git diff --check`. Expected: không thiếu/trùng trường, không link hỏng, chỉ Markdown trong diff, không thay workbook/binary/app.
- [ ] **Step 3: Rà soát 5 Review Focus.** Đọc chéo hai tài liệu với mã nguồn; xác nhận ví dụ và điều kiện giống nhau, trường tùy chọn/blank/unmapped không bị diễn giải ngược, không có lời hứa tự sinh/cấp số hoặc mọi lỗi đều cô lập theo dòng. Ghi rõ hạn chế thay vì sửa ứng dụng ngoài phạm vi.
- [ ] **Step 4: Commit phần liên kết và báo kết quả.** Run từ Git root: `git add IRM/docs/README.md IRM/docs/IMPORT-EXCEL-SPEC.md IRM/docs/HUONG-DAN-IMPORT-EXCEL-NHANH.md`, `git diff --cached --check`, `git diff --cached`, `git commit -m "docs: link Excel training guides and clarify applicability"`. Expected: Git sạch trên `develop`; báo đường dẫn hai tài liệu, commit và các kiểm chứng thực sự đã chạy. Không push.

## Self-review của kế hoạch

- Spec coverage: 35 trường/mẫu mục/ràng buộc nguồn ở Task 1; quy trình/checklist/lỗi ở Task 2; liên kết, provenance và tính nhất quán ở Task 3. Không có yêu cầu tạo workbook, Word/PDF hoặc validator phần mềm.
- Step scan: mỗi bước có nguồn, đầu ra và kết quả kiểm tra cụ thể; dùng audit tài liệu thay vì unit test prose hoặc RED giả.
- Type consistency: một đặc tả hiện có, một tệp nhanh mới; cùng danh sách 35 khóa theo spec/SystemFields; không có API hoặc kiểu mới cần triển khai.
- Review Focus: mỗi tình huống đã gắn với bước đối chiếu/test của Task 1–2 và audit cuối Task 3; các ràng buộc chưa được code kiểm tra phải được ghi rõ.
- Proportion: 3 task tài liệu, không có thân hàm ứng dụng, schema, dependency hoặc dự án mới.

## Handoff

Người dùng đã chọn cấu trúc hai mức. Giữ cách thực hiện trực tiếp từng task trong phiên (executing-plans) đã dùng trong chuỗi công việc này; chờ người dùng xem lại kế hoạch trước khi viết bộ hướng dẫn. Không hỏi lại lựa chọn cấu trúc hoặc tự mở rộng sang thay đổi ứng dụng.
