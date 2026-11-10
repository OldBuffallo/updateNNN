# Sơ đồ và báo cáo kiến trúc database IRM v0.1.0

- **Ngày cập nhật:** 17/08/2026
- **Nguồn sự thật:** `IRM/Data/IrmDbContext.cs`, các model trong `IRM/Data/Models/`, migration `deploy/sql/07-v0.1.0.sql`
- **Phạm vi:** schema ứng dụng v0.1.0 và các bảng legacy được EF Core ánh xạ; không phải bản reverse-engineer database production của khách hàng

## 1. Tóm tắt

IRM v0.1.0 dùng mô hình **additive**: giữ nguyên bảng legacy để WPF tiếp tục hoạt động và thêm một lớp bảng mới phục vụ web, lịch sử hiệu lực, thống kê, kiểm tra và bảo mật.

| Nhóm | Số bảng | Vai trò |
|---|---:|---|
| Legacy WPF | 13 | Tài khoản, doanh nghiệp, lao động và danh mục cũ |
| Mở rộng có trước v0.1.0 | 6 | Du học sinh, audit, import, archive và mapping template |
| Mở rộng v0.1.0 | 24 | Hồ sơ hợp nhất, lịch sử, địa bàn, doanh nghiệp mở rộng, kiểm tra, RBAC và đồng bộ |
| **Tổng** | **43** | Schema được `IrmDbContext` ánh xạ |

Hai môi trường dùng cùng mô hình logic:

- VPS demo/staging: SQLite trên persistent volume.
- Mục tiêu khách hàng: SQL Server 2014, chỉ chạy migration sau backup/restore rehearsal và phê duyệt riêng.

## 2. Sơ đồ tổng thể

```mermaid
flowchart LR
  subgraph LEGACY["Dữ liệu nghiệp vụ có trước v0.1.0"]
    ACC[Accounts]
    COM[Companies]
    EMP[Employees]
    STU[Students]
    NAT[Nationality]
    CAT[Fields / Careers / CareerGroups]
    CONTACT[Investment / PhoneNumbers / Emails / Attach]
  end

  subgraph PERSON["Hồ sơ người nước ngoài v0.1.0"]
    FP[ForeignPersons]
    LINK[ForeignPersonSourceLinks]
    STAY[StayCases]
    FAMILY[FamilyVisitDetails]
    RES[ResidencePeriods]
    DOC[ImmigrationDocuments]
    EID[ElectronicIdentities]
  end

  subgraph PLACE["Địa bàn / doanh nghiệp / lưu trú"]
    AU[AdministrativeUnits]
    PROFILE[CompanyProfiles]
    SITE[CompanySites]
    ZONE[EconomicZones]
    MEMBER[SiteZoneMemberships]
    ACM[AccommodationsV010]
    AGREEMENT[CompanyAccommodationAgreements]
    REP[CompanyRepresentatives]
    LEGAL[CompanyLegalDocuments]
    FILE[StoredFiles]
  end

  subgraph INSPECT["Kiểm tra"]
    INS[InspectionsV010]
    SUBJECT[InspectionSubjects]
  end

  subgraph PLATFORM["Bảo mật / đồng bộ / vận hành"]
    CRED[WebCredentials]
    ROLE[WebRoleAssignments]
    EVENT[LegacyChangeEvents]
    ISSUE[MigrationIssues]
    AUDIT[AuditLogs]
    VERSION[SchemaVersions]
  end

  ACC --> COM
  COM --> EMP
  NAT --> EMP
  NAT --> STU
  CAT --> EMP
  COM --> CONTACT
  EMP -. "SourceType + SourceId" .-> LINK
  STU -. "SourceType + SourceId" .-> LINK
  LINK --> FP
  FP --> STAY --> FAMILY
  FP --> RES
  FP --> DOC
  FP --> EID
  NAT --> FP
  COM --> PROFILE
  COM --> SITE --> MEMBER --> ZONE
  COM --> AGREEMENT --> ACM
  COM --> REP
  COM --> LEGAL --> FILE
  AU --> SITE
  AU --> ACM
  AU --> RES
  ACM --> RES
  COM --> RES
  AU --> INS
  SITE --> INS
  ACM --> INS
  INS --> SUBJECT --> FP
  ACC --> CRED
  ACC --> ROLE
  AU --> ROLE
  EVENT -. "worker idempotent" .-> LINK
  ISSUE -. "manual resolution" .-> LINK
  ACC -. "actor/entity" .-> AUDIT
  VERSION -. "schema gate" .-> FP
```

