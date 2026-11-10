-- =============================================================
-- IRM v2.0 — Script 00: Tạo database + seed data mẫu
-- Chạy ĐẦU TIÊN nếu chưa có database ReportManagerDB
-- =============================================================

-- Tạo database nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ReportManagerDB')
BEGIN
    CREATE DATABASE [ReportManagerDB];
    PRINT N'[OK] Đã tạo database ReportManagerDB';
END
ELSE
    PRINT N'[--] Database ReportManagerDB đã tồn tại';
GO

USE [ReportManagerDB];
GO

-- ══════════════════════════════════════════════
-- PHẦN 1: TẠO BẢNG
-- ══════════════════════════════════════════════

-- ── Accounts ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts')
CREATE TABLE [dbo].[Accounts] (
    [IDUser]      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Username]    NVARCHAR(100) NOT NULL,
    [Name]        NVARCHAR(200) NOT NULL,
    [Password]    NVARCHAR(200) NOT NULL,
    [Permission]  INT NOT NULL DEFAULT 0,
    [Delete_flag] INT NOT NULL DEFAULT 0
);
GO

-- ── Fields ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Fields')
CREATE TABLE [dbo].[Fields] (
    [IDField]     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FieldName]   NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Delete_flag] INT NOT NULL DEFAULT 0
);
GO

-- ── CareerGroups ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CareerGroups')
CREATE TABLE [dbo].[CareerGroups] (
    [IDCG]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CareerGroupName] NVARCHAR(200) NOT NULL
);
GO

-- ── Careers ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Careers')
CREATE TABLE [dbo].[Careers] (
    [IDCareer]    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CareerName]  NVARCHAR(200) NOT NULL,
    [IDCG]        INT NOT NULL,
    [Delete_flag] INT NOT NULL DEFAULT 0,
    FOREIGN KEY ([IDCG]) REFERENCES [CareerGroups]([IDCG])
);
GO

-- ── Nationality ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Nationality')
CREATE TABLE [dbo].[Nationality] (
    [IDNationality]   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [NationalityCode] NVARCHAR(10) NOT NULL UNIQUE,
    [NationalityName] NVARCHAR(200) NOT NULL,
    [Delete_flag]     INT NOT NULL DEFAULT 0
);
GO

-- ── Companies ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Companies')
CREATE TABLE [dbo].[Companies] (
    [IDCompany]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CompanyName]            NVARCHAR(500) NOT NULL,
    [TypeOfBusiniess]        NVARCHAR(200) NULL,
    [IDField]                INT NOT NULL DEFAULT 1,
    [LegalRepresentative]    NVARCHAR(200) NULL,
    [Address]                NVARCHAR(500) NULL,
    [TotalAmount]            INT NOT NULL DEFAULT 0,
    [AmountOfExemption]      INT NOT NULL DEFAULT 0,
    [QuantityAvailable]      INT NOT NULL DEFAULT 0,
    [QuantityNotYet]         INT NOT NULL DEFAULT 0,
    [NumberOfPersonalities]  INT NOT NULL DEFAULT 0,
    [RegistrationProfile]    NVARCHAR(500) NULL,
    [RegistrationProfileIndex] INT NOT NULL DEFAULT 0,
    [DescriptionOfActivities] NVARCHAR(MAX) NULL,
    [TrackerID]              INT NOT NULL DEFAULT 1,
    [Note]                   NVARCHAR(MAX) NULL,
    [UpdateDay]              NVARCHAR(50) NULL,
    [Delete_flag]            INT NOT NULL DEFAULT 0,
    FOREIGN KEY ([IDField]) REFERENCES [Fields]([IDField]),
    FOREIGN KEY ([TrackerID]) REFERENCES [Accounts]([IDUser])
);
GO

