/* Idempotent legacy backfill. Ambiguous/missing passports are recorded, never guessed. */
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersions WHERE Version=N'0.1.0') RAISERROR(N'Run 07-v0.1.0.sql first.',16,1);

BEGIN TRY
  BEGIN TRANSACTION;
  CREATE TABLE #Source(
    SourceType nvarchar(50) NOT NULL, SourceId nvarchar(100) NOT NULL, FullName nvarchar(250) NOT NULL,
    Birthday datetime2(0) NULL, NationalityCode nvarchar(10) NULL, PassportNumber nvarchar(100) NULL,
    PassportSearchKey varchar(64) NULL, PurposeCode nvarchar(50) NOT NULL, ValidFrom datetime2(0) NOT NULL,
    Signature nvarchar(500) NOT NULL
  );

  INSERT #Source
  SELECT N'EMPLOYEE',CONVERT(nvarchar(100),e.IDEmployee),COALESCE(NULLIF(LTRIM(RTRIM(e.StaffName)),N''),N'Chưa xác định'),
    TRY_CONVERT(datetime2(0),e.Birthday),e.Nationality,NULLIF(UPPER(REPLACE(REPLACE(LTRIM(RTRIM(e.Passport)),N' ',N''),N'-',N'')),N''),
    CASE WHEN NULLIF(LTRIM(RTRIM(e.Passport)),N'') IS NULL THEN NULL ELSE CONVERT(varchar(64),HASHBYTES('SHA2_256',UPPER(REPLACE(REPLACE(LTRIM(RTRIM(e.Passport)),N' ',N''),N'-',N''))),2) END,
    N'WORK',COALESCE(TRY_CONVERT(datetime2(0),e.DateCreated),CONVERT(datetime2(0),GETDATE())),
    UPPER(COALESCE(e.StaffName,N''))+N'|'+COALESCE(CONVERT(nvarchar(30),TRY_CONVERT(date,e.Birthday),126),N'')+N'|'+COALESCE(e.Nationality,N'')
  FROM dbo.Employees e WHERE ISNULL(e.Hidden_flag,0)=0;

  IF OBJECT_ID(N'dbo.Students',N'U') IS NOT NULL
    EXEC sp_executesql N'
      INSERT #Source
      SELECT N''STUDENT'',CONVERT(nvarchar(100),s.IDStudent),COALESCE(NULLIF(LTRIM(RTRIM(s.FullName)),N''''),N''Chưa xác định''),
        TRY_CONVERT(datetime2(0),s.Birthday),s.Nationality,NULLIF(UPPER(REPLACE(REPLACE(LTRIM(RTRIM(s.Passport)),N'' '',N''''),N''-'',N'''')),N''''),
        CASE WHEN NULLIF(LTRIM(RTRIM(s.Passport)),N'''') IS NULL THEN NULL ELSE CONVERT(varchar(64),HASHBYTES(''SHA2_256'',UPPER(REPLACE(REPLACE(LTRIM(RTRIM(s.Passport)),N'' '',N''''),N''-'',N''''))),2) END,
        N''STUDY'',COALESCE(TRY_CONVERT(datetime2(0),s.DateCreated),CONVERT(datetime2(0),GETDATE())),
        UPPER(COALESCE(s.FullName,N''''))+N''|''+COALESCE(CONVERT(nvarchar(30),TRY_CONVERT(date,s.Birthday),126),N'''')+N''|''+COALESCE(s.Nationality,N'''')
      FROM dbo.Students s WHERE ISNULL(s.Hidden_flag,0)=0;';

  INSERT dbo.MigrationIssues(SourceType,SourceId,IssueCode,Description)
  SELECT s.SourceType,s.SourceId,N'MISSING_PASSPORT',N'Cần xác minh thủ công: '+s.FullName
  FROM #Source s WHERE s.PassportSearchKey IS NULL
    AND NOT EXISTS(SELECT 1 FROM dbo.MigrationIssues m WHERE m.SourceType=s.SourceType AND m.SourceId=s.SourceId AND m.IssueCode=N'MISSING_PASSPORT');

  ;WITH Ambiguous AS(
    SELECT PassportSearchKey FROM #Source WHERE PassportSearchKey IS NOT NULL
    GROUP BY PassportSearchKey HAVING MIN(Signature)<>MAX(Signature)
  )
  INSERT dbo.MigrationIssues(SourceType,SourceId,IssueCode,Description)
  SELECT s.SourceType,s.SourceId,N'AMBIGUOUS_PASSPORT',N'Hộ chiếu trùng nhưng thông tin nhân thân khác: '+s.FullName
  FROM #Source s JOIN Ambiguous a ON a.PassportSearchKey=s.PassportSearchKey
  WHERE NOT EXISTS(SELECT 1 FROM dbo.MigrationIssues m WHERE m.SourceType=s.SourceType AND m.SourceId=s.SourceId AND m.IssueCode=N'AMBIGUOUS_PASSPORT');

  ;WITH Eligible AS(
    SELECT s.* FROM #Source s WHERE s.PassportSearchKey IS NOT NULL
      AND NOT EXISTS(SELECT 1 FROM #Source x WHERE x.PassportSearchKey=s.PassportSearchKey GROUP BY x.PassportSearchKey HAVING MIN(x.Signature)<>MAX(x.Signature))
  ), OnePerPassport AS(
    SELECT *,ROW_NUMBER() OVER(PARTITION BY PassportSearchKey ORDER BY CASE SourceType WHEN N'EMPLOYEE' THEN 0 ELSE 1 END,SourceId) rn FROM Eligible
  )
  INSERT dbo.ForeignPersons(FullName,Gender,Birthday,NationalityCode,PassportNumber,PassportSearchKey,IsDataIncomplete,IsDeleted)
  SELECT s.FullName,0,s.Birthday,s.NationalityCode,s.PassportNumber,s.PassportSearchKey,0,0
  FROM OnePerPassport s WHERE s.rn=1
    AND NOT EXISTS(SELECT 1 FROM dbo.ForeignPersons p WHERE p.PassportSearchKey=s.PassportSearchKey AND p.IsDeleted=0);

  ;WITH Eligible AS(
    SELECT s.* FROM #Source s WHERE s.PassportSearchKey IS NOT NULL
      AND NOT EXISTS(SELECT 1 FROM #Source x WHERE x.PassportSearchKey=s.PassportSearchKey GROUP BY x.PassportSearchKey HAVING MIN(x.Signature)<>MAX(x.Signature))
  )
  INSERT dbo.ForeignPersonSourceLinks(ForeignPersonId,SourceType,SourceId,LastSynchronizedAt)
  SELECT p.Id,s.SourceType,s.SourceId,SYSUTCDATETIME() FROM Eligible s
  JOIN dbo.ForeignPersons p ON p.PassportSearchKey=s.PassportSearchKey AND p.IsDeleted=0
  WHERE NOT EXISTS(SELECT 1 FROM dbo.ForeignPersonSourceLinks l WHERE l.SourceType=s.SourceType AND l.SourceId=s.SourceId);

  ;WITH Candidate AS(
    SELECT p.Id ForeignPersonId,s.PurposeCode,MIN(s.ValidFrom) ValidFrom,
      ROW_NUMBER() OVER(PARTITION BY p.Id ORDER BY CASE s.PurposeCode WHEN N'WORK' THEN 0 ELSE 1 END) rn
    FROM #Source s JOIN dbo.ForeignPersons p ON p.PassportSearchKey=s.PassportSearchKey AND p.IsDeleted=0
    WHERE s.PassportSearchKey IS NOT NULL GROUP BY p.Id,s.PurposeCode
  )
  INSERT dbo.StayCases(ForeignPersonId,PurposeCode,IsPrimary,ValidFrom,StatusCode)
  SELECT c.ForeignPersonId,c.PurposeCode,CASE WHEN c.rn=1 THEN 1 ELSE 0 END,c.ValidFrom,N'ACTIVE'
  FROM Candidate c WHERE NOT EXISTS(SELECT 1 FROM dbo.StayCases x WHERE x.ForeignPersonId=c.ForeignPersonId AND x.PurposeCode=c.PurposeCode);

  INSERT dbo.ImmigrationDocuments(ForeignPersonId,TypeCode,Number,ValidFrom,ValidTo,StatusCode,Note)
  SELECT p.Id,N'VISA',LTRIM(RTRIM(e.VisaNumber)),COALESCE(TRY_CONVERT(datetime2(0),e.DateCreated),CONVERT(datetime2(0),GETDATE())),
    TRY_CONVERT(datetime2(0),e.TemporaryStay),N'VALID',N'Backfill từ Employees; hạn dùng TemporaryStay theo dữ liệu legacy.'
  FROM dbo.Employees e JOIN dbo.ForeignPersonSourceLinks l ON l.SourceType=N'EMPLOYEE' AND l.SourceId=CONVERT(nvarchar(100),e.IDEmployee)
  JOIN dbo.ForeignPersons p ON p.Id=l.ForeignPersonId WHERE NULLIF(LTRIM(RTRIM(e.VisaNumber)),N'') IS NOT NULL
    AND NOT EXISTS(SELECT 1 FROM dbo.ImmigrationDocuments d WHERE d.ForeignPersonId=p.Id AND d.TypeCode=N'VISA' AND d.Number=LTRIM(RTRIM(e.VisaNumber)));

  IF OBJECT_ID(N'dbo.Students',N'U') IS NOT NULL
    EXEC sp_executesql N'
      INSERT dbo.ImmigrationDocuments(ForeignPersonId,TypeCode,Number,ValidFrom,ValidTo,StatusCode,Note)
      SELECT p.Id,N''VISA'',LTRIM(RTRIM(s.VisaNumber)),COALESCE(TRY_CONVERT(datetime2(0),s.DateCreated),CONVERT(datetime2(0),GETDATE())),
        TRY_CONVERT(datetime2(0),s.VisaExpiry),N''VALID'',N''Backfill từ Students.''
      FROM dbo.Students s JOIN dbo.ForeignPersonSourceLinks l ON l.SourceType=N''STUDENT'' AND l.SourceId=CONVERT(nvarchar(100),s.IDStudent)
      JOIN dbo.ForeignPersons p ON p.Id=l.ForeignPersonId WHERE NULLIF(LTRIM(RTRIM(s.VisaNumber)),N'''') IS NOT NULL
        AND NOT EXISTS(SELECT 1 FROM dbo.ImmigrationDocuments d WHERE d.ForeignPersonId=p.Id AND d.TypeCode=N''VISA'' AND d.Number=LTRIM(RTRIM(s.VisaNumber)));';

  COMMIT TRANSACTION;
END TRY
BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