Bản xem trực tiếp: [SVG](diagrams/irm-v0.1.0-database-overview.svg) · [PNG](diagrams/irm-v0.1.0-database-overview.png) · [nguồn Mermaid](diagrams/irm-v0.1.0-database-overview.mmd).

## 3. ERD hồ sơ người nước ngoài và lịch sử

```mermaid
erDiagram
  NATIONALITY {
    int IDNationality PK
    string NationalityCode UK
    string NationalityName
  }
  FOREIGN_PERSONS {
    int Id PK
    string FullName
    int Gender
    datetime Birthday
    string NationalityCode FK
    string PassportNumber
    string PassportSearchKey "indexed"
    bool IsDataIncomplete
    bool IsDeleted
  }
  FOREIGN_PERSON_SOURCE_LINKS {
    int Id PK
    int ForeignPersonId FK
    string SourceType "composite unique"
    string SourceId "composite unique"
    string SourceFingerprint
    datetime LastSynchronizedAt
  }
  STAY_CASES {
    int Id PK
    int ForeignPersonId FK
    string PurposeCode
    bool IsPrimary
    datetime ValidFrom
    datetime ValidTo
    int SponsorCompanyId FK
    int SponsorForeignPersonId FK
    string StatusCode
  }
  FAMILY_VISIT_DETAILS {
    int StayCaseId PK "also FK"
    string RelativeName
    string RelativeIdNumber
    string Relationship
    string RelativeTypeCode
    string RelativeNationalityCode
  }
  RESIDENCE_PERIODS {
    int Id PK
    int ForeignPersonId FK
    int AccommodationId FK
    int AdministrativeUnitId FK
    int ResponsibleCompanyId FK
    string AddressLine
    string ArrangementCode
    datetime ValidFrom
    datetime ValidTo
  }
  IMMIGRATION_DOCUMENTS {
    int Id PK
    int ForeignPersonId FK
    string TypeCode
    string Number
    datetime ValidFrom
    datetime ValidTo
    string StatusCode
  }
  ELECTRONIC_IDENTITIES {
    int Id PK
    int ForeignPersonId FK
    string IdentityNumber
    string IdentitySearchKey "indexed"
    datetime ValidFrom
    datetime ValidTo
    string StatusCode
  }

  NATIONALITY ||--o{ FOREIGN_PERSONS : classifies
  FOREIGN_PERSONS ||--o{ FOREIGN_PERSON_SOURCE_LINKS : links
  FOREIGN_PERSONS ||--o{ STAY_CASES : has
  FOREIGN_PERSONS o|--o{ STAY_CASES : sponsors
  STAY_CASES ||--o| FAMILY_VISIT_DETAILS : details
  FOREIGN_PERSONS ||--o{ RESIDENCE_PERIODS : resides
  FOREIGN_PERSONS ||--o{ IMMIGRATION_DOCUMENTS : holds
  FOREIGN_PERSONS ||--o{ ELECTRONIC_IDENTITIES : owns
```

`ForeignPersonSourceLinks` là liên kết polymorphic: cặp `(SourceType, SourceId)` trỏ logic tới `Employees`, `Students` hoặc nguồn khác. Database cố ý không tạo FK trực tiếp tới nhiều bảng nguồn; unique index bảo đảm một bản ghi nguồn chỉ liên kết một hồ sơ hợp nhất.

## 4. ERD doanh nghiệp, địa bàn, khu vực và lưu trú

