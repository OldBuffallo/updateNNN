# IRM Offline Official Release Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Phát hành bộ cài Windows offline R5 từ source mới, đồng bộ UI mint/emerald, kế thừa cơ chế cài đặt đã được người dùng xác nhận hoạt động tốt.

**Architecture:** Giữ Inno Setup → WinForms → worker PowerShell và .NET self-contained/SQL media hiện tại. Chỉnh lớp trình bày và metadata đợt phát hành, sau đó nghiệm thu chính artifact cuối trên môi trường biệt lập. Không viết lại installer hoặc đổi schema.

**Tech Stack:** ASP.NET Core 8/Blazor, Windows PowerShell 5.1, WinForms/System.Drawing, Inno Setup 6, SQL Server Express media đã khóa, xUnit, Node test runner.

**Spec:** [2026-11-14-offline-official-release-design.md](../specs/2026-11-14-offline-official-release-design.md). Đọc cả spec và plan trước thực hiện. Các lựa chọn palette sáng/R5 và chính sách ký trong spec đang chờ người dùng review kế hoạch; chưa triển khai trong phiên lập kế hoạch.

## Global Constraints

- Git root `NNN/`, giữ nhánh hiện có; mọi path bên dưới tính từ `IRM/`, trừ path `../BAN-GIAO/`.
- Giữ application version `1.0.2`; release revision `R5`; không đổi database version theo revision UI.
- Giữ `IRM.exe`, AppId `{A7A7AA33-A680-4E29-93C4-9F4720376A6B}`, service `IRM`, SQL instance `IRMEXPRESS`, database `IRM`, data root `C:\ProgramData\IRM`, port `5050/5443/14331`.
- Giữ schema, migrations, quyền, password compatibility, validation và luồng import/rollback; không sửa WPF hoặc danh mục nghiệp vụ thật.
- Giữ SQL media/CU và SHA-256 đã khóa trong `deploy/release-manifest.json`; không nâng dependency/runtime trong đợt chỉnh UI.
- Offline nghĩa là không tải Internet khi cài trên máy đủ điều kiện; .NET 8 self-contained và SQL base/CU nằm trong EXE. .NET Framework 4.7.2+ (Release >= 461808) là điều kiện có sẵn, chưa được bundle. Không tự thêm bootstrap tải mạng.
- Giữ ngưỡng kỹ thuật hiện cấu hình Windows build 14393 x64; ghi đúng OS/build đã thử, không mở rộng cam kết nền tảng.
- Không đưa CDN/font/tài nguyên mạng mới vào installer; không thay theme/registry Windows để đổi giao diện.
- Mọi thử nghiệm có ghi dữ liệu dùng VM/máy thử biệt lập và dữ liệu giả; không cài đè trên máy làm việc hoặc database khách để kiểm tra.
- Source phải commit và Git tree sạch trước đóng gói. Mỗi build có `SOURCE-REVISION.json`, checksum payload và SHA-256 EXE cuối sau ký.
- Gói mới vào `BAN-GIAO/04-CHO-NGHIEM-THU/`; chỉ đưa vào `01-HIEN-HANH/` sau nghiệm thu. Không ghi đè gói cũ; không commit binary/log/runtime/credentials; không push.

## Review Focus

- DPI 150/200%, màn hình 1366×768 và lỗi dài: còn thấy và thao tác được nút chính, không mất nội dung lỗi — Task 1 kiểm tra trực quan.
- Disabled/read-only, lỗi rồi thử lại: trạng thái phải rõ, khóa input khi cài, chỉ mở lại khi thất bại — Task 1 và Task 4 kiểm tra tương tác.
- Máy offline thiếu Framework hoặc SQL media: chặn đúng điều kiện trước cài, không âm thầm tải mạng — Task 2 preflight và Task 4 negative test.
- Nâng cấp dữ liệu đã có: giữ hồ sơ và chứng minh hành vi đặt lại Admin hiện tại; backup/restore đọc lại đúng dữ liệu giả — Task 4.
- Sai revision/payload hoặc sửa EXE sau ký: không phát hành gói sai commit/hash/chữ ký — Task 3 đối chiếu, Task 5 xác minh lại.

---

## File structure và trách nhiệm

