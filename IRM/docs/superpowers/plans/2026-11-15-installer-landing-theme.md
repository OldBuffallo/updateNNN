# IRM R5 Landing Page Installer UI & Handover Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đồng bộ bộ cài Windows offline IRM v1.0.2 R5 theo giao diện Landing Page (Phương án A: Split 2 cột với nền xanh olive vát khối, Công an hiệu, nhân vật 3D Công an Nhân dân và nút hành động màu cam) và hoàn thiện bàn giao vào `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`.

**Architecture:** Giữ nguyên kiến trúc Inno Setup → PowerShell WinForms (`installer-theme.ps1` + `installer-ui.ps1`) → worker `install.ps1` và payload .NET 8 self-contained + SQL Server 2022 Express CU27. Bổ sung lớp `LandingForm` vẽ nền gradient vát khối, logo và nhân vật 3D không khóa file từ `MemoryStream`, đồng bộ bảng màu các control và báo cáo HTML.

**Tech Stack:** ASP.NET Core 8/Blazor, Windows PowerShell 5.1, WinForms/System.Drawing, Inno Setup 6, xUnit, Node test runner.

**Spec:** [2026-11-15-installer-landing-theme-design.md](../specs/2026-11-15-installer-landing-theme-design.md)

## Global Constraints

- Git root `NNN/`, nhánh `develop`.
- Giữ nguyên version `1.0.2`, revision `R5`, `AppId`, dịch vụ `IRM`, SQL instance `IRMEXPRESS`, cổng `5050/5443/14331`, schema và worker.
- Đọc ảnh qua `MemoryStream` và clone `Bitmap` để không khóa tệp trong `$PayloadRoot`.
- Source commit sạch trước khi chạy `build-windows-installer.ps1`.
- Giữ nguyên gói cũ, lưu candidate ở `BAN-GIAO/04-CHO-NGHIEM-THU/` và bản hoàn thiện bàn giao ở `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`.

---

### Task 1: Cập nhật UI Landing Page (Phương án A) cho bộ cài
**Files:** Modify `deploy/windows/installer-theme.ps1`, `deploy/windows/installer-ui.ps1`
- [ ] Cập nhật `$script:installerColors`, thêm `LandingForm` vẽ gradient xanh olive vát khối, Công an hiệu, watermark và nhân vật 3D trên bệ đổ bóng; cập nhật màu `DarkButton`, `DarkCheckBox`, `DarkListView`, `DarkProgressBar`.
- [ ] Thêm `Get-IrmInstallerImage` đọc ảnh qua `MemoryStream` không khóa file.
- [ ] Chuyển `installer-ui.ps1` sang `LandingForm` với bố cục Split 2 cột (cột trái `646px`, cột phải `250px` cho nhân vật 3D), cập nhật CSS báo cáo HTML và giải phóng tài nguyên ảnh khi đóng form.

### Task 2: Kiểm chứng trực quan, Test tự động và Tài liệu
**Files:** Modify `docs/INSTALLATION-v1.0.2.md`, `docs/RELEASE-READINESS-R5.md`
- [ ] Chạy `render-installer.ps1` và `render-surfaces.ps1` xuất ảnh vào `../BAN-GIAO/05-HO-TRO/2026-11-15-R5-LANDING/after/`, mở xem trực tiếp tất cả ảnh trạng thái.
- [ ] Chạy `environment.Tests.ps1` trên cả x64 và SysWOW64 (x86).
- [ ] Chạy `dotnet build IRM.sln -c Release --nologo`, `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build` với SQL Server biệt lập, và `node --test` cho toàn bộ JS tests.
- [ ] Cập nhật tài liệu hướng dẫn và biên bản R5, sau đó commit sạch mã nguồn.

### Task 3: Đóng gói candidate R5 mới
**Files:** Run `deploy/windows/build-windows-installer.ps1`
- [ ] Đóng gói với `-OutputDirectory "<root>\BAN-GIAO\04-CHO-NGHIEM-THU\20261115-v1.0.2-R5-LANDING-UNSIGNED-TEST" -AllowUnsigned`.
- [ ] Đối chiếu SHA-256 EXE, `SOURCE-REVISION.json` (`dirty=false`, khớp commit) và toàn bộ `SHA256SUMS.txt`.

### Task 4: Hoàn thiện bàn giao vào `BAN-GIAO/01-HIEN-HANH/` và cập nhật truy vết
**Files:** Create files in `../BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`; Modify `../BAN-GIAO/README.md`, `docs/RELEASE-READINESS-R5.md`, `docs/DELIVERY-LAYOUT.md`, `docs/DELIVERY-INVENTORY.json`, `../BAN-GIAO/DANH-MUC-TEP.json`
- [ ] Tạo thư mục bàn giao `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` đầy đủ EXE, `.sha256`, `SOURCE-REVISION.json`, `DOC-TRUOC-KHI-CAI.txt`, `HUONG-DAN-CAI-DAT.md`, `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`, `VERIFICATION.json`, `SHA256SUMS.txt`.
- [ ] Cập nhật README bàn giao, inventory (`DELIVERY-INVENTORY.json` & `DANH-MUC-TEP.json`), `DELIVERY-LAYOUT.md`, `RELEASE-READINESS-R5.md`.
- [ ] Commit sạch phần cập nhật tài liệu truy vết.