```mermaid
erDiagram
  COMPANIES {
    int IDCompany PK
    string CompanyName
    int IDField FK
    int TrackerID FK
  }
  COMPANY_PROFILES {
    int CompanyId PK "also FK"
    string OwnershipTypeCode
    string TaxCode
    string BusinessRegistrationNumber
    datetime ValidFrom
    datetime ValidTo
  }
  ADMINISTRATIVE_UNITS {
    int Id PK
    string Code "composite unique"
    string Name
    string TypeCode
    int ParentId FK
    int PredecessorId FK
    datetime ValidFrom "composite unique"
    datetime ValidTo
  }
  COMPANY_SITES {
    int Id PK
    int CompanyId FK
    int AdministrativeUnitId FK
    string Name
    string SiteTypeCode
    string AddressLine
    datetime ValidFrom
    datetime ValidTo
  }
  ECONOMIC_ZONES {
    int Id PK
    string Code "composite unique"
    string Name
    string TypeCode
    datetime ValidFrom "composite unique"
    datetime ValidTo
  }
  SITE_ZONE_MEMBERSHIPS {
    int Id PK
    int CompanySiteId FK "composite unique"
    int EconomicZoneId FK "composite unique"
    datetime ValidFrom "composite unique"
    datetime ValidTo
  }
  ACCOMMODATIONS_V010 {
    int Id PK
    int AdministrativeUnitId FK
    string Name
    string TypeCode
    string AddressLine
    int Capacity
    bool IsDeleted
  }
  COMPANY_ACCOMMODATION_AGREEMENTS {
    int Id PK
    int CompanyId FK
    int AccommodationId FK
    string ScopeCode
    string UnitLabel
    datetime ValidFrom
    datetime ValidTo
  }
  COMPANY_REPRESENTATIVES {
    int Id PK
    int CompanyId FK
    string FullName
    string Title
    string IdNumber
    datetime ValidFrom
    datetime ValidTo
  }
  COMPANY_LEGAL_DOCUMENTS {
    int Id PK
    int CompanyId FK
    int StoredFileId FK
    int LegacyAttachId FK
    string TypeCode
    string DocumentNumber
    datetime IssueDate
    datetime ExpiryDate
  }
  STORED_FILES {
    int Id PK
    string StorageName
    string OriginalName
    string ContentType
    long Size
    string Sha256 "indexed"
    string ScanStatusCode
  }

  COMPANIES ||--o| COMPANY_PROFILES : extends
  COMPANIES ||--o{ COMPANY_SITES : operates
  ADMINISTRATIVE_UNITS ||--o{ COMPANY_SITES : locates
  COMPANY_SITES ||--o{ SITE_ZONE_MEMBERSHIPS : assigned
  ECONOMIC_ZONES ||--o{ SITE_ZONE_MEMBERSHIPS : groups
  ADMINISTRATIVE_UNITS o|--o{ ACCOMMODATIONS_V010 : locates
  COMPANIES ||--o{ COMPANY_ACCOMMODATION_AGREEMENTS : rents
  ACCOMMODATIONS_V010 ||--o{ COMPANY_ACCOMMODATION_AGREEMENTS : contracted
  COMPANIES ||--o{ COMPANY_REPRESENTATIVES : represented
  COMPANIES ||--o{ COMPANY_LEGAL_DOCUMENTS : owns
  STORED_FILES o|--o{ COMPANY_LEGAL_DOCUMENTS : stores
```

Quan hệ site–zone là many-to-many có thời gian hiệu lực. Quan hệ company–accommodation cũng có thời gian và phạm vi thuê (`WHOLE_PROPERTY`, căn/phòng hoặc hình thức khác), không gắn cứng cơ sở lưu trú vào một doanh nghiệp duy nhất.

## 5. ERD kiểm tra