| Tệp | Vai trò trong đợt này |
|---|---|
| `deploy/windows/installer-theme.ps1` | Palette, control tự vẽ, nút, title bar và dialog |
| `deploy/windows/installer-ui.ps1` | Bố cục, trạng thái nhìn thấy và báo cáo môi trường HTML; giữ worker dispatch |
| `deploy/windows/irm.iss` | UI vỏ giải nén; tên artifact R5, AppVersion không đổi |
| `deploy/release-manifest.json` | Đổi duy nhất `windows.artifact` sang tên R5 |
| `docs/INSTALLATION-v1.0.2.md` | Hướng dẫn theo UI mới và điều kiện offline/nâng cấp |
| `docs/RELEASE-READINESS-R5.md` (mới) | Biên bản kết quả, commit/hash, phạm vi OS và quyết định phát hành |
| `deploy/windows/tests/environment.Tests.ps1` | Chạy hồi quy hiện có; không thêm test soi chuỗi màu |
| `../BAN-GIAO/04-CHO-NGHIEM-THU/<timestamp>-v1.0.2-R5/` | EXE, payload, checksum, tài liệu của một candidate |
| `../BAN-GIAO/05-HO-TRO/<timestamp>-R5/` | Bằng chứng đã loại dữ liệu nhạy cảm: build/test, ảnh UI dữ liệu giả, biên bản thử |

Không sửa `install.ps1`, `windows-environment.ps1`, `backup.ps1`, `restore.ps1`, `repair.ps1`, `uninstall.ps1` chỉ để đổi UI. Không sửa app theme hoặc tăng tính năng ứng dụng trong kế hoạch này. Nếu phát hiện regression thực, tái hiện trước rồi xử lý riêng với phạm vi rõ ràng.

### Task 1: Đồng bộ UI bộ cài với ứng dụng mới

**Files:** Modify `deploy/windows/installer-theme.ps1`, `deploy/windows/installer-ui.ps1`, `deploy/windows/irm.iss` (chỉ UI ở task này). Reference `IRM/wwwroot/app.css`.

**Interfaces:** Consumes palette chính xác trong spec và phase hiện có `scan/configure/install/done`. Preserves `Set-IrmInstallerButtonTheme([Windows.Forms.Button]$Button, [switch]$Primary)`, `Show-IrmInstallerMessage([string]$Message,[string]$Caption,[string]$Buttons='OK')` trả `DialogResult`, `WindowTheme.ApplyTitleBar(Form)`, `installer-ui.ps1 -PayloadRoot <path>`, worker arguments/environment và exit code. Giữ tên các lớp `Dark*` hiện có để tránh đổi interface vì tên thẩm mỹ.

- [ ] **1. Ghi bằng chứng UI trước sửa trên VM thử.** Mở vỏ Inno và UI cũ; lưu ảnh dữ liệu giả cho bảng quét, cấu hình, busy, lỗi và hoàn tất. Đánh dấu phần dark/blue khác spec. Không chạy `installer-ui.ps1` trên máy làm việc: nó tự quét môi trường khi mở và có thể ghi báo cáo.
- [ ] **2. Đổi palette và control tự vẽ.** Áp dụng đúng bảng màu spec cho cả C# embedded và PowerShell, gồm nền hàng xen kẽ/selection, border, checkbox, progress, hover, disabled. Header/nút chính chữ trắng; surface chữ tối. Đổi `ApplyTitleBar(Form)` sang title bar sáng, giữ fallback DWM. Không chỉ đổi hashtable rồi bỏ sót màu hardcode.
- [ ] **3. Đồng bộ các bề mặt còn lại.** Đổi Inno `WizardStyle=modern light includetitlebar`; giữ luồng `CurStepChanged`. Chỉnh header, khoảng cách và anchor/scroll của WinForms khi cần để nút không ngoài vùng dùng được. Đổi CSS `Save-EnvironmentReport` sang mint/emerald với bản in trắng; giữ encode HTML. Không đổi kiểm tra mật khẩu >=12, xác nhận mật khẩu, license hoặc xử lý worker.
- [ ] **4. Kiểm tra trực quan và tương tác.** Trong VM, xem lại cùng các trạng thái ở 1366×768/1920×1080, DPI 100/125/150/200%, resize nhỏ nhất, bàn phím Tab/Enter/Escape, lỗi dài. Kỳ vọng: không che/cắt nút, dấu tiếng Việt đúng; mật khẩu vẫn ẩn; disabled rõ; lúc busy không sửa cấu hình/đóng cửa sổ; sau lỗi có thể thử lại; dialog Yes/No trả đúng. Chỉ kiểm tra hiển thị progress, không đòi phần trăm SQL chính xác.
- [ ] **5. Chạy parser và hồi quy preflight.** Dùng Windows PowerShell 5.1 `System.Management.Automation.Language.Parser.ParseFile` trên ba `.ps1` UI/theme/environment; báo tất cả parse errors, exit khác 0 nếu có. Chạy `powershell.exe -NoProfile -File deploy/windows/tests/environment.Tests.ps1` và bản `SysWOW64` theo Task 2; kỳ vọng 0 lỗi. Thay đổi thuần UI xác minh trực quan, không tạo unit test trùng bảng màu.
- [ ] **6. Review diff và commit.** Chỉ stage ba tệp UI nêu trên; `git diff --cached --check`, `git diff --cached`, rồi `git commit -m "style(installer): align offline setup with mint emerald UI"`.

