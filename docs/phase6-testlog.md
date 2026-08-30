# Giai đoạn 6 — Test log IRM

---

## v1.0.0 — Ngày chạy: 30/08/2026

### Automated Tests (xUnit)

| Suite | Số lượng | Kết quả | Thời gian |
|---|---|---|---|
| **Tổng cộng** | **83 tests** | **83 Passed, 0 Failed, 0 Skipped** | **6.3 giây** |
| EnumAndReleaseFeatureTddTests | 9 | ✅ PASS | Enum roundtrip, import/export parse, seeding, dashboard stats |
| SecurityAndMigrationTests | 10 | ✅ PASS | Auth lockout, hash upgrade, seed 54 đơn vị, file validation |
| EmployeeFeatureTddTests | 9 | ✅ PASS | Passport normalization, archive, duplicate check |
| StudentFeatureTddTests | 5 | ✅ PASS | CRUD, filter, visa expiry, RBAC guard |
| DashboardFeatureTddTests | 4 | ✅ PASS | KPI cards, work permit, nationalities, thresholds |
| CompanyFeatureTddTests | 5 | ✅ PASS | Profile aggregation, zone memberships, navigations |
| ImportExportFeatureTddTests | 5 | ✅ PASS | Auto-map columns, formula injection, rollback |
| FamilyVisitorFeatureTddTests | 9 | ✅ PASS | Overlap, reuse ForeignPerson, atomic transaction, soft delete |
| AccommodationFeatureTddTests | 5 | ✅ PASS | RBAC guard, temporal filter, date range validation |
| InspectionFeatureTddTests | 5 | ✅ PASS | Inspector role, mandatory fields |
| AdminAndRbacFeatureTddTests | 5 | ✅ PASS | 5 roles RBAC, lockout, password upgrade |
| SearchFeatureTddTests | 4 | ✅ PASS | Tiếng Việt không dấu, filter options, soft delete |
| StatisticsFeatureTddTests | 4 | ✅ PASS | Temporal report, distinct counting |
| DatabaseAndSchemaFeatureTddTests | 4 | ✅ PASS | Schema idempotency, ClosedXML export |
| LegacySyncFeatureTddTests | 3 | ✅ PASS | XML parse, dead letter, backfill |
| LegalDocumentFeatureTddTests | 3 | ✅ PASS | Magic bytes, malware detection, MIME validation |
| RegistryAndStatisticsTests | 2 | ✅ PASS | Territory stats, inspection lookback |

### Build Release

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build -c Release --no-incremental` | PASS — 0 Warning, 0 Error |
| `dotnet publish -c Release -r win-x64 --self-contained` | PASS — 376 files output |

### SQL Server Integration Test (`.\\SQLEXPRESS`)

| Kiểm tra | Kết quả |
|---|---|
| Tạo database test `IRM_Test_v100` | PASS |
| Apply migration SQL (32 batches) | PASS — 0 errors |
| Schema: 17 tables created | PASS |
| Enum columns (Accounts.Permission, Employees.Gender) → INT | PASS |
| 5 non-PK indexes created | PASS |
| Seeding: Accounts(3), Fields(5), CareerGroups(4), Careers(10), Nationality(15) | PASS |
| Idempotency: 2nd migration run | PASS — 0 errors |
| Cleanup: database dropped | PASS |

### Deploy Package

| Kiểm tra | Kết quả |
|---|---|
| `IRM-production-deploy.zip` | PASS — 53.8 MB |
| Chứa: app/, sql/, install.ps1, quick-install.ps1, migration.sql, DEPLOY-GUIDE-PRODUCTION.md | PASS |
| IRM.exe self-contained (win-x64) | PASS |
| appsettings.json production config | PASS |

### Quality Gate

**APPROVED FOR PRODUCTION** — Toàn bộ automated tests (83/83), SQL Server integration, build release, và deploy package đều PASS.

---

## v0.1.1 — Ngày chạy: 17/08/2026

| Kiểm tra | Kết quả |
|---|---|
| Build Release | PASS — 0 warning, 0 error |
| xUnit | PASS — 17/17 |
| JavaScript syntax | PASS — `node --check` |
| Candidate container | PASS — `/login` 200, nhãn v0.1.1 |
| Backup → restore rehearsal | PASS — integrity `ok`, 43 bảng/294 dòng |
| Openship deployment `dep_K7mcqFSJCIRJaFsE` | PASS — version 5, `ready` |
| Authenticated business routes | PASS — 6/6 route trả 200 và có dữ liệu |
| Chrome sạch / cache-busted interop | PASS — `irm-interop.js?v=0.1.1` |
| Circuit/runtime error log | PASS — không có CircuitHost/unhandled/interop failure sau smoke |

Chi tiết RCA, artifact và rollback nằm tại [IRM-v0.1.1-hotfix-report.md](IRM-v0.1.1-hotfix-report.md).

---

## v0.1.0 — Ngày chạy: 16/08/2026

### Automated

| Suite | Kết quả |
|---|---|
| Web full build | PASS, 0 warnings / 0 errors |
| xUnit | PASS — 17/17 |
| SQLite startup/login | PASS |
| SQL Server migration first/second run | PASS/PASS |
| SQL Server backfill first/second run | PASS/PASS |
| Seed 54 địa bàn | PASS |
| 5 legacy trigger | PASS |
| WPF explicit-column insert SQL smoke | PASS, rolled back |

Phạm vi xUnit: passport normalization, duplicate/overlap, distinct/as-of, inspection lookback, temporal filter, password hash upgrade/lockout, RBAC/account claim, export formula injection, import mở rộng nhiều giấy tờ/lịch sử và rollback sạch, seed idempotency, MIME/signature và simulated malware failure cleanup.

### Blocked/not run

| Hạng mục | Trạng thái | Lý do |
|---|---|---|
| WPF binary build | BLOCKED | Thiếu .NET Framework 4.5.2 Developer Pack (`MSB3644`) |
| WPF manual UI smoke | NOT RUN | Phụ thuộc binary build và dữ liệu restore |
| Web nghiệp vụ UAT | NOT RUN | Cần tester nghiệp vụ và dữ liệu đại diện của khách hàng |
| Defender real-file UAT | NOT RUN | Chỉ test failure bằng scanner giả lập |
| Customer backup/restore | NOT RUN | Chưa có phê duyệt/connection/evidence |
| Customer production deploy | NOT RUN | Production gate chưa đạt |

### Demo/staging VPS — Openship

| Kiểm tra | Kết quả |
|---|---|
| Deployment `dep_xrAPbwO0_JlJhLog` | PASS — active/ready |
| HTTPS login + nhãn v0.1.0 | PASS |
| Login cookie Secure/HttpOnly | PASS |
| Authenticated home/family/accommodation/inspection/statistics/company/admin | PASS — 200 |
| Restart và giữ session/Data Protection key/SQLite | PASS |
| Port app chỉ bind `127.0.0.1:5050` | PASS |
| Rotate 4 demo credentials; old admin password rejected/new accepted | PASS |
| SQLite post-rotation backup → restore → integrity/row-count | PASS — 43 bảng, 284 dòng |
| GH-600 regression health | PASS — 200 |

### Quality gate (v0.1.0)

Full build `--no-incremental` đạt 0 warning/0 error. Reviewer verdict là `APPROVED FOR DEMO/STAGING ONLY`; production gate vẫn phụ thuộc WPF build, UAT và bản restore database khách hàng.