-- ── Employees ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
CREATE TABLE [dbo].[Employees] (
    [IDEmployee]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StaffName]               NVARCHAR(200) NOT NULL,
    [Gender]                  INT NOT NULL DEFAULT 1,
    [Birthday]                DATETIME NULL,
    [Nationality]             NVARCHAR(10) NULL,
    [Passport]                NVARCHAR(50) NULL,
    [Address]                 NVARCHAR(500) NULL,
    [IDCareer]                INT NULL,
    [WorkPermit]              INT NOT NULL DEFAULT 0,
    [WorkPermitNumber]        NVARCHAR(100) NULL,
    [VisaNumber]              NVARCHAR(100) NULL,
    [TemporaryStay]           DATETIME NULL,
    [Note]                    NVARCHAR(MAX) NULL,
    [SettlementResults]       INT NOT NULL DEFAULT 0,
    [SettlementResultsString] NVARCHAR(200) NULL,
    [IDUser]                  INT NOT NULL DEFAULT 1,
    [IDCompany]               INT NOT NULL,
    [DateCreated]             DATETIME NULL DEFAULT GETDATE(),
    [CardCreationDate]        NVARCHAR(50) NULL,
    [WorkingStatus]           INT NOT NULL DEFAULT 0,
    [DateOfJoin]              DATETIME NULL,
    [DateOfLeave]             DATETIME NULL,
    [Hidden_flag]             INT NOT NULL DEFAULT 0,
    FOREIGN KEY ([IDCompany]) REFERENCES [Companies]([IDCompany]),
    FOREIGN KEY ([IDCareer]) REFERENCES [Careers]([IDCareer])
);
GO

-- ── Districts ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Districts')
CREATE TABLE [dbo].[Districts] (
    [IDDistrict]    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [DisTrictName]  NVARCHAR(200) NOT NULL,
    [Delete_flag]   INT NOT NULL DEFAULT 0
);
GO

-- ── Wards ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wards')
CREATE TABLE [dbo].[Wards] (
    [IDWard]      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [WardName]    NVARCHAR(200) NOT NULL,
    [Delete_flag] INT NOT NULL DEFAULT 0
);
GO

-- ── PhoneNumbers ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PhoneNumbers')
CREATE TABLE [dbo].[PhoneNumbers] (
    [IDPhoneNumber] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]          NVARCHAR(200) NULL,
    [Phone]         NVARCHAR(50) NULL,
    [IDCompany]     INT NOT NULL,
    [Delete_flag]   INT NOT NULL DEFAULT 0,
    FOREIGN KEY ([IDCompany]) REFERENCES [Companies]([IDCompany])
);
GO

-- ── Emails ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Emails')
CREATE TABLE [dbo].[Emails] (
    [IDEmail]     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]        NVARCHAR(200) NULL,
    [Mail]        NVARCHAR(200) NULL,
    [IDCompany]   INT NOT NULL,
    [Delete_flag] INT NOT NULL DEFAULT 0,
    FOREIGN KEY ([IDCompany]) REFERENCES [Companies]([IDCompany])
);
GO

-- ── Investment ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Investment')
CREATE TABLE [dbo].[Investment] (
    [IDInvestment]  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]          NVARCHAR(200) NULL,
    [Nationality]   NVARCHAR(200) NULL,
    [AmountOfMoney] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [IDCompany]     INT NOT NULL,
    [Passport]      NVARCHAR(50) NULL,
    FOREIGN KEY ([IDCompany]) REFERENCES [Companies]([IDCompany])
);
GO

-- ── Attach ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attach')
CREATE TABLE [dbo].[Attach] (
    [IDAttach]     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [IDCompany]    INT NOT NULL,
    [Type]         INT NOT NULL DEFAULT 0,
    [Name]         NVARCHAR(500) NULL,
    [Folder]       NVARCHAR(500) NULL,
    [DateCreated]  DATETIME NULL,
    [DateModified] DATETIME NULL,
    [Delete_flag]  INT NOT NULL DEFAULT 0,
    FOREIGN KEY ([IDCompany]) REFERENCES [Companies]([IDCompany])
);
GO

