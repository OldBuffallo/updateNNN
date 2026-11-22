# Giai đoạn 4 — Changelog IRM v0.1.0

> **Trạng thái:** Development/test implementation complete; production approval pending.

| CR | Yêu cầu | Trạng thái | Artifact chính |
|---|---|---|---|
| CR-010 | Hồ sơ thăm thân riêng và thân nhân NLĐ | Implemented | `ForeignPersons`, `StayCases`, `FamilyVisitDetails`, `/family-visitors` |
| CR-011 | CSLT và doanh nghiệp thuê | Implemented | `AccommodationsV010`, agreements, residence history |
| CR-012 | Kiểm tra hiện trường | Implemented | inspections + immutable snapshots + lookback |
| CR-013 | KTCK 15 ngày | Deferred phase 2 | Blueprint only; no page in v0.1.0 |
| CR-014 | Tra cứu lịch sử | Implemented from baseline | Temporal filter/as-of |
| CR-015 | Giấy tờ và định danh | Implemented | Central profile, documents, identities, import Excel mở rộng, statistics |
| CR-016 | Phân loại DN và khu vực | Implemented | Profiles/sites/zones/memberships |
| CR-017 | Thống kê địa bàn | Implemented | 54 units, distinct person drill-down, export |
| CR-018 | Pháp nhân DN | Implemented | representatives/legal documents/private files |
| CR-019 | WPF compatibility | Implemented at SQL contract level | Parameterized explicit-column inserts + smoke SQL |
| CR-019A | Bảo mật dịch vụ/export | Implemented | Cookie RBAC, service guards, account claim, formula neutralization, audit |
| CR-020 | Production deployment | Not performed | Requires separate backup/restore approval |

Chi tiết verdict và evidence: [IRM-v0.1.0-implementation-report.md](IRM-v0.1.0-implementation-report.md).