```mermaid
erDiagram
  INSPECTIONS_V010 {
    int Id PK
    datetime InspectedAt "indexed"
    string LocationText
    int AdministrativeUnitId FK
    int AccommodationId FK
    int CompanySiteId FK
    string InspectorNames
    string TypeCode
    int CreatedByAccountId
  }
  INSPECTION_SUBJECTS {
    int Id PK
    int InspectionId FK "composite unique"
    int ForeignPersonId FK "composite unique"
    string PurposeCodeSnapshot
    string DocumentsSnapshot
    string WorkDescriptionSnapshot
    string AddressSnapshot
    string ResultCode
    string ViolationDetails
    string ActionTaken
  }
  FOREIGN_PERSONS {
    int Id PK
    string FullName
    string PassportSearchKey
  }
  ADMINISTRATIVE_UNITS {
    int Id PK
    string Code
    string Name
  }
  ACCOMMODATIONS_V010 {
    int Id PK
    string Name
  }
  COMPANY_SITES {
    int Id PK
    string Name
  }

  ADMINISTRATIVE_UNITS o|--o{ INSPECTIONS_V010 : locates
  ACCOMMODATIONS_V010 o|--o{ INSPECTIONS_V010 : at
  COMPANY_SITES o|--o{ INSPECTIONS_V010 : at
  INSPECTIONS_V010 ||--o{ INSPECTION_SUBJECTS : contains
  FOREIGN_PERSONS ||--o{ INSPECTION_SUBJECTS : inspected
```

`InspectionSubjects` giữ snapshot diện cư trú, giấy tờ, công việc và địa chỉ tại thời điểm kiểm tra. Vì vậy báo cáo lịch sử không bị thay đổi khi hồ sơ hiện tại được cập nhật sau này. Unique index `(ForeignPersonId, InspectionId)` ngăn một người bị thêm hai lần trong cùng đợt.

## 6. ERD tài khoản, phân quyền và đồng bộ

```mermaid
erDiagram
  ACCOUNTS {
    int IDUser PK
    string Username
    string Password
    int Permission
    int Delete_flag
  }
  WEB_CREDENTIALS {
    int AccountId PK "also FK"
    string PasswordHash
    bool MustChangePassword
    int FailedLoginCount
    datetime LockedUntil
    datetime UpdatedAt
  }
  WEB_ROLE_ASSIGNMENTS {
    int Id PK
    int AccountId FK "composite unique"
    string RoleCode "composite unique"
    int AdministrativeUnitId FK "composite unique"
  }
  ADMINISTRATIVE_UNITS {
    int Id PK
    string Code
    string Name
  }
  LEGACY_CHANGE_EVENTS {
    long Id PK
    string EntityType
    string EntityId
    string Operation
    string BeforeXml
    string AfterXml
    string StatusCode "indexed"
    int RetryCount
    datetime OccurredAt "indexed"
  }
  MIGRATION_ISSUES {
    long Id PK
    string SourceType "indexed"
    string SourceId
    string IssueCode
    string Description
    string StatusCode "indexed"
    datetime ResolvedAt
  }
  SCHEMA_VERSIONS {
    int Id PK
    string Version UK
    string Checksum
    datetime AppliedAt
  }

  ACCOUNTS ||--o| WEB_CREDENTIALS : authenticates
  ACCOUNTS ||--o{ WEB_ROLE_ASSIGNMENTS : assigned
  ADMINISTRATIVE_UNITS o|--o{ WEB_ROLE_ASSIGNMENTS : scopes
```

`Accounts.Password` vẫn tồn tại để WPF legacy hoạt động. Web dùng `WebCredentials.PasswordHash`; lần đăng nhập đầu hoặc khi quản trị đổi mật khẩu sẽ tạo/cập nhật hash. Các role chuẩn gồm `Admin`, `DataEditor`, `Inspector`, `Reporter`, `Viewer`.

`LegacyChangeEvents`, `MigrationIssues`, `AuditLogs` và `SchemaVersions` không dùng FK tới bản ghi nghiệp vụ. Đây là chủ đích để giữ được bằng chứng khi bản ghi nguồn đã thay đổi/xóa hoặc khi migration gặp dữ liệu không hợp lệ.

## 7. ERD nghiệp vụ có trước v0.1.0

