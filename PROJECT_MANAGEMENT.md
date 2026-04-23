# Quản lý Dự án: Immigration Report Manager v2.0

Stack: ASP.NET Core 8 · Blazor Server · SQL Server 2014/2019

Tài liệu này định nghĩa **7 giai đoạn** của dự án, ánh xạ sang GitHub Issues & Kanban Board. Mỗi giai đoạn có tài liệu đính kèm, tiêu chí hoàn thành, và danh sách task cụ thể.

---

## Tổng quan Tiến trình

```text
[GĐ1: Khảo sát] → [GĐ2: Thiết kế] → [GĐ3: Demo] → [GĐ4: Chỉnh sửa]
                                                              ↓
                          [GĐ7: Bàn giao] ← [GĐ6: Kiểm tra] ← [GĐ5: Deploy Test]
```

| Giai đoạn | Tên | Tài liệu chính | Trạng thái |
| :---: | :--- | :--- | :---: |
| **GĐ1** | Khảo sát Hệ thống Cũ | `docs/phase1-analysis.md` | ✅ |
| **GĐ2** | Thiết kế Giải pháp | `docs/phase2-design.md` | ✅ |
| **GĐ3** | Demo Khách hàng | `demo.md`, `mockup-demo/` | ✅ |
| **GĐ4** | Chỉnh sửa theo Yêu cầu | `docs/phase4-changelog.md` | 🔄 |
| **GĐ5** | Deploy Test Server Thật | `deploy-guide.md`, `deploy/` | 🔄 |
| **GĐ6** | Kiểm tra & Sửa lỗi | `docs/phase6-testlog.md` | 📋 |
| **GĐ7** | Bàn giao & Hướng dẫn | `docs/phase7-handover.md` | 📋 |

**Chú thích:** ✅ Done · 🔄 In Progress · 📋 Todo

---

## Giai đoạn 1 — Khảo sát & Phân tích Hệ thống Cũ

**Mục tiêu:** Hiểu rõ phần mềm desktop cũ — cấu trúc dữ liệu, luồng nghiệp vụ, điểm yếu cần khắc phục — trước khi thiết kế giải pháp mới.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| `docs/phase1-analysis.md` | Báo cáo khảo sát: chức năng, hạn chế, rủi ro của hệ thống cũ |
| `docs/phase1-db-schema.md` | Sơ đồ cơ sở dữ liệu gốc (SQL Server 2014, các bảng chính) |

### Nội dung khảo sát cần ghi lại

- Danh sách màn hình/chức năng của phần mềm cũ
- Cấu trúc bảng SQL hiện tại (tên bảng, cột, quan hệ)
- Luồng nghiệp vụ: nhập liệu → báo cáo → xuất Excel
- Điểm yếu bảo mật (plain-text password, SQL Injection, v.v.)
- Khối lượng dữ liệu: số bản ghi, số người dùng, tần suất sử dụng

### Tiêu chí Hoàn thành (DoD)

- [ ] Đã liệt kê đầy đủ các chức năng hệ thống cũ
- [ ] Có sơ đồ ERD hoặc mô tả cấu trúc DB gốc
- [ ] Đã xác định ít nhất 3 vấn đề cần cải thiện
- [ ] Tài liệu được review bởi cả 2 thành viên

---

## Giai đoạn 2 — Thiết kế Giải pháp

**Mục tiêu:** Quyết định kiến trúc kỹ thuật, giải thích lý do lựa chọn, thiết kế DB mới và luồng ứng dụng trước khi code.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| `docs/phase2-design.md` | Tài liệu thiết kế kỹ thuật: kiến trúc, công nghệ, lý do lựa chọn |
| `docs/phase2-db-design.md` | Sơ đồ DB mới — Entity Relationship Diagram (Code First / EF Core) |
| `docs/phase2-wireframe.md` | Wireframe các màn hình chính (có thể link Figma/Excalidraw) |

### Nội dung thiết kế cần ghi lại

**Quyết định kiến trúc & lý do:**

| Quyết định | Lựa chọn | Lý do |
| :--- | :--- | :--- |
| Nền tảng | ASP.NET Core 8 Blazor Server | Real-time UI, không cần API riêng, phù hợp intranet |
| UI Framework | MudBlazor | Component phong phú, Material Design, miễn phí |
| Database | SQL Server 2014/2019 | Tương thích hệ thống cũ, khách hàng đã có license |
| Auth | ASP.NET Core Identity | Chuẩn bảo mật, hỗ trợ role-based authorization |
| ORM | Entity Framework Core 8 | Type-safe, Code First, migration dễ quản lý |
| Deploy | Windows Service (Self-hosted) | Không cần IIS, dễ cài đặt trên máy khách hàng |

