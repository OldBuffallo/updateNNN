/* Non-destructive rollback switch. New tables and captured events remain intact. */
IF OBJECT_ID(N'dbo.TR_IRM_Employees_Change',N'TR') IS NOT NULL DISABLE TRIGGER dbo.TR_IRM_Employees_Change ON dbo.Employees;
IF OBJECT_ID(N'dbo.TR_IRM_Companies_Change',N'TR') IS NOT NULL DISABLE TRIGGER dbo.TR_IRM_Companies_Change ON dbo.Companies;
IF OBJECT_ID(N'dbo.TR_IRM_Wards_Change',N'TR') IS NOT NULL DISABLE TRIGGER dbo.TR_IRM_Wards_Change ON dbo.Wards;
IF OBJECT_ID(N'dbo.TR_IRM_Districts_Change',N'TR') IS NOT NULL DISABLE TRIGGER dbo.TR_IRM_Districts_Change ON dbo.Districts;
IF OBJECT_ID(N'dbo.TR_IRM_Attach_Change',N'TR') IS NOT NULL DISABLE TRIGGER dbo.TR_IRM_Attach_Change ON dbo.Attach;