```mermaid
erDiagram
  ACCOUNTS ||--o{ COMPANIES : tracks
  FIELDS ||--o{ COMPANIES : classifies
  COMPANIES ||--o{ EMPLOYEES : employs
  CAREER_GROUPS ||--o{ CAREERS : groups
  CAREERS o|--o{ EMPLOYEES : occupation
  NATIONALITY o|--o{ EMPLOYEES : nationality
  NATIONALITY o|--o{ STUDENTS : nationality
  COMPANIES ||--o{ INVESTMENT : investments
  COMPANIES ||--o{ PHONE_NUMBERS : phones
  COMPANIES ||--o{ EMAILS : emails
  COMPANIES ||--o{ ATTACH : attachments

  ACCOUNTS { int IDUser PK }
  FIELDS { int IDField PK }
  COMPANIES { int IDCompany PK int IDField FK int TrackerID FK }
  EMPLOYEES { int IDEmployee PK int IDCompany FK int IDCareer FK string Nationality FK string Passport }
  STUDENTS { int IDStudent PK string Nationality FK string Passport }
  CAREER_GROUPS { int IDCG PK }
  CAREERS { int IDCareer PK int IDCG FK }
  NATIONALITY { int IDNationality PK string NationalityCode UK }
  INVESTMENT { int IDInvestment PK int IDCompany FK }
  PHONE_NUMBERS { int IDPhoneNumber PK int IDCompany FK }
  EMAILS { int IDEmail PK int IDCompany FK }
  ATTACH { int IDAttach PK int IDCompany FK }
```

`Districts` và `Wards` tiếp tục là danh mục độc lập của legacy. v0.1.0 không thêm `IDDistrict` vào `Wards`; địa bàn mới dùng `AdministrativeUnits` có hiệu lực và quan hệ tiền nhiệm.

## 8. Danh mục 43 bảng

### 8.1 Legacy WPF

| Bảng | PK | Quan hệ chính | Mục đích |
|---|---|---|---|
| `Accounts` | `IDUser` | tracker của `Companies` | Tài khoản WPF/nguồn danh tính web |
| `Companies` | `IDCompany` | `Fields`, `Accounts` | Doanh nghiệp legacy |
| `Employees` | `IDEmployee` | `Companies`, `Careers`, `Nationality` | Lao động nước ngoài legacy |
| `Fields` | `IDField` | 1–n `Companies` | Lĩnh vực doanh nghiệp |
| `Careers` | `IDCareer` | `CareerGroups`; 1–n `Employees` | Nghề nghiệp |
| `CareerGroups` | `IDCG` | 1–n `Careers` | Nhóm nghề |
| `Nationality` | `IDNationality` | alternate key `NationalityCode` | Quốc tịch dùng chung |
| `Investment` | `IDInvestment` | `Companies` | Vốn/đầu tư doanh nghiệp |
| `PhoneNumbers` | `IDPhoneNumber` | `Companies` | Số điện thoại doanh nghiệp |
| `Emails` | `IDEmail` | `Companies` | Email doanh nghiệp |
| `Districts` | `IDDistrict` | không FK v0.1.0 | Danh mục huyện legacy |
| `Wards` | `IDWard` | không FK v0.1.0 | Danh mục xã/phường legacy |
| `Attach` | `IDAttach` | `Companies` | Metadata/file đính kèm legacy |

### 8.2 Bảng mở rộng có trước v0.1.0

| Bảng | PK/index | Mục đích |
|---|---|---|
| `Students` | `IDStudent`; index passport/quốc tịch/hạn visa | Du học sinh, được tạo bởi migration `05-students-table.sql` |
| `AuditLogs` | `Id` | Nhật ký hành động/đăng nhập/xuất dữ liệu |
| `ImportHistories` | `Id`; index `SessionId` | Tổng hợp kết quả mỗi phiên import |
| `ImportBackups` | `Id`; index `ImportSessionId` | Snapshot phục vụ rollback import |
| `ArchivedEmployees` | `Id`; index `OriginalId`, `ArchiveReason` | Lưu lao động đã archive |
| `ColumnMappingTemplates` | `Id` | Template ánh xạ cột Excel |

### 8.3 Bảng v0.1.0