### Tiêu chí Hoàn thành (DoD)

- [ ] Kiến trúc được phê duyệt bởi cả 2 thành viên
- [ ] Có sơ đồ DB mới với đầy đủ bảng và quan hệ
- [ ] Wireframe ít nhất 5 màn hình chính
- [ ] Đã xác định rõ scope — tính năng nào làm, tính năng nào bỏ

### Tasks liên quan (GitHub Issues)

| ID | Task | Người làm |
| :--- | :--- | :---: |
| **1.1** | Khởi tạo dự án ASP.NET Core 8 Blazor Server | Dev A |
| **1.2** | Kết nối Database & EF Core Layer | Dev A |
| **1.3** | Thiết lập Identity & Role-based Authorization | Dev A |
| **1.4** | Khởi tạo SCSS/CSS cơ bản & Cấu hình Theme | Dev B |
| **2.1** | Xây dựng Layout chính (MainLayout) | Dev B |

---

## Giai đoạn 3 — Tạo Demo gửi Khách hàng

**Mục tiêu:** Xây dựng bản demo hoạt động được để khách hàng review trước khi triển khai thật. Demo bao gồm dữ liệu mẫu và các tính năng cốt lõi.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| [`demo.md`](demo.md) | Mô tả các màn hình demo, screenshot, luồng thao tác |
| [`mockup-demo/index.html`](mockup-demo/index.html) | Demo tĩnh chạy trực tiếp trên trình duyệt |
| `README.md` | Link demo online (GitHub Pages) |

### Checklist Demo

- [ ] Dashboard hiển thị KPI và biểu đồ với dữ liệu mẫu
- [ ] Danh sách Công ty & Nhân viên có phân trang và tìm kiếm
- [ ] Import Excel wizard 4 bước hoạt động end-to-end
- [ ] Tìm kiếm toàn văn trả kết quả tức thì
- [ ] Trang Báo cáo có thể lọc và xuất
- [ ] Demo chạy được trên máy khách hàng (hoặc link online)

### Tiêu chí Hoàn thành (DoD)

- [ ] Khách hàng đã xem demo và xác nhận đúng yêu cầu
- [ ] Đã ghi lại feedback của khách hàng vào `docs/phase4-changelog.md`
- [ ] Không có lỗi UI nghiêm trọng khi khách hàng thao tác

### Tasks liên quan (GitHub Issues)

| ID | Task | Người làm |
| :--- | :--- | :---: |
| **2.2** | Xây dựng DataGrid Component dùng chung | Dev B |
| **2.3** | Tạo bộ Global UI (Modals, Toast, Buttons) | Dev B |
| **3.2** | Quản lý Danh sách Hồ sơ (Immigration Records) | Dev A |
| **3.3** | Tính năng Tìm kiếm toàn văn | Dev A |
| **3.5** | Dashboard Báo cáo & Thống kê trực quan | Dev B |
| **3.6** | Smart Import / Export Excel | Dev A |
| **3.7** | Trang Báo cáo (Reports) | Dev B |

---

## Giai đoạn 4 — Chỉnh sửa theo Yêu cầu Khách hàng

**Mục tiêu:** Thu thập feedback sau demo, ưu tiên và thực hiện các thay đổi được yêu cầu trước khi deploy lên server thật.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| `docs/phase4-changelog.md` | Log từng yêu cầu thay đổi: ngày nhận, mô tả, người làm, trạng thái |

### Quy trình xử lý Change Request

```text
Khách hàng yêu cầu
        ↓
Ghi vào phase4-changelog.md (ngày, nội dung, độ ưu tiên)
        ↓
Tạo GitHub Issue → Tạo branch feature/cr-<mô-tả-ngắn>
        ↓
Code → PR → Review → Merge vào main
        ↓
Demo lại cho khách hàng xác nhận
```

### Template Change Request Log

```md
| # | Ngày nhận | Yêu cầu | Độ ưu tiên | Người làm | Trạng thái |
|---|---|---|---|---|---|
| CR-001 | DD/MM/YYYY | Mô tả thay đổi | Cao/Trung/Thấp | Dev A/B | Todo/Done |
```

### Tiêu chí Hoàn thành (DoD)

- [ ] Tất cả change request Cao/Trung đã được xử lý
- [ ] Khách hàng ký xác nhận (hoặc gửi email chấp thuận) bản cuối
- [ ] Không còn issue nào ở trạng thái "In Progress" liên quan đến CR

### Tasks liên quan (GitHub Issues)

| ID | Task | Người làm |
| :--- | :--- | :---: |
| **3.1** | Chức năng Đăng nhập & Đổi mật khẩu | Dev A + Dev B |
| **3.4** | Form Thêm mới & Cập nhật Hồ sơ | Dev B |
| **3.8** | Audit Log — Ghi lịch sử thao tác | Dev A |