-- ── AuditLogs ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
CREATE TABLE [dbo].[AuditLogs] (
    [Id]          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Action]      NVARCHAR(50)   NOT NULL,
    [EntityType]  NVARCHAR(100)  NOT NULL,
    [EntityId]    INT            NULL,
    [Description] NVARCHAR(MAX)  NULL,
    [Username]    NVARCHAR(100)  NULL,
    [Timestamp]   DATETIME       NOT NULL DEFAULT GETDATE(),
    [IpAddress]   NVARCHAR(50)   NULL
);
GO

-- ── ImportHistories (mở rộng) ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportHistories')
CREATE TABLE [dbo].[ImportHistories] (
    [Id]           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SessionId]    NVARCHAR(50)   NOT NULL,
    [FileName]     NVARCHAR(500)  NOT NULL,
    [CompanyId]    INT            NOT NULL DEFAULT 0,
    [CompanyName]  NVARCHAR(500)  NULL,
    [TotalRows]    INT            NOT NULL DEFAULT 0,
    [AddedRows]    INT            NOT NULL DEFAULT 0,
    [UpdatedRows]  INT            NOT NULL DEFAULT 0,
    [ErrorRows]    INT            NOT NULL DEFAULT 0,
    [Status]       NVARCHAR(20)   NOT NULL DEFAULT 'committed',
    [Username]     NVARCHAR(100)  NULL,
    [ImportDate]   DATETIME       NOT NULL DEFAULT GETDATE(),
    [ErrorDetails] NVARCHAR(MAX)  NULL
);
GO

-- ── ImportBackups ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportBackups')
CREATE TABLE [dbo].[ImportBackups] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ImportSessionId] NVARCHAR(50)   NOT NULL,
    [ActionType]      NVARCHAR(20)   NOT NULL,
    [EmployeeId]      INT            NOT NULL,
    [OldData]         NVARCHAR(MAX)  NULL,
    [CreatedAt]       DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

-- ── ColumnMappingTemplates ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ColumnMappingTemplates')
CREATE TABLE [dbo].[ColumnMappingTemplates] (
    [Id]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TemplateName] NVARCHAR(200)  NOT NULL,
    [CompanyId]    INT            NULL,
    [MappingJson]  NVARCHAR(MAX)  NOT NULL,
    [CreatedBy]    NVARCHAR(100)  NULL,
    [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE(),
    [UpdatedAt]    DATETIME       NULL
);
GO

-- ══════════════════════════════════════════════
-- PHẦN 2: SEED DATA MẪU
-- ══════════════════════════════════════════════

-- ── Accounts ──
IF NOT EXISTS (SELECT * FROM Accounts WHERE Username = 'admin')
BEGIN
    SET IDENTITY_INSERT [Accounts] ON;
    INSERT INTO [Accounts] (IDUser, Username, Name, Password, Permission, Delete_flag) VALUES
        (1, 'admin', N'Quản trị viên', '123456', 1, 0),
        (2, 'nguyenvana', N'Nguyễn Văn A', '123456', 0, 0),
        (3, 'tranthib', N'Trần Thị B', '123456', 0, 0);
    SET IDENTITY_INSERT [Accounts] OFF;
    PRINT N'[OK] Seed Accounts';
END
GO

-- ── Fields ──
IF NOT EXISTS (SELECT * FROM Fields WHERE IDField = 1)
BEGIN
    SET IDENTITY_INSERT [Fields] ON;
    INSERT INTO [Fields] (IDField, FieldName, Description, Delete_flag) VALUES
        (1, N'Sản xuất công nghiệp', N'Nhà máy, xưởng sản xuất', 0),
        (2, N'Công nghệ thông tin', N'Phần mềm, phần cứng, dịch vụ IT', 0),
        (3, N'Xây dựng', N'Thi công, xây dựng dân dụng', 0),
        (4, N'Thương mại - Dịch vụ', N'Bán hàng, nhà hàng, khách sạn', 0),
        (5, N'Giáo dục - Đào tạo', N'Trường học, trung tâm ngoại ngữ', 0);
    SET IDENTITY_INSERT [Fields] OFF;
    PRINT N'[OK] Seed Fields';
