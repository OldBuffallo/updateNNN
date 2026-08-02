-- =============================================================
-- IRM v2.0 — Script 05: Tạo bảng Students (Du học sinh)
-- Bảng hoàn toàn mới, KHÔNG ảnh hưởng dữ liệu cũ
-- Tương thích: SQL Server 2014+
-- =============================================================
USE [ReportManagerDB];
GO

PRINT N'=== Script 05: Tạo bảng Students (Du học sinh) ===';
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE [dbo].[Students] (
        [IDStudent]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FullName]            NVARCHAR(200)  NOT NULL,
        [Gender]              INT            NOT NULL DEFAULT 1,
        [Birthday]            DATETIME       NULL,
        [Nationality]         NVARCHAR(10)   NULL,
        [Passport]            NVARCHAR(50)   NULL,
        [Address]             NVARCHAR(500)  NULL,

        -- Thông tin học tập
        [SchoolName]          NVARCHAR(500)  NULL,
        [Major]               NVARCHAR(200)  NULL,
        [StudentCode]         NVARCHAR(50)   NULL,
        [EducationLevel]      INT            NOT NULL DEFAULT 0,
        [EnrollmentDate]      DATETIME       NULL,
        [ExpectedGraduation]  DATETIME       NULL,

        -- Visa & tạm trú
        [VisaNumber]          NVARCHAR(100)  NULL,
        [VisaExpiry]          DATETIME       NULL,
        [TemporaryStay]       DATETIME       NULL,

        -- Học bổng
        [ScholarshipType]     INT            NOT NULL DEFAULT 0,

        -- Trạng thái & quản trị
        [Status]              INT            NOT NULL DEFAULT 0,
        [Note]                NVARCHAR(MAX)  NULL,
        [IDUser]              INT            NOT NULL DEFAULT 1,
        [DateCreated]         DATETIME       NULL DEFAULT GETDATE(),
        [Hidden_flag]         INT            NOT NULL DEFAULT 0
    );
    PRINT N'  [OK] Đã tạo bảng Students';
END
ELSE
BEGIN
    PRINT N'  [SKIP] Bảng Students đã tồn tại — bỏ qua';
END
GO

-- Index cho tìm kiếm
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_Passport')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Students_Passport]
    ON [dbo].[Students] ([Passport])
    WHERE [Hidden_flag] = 0;
    PRINT N'  [OK] Index IX_Students_Passport';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_Nationality')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Students_Nationality]
    ON [dbo].[Students] ([Nationality])
    WHERE [Hidden_flag] = 0;
    PRINT N'  [OK] Index IX_Students_Nationality';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_VisaExpiry')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Students_VisaExpiry]
    ON [dbo].[Students] ([VisaExpiry])
    WHERE [Hidden_flag] = 0 AND [VisaExpiry] IS NOT NULL;
    PRINT N'  [OK] Index IX_Students_VisaExpiry';
END
GO

PRINT N'=== Script 05 hoàn tất ===';
GO