---

## Giai đoạn 5 — Deploy Test trên Server Thật

**Mục tiêu:** Triển khai ứng dụng lên môi trường server thật (staging hoặc production) theo đúng quy trình an toàn, kiểm tra vận hành thực tế.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| [`deploy-guide.md`](deploy-guide.md) | Hướng dẫn triển khai đầy đủ: cài đặt, cấu hình, chạy dịch vụ |
| [`deploy/`](deploy/) | Scripts, file cấu hình, công cụ hỗ trợ deploy |

### Checklist Deploy An toàn

**Chuẩn bị trước khi deploy:**

- [ ] Backup toàn bộ database hiện tại (`BACKUP DATABASE` SQL Server)
- [ ] Backup thư mục cài đặt phần mềm cũ
- [ ] Kiểm tra phiên bản .NET 8 Runtime đã cài trên server
- [ ] Kiểm tra SQL Server version (tương thích EF Core 8)
- [ ] Đã test build Release thành công trên máy dev

**Thực hiện deploy:**

- [ ] Chạy migration DB (`dotnet ef database update`)
- [ ] Copy binary vào thư mục server
- [ ] Cài đặt Windows Service (`sc create` hoặc script tự động)
- [ ] Cấu hình `appsettings.json` với connection string thật
- [ ] Mở port firewall (mặc định 5050)
- [ ] Khởi động service và kiểm tra log

**Kiểm tra sau deploy:**

- [ ] Đăng nhập được vào hệ thống mới
- [ ] Dữ liệu cũ được migrate đầy đủ
- [ ] Không có lỗi trong Windows Event Log
- [ ] Hiệu năng phản hồi < 3 giây cho các thao tác thông thường

### Kế hoạch Rollback

Nếu phát sinh lỗi nghiêm trọng sau deploy, thực hiện rollback theo thứ tự:

1. Stop Windows Service mới
2. Restore database từ backup
3. Khởi động lại phần mềm cũ
4. Thông báo khách hàng và ghi log lỗi

### Tasks liên quan (GitHub Issues)

| ID | Task | Người làm |
| :--- | :--- | :---: |
| **4.2** | Tạo Script Triển khai (Bản Single-Server) | Dev A |

---

## Giai đoạn 6 — Kiểm tra Hệ thống & Sửa lỗi

**Mục tiêu:** Vận hành thực tế trên server thật trong thời gian quan sát, ghi nhận và xử lý toàn bộ lỗi trước khi bàn giao chính thức.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| `docs/phase6-testlog.md` | Log kiểm tra: test case, kết quả, bug tìm được, trạng thái fix |

### Checklist Kiểm tra Hệ thống

**Chức năng:**

- [ ] Đăng nhập / Đăng xuất / Đổi mật khẩu
- [ ] CRUD Công ty (tạo, xem, sửa, xóa)
- [ ] CRUD Nhân viên / Hồ sơ lao động nước ngoài
- [ ] Import Excel — file hợp lệ và file lỗi định dạng
- [ ] Tìm kiếm toàn văn với nhiều từ khóa khác nhau
- [ ] Xuất báo cáo Excel/PDF
- [ ] Dashboard KPI và biểu đồ hiển thị đúng

**Bảo mật:**

- [ ] Không truy cập được trang nội bộ khi chưa đăng nhập
- [ ] Role Admin vs User có phân quyền đúng
- [ ] Audit log ghi lại đầy đủ thao tác nhạy cảm

**Hiệu năng:**

- [ ] Load danh sách 1000+ bản ghi < 3 giây
- [ ] Import file Excel 500 dòng hoàn thành trong < 30 giây
- [ ] Không rò rỉ bộ nhớ sau 8 giờ chạy liên tục

### Quy trình Báo & Xử lý Bug

```text
Tìm thấy lỗi → Ghi vào phase6-testlog.md
              → Tạo GitHub Issue (label: bug, priority: high/medium/low)
              → Dev xử lý → PR → Merge
              → Kiểm tra lại → Đóng Issue
```

### Tiêu chí Hoàn thành (DoD)

- [ ] Tất cả test case Critical và High đã pass
- [ ] Không còn bug mức Critical/High ở trạng thái open
- [ ] Hệ thống chạy ổn định liên tục ít nhất 3 ngày làm việc

---

## Giai đoạn 7 — Bàn giao Tài liệu & Hướng dẫn Sử dụng

**Mục tiêu:** Bàn giao toàn bộ tài liệu, hướng dẫn nhân viên khách hàng sử dụng hệ thống, và chuyển giao quyền quản trị.

### Tài liệu đính kèm

