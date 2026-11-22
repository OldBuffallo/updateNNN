/*
    IRM installed database verification (SQL Server 2022+)

    Run with an administrative connection, for example:
      sqlcmd -S .\IRMEXPRESS -E -d IRM -b -i verify-installed-database.sql

    The script is read-only. It exits with an error when corruption, schema drift,
    disabled/untrusted constraints, or orphaned foreign keys are detected.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'IRM'
    THROW 51000, N'Run this script against the IRM database.', 1;

PRINT N'[1/7] Database consistency';
DBCC CHECKDB (N'IRM') WITH NO_INFOMSGS, ALL_ERRORMSGS;

PRINT N'[2/7] Required schema';
DECLARE @RequiredTables table (TableName sysname NOT NULL PRIMARY KEY);
INSERT @RequiredTables (TableName) VALUES
    (N'Accounts'), (N'Companies'), (N'Employees'), (N'Fields'), (N'Careers'),
    (N'Nationality'), (N'ImportHistories'), (N'ImportBackups'),
    (N'ForeignPersons'), (N'StayCases'), (N'FamilyVisitDetails'),
    (N'CompanyProfiles'), (N'CompanySites'), (N'SiteZoneMemberships'),
    (N'WebCredentials'), (N'WebRoleAssignments'), (N'SchemaVersions'),
    (N'__EFMigrationsHistory');

IF EXISTS (
    SELECT 1
    FROM @RequiredTables r
    WHERE OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(r.TableName), N'U') IS NULL
)
BEGIN
    SELECT r.TableName AS MissingTable
    FROM @RequiredTables r
    WHERE OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(r.TableName), N'U') IS NULL;
    THROW 51001, N'IRM schema is incomplete: one or more required tables are missing.', 1;
END;

IF COL_LENGTH(N'dbo.Employees', N'VisaSymbol') IS NULL
    THROW 51002, N'IRM schema is outdated: Employees.VisaSymbol is missing.', 1;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20260927120137_AddVisaSymbol'
)
    THROW 51003, N'The latest required EF Core migration has not been applied.', 1;

PRINT N'[3/7] Constraints and indexes';
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE is_disabled = 1 OR is_not_trusted = 1
)
BEGIN
    SELECT OBJECT_SCHEMA_NAME(parent_object_id) AS SchemaName,
           OBJECT_NAME(parent_object_id) AS TableName,
           name AS ConstraintName, is_disabled, is_not_trusted
    FROM sys.foreign_keys
    WHERE is_disabled = 1 OR is_not_trusted = 1;
    THROW 51004, N'Disabled or untrusted foreign keys were found.', 1;
END;

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE is_disabled = 1 OR is_not_trusted = 1
)
BEGIN
    SELECT OBJECT_SCHEMA_NAME(parent_object_id) AS SchemaName,
           OBJECT_NAME(parent_object_id) AS TableName,
           name AS ConstraintName, is_disabled, is_not_trusted
    FROM sys.check_constraints
    WHERE is_disabled = 1 OR is_not_trusted = 1;
    THROW 51005, N'Disabled or untrusted check constraints were found.', 1;
END;

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id IN (SELECT object_id FROM sys.tables)
      AND is_disabled = 1
)
BEGIN
    SELECT OBJECT_SCHEMA_NAME(object_id) AS SchemaName,
           OBJECT_NAME(object_id) AS TableName, name AS IndexName
    FROM sys.indexes
    WHERE object_id IN (SELECT object_id FROM sys.tables)
      AND is_disabled = 1;
    THROW 51006, N'Disabled indexes were found.', 1;
END;

PRINT N'[4/7] Foreign-key orphan scan';
CREATE TABLE #Orphans
(
    ConstraintName sysname NOT NULL,
    ChildTable nvarchar(517) NOT NULL,
    ParentTable nvarchar(517) NOT NULL,
    OrphanCount bigint NOT NULL
);

DECLARE @ForeignKeyId int;
DECLARE @ConstraintName sysname;
DECLARE @ChildTable nvarchar(517);
DECLARE @ParentTable nvarchar(517);
DECLARE @Join nvarchar(max);
DECLARE @NotNull nvarchar(max);
DECLARE @Sql nvarchar(max);

DECLARE foreign_keys_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT fk.object_id, fk.name,
       QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)),
       QUOTENAME(OBJECT_SCHEMA_NAME(fk.referenced_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.referenced_object_id))
FROM sys.foreign_keys fk
WHERE fk.is_disabled = 0;

OPEN foreign_keys_cursor;
FETCH NEXT FROM foreign_keys_cursor INTO @ForeignKeyId, @ConstraintName, @ChildTable, @ParentTable;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @Join = STRING_AGG(
               N'c.' + QUOTENAME(COL_NAME(parent_object_id, parent_column_id)) +
               N' = p.' + QUOTENAME(COL_NAME(referenced_object_id, referenced_column_id)), N' AND ')
               WITHIN GROUP (ORDER BY constraint_column_id),
           @NotNull = STRING_AGG(
               N'c.' + QUOTENAME(COL_NAME(parent_object_id, parent_column_id)) + N' IS NOT NULL', N' AND ')
               WITHIN GROUP (ORDER BY constraint_column_id)
    FROM sys.foreign_key_columns
    WHERE constraint_object_id = @ForeignKeyId;

    SET @Sql = N'INSERT #Orphans (ConstraintName, ChildTable, ParentTable, OrphanCount) '
        + N'SELECT @Name, @Child, @Parent, COUNT_BIG(*) FROM ' + @ChildTable + N' c '
        + N'LEFT JOIN ' + @ParentTable + N' p ON ' + @Join + N' '
        + N'WHERE ' + @NotNull + N' AND p.'
        + QUOTENAME((SELECT TOP (1) COL_NAME(referenced_object_id, referenced_column_id)
                     FROM sys.foreign_key_columns
                     WHERE constraint_object_id = @ForeignKeyId
                     ORDER BY constraint_column_id)) + N' IS NULL;';

    EXEC sys.sp_executesql @Sql,
        N'@Name sysname, @Child nvarchar(517), @Parent nvarchar(517)',
        @Name = @ConstraintName, @Child = @ChildTable, @Parent = @ParentTable;

    FETCH NEXT FROM foreign_keys_cursor INTO @ForeignKeyId, @ConstraintName, @ChildTable, @ParentTable;
END;
CLOSE foreign_keys_cursor;
DEALLOCATE foreign_keys_cursor;

IF EXISTS (SELECT 1 FROM #Orphans WHERE OrphanCount > 0)
BEGIN
    SELECT * FROM #Orphans WHERE OrphanCount > 0 ORDER BY ChildTable, ConstraintName;
    THROW 51007, N'Orphaned rows were found.', 1;
END;

PRINT N'[5/7] Required seed data';
IF NOT EXISTS (SELECT 1 FROM dbo.Accounts)
    THROW 51008, N'No administrator account exists.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Fields)
    THROW 51009, N'Fields seed data is missing.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Careers)
    THROW 51010, N'Careers seed data is missing.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Nationality)
    THROW 51011, N'Nationality seed data is missing.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.AdministrativeUnits)
    THROW 51012, N'AdministrativeUnits seed data is missing.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE ISNULL(Delete_flag, 0) = 0)
    THROW 51014, N'No active initial company exists. Bootstrap is incomplete.', 1;

PRINT N'[6/7] Web administrator linkage';
IF EXISTS (
    SELECT 1
    FROM dbo.Accounts a
    LEFT JOIN dbo.WebCredentials wc ON wc.AccountId = a.IDUser
    LEFT JOIN dbo.WebRoleAssignments wr ON wr.AccountId = a.IDUser
    WHERE ISNULL(a.Delete_flag, 0) = 0
    GROUP BY a.IDUser
    HAVING COUNT(DISTINCT wc.AccountId) = 0 OR COUNT(DISTINCT wr.Id) = 0
)
BEGIN
    SELECT a.IDUser, a.UserName,
           COUNT(DISTINCT wc.AccountId) AS CredentialCount,
           COUNT(DISTINCT wr.Id) AS RoleCount
    FROM dbo.Accounts a
    LEFT JOIN dbo.WebCredentials wc ON wc.AccountId = a.IDUser
    LEFT JOIN dbo.WebRoleAssignments wr ON wr.AccountId = a.IDUser
    WHERE ISNULL(a.Delete_flag, 0) = 0
    GROUP BY a.IDUser, a.UserName
    HAVING COUNT(DISTINCT wc.AccountId) = 0 OR COUNT(DISTINCT wr.Id) = 0;
    THROW 51013, N'An active account has no web credential or role.', 1;
END;

PRINT N'[7/7] Operational summary';
SELECT
    DB_NAME() AS DatabaseName,
    CAST(DATABASEPROPERTYEX(DB_NAME(), N'Status') AS nvarchar(60)) AS DatabaseStatus,
    (SELECT COUNT_BIG(*) FROM dbo.Accounts WHERE ISNULL(Delete_flag, 0) = 0) AS ActiveAccounts,
    (SELECT COUNT_BIG(*) FROM dbo.Companies WHERE ISNULL(Delete_flag, 0) = 0) AS ActiveCompanies,
    (SELECT COUNT_BIG(*) FROM dbo.Employees WHERE ISNULL(Hidden_flag, 0) = 0) AS ActiveEmployees,
    (SELECT COUNT_BIG(*) FROM dbo.ImportHistories) AS ImportHistoryRows,
    (SELECT COUNT_BIG(*) FROM dbo.ImportBackups) AS ImportBackupRows;

PRINT N'IRM_DATABASE_VERIFICATION_OK';
