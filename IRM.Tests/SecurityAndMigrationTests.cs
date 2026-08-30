using IRM.Data;
using IRM.Data.Models;
using IRM.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace IRM.Tests;

public sealed class SecurityAndMigrationTests
{
    [Theory]
    [InlineData("=1+1", "'=1+1")]
    [InlineData("@cmd", "'@cmd")]
    [InlineData("normal", "normal")]
    public void ExcelExport_NeutralizesFormulaInjection(string value,string expected) =>
        Assert.Equal(expected,StatisticsExportService.Safe(value));

    [Fact]
    public void TemporalFilter_RejectsMixedModesAndReverseRange()
    {
        Assert.Throws<ArgumentException>(()=>new TemporalReportFilter{AsOfDate=DateTime.Today,From=DateTime.Today}.Validate());
        Assert.Throws<ArgumentException>(()=>new TemporalReportFilter{From=DateTime.Today,To=DateTime.Today.AddDays(-1)}.Validate());
    }

    [Fact]
    public async Task LegacyPassword_IsUpgradedAndFailuresAreLocked()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync(seedBaseline: false);
        var account=new Account{Username="admin",Name="Admin",Password="legacy",Permission=AccountPermission.Admin,Delete_flag=0};
        database.Context.Accounts.Add(account);await database.Context.SaveChangesAsync();
        var service=new AuthService(database.Context,new AuditService(database.Context));
        var principal=await service.AuthenticateWebAsync("admin","legacy","127.0.0.1");
        Assert.NotNull(principal);Assert.True(principal!.IsInRole(IrmRoles.Admin));
        Assert.NotEqual("legacy",database.Context.WebCredentials.Single().PasswordHash);
        for(var i=0;i<5;i++) await service.AuthenticateWebAsync("admin","bad","127.0.0.1");
        Assert.True(database.Context.WebCredentials.Single().LockedUntil>DateTime.UtcNow);
    }

    [Fact]
    public async Task AccountPasswordChange_ReplacesWebCredentialHash()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync(seedBaseline: false);
        var account = new Account { Username = "admin", Name = "Admin", Password = "old-password", Permission = AccountPermission.Admin, Delete_flag = 0 };
        database.Context.Accounts.Add(account);
        await database.Context.SaveChangesAsync();
        var service = new AuthService(database.Context, new AuditService(database.Context));
        Assert.NotNull(await service.AuthenticateWebAsync("admin", "old-password", "127.0.0.1"));

        account.Password = "new-password";
        await service.UpdateAccountAsync(account);

        Assert.Null(await service.AuthenticateWebAsync("admin", "old-password", "127.0.0.1"));
        Assert.NotNull(await service.AuthenticateWebAsync("admin", "new-password", "127.0.0.1"));
    }

    [Fact]
    public async Task SeedV010_IsIdempotentAndContains54Units()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync(seedBaseline: false);
        await DatabaseSeeder.SeedV010Async(database.Context);await DatabaseSeeder.SeedV010Async(database.Context);
        Assert.Equal(54,database.Context.AdministrativeUnits.Count());
        Assert.Single(database.Context.SchemaVersions.Where(x=>x.Version=="0.1.0"));
    }

    [Fact]
    public void LegalFileValidation_RejectsMimeMismatchAndBadSignature()
    {
        Assert.Throws<InvalidOperationException>(()=>LegalDocumentService.ValidateUploadMetadata(".exe","application/octet-stream"));
        Assert.Throws<InvalidOperationException>(()=>LegalDocumentService.ValidateUploadMetadata(".pdf","image/png"));
        Assert.False(LegalDocumentService.HasAllowedSignature(new byte[8],".pdf"));
        Assert.True(LegalDocumentService.HasAllowedSignature(new byte[]{0x25,0x50,0x44,0x46,0x2D,0x31,0x2E,0x34},".pdf"));
    }

    [Fact]
    public async Task Import_ExtendedFieldsCreateUnifiedHistoryAndRollbackCleanly()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync(seedBaseline: false);
        await DatabaseSeeder.SeedV010Async(database.Context);
        var account = new Account { Username="editor",Name="Editor",Password="legacy",Permission=AccountPermission.Admin };
        var field = new Field { FieldName="Test" };
        database.Context.AddRange(account,field); await database.Context.SaveChangesAsync();
        var company = new Company { CompanyName="Import test",IDField=field.IDField,TrackerID=account.IDUser };
        database.Context.Companies.Add(company); await database.Context.SaveChangesAsync();
        var row = new ImportPreviewRow
        {
            ParsedEmployee = new Employee { StaffName="Import Person",Passport="IMP-001",IDCompany=company.IDCompany },
            ExtendedFields = new(StringComparer.OrdinalIgnoreCase)
            {
                ["StayPurposeCode"]="TOURISM", ["StayValidFrom"]="2026-01-01",
                ["ElectronicIdentityNumber"]="EID-001", ["DocumentTypeCode"]="VISA;TEMPORARY_RESIDENCE_CARD",
                ["DocumentNumber"]="V-001;TRC-001", ["AdministrativeUnitCode"]="QN-W-001",
                ["ResidenceAddress"]="Hạ Long", ["ResidenceValidFrom"]="2026-01-01"
            }
        };
        var service = new ImportService(database.Context,new AuditService(database.Context),new AllowGuard());
        var result = await service.ExecuteImportAsync([row],[],company.IDCompany,"extended.xlsx","editor");
        Assert.True(result.Success); Assert.Equal(1,result.AddedCount);
        Assert.Single(database.Context.ForeignPersons); Assert.Equal(2,database.Context.ImmigrationDocuments.Count());
        Assert.Single(database.Context.ElectronicIdentities); Assert.Single(database.Context.ResidencePeriods);
        Assert.Equal(StayPurposeCodes.Tourism,database.Context.StayCases.Single().PurposeCode);
        Assert.True(await service.RollbackImportAsync(result.SessionId));
        Assert.Empty(database.Context.ForeignPersons); Assert.Empty(database.Context.ImmigrationDocuments);
        Assert.Equal(1,database.Context.Employees.Single().Hidden_flag);
    }

    [Fact]
    public async Task LegalFile_MalwareFailureRemovesTemporaryFile()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        var root=Path.Combine(Path.GetTempPath(),"irm-file-test-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"FileStorage:Root",root}}).Build();
            var service=new LegalDocumentService(database.Context,new AuditService(database.Context),configuration,
                new TestEnvironment(root),new AllowGuard(),new RejectScanner());
            await Assert.ThrowsAsync<InvalidOperationException>(()=>service.StoreAsync(
                new MemoryStream(new byte[]{0x25,0x50,0x44,0x46,0x2D,0x31,0x2E,0x34}),"scan.pdf","application/pdf"));
            Assert.Empty(Directory.GetFiles(root));
            Assert.Empty(database.Context.StoredFiles);
        }
        finally { Directory.Delete(root,true); }
    }

    private sealed class AllowGuard:IServiceAuthorizationGuard
    {
        public Task RequireAnyRoleAsync(params string[] roles)=>Task.CompletedTask;
        public Task<int> GetRequiredAccountIdAsync()=>Task.FromResult(1);
    }
    private sealed class RejectScanner:IMalwareScanner { public Task ScanAsync(string path,CancellationToken cancellationToken=default)=>throw new InvalidOperationException("malware"); }
    private sealed class TestEnvironment(string root):IWebHostEnvironment
    {
        public string ApplicationName{get;set;}="IRM.Tests"; public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();
        public string WebRootPath{get;set;}=root; public string EnvironmentName{get;set;}="Test";
        public string ContentRootPath{get;set;}=root; public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();
    }
}