| File | Nội dung |
| :--- | :--- |
| `docs/phase7-handover.md` | Biên bản bàn giao chính thức (ký tên 2 bên) |
| `docs/user-manual.md` | Hướng dẫn sử dụng cho người dùng cuối (có ảnh chụp màn hình) |
| `docs/admin-manual.md` | Hướng dẫn quản trị: backup DB, thêm user, cập nhật phần mềm |
| [`deploy-guide.md`](deploy-guide.md) | Hướng dẫn cài đặt lại (nếu cần thiết trong tương lai) |

### Checklist Bàn giao

**Tài liệu kỹ thuật:**

- [ ] Source code được commit đầy đủ lên GitHub (nhánh `main`)
- [ ] `README.md` cập nhật đúng với phiên bản cuối
- [ ] `deploy-guide.md` đã được kiểm tra lại sau lần deploy thật
- [ ] Credentials (tài khoản DB, admin mặc định) được bàn giao qua kênh bảo mật

**Hướng dẫn sử dụng:**

- [ ] Hướng dẫn đăng nhập và đổi mật khẩu lần đầu
- [ ] Hướng dẫn thêm/sửa/xóa hồ sơ lao động
- [ ] Hướng dẫn import dữ liệu từ Excel
- [ ] Hướng dẫn xuất báo cáo
- [ ] Hướng dẫn xem Dashboard và đọc thống kê

**Hướng dẫn Quản trị:**

- [ ] Cách backup database định kỳ
- [ ] Cách thêm/xóa tài khoản người dùng
- [ ] Cách cập nhật phiên bản mới (update service)
- [ ] Cách đọc log lỗi khi hệ thống có sự cố

### Tiêu chí Hoàn thành (DoD)

- [ ] Khách hàng đã nhận đủ tài liệu và xác nhận bằng email/chữ ký
- [ ] Đã training ít nhất 1 buổi cho nhân viên sử dụng
- [ ] Quản trị viên phía khách hàng có thể tự reset mật khẩu và backup DB
- [ ] Biên bản bàn giao được 2 bên ký

### Tasks liên quan (GitHub Issues)

| ID | Task | Người làm |
| :--- | :--- | :---: |
| **4.1** | Tích hợp chức năng Hỗ trợ nội bộ (Contextual Help) | Dev B |

---

## Quy ước Làm việc trên GitHub

Tuân thủ **GitHub Flow** để 2 người không dẫm chân nhau:

1. **Issues:** Mọi task đều phải có Issue trước khi code.
2. **Branching:** Tạo nhánh từ `main`:
   - Tính năng mới: `feature/ten-tinh-nang` (vd: `feature/excel-import`)
   - Sửa lỗi: `bugfix/ten-loi` (vd: `bugfix/login-sql-injection`)
   - Change request: `fix/cr-<số>-mô-tả` (vd: `fix/cr-003-them-cot-bao-cao`)
3. **Commits:** Theo chuẩn Conventional Commits:
   - `feat: add user login page`
   - `fix: correct pagination on employee list`
   - `docs: update deploy guide for Windows Server 2019`
4. **Pull Request:** Mở PR khi xong, tag người còn lại review. **Không tự merge code của mình.**

### Template Issue

```md
### 📝 User Story
Là một <Vai trò>, tôi muốn <Hành động> để <Mục đích>.

### ✨ Definition of Done
- [ ] Tính năng hoạt động không lỗi.
- [ ] UI Responsive, tuân thủ bảng màu đã định.
- [ ] Đã kiểm tra bảo mật đầu vào.
- [ ] Không còn Warning từ Compiler.

### 🔗 Liên kết
- Giai đoạn: GĐ_
- Bảng SQL liên đới:
- Mockup/Wireframe:
```

---

## Cấu trúc Thư mục Tài liệu (docs/)

```text
docs/
├── phase1-analysis.md       # GĐ1: Phân tích hệ thống cũ
├── phase1-db-schema.md      # GĐ1: Sơ đồ DB gốc
├── phase2-design.md         # GĐ2: Tài liệu thiết kế kỹ thuật
├── phase2-db-design.md      # GĐ2: ERD hệ thống mới
├── phase2-wireframe.md      # GĐ2: Wireframe màn hình
├── phase4-changelog.md      # GĐ4: Log change request
├── phase6-testlog.md        # GĐ6: Log kiểm tra & bug
├── phase7-handover.md       # GĐ7: Biên bản bàn giao
├── user-manual.md           # GĐ7: Hướng dẫn người dùng cuối
└── admin-manual.md          # GĐ7: Hướng dẫn quản trị
```

> **Lưu ý:** Tạo thư mục `docs/` và commit từng file khi hoàn thành mỗi giai đoạn. Dùng Pull Request để merge tài liệu vào `main`, không commit thẳng.