### Task 2: Chốt source và metadata R5

**Files:** Modify `deploy/release-manifest.json`, `deploy/windows/irm.iss`, `docs/INSTALLATION-v1.0.2.md`; Create `docs/RELEASE-READINESS-R5.md`.

**Interfaces:** Produces `windows.artifact = "IRM-v1.0.2-windows-x64-offline-setup-R5.exe"` và `OutputBaseFilename=IRM-v1.0.2-windows-x64-offline-setup-R5`; giữ `AppVersion=1.0.2`. Handoff cho build script hiện có qua `-OutputDirectory`; không cần API hay switch mới.

- [ ] **1. Cập nhật metadata và tài liệu.** Đổi đúng hai tên artifact trên. Hướng dẫn mô tả UI sáng mới, ba bước, điều kiện Framework đã có, không cần Internet khi cài, Admin được tạo/đặt lại khi nâng cấp và vị trí backup. Biên bản R5 ghi xác nhận bộ cài cũ từ người dùng; mọi mục chưa chạy để `Chưa thực hiện`, không copy PASS R4.
- [ ] **2. Build Release.** Từ `IRM/`: `dotnet build IRM.sln -c Release --nologo`; kỳ vọng exit 0, không lỗi, xem xét mọi cảnh báo mới (baseline lập kế hoạch 0).
- [ ] **3. Chạy toàn bộ .NET với SQL biệt lập.** `$env:IRM_TEST_SQLSERVER='Server=.\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True'`; `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build -- RunConfiguration.TreatNoTestsAsError=true`. Chỉ dùng SQL test có quyền tạo/drop database `IRM_RefactorTest_*`; kỳ vọng không fail/skip (baseline 337). Nếu máy thực thi không có SQL test, ghi thiếu kiểm chứng và chuẩn bị môi trường thử trước phát hành, không dùng DB khách.
- [ ] **4. Chạy JavaScript.** `$irmTests=@(Get-ChildItem IRM.Tests/Browser -Filter '*.test.cjs' | ForEach-Object FullName); node --test @irmTests`; kỳ vọng 0 fail/skip (baseline 47). Đây không phải nghiệm thu browser thật.
- [ ] **5. Chạy preflight hai kiến trúc host.** `powershell.exe -NoProfile -File deploy/windows/tests/environment.Tests.ps1`; `& "$env:SystemRoot\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -File deploy/windows/tests/environment.Tests.ps1`; kỳ vọng cả hai exit 0, gồm các ca thiếu Framework, Windows không đủ điều kiện và tự chọn native host.
- [ ] **6. Đối chiếu phạm vi và commit.** Kiểm tra manifest chỉ đổi artifact, Inno không đổi AppId/MinVersion/worker; worker và migration không đổi so với source lúc bắt đầu thực thi. Ghi kết quả thật vào biên bản. Stage đúng bốn tệp task, review staged diff/whitespace, commit `chore(release): prepare offline R5 release metadata`. Lưu full `git rev-parse HEAD` làm source release; `git status --porcelain` phải trống.

### Task 3: Dựng và kiểm tra candidate offline

**Files:** Read `deploy/windows/build-windows-installer.ps1`, manifest và hướng dẫn; output dưới `../BAN-GIAO/04-CHO-NGHIEM-THU/<timestamp>-v1.0.2-R5/`. Không sửa script build nếu contract hiện tại đã đủ.

**Interfaces:** Consumes source commit sạch Task 2, Inno compiler, vendor media và chứng thư ký. Produces EXE R5, `.sha256`, `windows-stage/SOURCE-REVISION.json`, `SHA256SUMS.txt`, SBOM và hướng dẫn trong payload. Tên candidate được giữ cố định cho các task sau.

