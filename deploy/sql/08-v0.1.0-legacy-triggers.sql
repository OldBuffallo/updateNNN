/* Install after 07-v0.1.0.sql. These triggers only enqueue snapshots; they do not alter legacy rows. */
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.LegacyChangeEvents', N'U') IS NULL RAISERROR(N'Run 07-v0.1.0.sql first.',16,1);
GO

IF OBJECT_ID(N'dbo.TR_IRM_Employees_Change', N'TR') IS NOT NULL DROP TRIGGER dbo.TR_IRM_Employees_Change;
GO
CREATE TRIGGER dbo.TR_IRM_Employees_Change ON dbo.Employees AFTER INSERT,UPDATE,DELETE AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.LegacyChangeEvents(EntityType,EntityId,Operation,BeforeXml,AfterXml)
  SELECT N'Employees',CONVERT(nvarchar(100),COALESCE(i.IDEmployee,d.IDEmployee)),
    CASE WHEN d.IDEmployee IS NULL THEN N'INSERT' WHEN i.IDEmployee IS NULL THEN N'DELETE' ELSE N'UPDATE' END,
    CASE WHEN d.IDEmployee IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT d.* FOR XML PATH('row'),TYPE)) END,
    CASE WHEN i.IDEmployee IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT i.* FOR XML PATH('row'),TYPE)) END
  FROM inserted i FULL OUTER JOIN deleted d ON d.IDEmployee=i.IDEmployee;
END;
GO

IF OBJECT_ID(N'dbo.TR_IRM_Companies_Change', N'TR') IS NOT NULL DROP TRIGGER dbo.TR_IRM_Companies_Change;
GO
CREATE TRIGGER dbo.TR_IRM_Companies_Change ON dbo.Companies AFTER INSERT,UPDATE,DELETE AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.LegacyChangeEvents(EntityType,EntityId,Operation,BeforeXml,AfterXml)
  SELECT N'Companies',CONVERT(nvarchar(100),COALESCE(i.IDCompany,d.IDCompany)),
    CASE WHEN d.IDCompany IS NULL THEN N'INSERT' WHEN i.IDCompany IS NULL THEN N'DELETE' ELSE N'UPDATE' END,
    CASE WHEN d.IDCompany IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT d.* FOR XML PATH('row'),TYPE)) END,
    CASE WHEN i.IDCompany IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT i.* FOR XML PATH('row'),TYPE)) END
  FROM inserted i FULL OUTER JOIN deleted d ON d.IDCompany=i.IDCompany;
END;
GO

IF OBJECT_ID(N'dbo.TR_IRM_Wards_Change', N'TR') IS NOT NULL DROP TRIGGER dbo.TR_IRM_Wards_Change;
GO
CREATE TRIGGER dbo.TR_IRM_Wards_Change ON dbo.Wards AFTER INSERT,UPDATE,DELETE AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.LegacyChangeEvents(EntityType,EntityId,Operation,BeforeXml,AfterXml)
  SELECT N'Wards',CONVERT(nvarchar(100),COALESCE(i.IDWard,d.IDWard)),
    CASE WHEN d.IDWard IS NULL THEN N'INSERT' WHEN i.IDWard IS NULL THEN N'DELETE' ELSE N'UPDATE' END,
    CASE WHEN d.IDWard IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT d.* FOR XML PATH('row'),TYPE)) END,
    CASE WHEN i.IDWard IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT i.* FOR XML PATH('row'),TYPE)) END
  FROM inserted i FULL OUTER JOIN deleted d ON d.IDWard=i.IDWard;
END;
GO

IF OBJECT_ID(N'dbo.TR_IRM_Districts_Change', N'TR') IS NOT NULL DROP TRIGGER dbo.TR_IRM_Districts_Change;
GO
CREATE TRIGGER dbo.TR_IRM_Districts_Change ON dbo.Districts AFTER INSERT,UPDATE,DELETE AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.LegacyChangeEvents(EntityType,EntityId,Operation,BeforeXml,AfterXml)
  SELECT N'Districts',CONVERT(nvarchar(100),COALESCE(i.IDDistrict,d.IDDistrict)),
    CASE WHEN d.IDDistrict IS NULL THEN N'INSERT' WHEN i.IDDistrict IS NULL THEN N'DELETE' ELSE N'UPDATE' END,
    CASE WHEN d.IDDistrict IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT d.* FOR XML PATH('row'),TYPE)) END,
    CASE WHEN i.IDDistrict IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT i.* FOR XML PATH('row'),TYPE)) END
  FROM inserted i FULL OUTER JOIN deleted d ON d.IDDistrict=i.IDDistrict;
END;
GO

IF OBJECT_ID(N'dbo.TR_IRM_Attach_Change', N'TR') IS NOT NULL DROP TRIGGER dbo.TR_IRM_Attach_Change;
GO
CREATE TRIGGER dbo.TR_IRM_Attach_Change ON dbo.Attach AFTER INSERT,UPDATE,DELETE AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.LegacyChangeEvents(EntityType,EntityId,Operation,BeforeXml,AfterXml)
  SELECT N'Attach',CONVERT(nvarchar(100),COALESCE(i.IDAttach,d.IDAttach)),
    CASE WHEN d.IDAttach IS NULL THEN N'INSERT' WHEN i.IDAttach IS NULL THEN N'DELETE' ELSE N'UPDATE' END,
    CASE WHEN d.IDAttach IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT d.* FOR XML PATH('row'),TYPE)) END,
    CASE WHEN i.IDAttach IS NULL THEN NULL ELSE CONVERT(nvarchar(max),(SELECT i.* FOR XML PATH('row'),TYPE)) END
  FROM inserted i FULL OUTER JOIN deleted d ON d.IDAttach=i.IDAttach;
END;
GO
