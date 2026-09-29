IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Accounts] (
        [IDUser] int NOT NULL IDENTITY,
        [Username] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [Permission] int NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([IDUser])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [AdministrativeUnits] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [TypeCode] nvarchar(max) NOT NULL,
        [ParentId] int NULL,
        [PredecessorId] int NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [LegacyDistrictId] int NULL,
        [LegacyWardId] int NULL,
        CONSTRAINT [PK_AdministrativeUnits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdministrativeUnits_AdministrativeUnits_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdministrativeUnits_AdministrativeUnits_PredecessorId] FOREIGN KEY ([PredecessorId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ArchivedEmployees] (
        [Id] bigint NOT NULL IDENTITY,
        [OriginalId] int NOT NULL,
        [StaffName] nvarchar(max) NOT NULL,
        [Gender] int NOT NULL,
        [Birthday] datetime2 NULL,
        [Nationality] nvarchar(max) NULL,
        [Passport] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [IDCareer] int NULL,
        [WorkPermit] int NOT NULL,
        [WorkPermitNumber] nvarchar(max) NULL,
        [VisaNumber] nvarchar(max) NULL,
        [TemporaryStay] datetime2 NULL,
        [Note] nvarchar(max) NULL,
        [SettlementResults] int NOT NULL,
        [SettlementResultsString] nvarchar(max) NULL,
        [IDUser] int NOT NULL,
        [IDCompany] int NOT NULL,
        [DateCreated] datetime2 NULL,
        [CardCreationDate] datetime2 NULL,
        [WorkingStatus] int NOT NULL,
        [DateOfJoin] datetime2 NULL,
        [DateOfLeave] datetime2 NULL,
        [FamilyVisit] int NOT NULL,
        [FamilyVisitRelativeName] nvarchar(max) NULL,
        [FamilyVisitRelationship] nvarchar(max) NULL,
        [FamilyVisitRelativeIdCard] nvarchar(max) NULL,
        [FamilyVisitStartDate] datetime2 NULL,
        [FamilyVisitEndDate] datetime2 NULL,
        [FamilyVisitNote] nvarchar(max) NULL,
        [CompanyName] nvarchar(max) NULL,
        [CareerName] nvarchar(max) NULL,
        [ArchiveReason] nvarchar(450) NOT NULL,
        [ArchivedBy] nvarchar(max) NULL,
        [ArchivedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ArchivedEmployees] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [Action] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] int NULL,
        [Description] nvarchar(max) NULL,
        [ChangesJson] nvarchar(max) NULL,
        [Username] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        [IpAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [CareerGroups] (
        [IDCG] int NOT NULL IDENTITY,
        [CareerGroupName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CareerGroups] PRIMARY KEY ([IDCG])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ColumnMappingTemplates] (
        [Id] int NOT NULL IDENTITY,
        [TemplateName] nvarchar(max) NOT NULL,
        [CompanyId] int NULL,
        [MappingJson] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ColumnMappingTemplates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Districts] (
        [IDDistrict] int NOT NULL IDENTITY,
        [DisTrictName] nvarchar(max) NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Districts] PRIMARY KEY ([IDDistrict])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [EconomicZones] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [TypeCode] nvarchar(max) NOT NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_EconomicZones] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Fields] (
        [IDField] int NOT NULL IDENTITY,
        [FieldName] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Fields] PRIMARY KEY ([IDField])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ImportBackups] (
        [Id] bigint NOT NULL IDENTITY,
        [ImportSessionId] nvarchar(450) NOT NULL,
        [ActionType] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(450) NOT NULL,
        [EntityId] int NOT NULL,
        [OldData] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ImportBackups] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ImportHistories] (
        [Id] bigint NOT NULL IDENTITY,
        [SessionId] nvarchar(450) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [CompanyId] int NOT NULL,
        [CompanyName] nvarchar(max) NULL,
        [TotalRows] int NOT NULL,
        [AddedRows] int NOT NULL,
        [UpdatedRows] int NOT NULL,
        [ErrorRows] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Username] nvarchar(max) NULL,
        [ImportDate] datetime2 NOT NULL,
        [ErrorDetails] nvarchar(max) NULL,
        CONSTRAINT [PK_ImportHistories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [LegacyChangeEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NOT NULL,
        [Operation] nvarchar(max) NOT NULL,
        [BeforeXml] nvarchar(max) NULL,
        [AfterXml] nvarchar(max) NULL,
        [OccurredAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [RetryCount] int NOT NULL,
        [StatusCode] nvarchar(450) NOT NULL,
        [LastError] nvarchar(max) NULL,
        CONSTRAINT [PK_LegacyChangeEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [MigrationIssues] (
        [Id] bigint NOT NULL IDENTITY,
        [SourceType] nvarchar(450) NOT NULL,
        [SourceId] nvarchar(max) NULL,
        [IssueCode] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [StatusCode] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ResolvedAt] datetime2 NULL,
        CONSTRAINT [PK_MigrationIssues] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Nationality] (
        [IDNationality] int NOT NULL IDENTITY,
        [NationalityCode] nvarchar(450) NOT NULL,
        [NationalityName] nvarchar(max) NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Nationality] PRIMARY KEY ([IDNationality]),
        CONSTRAINT [AK_Nationality_NationalityCode] UNIQUE ([NationalityCode])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [SchemaVersions] (
        [Id] int NOT NULL IDENTITY,
        [Version] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Checksum] nvarchar(max) NOT NULL,
        [AppliedAt] datetime2 NOT NULL,
        [AppliedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SchemaVersions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [StoredFiles] (
        [Id] int NOT NULL IDENTITY,
        [StorageName] nvarchar(max) NOT NULL,
        [OriginalName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [Size] bigint NOT NULL,
        [Sha256] nvarchar(450) NOT NULL,
        [ScanStatusCode] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedByAccountId] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StoredFiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Wards] (
        [IDWard] int NOT NULL IDENTITY,
        [WardName] nvarchar(max) NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Wards] PRIMARY KEY ([IDWard])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [WebCredentials] (
        [AccountId] int NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [MustChangePassword] bit NOT NULL,
        [FailedLoginCount] int NOT NULL,
        [LockedUntil] datetime2 NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WebCredentials] PRIMARY KEY ([AccountId]),
        CONSTRAINT [FK_WebCredentials_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([IDUser]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [AccommodationsV010] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [TypeCode] nvarchar(max) NOT NULL,
        [AddressLine] nvarchar(max) NOT NULL,
        [AdministrativeUnitId] int NULL,
        [ContactName] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        [Capacity] int NULL,
        [Note] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AccommodationsV010] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AccommodationsV010_AdministrativeUnits_AdministrativeUnitId] FOREIGN KEY ([AdministrativeUnitId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [WebRoleAssignments] (
        [Id] int NOT NULL IDENTITY,
        [AccountId] int NOT NULL,
        [RoleCode] nvarchar(450) NOT NULL,
        [AdministrativeUnitId] int NULL,
        CONSTRAINT [PK_WebRoleAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WebRoleAssignments_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([IDUser]) ON DELETE CASCADE,
        CONSTRAINT [FK_WebRoleAssignments_AdministrativeUnits_AdministrativeUnitId] FOREIGN KEY ([AdministrativeUnitId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Careers] (
        [IDCareer] int NOT NULL IDENTITY,
        [CareerName] nvarchar(max) NOT NULL,
        [IDCG] int NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Careers] PRIMARY KEY ([IDCareer]),
        CONSTRAINT [FK_Careers_CareerGroups_IDCG] FOREIGN KEY ([IDCG]) REFERENCES [CareerGroups] ([IDCG]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Companies] (
        [IDCompany] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(max) NOT NULL,
        [TypeOfBusiniess] nvarchar(max) NULL,
        [IDField] int NOT NULL,
        [LegalRepresentative] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [TotalAmount] int NOT NULL,
        [AmountOfExemption] int NOT NULL,
        [QuantityAvailable] int NOT NULL,
        [QuantityNotYet] int NOT NULL,
        [NumberOfPersonalities] int NOT NULL,
        [RegistrationProfile] nvarchar(max) NULL,
        [RegistrationProfileIndex] int NOT NULL,
        [DescriptionOfActivities] nvarchar(max) NULL,
        [TrackerID] int NOT NULL,
        [Note] nvarchar(max) NULL,
        [UpdateDay] datetime2 NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([IDCompany]),
        CONSTRAINT [FK_Companies_Accounts_TrackerID] FOREIGN KEY ([TrackerID]) REFERENCES [Accounts] ([IDUser]) ON DELETE CASCADE,
        CONSTRAINT [FK_Companies_Fields_IDField] FOREIGN KEY ([IDField]) REFERENCES [Fields] ([IDField]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ForeignPersons] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(max) NOT NULL,
        [Gender] int NOT NULL,
        [Birthday] datetime2 NULL,
        [NationalityCode] nvarchar(450) NULL,
        [PassportNumber] nvarchar(max) NULL,
        [PassportSearchKey] nvarchar(450) NULL,
        [IsDataIncomplete] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ForeignPersons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ForeignPersons_Nationality_NationalityCode] FOREIGN KEY ([NationalityCode]) REFERENCES [Nationality] ([NationalityCode]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Students] (
        [IDStudent] int NOT NULL IDENTITY,
        [FullName] nvarchar(max) NOT NULL,
        [Gender] int NOT NULL,
        [Birthday] datetime2 NULL,
        [Nationality] nvarchar(450) NULL,
        [Passport] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [SchoolName] nvarchar(max) NULL,
        [Major] nvarchar(max) NULL,
        [StudentCode] nvarchar(max) NULL,
        [EducationLevel] int NOT NULL DEFAULT 0,
        [EnrollmentDate] datetime2 NULL,
        [ExpectedGraduation] datetime2 NULL,
        [VisaNumber] nvarchar(max) NULL,
        [VisaExpiry] datetime2 NULL,
        [TemporaryStay] datetime2 NULL,
        [ScholarshipType] int NOT NULL DEFAULT 0,
        [Status] int NOT NULL DEFAULT 0,
        [Note] nvarchar(max) NULL,
        [IDUser] int NOT NULL,
        [DateCreated] datetime2 NULL,
        [Hidden_flag] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Students] PRIMARY KEY ([IDStudent]),
        CONSTRAINT [FK_Students_Nationality_Nationality] FOREIGN KEY ([Nationality]) REFERENCES [Nationality] ([NationalityCode])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Attach] (
        [IDAttach] int NOT NULL IDENTITY,
        [IDCompany] int NOT NULL,
        [Type] int NOT NULL,
        [Name] nvarchar(max) NULL,
        [Folder] nvarchar(max) NULL,
        [DateCreated] datetime2 NULL,
        [DateModified] datetime2 NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Attach] PRIMARY KEY ([IDAttach]),
        CONSTRAINT [FK_Attach_Companies_IDCompany] FOREIGN KEY ([IDCompany]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [CompanyAccommodationAgreements] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [AccommodationId] int NOT NULL,
        [ScopeCode] nvarchar(max) NOT NULL,
        [UnitLabel] nvarchar(max) NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_CompanyAccommodationAgreements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyAccommodationAgreements_AccommodationsV010_AccommodationId] FOREIGN KEY ([AccommodationId]) REFERENCES [AccommodationsV010] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CompanyAccommodationAgreements_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [CompanyProfiles] (
        [CompanyId] int NOT NULL,
        [OwnershipTypeCode] nvarchar(max) NOT NULL,
        [TaxCode] nvarchar(max) NULL,
        [BusinessRegistrationNumber] nvarchar(max) NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        CONSTRAINT [PK_CompanyProfiles] PRIMARY KEY ([CompanyId]),
        CONSTRAINT [FK_CompanyProfiles_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [CompanyRepresentatives] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Title] nvarchar(max) NULL,
        [IdNumber] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        CONSTRAINT [PK_CompanyRepresentatives] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyRepresentatives_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [CompanySites] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SiteTypeCode] nvarchar(max) NOT NULL,
        [AddressLine] nvarchar(max) NOT NULL,
        [AdministrativeUnitId] int NOT NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CompanySites] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanySites_AdministrativeUnits_AdministrativeUnitId] FOREIGN KEY ([AdministrativeUnitId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CompanySites_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Emails] (
        [IDEmail] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Mail] nvarchar(max) NULL,
        [IDCompany] int NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_Emails] PRIMARY KEY ([IDEmail]),
        CONSTRAINT [FK_Emails_Companies_IDCompany] FOREIGN KEY ([IDCompany]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Employees] (
        [IDEmployee] int NOT NULL IDENTITY,
        [StaffName] nvarchar(max) NOT NULL,
        [Gender] int NOT NULL,
        [Birthday] datetime2 NULL,
        [Nationality] nvarchar(450) NULL,
        [Passport] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [IDCareer] int NULL,
        [WorkPermit] int NOT NULL,
        [WorkPermitNumber] nvarchar(max) NULL,
        [VisaNumber] nvarchar(max) NULL,
        [TemporaryStay] datetime2 NULL,
        [Note] nvarchar(max) NULL,
        [SettlementResults] int NOT NULL,
        [SettlementResultsString] nvarchar(max) NULL,
        [IDUser] int NOT NULL,
        [IDCompany] int NOT NULL,
        [DateCreated] datetime2 NULL,
        [CardCreationDate] datetime2 NULL,
        [WorkingStatus] int NOT NULL,
        [DateOfJoin] datetime2 NULL,
        [DateOfLeave] datetime2 NULL,
        [Hidden_flag] int NOT NULL,
        [FamilyVisit] int NOT NULL DEFAULT 0,
        [FamilyVisitRelativeName] nvarchar(max) NULL,
        [FamilyVisitRelationship] nvarchar(max) NULL,
        [FamilyVisitRelativeIdCard] nvarchar(max) NULL,
        [FamilyVisitStartDate] datetime2 NULL,
        [FamilyVisitEndDate] datetime2 NULL,
        [FamilyVisitNote] nvarchar(max) NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([IDEmployee]),
        CONSTRAINT [FK_Employees_Careers_IDCareer] FOREIGN KEY ([IDCareer]) REFERENCES [Careers] ([IDCareer]),
        CONSTRAINT [FK_Employees_Companies_IDCompany] FOREIGN KEY ([IDCompany]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE,
        CONSTRAINT [FK_Employees_Nationality_Nationality] FOREIGN KEY ([Nationality]) REFERENCES [Nationality] ([NationalityCode])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [Investment] (
        [IDInvestment] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Nationality] nvarchar(max) NULL,
        [AmountOfMoney] decimal(18,2) NOT NULL,
        [IDCompany] int NOT NULL,
        [Passport] nvarchar(max) NULL,
        CONSTRAINT [PK_Investment] PRIMARY KEY ([IDInvestment]),
        CONSTRAINT [FK_Investment_Companies_IDCompany] FOREIGN KEY ([IDCompany]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [PhoneNumbers] (
        [IDPhoneNumber] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [IDCompany] int NOT NULL,
        [Delete_flag] int NOT NULL,
        CONSTRAINT [PK_PhoneNumbers] PRIMARY KEY ([IDPhoneNumber]),
        CONSTRAINT [FK_PhoneNumbers_Companies_IDCompany] FOREIGN KEY ([IDCompany]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ElectronicIdentities] (
        [Id] int NOT NULL IDENTITY,
        [ForeignPersonId] int NOT NULL,
        [IdentityNumber] nvarchar(max) NOT NULL,
        [IdentitySearchKey] nvarchar(450) NOT NULL,
        [IssuedAt] datetime2 NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [StatusCode] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ElectronicIdentities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ElectronicIdentities_ForeignPersons_ForeignPersonId] FOREIGN KEY ([ForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ForeignPersonSourceLinks] (
        [Id] int NOT NULL IDENTITY,
        [ForeignPersonId] int NOT NULL,
        [SourceType] nvarchar(450) NOT NULL,
        [SourceId] nvarchar(450) NOT NULL,
        [SourceFingerprint] nvarchar(max) NULL,
        [LastSynchronizedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ForeignPersonSourceLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ForeignPersonSourceLinks_ForeignPersons_ForeignPersonId] FOREIGN KEY ([ForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ImmigrationDocuments] (
        [Id] int NOT NULL IDENTITY,
        [ForeignPersonId] int NOT NULL,
        [TypeCode] nvarchar(450) NOT NULL,
        [Number] nvarchar(max) NOT NULL,
        [IssuedAt] datetime2 NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [IssuedBy] nvarchar(max) NULL,
        [StatusCode] nvarchar(max) NOT NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_ImmigrationDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ImmigrationDocuments_ForeignPersons_ForeignPersonId] FOREIGN KEY ([ForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [ResidencePeriods] (
        [Id] int NOT NULL IDENTITY,
        [ForeignPersonId] int NOT NULL,
        [AccommodationId] int NULL,
        [AdministrativeUnitId] int NOT NULL,
        [AddressLine] nvarchar(max) NOT NULL,
        [UnitLabel] nvarchar(max) NULL,
        [ArrangementCode] nvarchar(max) NOT NULL,
        [ResponsibleCompanyId] int NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_ResidencePeriods] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResidencePeriods_AccommodationsV010_AccommodationId] FOREIGN KEY ([AccommodationId]) REFERENCES [AccommodationsV010] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResidencePeriods_AdministrativeUnits_AdministrativeUnitId] FOREIGN KEY ([AdministrativeUnitId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResidencePeriods_Companies_ResponsibleCompanyId] FOREIGN KEY ([ResponsibleCompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResidencePeriods_ForeignPersons_ForeignPersonId] FOREIGN KEY ([ForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [StayCases] (
        [Id] int NOT NULL IDENTITY,
        [ForeignPersonId] int NOT NULL,
        [PurposeCode] nvarchar(max) NOT NULL,
        [IsPrimary] bit NOT NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [SponsorCompanyId] int NULL,
        [SponsorForeignPersonId] int NULL,
        [StatusCode] nvarchar(max) NOT NULL,
        [Note] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_StayCases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StayCases_Companies_SponsorCompanyId] FOREIGN KEY ([SponsorCompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StayCases_ForeignPersons_ForeignPersonId] FOREIGN KEY ([ForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StayCases_ForeignPersons_SponsorForeignPersonId] FOREIGN KEY ([SponsorForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [CompanyLegalDocuments] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [TypeCode] nvarchar(450) NOT NULL,
        [DocumentNumber] nvarchar(450) NULL,
        [IssueDate] datetime2 NULL,
        [IssuedBy] nvarchar(max) NULL,
        [ExpiryDate] datetime2 NULL,
        [StoredFileId] int NULL,
        [LegacyAttachId] int NULL,
        [Note] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CompanyLegalDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyLegalDocuments_Attach_LegacyAttachId] FOREIGN KEY ([LegacyAttachId]) REFERENCES [Attach] ([IDAttach]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CompanyLegalDocuments_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([IDCompany]) ON DELETE CASCADE,
        CONSTRAINT [FK_CompanyLegalDocuments_StoredFiles_StoredFileId] FOREIGN KEY ([StoredFileId]) REFERENCES [StoredFiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [InspectionsV010] (
        [Id] int NOT NULL IDENTITY,
        [InspectedAt] datetime2 NOT NULL,
        [LocationText] nvarchar(max) NOT NULL,
        [AdministrativeUnitId] int NULL,
        [AccommodationId] int NULL,
        [CompanySiteId] int NULL,
        [InspectorNames] nvarchar(max) NOT NULL,
        [InspectionUnit] nvarchar(max) NULL,
        [TypeCode] nvarchar(max) NOT NULL,
        [Summary] nvarchar(max) NULL,
        [Note] nvarchar(max) NULL,
        [CreatedByAccountId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InspectionsV010] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InspectionsV010_AccommodationsV010_AccommodationId] FOREIGN KEY ([AccommodationId]) REFERENCES [AccommodationsV010] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InspectionsV010_AdministrativeUnits_AdministrativeUnitId] FOREIGN KEY ([AdministrativeUnitId]) REFERENCES [AdministrativeUnits] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InspectionsV010_CompanySites_CompanySiteId] FOREIGN KEY ([CompanySiteId]) REFERENCES [CompanySites] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [SiteZoneMemberships] (
        [Id] int NOT NULL IDENTITY,
        [CompanySiteId] int NOT NULL,
        [EconomicZoneId] int NOT NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        CONSTRAINT [PK_SiteZoneMemberships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SiteZoneMemberships_CompanySites_CompanySiteId] FOREIGN KEY ([CompanySiteId]) REFERENCES [CompanySites] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SiteZoneMemberships_EconomicZones_EconomicZoneId] FOREIGN KEY ([EconomicZoneId]) REFERENCES [EconomicZones] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [FamilyVisitDetails] (
        [StayCaseId] int NOT NULL,
        [RelativeName] nvarchar(max) NOT NULL,
        [RelativeIdNumber] nvarchar(max) NULL,
        [RelativePhone] nvarchar(max) NULL,
        [RelativeAddress] nvarchar(max) NULL,
        [Relationship] nvarchar(max) NOT NULL,
        [RelativeTypeCode] nvarchar(max) NOT NULL,
        [RelativeNationalityCode] nvarchar(max) NULL,
        CONSTRAINT [PK_FamilyVisitDetails] PRIMARY KEY ([StayCaseId]),
        CONSTRAINT [FK_FamilyVisitDetails_StayCases_StayCaseId] FOREIGN KEY ([StayCaseId]) REFERENCES [StayCases] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE TABLE [InspectionSubjects] (
        [Id] int NOT NULL IDENTITY,
        [InspectionId] int NOT NULL,
        [ForeignPersonId] int NOT NULL,
        [PurposeCodeSnapshot] nvarchar(max) NOT NULL,
        [DocumentsSnapshot] nvarchar(max) NULL,
        [WorkDescriptionSnapshot] nvarchar(max) NULL,
        [AddressSnapshot] nvarchar(max) NULL,
        [ResultCode] nvarchar(max) NOT NULL,
        [ViolationDetails] nvarchar(max) NULL,
        [ActionTaken] nvarchar(max) NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_InspectionSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InspectionSubjects_ForeignPersons_ForeignPersonId] FOREIGN KEY ([ForeignPersonId]) REFERENCES [ForeignPersons] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InspectionSubjects_InspectionsV010_InspectionId] FOREIGN KEY ([InspectionId]) REFERENCES [InspectionsV010] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_AccommodationsV010_AdministrativeUnitId] ON [AccommodationsV010] ([AdministrativeUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AdministrativeUnits_Code_ValidFrom] ON [AdministrativeUnits] ([Code], [ValidFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_AdministrativeUnits_ParentId] ON [AdministrativeUnits] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_AdministrativeUnits_PredecessorId] ON [AdministrativeUnits] ([PredecessorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_AU_LegacyDistrictId] ON [AdministrativeUnits] ([LegacyDistrictId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_AU_LegacyWardId] ON [AdministrativeUnits] ([LegacyWardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ArchivedEmployees_ArchiveReason] ON [ArchivedEmployees] ([ArchiveReason]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ArchivedEmployees_OriginalId] ON [ArchivedEmployees] ([OriginalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Attach_IDCompany] ON [Attach] ([IDCompany]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Careers_IDCG] ON [Careers] ([IDCG]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Companies_DeleteFlag] ON [Companies] ([Delete_flag]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Companies_IDField] ON [Companies] ([IDField]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Companies_TrackerID] ON [Companies] ([TrackerID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanyAccommodationAgreements_AccommodationId] ON [CompanyAccommodationAgreements] ([AccommodationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanyAccommodationAgreements_CompanyId_AccommodationId_ValidFrom] ON [CompanyAccommodationAgreements] ([CompanyId], [AccommodationId], [ValidFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanyLegalDocuments_CompanyId_TypeCode_DocumentNumber] ON [CompanyLegalDocuments] ([CompanyId], [TypeCode], [DocumentNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanyLegalDocuments_LegacyAttachId] ON [CompanyLegalDocuments] ([LegacyAttachId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanyLegalDocuments_StoredFileId] ON [CompanyLegalDocuments] ([StoredFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanyRepresentatives_CompanyId] ON [CompanyRepresentatives] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanySites_AdministrativeUnitId] ON [CompanySites] ([AdministrativeUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_CompanySites_CompanyId_ValidFrom_ValidTo] ON [CompanySites] ([CompanyId], [ValidFrom], [ValidTo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EconomicZones_Code_ValidFrom] ON [EconomicZones] ([Code], [ValidFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ElectronicIdentities_ForeignPersonId] ON [ElectronicIdentities] ([ForeignPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ElectronicIdentities_IdentitySearchKey] ON [ElectronicIdentities] ([IdentitySearchKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Emails_IDCompany] ON [Emails] ([IDCompany]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Employees_FamilyVisit] ON [Employees] ([FamilyVisit]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Employees_IDCareer] ON [Employees] ([IDCareer]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Employees_IDCompany] ON [Employees] ([IDCompany]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Employees_Nationality] ON [Employees] ([Nationality]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Employees_TemporaryStay] ON [Employees] ([TemporaryStay]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Employees_WorkStatus] ON [Employees] ([WorkingStatus], [Hidden_flag]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ForeignPersons_NationalityCode] ON [ForeignPersons] ([NationalityCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ForeignPersons_PassportSearchKey] ON [ForeignPersons] ([PassportSearchKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ForeignPersonSourceLinks_ForeignPersonId] ON [ForeignPersonSourceLinks] ([ForeignPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ForeignPersonSourceLinks_SourceType_SourceId] ON [ForeignPersonSourceLinks] ([SourceType], [SourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ImmigrationDocuments_ForeignPersonId_TypeCode_ValidTo] ON [ImmigrationDocuments] ([ForeignPersonId], [TypeCode], [ValidTo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ImportBackups_EntityTypeId] ON [ImportBackups] ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ImportBackups_ImportSessionId] ON [ImportBackups] ([ImportSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_SessionId] ON [ImportHistories] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InspectionSubjects_ForeignPersonId_InspectionId] ON [InspectionSubjects] ([ForeignPersonId], [InspectionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_InspectionSubjects_InspectionId] ON [InspectionSubjects] ([InspectionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_InspectionsV010_AccommodationId] ON [InspectionsV010] ([AccommodationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_InspectionsV010_AdministrativeUnitId] ON [InspectionsV010] ([AdministrativeUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_InspectionsV010_CompanySiteId] ON [InspectionsV010] ([CompanySiteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_InspectionsV010_InspectedAt] ON [InspectionsV010] ([InspectedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Investment_IDCompany] ON [Investment] ([IDCompany]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_LegacyChangeEvents_StatusCode_OccurredAt] ON [LegacyChangeEvents] ([StatusCode], [OccurredAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_MigrationIssues_StatusCode_SourceType] ON [MigrationIssues] ([StatusCode], [SourceType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_PhoneNumbers_IDCompany] ON [PhoneNumbers] ([IDCompany]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ResidencePeriods_AccommodationId] ON [ResidencePeriods] ([AccommodationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ResidencePeriods_AdministrativeUnitId_ValidFrom_ValidTo] ON [ResidencePeriods] ([AdministrativeUnitId], [ValidFrom], [ValidTo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ResidencePeriods_ForeignPersonId] ON [ResidencePeriods] ([ForeignPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_ResidencePeriods_ResponsibleCompanyId] ON [ResidencePeriods] ([ResponsibleCompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SchemaVersions_Version] ON [SchemaVersions] ([Version]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SiteZoneMemberships_CompanySiteId_EconomicZoneId_ValidFrom] ON [SiteZoneMemberships] ([CompanySiteId], [EconomicZoneId], [ValidFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_SiteZoneMemberships_EconomicZoneId] ON [SiteZoneMemberships] ([EconomicZoneId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_StayCases_ForeignPersonId_ValidFrom_ValidTo] ON [StayCases] ([ForeignPersonId], [ValidFrom], [ValidTo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_StayCases_SponsorCompanyId] ON [StayCases] ([SponsorCompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_StayCases_SponsorForeignPersonId] ON [StayCases] ([SponsorForeignPersonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_StoredFiles_Sha256] ON [StoredFiles] ([Sha256]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Students_Nationality] ON [Students] ([Nationality]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Students_StatusHidden] ON [Students] ([Status], [Hidden_flag]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_Students_VisaExpiry] ON [Students] ([VisaExpiry]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_WebRoleAssignments_AccountId_RoleCode_AdministrativeUnitId] ON [WebRoleAssignments] ([AccountId], [RoleCode], [AdministrativeUnitId]) WHERE [AdministrativeUnitId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    CREATE INDEX [IX_WebRoleAssignments_AdministrativeUnitId] ON [WebRoleAssignments] ([AdministrativeUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921030335_InitialSqlServerV101'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921030335_InitialSqlServerV101', N'8.0.31');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260927120137_AddVisaSymbol'
)
BEGIN
    ALTER TABLE [Employees] ADD [VisaSymbol] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260927120137_AddVisaSymbol'
)
BEGIN
    ALTER TABLE [ArchivedEmployees] ADD [VisaSymbol] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260927120137_AddVisaSymbol'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260927120137_AddVisaSymbol', N'8.0.31');
END;
GO

COMMIT;
GO