- [ ] **1. Kiểm tra đầu vào build.** Xác minh SQL base/CU khớp SHA-256 manifest, chữ ký Microsoft hợp lệ; có Inno compiler, certificate Code Signing có private key và timestamp URL. Chỉ báo trạng thái, không xuất secrets. Không coi thiếu chứng thư là lỗi UI; có thể hoàn tất UI/candidate thử trước, nhưng không gắn nhãn chính thức trái chính sách hiện có.
- [ ] **2. Chạy build trên source sạch.** Đặt `$r5Output` thành đường dẫn tuyệt đối mới dưới `BAN-GIAO/04-CHO-NGHIEM-THU/<timestamp>-v1.0.2-R5`; chạy `powershell.exe -NoProfile -File deploy/windows/build-windows-installer.ps1 -OutputDirectory $r5Output`. Script nhận certificate/timestamp qua cấu hình hiện có. Không truyền `-AllowUnsigned` cho bản chính thức. Máy build có thể dùng mạng để restore/timestamp; yêu cầu offline áp dụng máy cài.
- [ ] **3. Xác minh gói cuối.** Kiểm tra EXE exit build thành công, `Get-AuthenticodeSignature` là `Valid`, SHA-256 trùng sidecar; `SOURCE-REVISION.json.commit` trùng full commit Task 2, `dirty=false`. Trong VM ở Task 4 đối chiếu payload thực được EXE giải nén với toàn bộ `SHA256SUMS.txt` trước khi temp cleanup; kiểm tra không có tệp dư không giải thích được. Không chỉ kiểm tra stage ngoài EXE rồi coi là đã kiểm tra EXE.
- [ ] **4. Kiểm tra nội dung bàn giao.** Có IRM/runtime, SQL base/CU, scripts, schema, hướng dẫn và static assets ứng dụng. Không có DB thật, workbook chứa danh mục thật, khóa, credentials, log, test binaries. Ghi checksum và kết quả vào bằng chứng hỗ trợ; không sửa hoặc ký lại EXE sau khi lấy hash cuối mà không tạo hash và nghiệm thu lại.

### Task 4: Nghiệm thu chính EXE mới trên máy thử

**Files:** Update `docs/RELEASE-READINESS-R5.md`; evidence dưới `../BAN-GIAO/05-HO-TRO/<timestamp>-R5/`. Read worker/backup/restore hiện có khi thao tác; không thay implementation trong task nghiệm thu.

**Interfaces:** Consumes exact EXE/hash Task 3. Produces biên bản theo từng case `PASS/FAIL/Chưa thực hiện`, OS/build/DPI, commit/hash, kết quả và vấn đề còn lại. Không dùng confirmation cũ thay kết quả candidate mới.

- [ ] **1. Chuẩn bị hai snapshot VM cô lập.** Một máy đủ điều kiện chưa cài IRM/SQL; một máy cài gói cũ có dữ liệu giả xác định trước (công ty, hồ sơ, tài khoản thử, file đính kèm). Ghi tên/hash gói cũ thực sự dùng. Chụp snapshot và ngắt Internet trước chạy EXE R5; không tắt bảo vệ Windows để test.
- [ ] **2. Cài mới offline.** Chạy đúng EXE; kiểm tra UI theo Task 1, SQL base/CU lấy trong payload, self-test/health thành công, mở IRM từ nút hoàn tất/shortcut và sau reboot. Kiểm tra không phát sinh tải thành phần từ Internet; thiếu mạng không làm hỏng luồng cài đủ điều kiện. Ghi thời gian/kết quả, không ghi mật khẩu.
- [ ] **3. Kiểm tra thất bại có kiểm soát.** Snapshot thử thiếu Framework phải báo đúng mục và chặn trước SQL; bản sao payload thử thiếu SQL media phải báo thiếu (không sửa EXE bàn giao). Khi worker thất bại, UI hiện thông tin đọc được và cho thử lại; không ghi thành công giả. Hoàn nguyên snapshot sau ca lỗi. Ghi rõ negative case payload rời khác nghiệm thu EXE nguyên vẹn.
- [ ] **4. Nâng cấp offline từ gói cũ.** So sánh số lượng và giá trị định danh dữ liệu giả trước/sau; không bị reset DB; tài khoản Admin nhập trong installer tuân hành vi đặt lại đã nêu. Xác minh app thực chạy UI/static assets mới, dịch vụ và health sau restart. Không đổi danh mục seed để làm phép thử đạt.
- [ ] **5. Smoke ứng dụng từ bản đã cài.** Dùng tài khoản/dữ liệu giả: đăng nhập, mở dashboard/danh sách hồ sơ, tạo/sửa hồ sơ, tải mẫu tĩnh, import hợp lệ/có lỗi và hoàn tác, xuất báo cáo, upload/quét file. Đối chiếu số dòng và lỗi với fixture đã chọn; không gán kỳ vọng 12 mới/15 lỗi nếu danh mục fixture khác. Kiểm tra popup, light/dark và viewport 1280×800/390×844, zoom 200% tại các màn import bị thay gần đây. Ghi đúng case đã chạy.
- [ ] **6. Backup và restore trong VM.** Chạy `C:\ProgramData\IRM\tools\backup.ps1`; với archive vừa tạo, chạy `restore.ps1 -ArchivePath <archive.bak.gz>` theo prompt bảo mật. So sánh dữ liệu giả/file liên quan trước backup và sau restore, kiểm tra login và `/health/ready`. Không chỉ kiểm tra có file backup.
- [ ] **7. Chốt biên bản.** Chỉ đánh dấu sẵn sàng khi Task 1–4 đạt và đúng hash. Nếu thiếu VM/chứng thư hoặc còn case lỗi: báo điều kiện cụ thể và giữ candidate, không gọi đó là lỗi bộ cài cũ. Sau sửa source phải commit, tạo candidate mới và chạy lại các case bị ảnh hưởng trên EXE mới.