END
GO

-- ── CareerGroups ──
IF NOT EXISTS (SELECT * FROM CareerGroups WHERE IDCG = 1)
BEGIN
    SET IDENTITY_INSERT [CareerGroups] ON;
    INSERT INTO [CareerGroups] (IDCG, CareerGroupName) VALUES
        (1, N'Quản lý'),
        (2, N'Kỹ thuật'),
        (3, N'Chuyên gia'),
        (4, N'Lao động phổ thông');
    SET IDENTITY_INSERT [CareerGroups] OFF;
    PRINT N'[OK] Seed CareerGroups';
END
GO

-- ── Careers ──
IF NOT EXISTS (SELECT * FROM Careers WHERE IDCareer = 1)
BEGIN
    SET IDENTITY_INSERT [Careers] ON;
    INSERT INTO [Careers] (IDCareer, CareerName, IDCG, Delete_flag) VALUES
        (1,  N'Giám đốc', 1, 0),
        (2,  N'Phó Giám đốc', 1, 0),
        (3,  N'Quản lý', 1, 0),
        (4,  N'Kỹ sư', 2, 0),
        (5,  N'Kỹ thuật viên', 2, 0),
        (6,  N'Chuyên gia', 3, 0),
        (7,  N'Phiên dịch', 3, 0),
        (8,  N'Lao động', 4, 0),
        (9,  N'Trưởng phòng', 1, 0),
        (10, N'Tư vấn', 3, 0);
    SET IDENTITY_INSERT [Careers] OFF;
    PRINT N'[OK] Seed Careers';
END
GO

-- ── Nationality ──
IF NOT EXISTS (SELECT * FROM Nationality WHERE IDNationality = 1)
BEGIN
    SET IDENTITY_INSERT [Nationality] ON;
    INSERT INTO [Nationality] (IDNationality, NationalityCode, NationalityName, Delete_flag) VALUES
        (1,  'CN', N'Trung Quốc', 0),
        (2,  'KR', N'Hàn Quốc', 0),
        (3,  'JP', N'Nhật Bản', 0),
        (4,  'TW', N'Đài Loan', 0),
        (5,  'US', N'Hoa Kỳ', 0),
        (6,  'GB', N'Anh Quốc', 0),
        (7,  'FR', N'Pháp', 0),
        (8,  'DE', N'Đức', 0),
        (9,  'TH', N'Thái Lan', 0),
        (10, 'MY', N'Malaysia', 0),
        (11, 'SG', N'Singapore', 0),
        (12, 'IN', N'Ấn Độ', 0),
        (13, 'PH', N'Philippines', 0),
        (14, 'ID', N'Indonesia', 0),
        (15, 'AU', N'Úc', 0);
    SET IDENTITY_INSERT [Nationality] OFF;
    PRINT N'[OK] Seed Nationality';
END
GO

-- ── Companies ──
IF NOT EXISTS (SELECT * FROM Companies WHERE IDCompany = 1)
BEGIN
    SET IDENTITY_INSERT [Companies] ON;
    INSERT INTO [Companies] (IDCompany, CompanyName, TypeOfBusiniess, IDField, LegalRepresentative, Address, TotalAmount, TrackerID, Delete_flag) VALUES
        (1, N'Công ty TNHH Samsung Electronics Việt Nam', N'100% vốn nước ngoài', 1, N'Choi Joo Ho', N'KCN Yên Phong, Bắc Ninh', 50, 1, 0),
        (2, N'Công ty CP Hyundai Aluminum Vina', N'Liên doanh', 1, N'Park Sung Jin', N'KCN Đại An, Hải Dương', 30, 1, 0),
        (3, N'Công ty TNHH Canon Việt Nam', N'100% vốn nước ngoài', 1, N'Tanaka Hiroshi', N'KCN Quế Võ, Bắc Ninh', 25, 2, 0),
        (4, N'Công ty TNHH LG Display Việt Nam', N'100% vốn nước ngoài', 1, N'Lee Dong Hoon', N'KCN Tràng Duệ, Hải Phòng', 40, 1, 0),
        (5, N'Công ty TNHH Foxconn Bình Dương', N'100% vốn nước ngoài', 1, N'Chen Wei', N'KCN VSIP II, Bình Dương', 35, 3, 0);
    SET IDENTITY_INSERT [Companies] OFF;
    PRINT N'[OK] Seed Companies';
