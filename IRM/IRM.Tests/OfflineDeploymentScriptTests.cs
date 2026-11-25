namespace IRM.Tests;

public sealed class OfflineDeploymentScriptTests
{
    private static string ResolveSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IRM.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    [Fact]
    public void BackupScript_SetsWorkingDirectoryToAppRoot()
    {
        var path = Path.Combine(ResolveSolutionRoot(), "deploy", "windows", "backup.ps1");
        var text = File.ReadAllText(path);

        Assert.Contains("Push-Location $appRoot", text, StringComparison.Ordinal);
        Assert.Contains("Pop-Location", text, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerPreflight_DoesNotHardBlockLowBaseClockCpusOrThirdPartyAntivirus()
    {
        var root = ResolveSolutionRoot();
        var ui = File.ReadAllText(Path.Combine(root, "deploy", "windows", "installer-ui.ps1"));
        var install = File.ReadAllText(Path.Combine(root, "deploy", "windows", "install.ps1"));

        Assert.DoesNotContain("MaxClockSpeed -lt 2000", install, StringComparison.Ordinal);
        Assert.DoesNotContain("MaxClockSpeed -ge 2000", ui, StringComparison.Ordinal);
        Assert.Contains("Bkav/Kaspersky/ESET", ui, StringComparison.Ordinal);
    }

    [Fact]
    public void HealthReadyChecks_UseRetryLoopAcrossInstallRepairAndRestore()
    {
        var root = ResolveSolutionRoot();
        foreach (var file in new[] { "install.ps1", "repair.ps1", "restore.ps1" })
        {
            var text = File.ReadAllText(Path.Combine(root, "deploy", "windows", file));
            Assert.Contains("for ($attempt = 1;", text, StringComparison.Ordinal);
            Assert.Contains("/health/ready", text, StringComparison.Ordinal);
        }

        var installText = File.ReadAllText(Path.Combine(root, "deploy", "windows", "install.ps1"));
        Assert.DoesNotContain("Server=.\\IRMEXPRESS", installText, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerMetadataAndSqlSchema_TargetKh2AndIncludeAllThreeMigrations()
    {
        var root = ResolveSolutionRoot();
        var iss = File.ReadAllText(Path.Combine(root, "deploy", "windows", "irm.iss"));
        var manifest = File.ReadAllText(Path.Combine(root, "deploy", "release-manifest.json"));
        var verifySql = File.ReadAllText(Path.Combine(root, "deploy", "database", "verify-installed-database.sql"));
        var schemaSql = File.ReadAllText(Path.Combine(root, "deploy", "database", "IRM-v1.0.1-schema.sql"));

        Assert.Contains("IRM-v1.0.2-windows-x64-offline-setup-KH2", iss, StringComparison.Ordinal);
        Assert.Contains("IRM-v1.0.2-windows-x64-offline-setup-KH2.exe", manifest, StringComparison.Ordinal);
        Assert.Contains("20261122093015_MakeEmployeeCompanyNullable", verifySql, StringComparison.Ordinal);
        Assert.Contains("20260927120137_AddVisaSymbol", schemaSql, StringComparison.Ordinal);
        Assert.Contains("20261122093015_MakeEmployeeCompanyNullable", schemaSql, StringComparison.Ordinal);
        Assert.Contains("DROP INDEX [IX_Employees_IDCompany] ON [Employees]", schemaSql, StringComparison.Ordinal);
    }
}
