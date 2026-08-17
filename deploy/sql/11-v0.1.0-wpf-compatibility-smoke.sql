/* Run against a restored test database only. All inserted rows are rolled back. */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
  BEGIN TRANSACTION;
  DECLARE @AccountId int=(SELECT TOP(1) IDUser FROM dbo.Accounts WHERE Delete_flag=0 ORDER BY IDUser);
  DECLARE @FieldId int=(SELECT TOP(1) IDField FROM dbo.Fields WHERE Delete_flag=0 ORDER BY IDField);
  DECLARE @CareerId int=(SELECT TOP(1) IDCareer FROM dbo.Careers WHERE Delete_flag=0 ORDER BY IDCareer);
  IF @AccountId IS NULL OR @FieldId IS NULL OR @CareerId IS NULL RAISERROR(N'Missing legacy catalog seed for smoke test.',16,1);

  INSERT dbo.Companies
    (CompanyName,TypeOfBusiniess,Uptime,Address,IDField,LegalRepresentative,TotalAmount,AmountOfExemption,
     QuantityAvailable,QuantityNotYet,NumberOfPersonalities,RegistrationProfile,Note,UpdateDay,Delete_flag,
     DescriptionOfActivities,TrackerID)
  VALUES(N'IRM V010 WPF SMOKE',N'Test',N'2026',N'Test address',@FieldId,N'Test',0,0,0,0,0,N'',N'',GETDATE(),0,N'',@AccountId);
  DECLARE @CompanyId int=SCOPE_IDENTITY();

  INSERT dbo.Employees
    (StaffName,Gender,Birthday,Nationality,Passport,Address,IDCareer,WorkPermit,WorkPermitNumber,VisaNumber,
     TemporaryStay,SettlementResults,SettlementResultsString,IDUser,IDCompany,Hidden_flag,Note,DateCreated,
     CardCreationDate,WorkingStatus,DateOfJoin,DateOfLeave)
  VALUES(N'IRM V010 WPF SMOKE',1,'1990-01-01',NULL,N'IRM-V010-SMOKE',N'Test',@CareerId,0,N'',N'',
     DATEADD(day,30,GETDATE()),0,N'',@AccountId,@CompanyId,0,N'',GETDATE(),NULL,0,NULL,NULL);

  INSERT dbo.Wards(WardName,Delete_flag) VALUES(N'IRM V010 WPF SMOKE',0);
  INSERT dbo.Districts(DisTrictName,Delete_flag) VALUES(N'IRM V010 WPF SMOKE',0);
  INSERT dbo.Attach(IDCompany,Type,Name,Folder,DateCreated,Delete_flag,DateModified,DateDelete)
    VALUES(@CompanyId,1,N'smoke.pdf',N'C:\IRM-Smoke',GETDATE(),0,NULL,NULL);

  IF (SELECT COUNT(*) FROM dbo.LegacyChangeEvents WHERE EntityType IN(N'Companies',N'Employees',N'Wards',N'Districts',N'Attach')) < 5
    RAISERROR(N'Legacy triggers did not capture every WPF smoke insert.',16,1);
  ROLLBACK TRANSACTION;
  SELECT N'PASS' Result,N'WPF explicit-column inserts and legacy triggers succeeded; transaction rolled back.' Evidence;
END TRY
BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
