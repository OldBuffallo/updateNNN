-- =============================================================
-- IRM v2.0 — Script 03: Bảng hỗ trợ Import Excel
-- Mở rộng ImportHistories, thêm ImportBackups, ColumnMappingTemplates
-- =============================================================

USE [ReportManagerDB];
GO

-- 1. ImportHistories: create if missing, preserve existing rows if table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportHistories')
BEGIN
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
    PRINT N'[OK] Created ImportHistories';
END
ELSE
BEGIN
    PRINT N'[SKIP] ImportHistories exists - preserving data';
END
GO

IF COL_LENGTH('dbo.ImportHistories', 'SessionId') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [SessionId] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ImportHistories_SessionId] DEFAULT('');
IF COL_LENGTH('dbo.ImportHistories', 'FileName') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [FileName] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ImportHistories_FileName] DEFAULT('');
IF COL_LENGTH('dbo.ImportHistories', 'CompanyId') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [CompanyId] INT NOT NULL CONSTRAINT [DF_ImportHistories_CompanyId] DEFAULT(0);
IF COL_LENGTH('dbo.ImportHistories', 'CompanyName') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [CompanyName] NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ImportHistories', 'TotalRows') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [TotalRows] INT NOT NULL CONSTRAINT [DF_ImportHistories_TotalRows] DEFAULT(0);
IF COL_LENGTH('dbo.ImportHistories', 'AddedRows') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [AddedRows] INT NOT NULL CONSTRAINT [DF_ImportHistories_AddedRows] DEFAULT(0);
IF COL_LENGTH('dbo.ImportHistories', 'UpdatedRows') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [UpdatedRows] INT NOT NULL CONSTRAINT [DF_ImportHistories_UpdatedRows] DEFAULT(0);
IF COL_LENGTH('dbo.ImportHistories', 'ErrorRows') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [ErrorRows] INT NOT NULL CONSTRAINT [DF_ImportHistories_ErrorRows] DEFAULT(0);
IF COL_LENGTH('dbo.ImportHistories', 'Status') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_ImportHistories_Status] DEFAULT('committed');
IF COL_LENGTH('dbo.ImportHistories', 'Username') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [Username] NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.ImportHistories', 'ImportDate') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [ImportDate] DATETIME NOT NULL CONSTRAINT [DF_ImportHistories_ImportDate] DEFAULT(GETDATE());
IF COL_LENGTH('dbo.ImportHistories', 'ErrorDetails') IS NULL
    ALTER TABLE [dbo].[ImportHistories] ADD [ErrorDetails] NVARCHAR(MAX) NULL;
PRINT N'[OK] ImportHistories schema ready';
GO
-- ── 2. Bảng ImportBackups — lưu dữ liệu cũ để rollback ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportBackups')
BEGIN
    CREATE TABLE [dbo].[ImportBackups] (
        [Id]              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ImportSessionId] NVARCHAR(50)   NOT NULL,
        [ActionType]      NVARCHAR(20)   NOT NULL,  -- 'INSERT' hoặc 'UPDATE'
        [EmployeeId]      INT            NOT NULL,
        [OldData]         NVARCHAR(MAX)  NULL,       -- JSON snapshot dữ liệu cũ (cho UPDATE)
        [CreatedAt]       DATETIME       NOT NULL DEFAULT GETDATE()
    );
    PRINT N'[OK] Đã tạo bảng ImportBackups';
END
GO

-- ── 3. Bảng ColumnMappingTemplates — template ghép cột ──
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ColumnMappingTemplates')
BEGIN
    CREATE TABLE [dbo].[ColumnMappingTemplates] (
        [Id]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TemplateName] NVARCHAR(200)  NOT NULL,
        [CompanyId]    INT            NULL,
        [MappingJson]  NVARCHAR(MAX)  NOT NULL,   -- JSON: {"0":"StaffName","1":"Gender",...}
        [CreatedBy]    NVARCHAR(100)  NULL,
        [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]    DATETIME       NULL
    );
    PRINT N'[OK] Đã tạo bảng ColumnMappingTemplates';
END
GO

-- ── 4. Index cho bảng mới ──
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ImportBackups_SessionId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ImportBackups_SessionId]
    ON [dbo].[ImportBackups] ([ImportSessionId]);
    PRINT N'[OK] Index IX_ImportBackups_SessionId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ImportHistories_SessionId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ImportHistories_SessionId]
    ON [dbo].[ImportHistories] ([SessionId]);
    PRINT N'[OK] Index IX_ImportHistories_SessionId';
END
GO

PRINT N'';
PRINT N'=== Script 03 hoàn tất ===';
GO