| Bảng | PK | FK/unique quan trọng | Mục đích |
|---|---|---|---|
| `SchemaVersions` | `Id` | unique `Version` | Gate phiên bản schema/checksum |
| `ForeignPersons` | `Id` | `NationalityCode`; index passport hash | Danh tính người nước ngoài hợp nhất |
| `ForeignPersonSourceLinks` | `Id` | `ForeignPersonId`; unique source pair | Liên kết Employee/Student/nguồn khác |
| `StayCases` | `Id` | person, sponsor company/person | Diện cư trú theo thời gian |
| `FamilyVisitDetails` | `StayCaseId` | PK đồng thời FK | Chi tiết thân nhân/người bảo lãnh |
| `AdministrativeUnits` | `Id` | unique `(Code, ValidFrom)` | Địa bàn có hiệu lực/tiền nhiệm |
| `ResidencePeriods` | `Id` | person, địa bàn, CSLT, doanh nghiệp | Lịch sử nơi ở |
| `ImmigrationDocuments` | `Id` | person | Visa/gia hạn/thẻ và hiệu lực |
| `ElectronicIdentities` | `Id` | person; index identity hash | Số định danh điện tử theo thời gian |
| `CompanyProfiles` | `CompanyId` | PK/FK one-to-one | FDI/trong nước, MST, ĐKKD |
| `CompanySites` | `Id` | company, địa bàn | Nhiều địa điểm của doanh nghiệp |
| `EconomicZones` | `Id` | unique `(Code, ValidFrom)` | KCN/KKT/cụm công nghiệp |
| `SiteZoneMemberships` | `Id` | unique site–zone–from | Many-to-many site/khu theo thời gian |
| `AccommodationsV010` | `Id` | địa bàn | Cơ sở lưu trú vật lý |
| `CompanyAccommodationAgreements` | `Id` | company, accommodation | Hợp đồng/phạm vi thuê theo thời gian |
| `CompanyRepresentatives` | `Id` | company | Lịch sử đại diện pháp luật |
| `StoredFiles` | `Id` | index SHA-256 | Metadata file ngoài webroot |
| `CompanyLegalDocuments` | `Id` | company, stored file/legacy attach | Hồ sơ pháp nhân |
| `InspectionsV010` | `Id` | địa bàn/CSLT/site | Đợt kiểm tra |
| `InspectionSubjects` | `Id` | unique person–inspection | Người và snapshot kết quả kiểm tra |
| `WebCredentials` | `AccountId` | PK/FK one-to-one | Hash, throttling, khóa đăng nhập |
| `WebRoleAssignments` | `Id` | unique account–role–area | RBAC và phạm vi địa bàn |
| `LegacyChangeEvents` | `Id` | index status/time | Outbox snapshot từ trigger legacy |
| `MigrationIssues` | `Id` | index status/source | Hàng chờ dữ liệu cần xác minh |

## 9. Quy tắc dữ liệu quan trọng

1. Một bản ghi nguồn chỉ có một liên kết hợp nhất nhờ unique `(SourceType, SourceId)`.
2. Passport và số định danh có search key chuẩn hóa; không dùng chuỗi hiển thị làm khóa thống kê.
3. Mỗi người chỉ có một `StayCase.IsPrimary` hiệu lực tại một thời điểm; overlap được chặn ở service/test.
4. `ValidFrom/ValidTo` xuất hiện ở diện cư trú, nơi ở, giấy tờ, định danh, địa bàn, site, zone membership, đại diện và hợp đồng lưu trú.
5. Thống kê tổng người phải dùng `COUNT(DISTINCT ForeignPersonId)`.
6. Báo cáo as-of dùng trạng thái hiệu lực tại một ngày; báo cáo range dùng sự kiện phát sinh trong khoảng.
7. Snapshot kiểm tra không join ngược để thay thế bằng dữ liệu hiện tại.
8. File scan nằm ngoài database; `StoredFiles` chỉ lưu metadata, SHA-256 và trạng thái quét.

## 10. Cascade, restrict và vòng đời dữ liệu

### Cascade được cấu hình

- `ForeignPersons` → source links, stay cases, residence periods, immigration documents, electronic identities.
- `StayCases` → `FamilyVisitDetails`.
- `Companies` → profile, sites, agreements, representatives, legal documents.
- `CompanySites`/`EconomicZones` → memberships.
- `AccommodationsV010` → agreements.
- `InspectionsV010` → subjects.
- `Accounts` → web credential và role assignments.

