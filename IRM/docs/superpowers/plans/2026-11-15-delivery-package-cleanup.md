# IRM Delivery Package Cleanup & Handover Streamlining Plan (Phương án 1)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan step-by-step. Check off each item only after verifying on disk.

**Goal:** Tổ chức lại thư mục `BAN-GIAO/` theo **Phương án 1** (đồng bộ `IRM/docs/DELIVERY-LAYOUT.md`) sao cho:
1. `BAN-GIAO/01-HIEN-HANH/` chỉ chứa **duy nhất 1 gói cài đặt hiện hành**: `v1.0.2-R5-UNSIGNED-TEST/` (bằng cách di chuyển nguyên vẹn `v1.0.2-DARK-R4-UNSIGNED-TEST/` vào `BAN-GIAO/02-LUU-TRU/`).
2. Bổ sung `HUONG-DAN-CAI-DAT.html` tự chứa (phong cách olive/cam của R5, 10 mục hướng dẫn đầy đủ, thông số khớp hoàn toàn `install.ps1` / `installer-ui.ps1` / `HUONG-DAN-CAI-DAT.md`) vào `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` và cập nhật `DOC-TRUOC-KHI-CAI.txt`, `SHA256SUMS.txt`.
3. Dọn dẹp các tệp build tạm/trùng lặp trong `BAN-GIAO/04-CHO-NGHIEM-THU/` (~4,86 GB) sau khi lưu dấu vết manifest (`SOURCE-REVISION.json`, `stage-SHA256SUMS.txt`, `.exe.sha256`) và xác nhận media SQL gốc trong `IRM/deploy/vendor/windows/` nguyên vẹn.
4. Đồng bộ `IRM/docs/DELIVERY-INVENTORY.json`, `BAN-GIAO/DANH-MUC-TEP.json`, `BAN-GIAO/README.md`, `IRM/docs/DELIVERY-LAYOUT.md`, `IRM/docs/RELEASE-READINESS-R5.md`, kiểm chứng `170/170` tệp trong inventory khớp dung lượng & SHA-256 trên đĩa và commit trên nhánh `develop`.

---

## Step 1: Save Plan Document
- [x] Lưu kế hoạch tại `IRM/docs/superpowers/plans/2026-11-15-delivery-package-cleanup.md`.

## Step 2: Move `v1.0.2-DARK-R4-UNSIGNED-TEST` to `02-LUU-TRU/`
- [x] Di chuyển nguyên thư mục `BAN-GIAO/01-HIEN-HANH/v1.0.2-DARK-R4-UNSIGNED-TEST/` sang `BAN-GIAO/02-LUU-TRU/v1.0.2-DARK-R4-UNSIGNED-TEST/`.
- [x] Xác minh `BAN-GIAO/01-HIEN-HANH/` chỉ còn đúng `v1.0.2-R5-UNSIGNED-TEST/`.
- [x] Xác minh 8 tệp trong `BAN-GIAO/02-LUU-TRU/v1.0.2-DARK-R4-UNSIGNED-TEST/` khớp nguyên vẹn `SHA256SUMS.txt` (không chỉnh sửa bất kỳ tệp nào trong R4).

## Step 3: Add `HUONG-DAN-CAI-DAT.html` & Update R5 Package Checksums
- [x] Tạo `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/HUONG-DAN-CAI-DAT.html` (UTF-8, bảng màu `#3f5223` / `#eef5dc` / `#ea580c`, hỗ trợ in `@media print`) với đầy đủ 10 mục tác nghiệp khớp `install.ps1`, `installer-ui.ps1`, `HUONG-DAN-CAI-DAT.md` và cấu trúc 10 mục của bản R4.
- [x] Cập nhật `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/DOC-TRUOC-KHI-CAI.txt` để trỏ tới `HUONG-DAN-CAI-DAT.html` / `HUONG-DAN-CAI-DAT.md` và ghi chú R4 đã chuyển sang `02-LUU-TRU/v1.0.2-DARK-R4-UNSIGNED-TEST/`.
- [x] Tính lại `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/SHA256SUMS.txt` cho đủ 8 tệp còn lại trong gói R5 và kiểm chứng 8/8 khớp SHA-256 trên đĩa.

## Step 4: Clean Up `BAN-GIAO/04-CHO-NGHIEM-THU/`
- [x] Kiểm tra `IRM/deploy/vendor/windows/SQLEXPR_x64_ENU.exe` và `SQLServer2022-KB5104824-x64.exe` nguyên vẹn khớp `IRM/deploy/release-manifest.json`, và `DELIVERY-INVENTORY.json` có `0` mục nằm trong `04-CHO-NGHIEM-THU/`.
- [x] Trong `BAN-GIAO/04-CHO-NGHIEM-THU/20261115-v1.0.2-R5-LANDING-UNSIGNED-TEST/`: sao chép `windows-stage/SOURCE-REVISION.json` → `SOURCE-REVISION.json` và `windows-stage/SHA256SUMS.txt` → `stage-SHA256SUMS.txt`; xóa `windows-stage/` (`867 MB`) và bản EXE trùng `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` (`754 MB`, bản chính đã lưu tại `01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`), giữ lại `.exe.sha256`, `SOURCE-REVISION.json`, `stage-SHA256SUMS.txt`.
- [x] Xóa 2 thư mục candidate cũ: `20261111-183841-705-v1.0.2` và `20261114-v1.0.2-R5-UNSIGNED-TEST`.
- [x] Xác minh dung lượng `BAN-GIAO/04-CHO-NGHIEM-THU/` giảm xuống `< 1 MB`.

## Step 5: Sync Inventory, Documentation & Commit
- [x] Cập nhật `IRM/docs/DELIVERY-INVENTORY.json`: đổi `"path"` của 8 tệp R4 sang `02-LUU-TRU/v1.0.2-DARK-R4-UNSIGNED-TEST/...`, cập nhật `DOC-TRUOC-KHI-CAI.txt` & `SHA256SUMS.txt` của R5, thêm `01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/HUONG-DAN-CAI-DAT.html`, cập nhật `"fileCount"` thành `170`.
- [x] Đồng bộ sang `BAN-GIAO/DANH-MUC-TEP.json` và chạy script xác minh `170/170` tệp trên đĩa (`0` missing, `0` size mismatch, `0` hash mismatch).
- [x] Cập nhật `BAN-GIAO/README.md`, `IRM/docs/DELIVERY-LAYOUT.md` và `IRM/docs/RELEASE-READINESS-R5.md`.
- [x] Stage các thay đổi tracked trong `IRM/docs/`, kiểm tra `git status --short` & `git diff --cached --check`, commit cục bộ trên `develop` (không push).
