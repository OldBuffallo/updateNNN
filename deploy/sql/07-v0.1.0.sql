/* IRM 0.1.0 - SQL Server 2014 compatible, additive and idempotent.
   Run only after deploy/preflight-v0.1.0.ps1 has produced evidence for a tested backup/restore. */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Version nvarchar(32) = N'0.1.0';
DECLARE @Checksum nvarchar(128) = N'SHA256:IRM-0.1.0-20260816-001';

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL
        CREATE TABLE dbo.SchemaVersions (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaVersions PRIMARY KEY,
            Version nvarchar(32) NOT NULL,
            Description nvarchar(500) NOT NULL,
            Checksum nvarchar(128) NOT NULL,
            AppliedAt datetime2(0) NOT NULL CONSTRAINT DF_SchemaVersions_AppliedAt DEFAULT SYSUTCDATETIME(),
            AppliedBy nvarchar(128) NOT NULL,
            CONSTRAINT UQ_SchemaVersions_Version UNIQUE (Version)
        );

    IF EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE Version = @Version)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE Version = @Version AND Checksum = @Checksum)
            RAISERROR(N'Schema 0.1.0 exists with a different checksum.', 16, 1);
        COMMIT TRANSACTION;
        RETURN;
    END;

    IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL OR OBJECT_ID(N'dbo.Employees', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Companies', N'U') IS NULL
        RAISERROR(N'Legacy baseline is incomplete. Accounts, Employees and Companies are required.', 16, 1);
    IF COL_LENGTH(N'dbo.Companies',N'Uptime') IS NULL OR COL_LENGTH(N'dbo.Attach',N'DateDelete') IS NULL
        RAISERROR(N'Legacy schema does not match the WPF baseline (Companies.Uptime or Attach.DateDelete is missing). Stop and reconcile the baseline.',16,1);

    IF OBJECT_ID(N'dbo.AdministrativeUnits', N'U') IS NULL
        CREATE TABLE dbo.AdministrativeUnits (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdministrativeUnits PRIMARY KEY,
            Code nvarchar(32) NOT NULL, Name nvarchar(250) NOT NULL, TypeCode nvarchar(32) NOT NULL,
            ParentId int NULL, PredecessorId int NULL, ValidFrom datetime2(0) NOT NULL,
            ValidTo datetime2(0) NULL, IsDeleted bit NOT NULL CONSTRAINT DF_AdministrativeUnits_IsDeleted DEFAULT 0,
            CONSTRAINT FK_AdministrativeUnits_Parent FOREIGN KEY (ParentId) REFERENCES dbo.AdministrativeUnits(Id),
            CONSTRAINT FK_AdministrativeUnits_Predecessor FOREIGN KEY (PredecessorId) REFERENCES dbo.AdministrativeUnits(Id),
            CONSTRAINT UQ_AdministrativeUnits_CodeFrom UNIQUE (Code, ValidFrom),
            CONSTRAINT CK_AdministrativeUnits_Type CHECK (TypeCode IN (N'PROVINCE',N'COMMUNE',N'WARD',N'SPECIAL_ZONE')),
            CONSTRAINT CK_AdministrativeUnits_Dates CHECK (ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.ForeignPersons', N'U') IS NULL
        CREATE TABLE dbo.ForeignPersons (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForeignPersons PRIMARY KEY,
            FullName nvarchar(250) NOT NULL, Gender int NOT NULL CONSTRAINT DF_ForeignPersons_Gender DEFAULT 0,
            Birthday datetime2(0) NULL, NationalityCode nvarchar(10) NULL, PassportNumber nvarchar(100) NULL,
            PassportSearchKey varchar(64) NULL, IsDataIncomplete bit NOT NULL CONSTRAINT DF_ForeignPersons_Incomplete DEFAULT 0,
            IsDeleted bit NOT NULL CONSTRAINT DF_ForeignPersons_Deleted DEFAULT 0,
            CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ForeignPersons_Created DEFAULT SYSUTCDATETIME(),
            UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_ForeignPersons_Updated DEFAULT SYSUTCDATETIME()
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ForeignPersons') AND name=N'IX_ForeignPersons_PassportSearchKey')
        CREATE INDEX IX_ForeignPersons_PassportSearchKey ON dbo.ForeignPersons(PassportSearchKey) WHERE PassportSearchKey IS NOT NULL AND IsDeleted=0;

    IF OBJECT_ID(N'dbo.ForeignPersonSourceLinks', N'U') IS NULL
        CREATE TABLE dbo.ForeignPersonSourceLinks (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForeignPersonSourceLinks PRIMARY KEY,
            ForeignPersonId int NOT NULL, SourceType nvarchar(50) NOT NULL, SourceId nvarchar(100) NOT NULL,
            SourceFingerprint nvarchar(128) NULL, LastSynchronizedAt datetime2(0) NOT NULL CONSTRAINT DF_SourceLinks_Sync DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_SourceLinks_Person FOREIGN KEY(ForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT UQ_SourceLinks_Source UNIQUE(SourceType, SourceId)
        );

    IF OBJECT_ID(N'dbo.CompanyProfiles', N'U') IS NULL
        CREATE TABLE dbo.CompanyProfiles (
            CompanyId int NOT NULL CONSTRAINT PK_CompanyProfiles PRIMARY KEY, OwnershipTypeCode nvarchar(32) NOT NULL,
            TaxCode nvarchar(50) NULL, BusinessRegistrationNumber nvarchar(100) NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL,
            CONSTRAINT FK_CompanyProfiles_Company FOREIGN KEY(CompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT CK_CompanyProfiles_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.EconomicZones', N'U') IS NULL
        CREATE TABLE dbo.EconomicZones (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_EconomicZones PRIMARY KEY,
            Code nvarchar(50) NOT NULL, Name nvarchar(250) NOT NULL, TypeCode nvarchar(50) NOT NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL,
            IsDeleted bit NOT NULL CONSTRAINT DF_EconomicZones_Deleted DEFAULT 0,
            CONSTRAINT UQ_EconomicZones_CodeFrom UNIQUE(Code,ValidFrom),
            CONSTRAINT CK_EconomicZones_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.CompanySites', N'U') IS NULL
        CREATE TABLE dbo.CompanySites (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompanySites PRIMARY KEY, CompanyId int NOT NULL,
            Name nvarchar(250) NOT NULL, SiteTypeCode nvarchar(50) NOT NULL, AddressLine nvarchar(500) NOT NULL,
            AdministrativeUnitId int NOT NULL, ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL,
            IsDeleted bit NOT NULL CONSTRAINT DF_CompanySites_Deleted DEFAULT 0,
            CONSTRAINT FK_CompanySites_Company FOREIGN KEY(CompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT FK_CompanySites_Unit FOREIGN KEY(AdministrativeUnitId) REFERENCES dbo.AdministrativeUnits(Id),
            CONSTRAINT CK_CompanySites_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.CompanySites') AND name=N'IX_CompanySites_CompanyDates')
        CREATE INDEX IX_CompanySites_CompanyDates ON dbo.CompanySites(CompanyId,ValidFrom,ValidTo);

    IF OBJECT_ID(N'dbo.SiteZoneMemberships', N'U') IS NULL
        CREATE TABLE dbo.SiteZoneMemberships (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SiteZoneMemberships PRIMARY KEY,
            CompanySiteId int NOT NULL, EconomicZoneId int NOT NULL, ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL,
            CONSTRAINT FK_SiteZoneMemberships_Site FOREIGN KEY(CompanySiteId) REFERENCES dbo.CompanySites(Id),
            CONSTRAINT FK_SiteZoneMemberships_Zone FOREIGN KEY(EconomicZoneId) REFERENCES dbo.EconomicZones(Id),
            CONSTRAINT UQ_SiteZoneMemberships UNIQUE(CompanySiteId,EconomicZoneId,ValidFrom),
            CONSTRAINT CK_SiteZoneMemberships_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.AccommodationsV010', N'U') IS NULL
        CREATE TABLE dbo.AccommodationsV010 (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccommodationsV010 PRIMARY KEY,
            Name nvarchar(250) NOT NULL, TypeCode nvarchar(50) NOT NULL, AddressLine nvarchar(500) NOT NULL,
            AdministrativeUnitId int NULL, ContactName nvarchar(250) NULL, ContactPhone nvarchar(50) NULL,
            Capacity int NULL, Note nvarchar(max) NULL, IsDeleted bit NOT NULL CONSTRAINT DF_AccommodationsV010_Deleted DEFAULT 0,
            CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AccommodationsV010_Created DEFAULT SYSUTCDATETIME(),
            UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_AccommodationsV010_Updated DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_AccommodationsV010_Unit FOREIGN KEY(AdministrativeUnitId) REFERENCES dbo.AdministrativeUnits(Id)
        );

    IF OBJECT_ID(N'dbo.CompanyAccommodationAgreements', N'U') IS NULL
        CREATE TABLE dbo.CompanyAccommodationAgreements (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompanyAccommodationAgreements PRIMARY KEY,
            CompanyId int NOT NULL, AccommodationId int NOT NULL, ScopeCode nvarchar(50) NOT NULL,
            UnitLabel nvarchar(100) NULL, ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL, Note nvarchar(max) NULL,
            CONSTRAINT FK_Agreements_Company FOREIGN KEY(CompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT FK_Agreements_Accommodation FOREIGN KEY(AccommodationId) REFERENCES dbo.AccommodationsV010(Id),
            CONSTRAINT CK_Agreements_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.StayCases', N'U') IS NULL
        CREATE TABLE dbo.StayCases (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_StayCases PRIMARY KEY,
            ForeignPersonId int NOT NULL, PurposeCode nvarchar(50) NOT NULL, IsPrimary bit NOT NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL, SponsorCompanyId int NULL,
            SponsorForeignPersonId int NULL, StatusCode nvarchar(32) NOT NULL, Note nvarchar(max) NULL,
            CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_StayCases_Created DEFAULT SYSUTCDATETIME(),
            UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_StayCases_Updated DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_StayCases_Person FOREIGN KEY(ForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT FK_StayCases_Company FOREIGN KEY(SponsorCompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT FK_StayCases_SponsorPerson FOREIGN KEY(SponsorForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT CK_StayCases_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom),
            CONSTRAINT CK_StayCases_Purpose CHECK(PurposeCode IN (N'WORK',N'FAMILY_VISIT',N'STUDY',N'TOURISM',N'BORDER_VISA_EXEMPT',N'OTHER'))
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.StayCases') AND name=N'IX_StayCases_PersonDates')
        CREATE INDEX IX_StayCases_PersonDates ON dbo.StayCases(ForeignPersonId,ValidFrom,ValidTo) INCLUDE(PurposeCode,IsPrimary,StatusCode);

    IF OBJECT_ID(N'dbo.FamilyVisitDetails', N'U') IS NULL
        CREATE TABLE dbo.FamilyVisitDetails (
            StayCaseId int NOT NULL CONSTRAINT PK_FamilyVisitDetails PRIMARY KEY,
            RelativeName nvarchar(250) NOT NULL, RelativeIdNumber nvarchar(100) NULL, RelativePhone nvarchar(50) NULL,
            RelativeAddress nvarchar(500) NULL, Relationship nvarchar(100) NOT NULL,
            RelativeTypeCode nvarchar(50) NOT NULL, RelativeNationalityCode nvarchar(10) NULL,
            CONSTRAINT FK_FamilyVisitDetails_Case FOREIGN KEY(StayCaseId) REFERENCES dbo.StayCases(Id)
        );

    IF OBJECT_ID(N'dbo.ResidencePeriods', N'U') IS NULL
        CREATE TABLE dbo.ResidencePeriods (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ResidencePeriods PRIMARY KEY, ForeignPersonId int NOT NULL,
            AccommodationId int NULL, AdministrativeUnitId int NOT NULL, AddressLine nvarchar(500) NOT NULL,
            UnitLabel nvarchar(100) NULL, ArrangementCode nvarchar(50) NOT NULL, ResponsibleCompanyId int NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL, Note nvarchar(max) NULL,
            CONSTRAINT FK_ResidencePeriods_Person FOREIGN KEY(ForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT FK_ResidencePeriods_Accommodation FOREIGN KEY(AccommodationId) REFERENCES dbo.AccommodationsV010(Id),
            CONSTRAINT FK_ResidencePeriods_Unit FOREIGN KEY(AdministrativeUnitId) REFERENCES dbo.AdministrativeUnits(Id),
            CONSTRAINT FK_ResidencePeriods_Company FOREIGN KEY(ResponsibleCompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT CK_ResidencePeriods_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ResidencePeriods') AND name=N'IX_ResidencePeriods_UnitDates')
        CREATE INDEX IX_ResidencePeriods_UnitDates ON dbo.ResidencePeriods(AdministrativeUnitId,ValidFrom,ValidTo) INCLUDE(ForeignPersonId,AccommodationId);

    IF OBJECT_ID(N'dbo.ImmigrationDocuments', N'U') IS NULL
        CREATE TABLE dbo.ImmigrationDocuments (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ImmigrationDocuments PRIMARY KEY, ForeignPersonId int NOT NULL,
            TypeCode nvarchar(50) NOT NULL, Number nvarchar(150) NOT NULL, IssuedAt datetime2(0) NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL, IssuedBy nvarchar(250) NULL,
            StatusCode nvarchar(32) NOT NULL, Note nvarchar(max) NULL,
            CONSTRAINT FK_ImmigrationDocuments_Person FOREIGN KEY(ForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT CK_ImmigrationDocuments_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.ElectronicIdentities', N'U') IS NULL
        CREATE TABLE dbo.ElectronicIdentities (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ElectronicIdentities PRIMARY KEY, ForeignPersonId int NOT NULL,
            IdentityNumber nvarchar(150) NOT NULL, IdentitySearchKey varchar(64) NOT NULL, IssuedAt datetime2(0) NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL, StatusCode nvarchar(32) NOT NULL,
            CONSTRAINT FK_ElectronicIdentities_Person FOREIGN KEY(ForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT CK_ElectronicIdentities_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.CompanyRepresentatives', N'U') IS NULL
        CREATE TABLE dbo.CompanyRepresentatives (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompanyRepresentatives PRIMARY KEY, CompanyId int NOT NULL,
            FullName nvarchar(250) NOT NULL, Title nvarchar(150) NULL, IdNumber nvarchar(100) NULL, Phone nvarchar(50) NULL,
            ValidFrom datetime2(0) NOT NULL, ValidTo datetime2(0) NULL,
            CONSTRAINT FK_CompanyRepresentatives_Company FOREIGN KEY(CompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT CK_CompanyRepresentatives_Dates CHECK(ValidTo IS NULL OR ValidTo >= ValidFrom)
        );

    IF OBJECT_ID(N'dbo.StoredFiles', N'U') IS NULL
        CREATE TABLE dbo.StoredFiles (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_StoredFiles PRIMARY KEY, StorageName nvarchar(100) NOT NULL,
            OriginalName nvarchar(500) NOT NULL, ContentType nvarchar(100) NOT NULL, Size bigint NOT NULL,
            Sha256 varchar(64) NOT NULL, ScanStatusCode nvarchar(32) NOT NULL,
            CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_StoredFiles_Created DEFAULT SYSUTCDATETIME(),
            CreatedByAccountId int NOT NULL, IsDeleted bit NOT NULL CONSTRAINT DF_StoredFiles_Deleted DEFAULT 0,
            CONSTRAINT FK_StoredFiles_Account FOREIGN KEY(CreatedByAccountId) REFERENCES dbo.Accounts(IDUser),
            CONSTRAINT UQ_StoredFiles_StorageName UNIQUE(StorageName)
        );

    IF OBJECT_ID(N'dbo.CompanyLegalDocuments', N'U') IS NULL
        CREATE TABLE dbo.CompanyLegalDocuments (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompanyLegalDocuments PRIMARY KEY, CompanyId int NOT NULL,
            TypeCode nvarchar(50) NOT NULL, DocumentNumber nvarchar(150) NULL, IssueDate datetime2(0) NULL,
            IssuedBy nvarchar(250) NULL, ExpiryDate datetime2(0) NULL, StoredFileId int NULL,
            LegacyAttachId int NULL, Note nvarchar(max) NULL, IsDeleted bit NOT NULL CONSTRAINT DF_CompanyLegalDocuments_Deleted DEFAULT 0,
            CONSTRAINT FK_CompanyLegalDocuments_Company FOREIGN KEY(CompanyId) REFERENCES dbo.Companies(IDCompany),
            CONSTRAINT FK_CompanyLegalDocuments_File FOREIGN KEY(StoredFileId) REFERENCES dbo.StoredFiles(Id),
            CONSTRAINT FK_CompanyLegalDocuments_LegacyAttach FOREIGN KEY(LegacyAttachId) REFERENCES dbo.Attach(IDAttach)
        );

    IF OBJECT_ID(N'dbo.InspectionsV010', N'U') IS NULL
        CREATE TABLE dbo.InspectionsV010 (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_InspectionsV010 PRIMARY KEY, InspectedAt datetime2(0) NOT NULL,
            LocationText nvarchar(500) NOT NULL, AdministrativeUnitId int NULL, AccommodationId int NULL, CompanySiteId int NULL,
            InspectorNames nvarchar(500) NOT NULL, InspectionUnit nvarchar(250) NULL, TypeCode nvarchar(50) NOT NULL,
            Summary nvarchar(max) NULL, Note nvarchar(max) NULL, CreatedByAccountId int NOT NULL,
            CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_InspectionsV010_Created DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_InspectionsV010_Unit FOREIGN KEY(AdministrativeUnitId) REFERENCES dbo.AdministrativeUnits(Id),
            CONSTRAINT FK_InspectionsV010_Accommodation FOREIGN KEY(AccommodationId) REFERENCES dbo.AccommodationsV010(Id),
            CONSTRAINT FK_InspectionsV010_Site FOREIGN KEY(CompanySiteId) REFERENCES dbo.CompanySites(Id),
            CONSTRAINT FK_InspectionsV010_Account FOREIGN KEY(CreatedByAccountId) REFERENCES dbo.Accounts(IDUser)
        );

    IF OBJECT_ID(N'dbo.InspectionSubjects', N'U') IS NULL
        CREATE TABLE dbo.InspectionSubjects (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_InspectionSubjects PRIMARY KEY,
            InspectionId int NOT NULL, ForeignPersonId int NOT NULL, PurposeCodeSnapshot nvarchar(50) NOT NULL,
            DocumentsSnapshot nvarchar(max) NULL, WorkDescriptionSnapshot nvarchar(max) NULL, AddressSnapshot nvarchar(500) NULL,
            ResultCode nvarchar(32) NOT NULL, ViolationDetails nvarchar(max) NULL, ActionTaken nvarchar(max) NULL, Note nvarchar(max) NULL,
            CONSTRAINT FK_InspectionSubjects_Inspection FOREIGN KEY(InspectionId) REFERENCES dbo.InspectionsV010(Id),
            CONSTRAINT FK_InspectionSubjects_Person FOREIGN KEY(ForeignPersonId) REFERENCES dbo.ForeignPersons(Id),
            CONSTRAINT UQ_InspectionSubjects UNIQUE(ForeignPersonId,InspectionId)
        );

    IF OBJECT_ID(N'dbo.WebCredentials', N'U') IS NULL
        CREATE TABLE dbo.WebCredentials (
            AccountId int NOT NULL CONSTRAINT PK_WebCredentials PRIMARY KEY, PasswordHash nvarchar(1000) NOT NULL,
            MustChangePassword bit NOT NULL CONSTRAINT DF_WebCredentials_MustChange DEFAULT 0,
            FailedLoginCount int NOT NULL CONSTRAINT DF_WebCredentials_Failed DEFAULT 0, LockedUntil datetime2(0) NULL,
            UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_WebCredentials_Updated DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_WebCredentials_Account FOREIGN KEY(AccountId) REFERENCES dbo.Accounts(IDUser)
        );

    IF OBJECT_ID(N'dbo.WebRoleAssignments', N'U') IS NULL
        CREATE TABLE dbo.WebRoleAssignments (
            Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_WebRoleAssignments PRIMARY KEY, AccountId int NOT NULL,
            RoleCode nvarchar(50) NOT NULL, AdministrativeUnitId int NULL,
            CONSTRAINT FK_WebRoleAssignments_Account FOREIGN KEY(AccountId) REFERENCES dbo.Accounts(IDUser),
            CONSTRAINT FK_WebRoleAssignments_Unit FOREIGN KEY(AdministrativeUnitId) REFERENCES dbo.AdministrativeUnits(Id),
            CONSTRAINT CK_WebRoleAssignments_Role CHECK(RoleCode IN (N'ADMIN',N'DATA_EDITOR',N'INSPECTOR',N'REPORTER',N'VIEWER'))
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.WebRoleAssignments') AND name=N'UQ_WebRoleAssignments')
        CREATE UNIQUE INDEX UQ_WebRoleAssignments ON dbo.WebRoleAssignments(AccountId,RoleCode,AdministrativeUnitId);

    IF OBJECT_ID(N'dbo.LegacyChangeEvents', N'U') IS NULL
        CREATE TABLE dbo.LegacyChangeEvents (
            Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_LegacyChangeEvents PRIMARY KEY,
            EntityType nvarchar(50) NOT NULL, EntityId nvarchar(100) NOT NULL, Operation nvarchar(20) NOT NULL,
            BeforeXml nvarchar(max) NULL, AfterXml nvarchar(max) NULL,
            OccurredAt datetime2(0) NOT NULL CONSTRAINT DF_LegacyChangeEvents_Occurred DEFAULT SYSUTCDATETIME(),
            ProcessedAt datetime2(0) NULL, RetryCount int NOT NULL CONSTRAINT DF_LegacyChangeEvents_Retry DEFAULT 0,
            StatusCode nvarchar(32) NOT NULL CONSTRAINT DF_LegacyChangeEvents_Status DEFAULT N'PENDING', LastError nvarchar(2000) NULL
        );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.LegacyChangeEvents') AND name=N'IX_LegacyChangeEvents_StatusTime')
        CREATE INDEX IX_LegacyChangeEvents_StatusTime ON dbo.LegacyChangeEvents(StatusCode,OccurredAt);

    IF OBJECT_ID(N'dbo.MigrationIssues', N'U') IS NULL
        CREATE TABLE dbo.MigrationIssues (
            Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_MigrationIssues PRIMARY KEY,
            SourceType nvarchar(50) NOT NULL, SourceId nvarchar(100) NULL, IssueCode nvarchar(50) NOT NULL,
            Description nvarchar(2000) NOT NULL, StatusCode nvarchar(32) NOT NULL CONSTRAINT DF_MigrationIssues_Status DEFAULT N'OPEN',
            CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_MigrationIssues_Created DEFAULT SYSUTCDATETIME(), ResolvedAt datetime2(0) NULL
        );

    DECLARE @Units TABLE(Code nvarchar(32), Name nvarchar(250), TypeCode nvarchar(32));
    INSERT INTO @Units VALUES
      (N'QN-C-001',N'Quảng La',N'COMMUNE'),(N'QN-C-002',N'Thống Nhất',N'COMMUNE'),(N'QN-C-003',N'Hải Hòa',N'COMMUNE'),
      (N'QN-C-004',N'Tiên Yên',N'COMMUNE'),(N'QN-C-005',N'Điền Xá',N'COMMUNE'),(N'QN-C-006',N'Đông Ngũ',N'COMMUNE'),
      (N'QN-C-007',N'Hải Lạng',N'COMMUNE'),(N'QN-C-008',N'Lương Minh',N'COMMUNE'),(N'QN-C-009',N'Kỳ Thượng',N'COMMUNE'),
      (N'QN-C-010',N'Ba Chẽ',N'COMMUNE'),(N'QN-C-011',N'Quảng Tân',N'COMMUNE'),(N'QN-C-012',N'Đầm Hà',N'COMMUNE'),
      (N'QN-C-013',N'Quảng Hà',N'COMMUNE'),(N'QN-C-014',N'Đường Hoa',N'COMMUNE'),(N'QN-C-015',N'Quảng Đức',N'COMMUNE'),
      (N'QN-C-016',N'Hoành Mô',N'COMMUNE'),(N'QN-C-017',N'Lục Hồn',N'COMMUNE'),(N'QN-C-018',N'Bình Liêu',N'COMMUNE'),
      (N'QN-C-019',N'Hải Sơn',N'COMMUNE'),(N'QN-C-020',N'Hải Ninh',N'COMMUNE'),(N'QN-C-021',N'Vĩnh Thực',N'COMMUNE'),
      (N'QN-C-022',N'Cái Chiên',N'COMMUNE'),
      (N'QN-W-001',N'An Sinh',N'WARD'),(N'QN-W-002',N'Đông Triều',N'WARD'),(N'QN-W-003',N'Bình Khê',N'WARD'),
      (N'QN-W-004',N'Mạo Khê',N'WARD'),(N'QN-W-005',N'Hoàng Quế',N'WARD'),(N'QN-W-006',N'Yên Tử',N'WARD'),
      (N'QN-W-007',N'Vàng Danh',N'WARD'),(N'QN-W-008',N'Uông Bí',N'WARD'),(N'QN-W-009',N'Đông Mai',N'WARD'),
      (N'QN-W-010',N'Hiệp Hòa',N'WARD'),(N'QN-W-011',N'Quảng Yên',N'WARD'),(N'QN-W-012',N'Hà An',N'WARD'),
      (N'QN-W-013',N'Phong Cốc',N'WARD'),(N'QN-W-014',N'Liên Hòa',N'WARD'),(N'QN-W-015',N'Tuần Châu',N'WARD'),
      (N'QN-W-016',N'Việt Hưng',N'WARD'),(N'QN-W-017',N'Bãi Cháy',N'WARD'),(N'QN-W-018',N'Hà Tu',N'WARD'),
      (N'QN-W-019',N'Hà Lầm',N'WARD'),(N'QN-W-020',N'Cao Xanh',N'WARD'),(N'QN-W-021',N'Hồng Gai',N'WARD'),
      (N'QN-W-022',N'Hạ Long',N'WARD'),(N'QN-W-023',N'Hoành Bồ',N'WARD'),(N'QN-W-024',N'Mông Dương',N'WARD'),
      (N'QN-W-025',N'Quang Hanh',N'WARD'),(N'QN-W-026',N'Cẩm Phả',N'WARD'),(N'QN-W-027',N'Cửa Ông',N'WARD'),
      (N'QN-W-028',N'Móng Cái 1',N'WARD'),(N'QN-W-029',N'Móng Cái 2',N'WARD'),(N'QN-W-030',N'Móng Cái 3',N'WARD'),
      (N'QN-S-001',N'Vân Đồn',N'SPECIAL_ZONE'),(N'QN-S-002',N'Cô Tô',N'SPECIAL_ZONE');
    INSERT dbo.AdministrativeUnits(Code,Name,TypeCode,ValidFrom,IsDeleted)
        SELECT u.Code,u.Name,u.TypeCode,CONVERT(datetime2(0),'2025-07-01'),0 FROM @Units u
        WHERE NOT EXISTS(SELECT 1 FROM dbo.AdministrativeUnits a WHERE a.Code=u.Code AND a.ValidFrom=CONVERT(datetime2(0),'2025-07-01'));

    INSERT dbo.WebRoleAssignments(AccountId,RoleCode,AdministrativeUnitId)
        SELECT a.IDUser, CASE WHEN a.Permission=1 THEN N'ADMIN' ELSE N'DATA_EDITOR' END, NULL
        FROM dbo.Accounts a WHERE a.Delete_flag=0
        AND NOT EXISTS(SELECT 1 FROM dbo.WebRoleAssignments r WHERE r.AccountId=a.IDUser);

    INSERT dbo.SchemaVersions(Version,Description,Checksum,AppliedBy)
        VALUES(@Version,N'IRM 0.1.0 additive extension schema',@Checksum,COALESCE(SUSER_SNAME(),N'unknown'));
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