### Restrict được cấu hình

- Quốc tịch đang được person sử dụng.
- Sponsor company/person của stay case.
- Địa bàn, CSLT và doanh nghiệp được lịch sử nơi ở/site/inspection tham chiếu.
- Stored file hoặc `Attach` legacy đang gắn với hồ sơ pháp nhân.
- Foreign person đã xuất hiện trong inspection subject.

Ứng dụng ưu tiên soft delete/effective dating. Không dùng cascade để xóa lịch sử nghiệp vụ đã phát sinh.

## 11. Luồng đồng bộ legacy → v0.1.0

```mermaid
sequenceDiagram
  participant WPF
  participant L as Legacy tables
  participant T as SQL triggers
  participant O as LegacyChangeEvents
  participant H as LegacySyncWorker
  participant N as v0.1.0 tables
  participant I as MigrationIssues

  WPF->>L: INSERT / UPDATE / DELETE
  L->>T: row change
  T->>O: XML before/after snapshot
  H->>O: claim PENDING event
  H->>N: upsert idempotent by source link
  alt thiếu hoặc mâu thuẫn dữ liệu
    H->>I: create OPEN issue
  else hợp lệ
    H->>O: mark PROCESSED
  end
```

Trigger chỉ ghi outbox; không thực hiện business logic nặng trong transaction của WPF. Worker có retry và dead-letter status để không làm mất sự kiện lỗi.

## 12. Đánh giá và khuyến nghị

### Điểm phù hợp

- Không phá schema legacy và cho phép WPF/web chạy song song.
- Có identity trung tâm nên tránh đếm trùng Employee–Student–thăm thân.
- Mô hình temporal đáp ứng báo cáo quá khứ từ baseline.
- Quan hệ nhiều site/nhiều khu/nhiều CSLT phản ánh đúng thực tế doanh nghiệp.
- Inspection snapshot hỗ trợ loại trừ theo lookback mà không xóa lịch sử.

### Rủi ro/hạn chế còn lại

- Liên kết polymorphic không có FK vật lý; phải giám sát orphan bằng worker và `MigrationIssues`.
- Overlap thời gian được chặn ở application service, chưa phải mọi trường hợp đều có constraint SQL.
- `CreatedByAccountId` trong `StoredFiles` và `InspectionsV010` hiện là liên kết logic, chưa cấu hình FK EF.
- `AuditLogs`, import session và migration issue chủ ý không có FK nên cần retention/cleanup policy riêng.
- Dữ liệu trước baseline là best effort; không thể suy ra chính xác lịch sử chưa từng được ghi.
- Module cửa khẩu (`BorderGates`, `BorderMovements`, `BorderStayEpisodes`) mới là blueprint phase 2, chưa nằm trong 43 bảng hiện tại.
- Production phải reverse-engineer schema thực tế của khách hàng và đối chiếu với báo cáo này sau backup/restore rehearsal.

### Đề xuất tiếp theo

1. UAT ERD với cán bộ nghiệp vụ, đặc biệt sponsor, loại CSLT và membership khu kinh tế.
2. Thêm job kiểm tra orphan cho source links và các logical account links.
3. Đánh giá constraint chống overlap ở SQL Server nếu không phá tương thích WPF.
4. Tạo dashboard `MigrationIssues`/dead-letter và SLA xử lý.
5. Sau khi có bản restore khách hàng, xuất schema/row-count/index/FK thực tế và lập diff ký duyệt trước go-live.

## 13. Kết luận

Schema v0.1.0 đáp ứng mô hình dữ liệu cho yêu cầu 1–3 và 5–9 theo hướng additive. `ForeignPersons` là trung tâm thống kê; `AdministrativeUnits`, effective dating và các bảng liên kết tạo nền cho báo cáo theo thời gian/địa bàn mà không sửa cột legacy. Sơ đồ này được duyệt cho **development/demo/staging**; production vẫn phụ thuộc baseline và UAT trên database khách hàng.