END
GO

-- ── Districts ──
IF NOT EXISTS (SELECT * FROM Districts WHERE IDDistrict = 1)
BEGIN
    SET IDENTITY_INSERT [Districts] ON;
    INSERT INTO [Districts] (IDDistrict, DisTrictName, Delete_flag) VALUES
        (1, N'Thành phố Bắc Ninh', 0),
        (2, N'Huyện Yên Phong', 0),
        (3, N'Huyện Quế Võ', 0);
    SET IDENTITY_INSERT [Districts] OFF;
    PRINT N'[OK] Seed Districts';
END
GO

-- ── Wards ──
IF NOT EXISTS (SELECT * FROM Wards WHERE IDWard = 1)
BEGIN
    SET IDENTITY_INSERT [Wards] ON;
    INSERT INTO [Wards] (IDWard, WardName, Delete_flag) VALUES
        (1, N'Phường Vũ Ninh', 0),
        (2, N'Phường Suối Hoa', 0);
    SET IDENTITY_INSERT [Wards] OFF;
    PRINT N'[OK] Seed Wards';
END
GO

-- ── Employees mẫu (để test trùng lặp khi import) ──
IF NOT EXISTS (SELECT * FROM Employees WHERE Passport = 'E12345678')
BEGIN
    INSERT INTO [Employees] (StaffName, Gender, Birthday, Nationality, Passport, Address, IDCareer, WorkPermit, TemporaryStay, IDUser, IDCompany, WorkingStatus, Hidden_flag) VALUES
        (N'Zhang Wei', 1, '1985-03-15', 'CN', 'E12345678', N'Bắc Kinh, Trung Quốc', 4, 1, '2027-06-30', 1, 1, 0, 0),
        (N'Li Ming', 1, '1990-07-22', 'CN', 'E87654321', N'Thượng Hải, Trung Quốc', 5, 1, '2027-03-15', 1, 1, 0, 0),
        (N'Park Ji-sung', 1, '1988-11-05', 'KR', 'M11223344', N'Seoul, Hàn Quốc', 3, 1, '2026-12-31', 1, 2, 0, 0);
    PRINT N'[OK] Seed Employees mẫu (3 records)';
END
GO

-- ══════════════════════════════════════════════
-- PHẦN 3: INDEX
-- ══════════════════════════════════════════════

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_Passport')
    CREATE NONCLUSTERED INDEX [IX_Employees_Passport]
    ON [dbo].[Employees] ([Passport]) WHERE [Hidden_flag] = 0;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_IDCompany')
    CREATE NONCLUSTERED INDEX [IX_Employees_IDCompany]
    ON [dbo].[Employees] ([IDCompany])
    INCLUDE ([StaffName], [Passport], [Nationality], [WorkPermit], [TemporaryStay])
    WHERE [Hidden_flag] = 0;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ImportBackups_SessionId')
    CREATE NONCLUSTERED INDEX [IX_ImportBackups_SessionId]
    ON [dbo].[ImportBackups] ([ImportSessionId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ImportHistories_SessionId')
    CREATE NONCLUSTERED INDEX [IX_ImportHistories_SessionId]
    ON [dbo].[ImportHistories] ([SessionId]);
GO

PRINT N'';
PRINT N'=== Database ReportManagerDB đã sẵn sàng ===';
GO
