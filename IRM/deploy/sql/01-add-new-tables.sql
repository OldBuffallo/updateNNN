-- =============================================================
-- IRM v2.0 — Script 01: Thêm bảng mới
-- Chạy trên database ReportManagerDB đã restore
-- =============================================================

USE [ReportManagerDB];
GO

-- Bảng AuditLogs — Nhật ký hoạt động
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
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
    PRINT N'[OK] Đã tạo bảng AuditLogs';
END
ELSE
    PRINT N'[--] Bảng AuditLogs đã tồn tại — bỏ qua';
GO

-- Bảng ImportHistories — Lịch sử import Excel
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ImportHistories')
BEGIN
    CREATE TABLE [dbo].[ImportHistories] (
        [Id]           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FileName]     NVARCHAR(500)  NOT NULL,
        [TotalRows]    INT            NOT NULL DEFAULT 0,
        [SuccessRows]  INT            NOT NULL DEFAULT 0,
        [ErrorRows]    INT            NOT NULL DEFAULT 0,
        [CompanyId]    INT            NOT NULL DEFAULT 0,
        [Username]     NVARCHAR(100)  NULL,
        [ImportDate]   DATETIME       NOT NULL DEFAULT GETDATE(),
        [ErrorDetails] NVARCHAR(MAX)  NULL
    );
    PRINT N'[OK] Đã tạo bảng ImportHistories';
END
ELSE
    PRINT N'[--] Bảng ImportHistories đã tồn tại — bỏ qua';
GO

PRINT N'';
PRINT N'=== Script 01 hoàn tất ===';
GO
