-- =============================================================
-- IRM v2.0 — Script 02: Thêm index tối ưu truy vấn
-- Chạy trên database ReportManagerDB đã restore
-- =============================================================

USE [ReportManagerDB];
GO

-- Index cho tìm kiếm nhân viên theo hộ chiếu
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_Passport')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Employees_Passport]
    ON [dbo].[Employees] ([Passport])
    WHERE [Hidden_flag] = 0;
    PRINT N'[OK] Index IX_Employees_Passport';
END
GO

-- Index cho tìm kiếm nhân viên theo công ty
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_IDCompany')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Employees_IDCompany]
    ON [dbo].[Employees] ([IDCompany])
    INCLUDE ([StaffName], [Passport], [Nationality], [WorkPermit], [TemporaryStay])
    WHERE [Hidden_flag] = 0;
    PRINT N'[OK] Index IX_Employees_IDCompany';
END
GO

-- Index cho tìm kiếm nhân viên sắp hết hạn
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_TemporaryStay')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Employees_TemporaryStay]
    ON [dbo].[Employees] ([TemporaryStay])
    WHERE [Hidden_flag] = 0 AND [WorkingStatus] = 0;
    PRINT N'[OK] Index IX_Employees_TemporaryStay';
END
GO

-- Index cho tìm kiếm công ty theo tên
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Companies_CompanyName')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Companies_CompanyName]
    ON [dbo].[Companies] ([CompanyName])
    WHERE [Delete_flag] = 0;
    PRINT N'[OK] Index IX_Companies_CompanyName';
END
GO

-- Index cho AuditLogs
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Timestamp')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Timestamp]
    ON [dbo].[AuditLogs] ([Timestamp] DESC);
    PRINT N'[OK] Index IX_AuditLogs_Timestamp';
END
GO

PRINT N'';
PRINT N'=== Script 02 hoàn tất ===';
GO