### Task 5: Chốt bộ bàn giao và lưu vết

**Files:** Update `docs/RELEASE-READINESS-R5.md`, `docs/DELIVERY-LAYOUT.md`, `docs/DELIVERY-INVENTORY.json`; update ignored `../BAN-GIAO/README.md`, `../BAN-GIAO/DANH-MUC-TEP.json`; copy final candidate to `../BAN-GIAO/01-HIEN-HANH/v1.0.2-R5/`.

**Interfaces:** Consumes biên bản PASS và exact signed EXE/hash Task 4. Produces một thư mục giao khách có EXE, checksum, hướng dẫn, ghi chú R5/source commit, kết quả nghiệm thu và hướng dẫn backup/khôi phục; không kèm cả stage SQL/runtime dư thừa cho khách.

- [ ] **1. Chuẩn bị tệp giao khách.** Copy EXE và sidecar đã nghiệm thu, hướng dẫn, ghi chú bản mới; đưa bản sao `SOURCE-REVISION.json` ra cạnh EXE cho dễ đối chiếu. Không chứa chứng thư private key, credentials hoặc dữ liệu thật. Hướng dẫn offline/đặt lại Admin phải nhất quán nội dung trong EXE.
- [ ] **2. Xác minh trước đưa vào hiện hành.** Kiểm tra lại chữ ký, hash EXE, mọi file trong danh mục, full commit và ngày build. Chỉ khi biên bản đạt, copy vào thư mục R5 mới; giữ nguyên thư mục/checksum R4, cập nhật README trỏ R5. Không cần di chuyển/xóa R4 để phát hành R5.
- [ ] **3. Cập nhật truy vết.** Append mục mới vào inventory theo cấu trúc hiện có, giữ nguyên mọi mục lịch sử; đồng bộ bản inventory trong `BAN-GIAO`. Ghi commit source build và commit tài liệu hậu nghiệm thu là hai mốc khác nhau. Commit tài liệu sau build không làm thay đổi source commit đã nhúng trong EXE.
- [ ] **4. Review và commit tài liệu.** Stage đúng ba tệp docs được quản lý, chạy `git diff --cached --check` và đọc staged diff; commit `docs(release): record verified offline R5 handover`. Kiểm tra `git status --short`, không stage `BAN-GIAO`, binary/log/credentials; không push.
- [ ] **5. Báo kết quả.** Cung cấp đường dẫn EXE giao khách, revision, source commit, SHA-256, trạng thái ký và các phép thử đạt; ghi rõ việc chuyển dữ liệu khách nằm ngoài phạm vi. Chỉ nói “đã phát hành” khi artifact thực sự được đưa vào hiện hành theo bước 2.

## Tự rà soát kế hoạch và bàn giao thực thi

- Phạm vi UI → Task 1; version/docs/source → Task 2; offline/provenance/signing → Task 3; regression candidate → Task 4; handover/history → Task 5.
- Năm Review Focus đều có phép kiểm tra tương ứng; không thêm unit test chỉ soi màu/markup cho thay đổi trình bày.
- Giữ interface worker và version/schema; không tạo helper/API chưa được định nghĩa. Không dùng bản test unsigned làm bản chính thức mặc nhiên.
- Chưa sửa sản phẩm hoặc build EXE trong bước lập kế hoạch. Phương thức đề xuất: **Native**, vì các thay đổi nằm trong cùng luồng installer và task đóng gói/nghiệm thu phụ thuộc tuần tự; thực thi theo `executing-plans` sau khi người dùng review. Nếu người dùng chọn subagent, dùng `subagent-driven-development`.
