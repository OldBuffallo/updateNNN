-- =============================================
-- 06-add-registration-profile-index.sql
-- Thêm cột RegistrationProfileIndex vào bảng Companies
-- An toàn: chỉ thêm nếu chưa có
-- =============================================

USE [ReportManagerDB]
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Companies')
    AND name = 'RegistrationProfileIndex'
)
BEGIN
    ALTER TABLE [dbo].[Companies]
        ADD [RegistrationProfileIndex] INT NOT NULL DEFAULT 0;
    PRINT N'✅ Đã thêm cột RegistrationProfileIndex vào bảng Companies';
END
ELSE
BEGIN
    PRINT N'ℹ️ Cột RegistrationProfileIndex đã tồn tại — bỏ qua';
END
GO
