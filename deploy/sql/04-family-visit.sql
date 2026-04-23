-- =============================================================
-- IRM v2.0 — Script 04: Thêm trường Thăm thân (Family Visit)
-- Tác giả: IRM Migration Tool
-- Ngày tạo: 2026-04-22
-- Mô tả: Thêm 7 cột FamilyVisit* vào bảng Employees
--         để quản lý thông tin thăm thân của lao động nước ngoài.
-- Tương thích: SQL Server 2014+
-- =============================================================
USE [ReportManagerDB];
GO

PRINT N'=== Script 04: Thêm trường Thăm thân (Family Visit) ===';
GO

-- ── Thêm cột vào bảng Employees ──
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Employees') AND name = 'FamilyVisit')
BEGIN
    ALTER TABLE [dbo].[Employees] ADD
        [FamilyVisit]               INT           NOT NULL DEFAULT 0,
        [FamilyVisitRelativeName]   NVARCHAR(200) NULL,
        [FamilyVisitRelationship]   NVARCHAR(100) NULL,
        [FamilyVisitRelativeIdCard] NVARCHAR(50)  NULL,
        [FamilyVisitStartDate]      DATETIME      NULL,
        [FamilyVisitEndDate]        DATETIME      NULL,
        [FamilyVisitNote]           NVARCHAR(500) NULL;
    PRINT N'  [OK] Đã thêm 7 cột FamilyVisit vào bảng Employees';
END
ELSE
BEGIN
    PRINT N'  [SKIP] Các cột FamilyVisit đã tồn tại — bỏ qua';
END
GO

-- ── Index cho truy vấn báo cáo thăm thân ──
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_FamilyVisit')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Employees_FamilyVisit]
    ON [dbo].[Employees] ([FamilyVisit])
    WHERE [FamilyVisit] = 1 AND [Hidden_flag] = 0;
    PRINT N'  [OK] Đã tạo index IX_Employees_FamilyVisit';
END
GO

-- ── Index cho cảnh báo hết hạn thăm thân ──
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_FamilyVisitEndDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Employees_FamilyVisitEndDate]
    ON [dbo].[Employees] ([FamilyVisitEndDate])
    WHERE [FamilyVisit] = 1 AND [Hidden_flag] = 0 AND [FamilyVisitEndDate] IS NOT NULL;
    PRINT N'  [OK] Đã tạo index IX_Employees_FamilyVisitEndDate';
END
GO

PRINT N'=== Script 04 hoàn tất ===';
GO
