param(
    [switch]$SkipInstall,
    [string]$SqlInstance = "",
    [string]$SqlUser = "",
    [string]$SqlPassword = ""
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$SqlDir = Join-Path $PSScriptRoot "sql"

Write-Host ""
Write-Host "=== IRM - Setup SQL Server for Dev/Test ===" -ForegroundColor Cyan
Write-Host ""

# --- Check Admin ---
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")
if (-not $isAdmin -and -not $SkipInstall) {
    Write-Host "[!] Please run PowerShell as Administrator" -ForegroundColor Red
    exit 1
}

# --- Step 1: Check existing SQL Server ---
Write-Host "[1/5] Checking SQL Server..." -ForegroundColor Yellow

$existingInstances = @()
try {
    $reg = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server' -Name InstalledInstances -ErrorAction SilentlyContinue
    if ($reg) { $existingInstances += $reg.InstalledInstances }
} catch {}

$localDbInstalled = $false
try {
    $sqllocaldb = Get-Command "sqllocaldb" -ErrorAction SilentlyContinue
    if ($sqllocaldb) { $localDbInstalled = $true }
} catch {}

$sqlcmdPath = $null
$sqlcmdPaths = @(
    "${env:ProgramFiles}\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe",
    "${env:ProgramFiles}\Microsoft SQL Server\Client SDK\ODBC\130\Tools\Binn\sqlcmd.exe",
    "${env:ProgramFiles}\Microsoft SQL Server\120\Tools\Binn\sqlcmd.exe",
    "${env:ProgramFiles}\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe"
)
foreach ($p in $sqlcmdPaths) {
    if (Test-Path $p) { $sqlcmdPath = $p; break }
}
if (-not $sqlcmdPath) {
    $sqlcmdCmd = Get-Command "sqlcmd" -ErrorAction SilentlyContinue
    if ($sqlcmdCmd) { $sqlcmdPath = $sqlcmdCmd.Source }
}

if ($existingInstances.Count -gt 0) {
    Write-Host "  [OK] Found SQL Server: $($existingInstances -join ', ')" -ForegroundColor Green
    if (-not $SqlInstance) {
        if ($existingInstances -contains "SQLEXPRESS") {
            $SqlInstance = ".\SQLEXPRESS"
        }
        elseif ($existingInstances -contains "MSSQLSERVER") {
            $SqlInstance = "."
        }
        else {
            $SqlInstance = ".\$($existingInstances[0])"
        }
    }
}
elseif ($localDbInstalled) {
    Write-Host "  [OK] Found SQL Server LocalDB" -ForegroundColor Green
    if (-not $SqlInstance) { $SqlInstance = "(localdb)\MSSQLLocalDB" }
}
else {
    Write-Host "  [--] No SQL Server found" -ForegroundColor Yellow
}

# --- Step 2: Install SQL Server if needed ---
if (-not $SkipInstall -and $existingInstances.Count -eq 0 -and -not $localDbInstalled) {
    Write-Host ""
    Write-Host "[2/5] Installing SQL Server Express..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Choose version:" -ForegroundColor Cyan
    Write-Host "  [1] SQL Server 2014 Express with Tools (recommended)" -ForegroundColor White
    Write-Host "  [2] SQL Server 2019 Express" -ForegroundColor White
    Write-Host "  [3] SQL Server 2014 LocalDB (lightweight, dev only)" -ForegroundColor White
    Write-Host "  [4] Skip - I will install manually" -ForegroundColor Gray
    Write-Host ""
    $choice = Read-Host "  Enter choice (1-4)"

    switch ($choice) {
        "1" {
            $downloadUrl = "https://download.microsoft.com/download/E/A/E/EAE6F7FC-767A-4038-A954-49B8B05D04EB/ExpressAndTools%2064BIT/SQLEXPRWT_x64_ENU.exe"
            $installerPath = Join-Path $env:TEMP "SQLEXPRWT_x64_2014.exe"
            Write-Host "  Downloading SQL Server 2014 Express with Tools..." -ForegroundColor Cyan
            Write-Host "  URL: $downloadUrl" -ForegroundColor Gray
            Write-Host "  (~800MB, please wait)" -ForegroundColor Gray
            try {
                [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
                Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -UseBasicParsing
            }
            catch {
                Write-Host "  [!] Download failed. Please download manually:" -ForegroundColor Red
                Write-Host "  https://www.microsoft.com/en-us/download/details.aspx?id=42299" -ForegroundColor Cyan
                Write-Host "  After installing, run: .\setup-sqlserver-localdb.ps1 -SkipInstall" -ForegroundColor Yellow
                exit 1
            }
            Write-Host "  Installing (silent mode)..." -ForegroundColor Cyan
            $installArgs = "/Q /ACTION=Install /FEATURES=SQLEngine,Tools /INSTANCENAME=SQLEXPRESS /SQLSYSADMINACCOUNTS=`"BUILTIN\Administrators`" /SECURITYMODE=SQL /SAPWD=IRM@2026!Dev /TCPENABLED=1 /NPENABLED=1 /IACCEPTSQLSERVERLICENSETERMS"
            Start-Process -FilePath $installerPath -ArgumentList $installArgs -Wait -NoNewWindow
            $SqlInstance = ".\SQLEXPRESS"
            Write-Host "  [OK] SQL Server 2014 Express installed (SQLEXPRESS)" -ForegroundColor Green
        }
        "2" {
            $downloadUrl = "https://go.microsoft.com/fwlink/?linkid=866658"
            $installerPath = Join-Path $env:TEMP "SQL2019-SSEI-Expr.exe"
            Write-Host "  Downloading SQL Server 2019 Express..." -ForegroundColor Cyan
            try {
                [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
                Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -UseBasicParsing
            }
            catch {
                Write-Host "  [!] Download failed." -ForegroundColor Red
                Write-Host "  https://www.microsoft.com/en-us/sql-server/sql-server-downloads" -ForegroundColor Cyan
                exit 1
            }
            Write-Host "  Running installer (choose 'Basic')..." -ForegroundColor Yellow
            Start-Process -FilePath $installerPath -Wait
            $SqlInstance = ".\SQLEXPRESS"
            Write-Host "  [OK] Installation complete" -ForegroundColor Green
        }
        "3" {
            $downloadUrl = "https://download.microsoft.com/download/E/A/E/EAE6F7FC-767A-4038-A954-49B8B05D04EB/LocalDB%2064BIT/SqlLocalDB.msi"
            $installerPath = Join-Path $env:TEMP "SqlLocalDB2014.msi"
            Write-Host "  Downloading SQL Server 2014 LocalDB..." -ForegroundColor Cyan
            try {
                [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
                Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -UseBasicParsing
            }
            catch {
                Write-Host "  [!] Download failed." -ForegroundColor Red
                Write-Host "  https://www.microsoft.com/en-us/download/details.aspx?id=42299" -ForegroundColor Cyan
                exit 1
            }
            Write-Host "  Installing LocalDB..." -ForegroundColor Cyan
            Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$installerPath`" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES" -Wait -NoNewWindow
            & sqllocaldb create "MSSQLLocalDB" 12.0 -s
            $SqlInstance = "(localdb)\MSSQLLocalDB"
            Write-Host "  [OK] SQL Server 2014 LocalDB installed" -ForegroundColor Green
        }
        default {
            Write-Host "  Skipped. Run again with -SkipInstall after manual install." -ForegroundColor Gray
            exit 0
        }
    }

    foreach ($p in $sqlcmdPaths) {
        if (Test-Path $p) { $sqlcmdPath = $p; break }
    }
}
else {
    Write-Host "[2/5] Skip install (SQL Server exists)" -ForegroundColor Green
}

Write-Host ""
Write-Host "  Using SQL Instance: $SqlInstance" -ForegroundColor Cyan

# --- Step 3: Create Database ---
Write-Host ""
Write-Host "[3/5] Creating database ReportManagerDB..." -ForegroundColor Yellow

function Invoke-SqlScript {
    param([string]$SqlFile, [string]$Instance, [string]$User = "", [string]$Pass = "")
    $fileName = Split-Path -Leaf $SqlFile
    Write-Host "  Running: $fileName" -ForegroundColor Gray
    if ($sqlcmdPath) {
        $cmdArgs = @("-S", $Instance, "-i", $SqlFile, "-b")
        if ($User) { $cmdArgs += @("-U", $User, "-P", $Pass) }
        else { $cmdArgs += "-E" }
        $result = & $sqlcmdPath @cmdArgs 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "    [!] Error: $result" -ForegroundColor Red
            return $false
        }
        $result | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
    }
    else {
        try {
            Import-Module SqlServer -ErrorAction SilentlyContinue
            if (-not (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue)) {
                Import-Module SQLPS -ErrorAction SilentlyContinue -DisableNameChecking
            }
            $params = @{ ServerInstance = $Instance; InputFile = $SqlFile; ErrorAction = "Stop" }
            if ($User) { $params["Username"] = $User; $params["Password"] = $Pass }
            Invoke-Sqlcmd @params
        }
        catch {
            Write-Host "    [!] Error: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
    return $true
}

$sqlScripts = @("00-full-setup.sql", "01-add-new-tables.sql", "02-add-indexes.sql", "03-import-tables.sql", "04-family-visit.sql")
$allOk = $true
foreach ($script in $sqlScripts) {
    $scriptPath = Join-Path $SqlDir $script
    if (Test-Path $scriptPath) {
        $ok = Invoke-SqlScript -SqlFile $scriptPath -Instance $SqlInstance -User $SqlUser -Pass $SqlPassword
        if (-not $ok) { $allOk = $false; Write-Host "  [!] Failed: $script" -ForegroundColor Red }
    }
    else {
        Write-Host "  [SKIP] Not found: $script" -ForegroundColor Yellow
    }
}

if ($allOk) { Write-Host "  [OK] Database ReportManagerDB ready!" -ForegroundColor Green }
else { Write-Host "  [!] Some errors occurred" -ForegroundColor Red }

# --- Step 4: Update connection string ---
Write-Host ""
Write-Host "[4/5] Updating connection string..." -ForegroundColor Yellow

$devSettingsPath = Join-Path $ProjectRoot "IRM\appsettings.Development.json"
if ($SqlUser) {
    $connStr = "Server=$SqlInstance;Database=ReportManagerDB;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;"
}
else {
    $connStr = "Server=$SqlInstance;Database=ReportManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

$devSettings = @{
    ConnectionStrings = @{ DefaultConnection = $connStr }
    Logging = @{ LogLevel = @{ Default = "Information"; "Microsoft.AspNetCore" = "Warning" } }
} | ConvertTo-Json -Depth 4

Set-Content -Path $devSettingsPath -Value $devSettings -Encoding UTF8
Write-Host "  [OK] Updated appsettings.Development.json" -ForegroundColor Green
Write-Host "  Connection: $connStr" -ForegroundColor Gray

# --- Step 5: Test connection ---
Write-Host ""
Write-Host "[5/5] Testing connection..." -ForegroundColor Yellow

$testSql = "SELECT COUNT(*) AS TableCount FROM ReportManagerDB.sys.tables;"
$testOk = $false
if ($sqlcmdPath) {
    $testArgs = @("-S", $SqlInstance, "-Q", $testSql, "-b", "-h", "-1")
    if ($SqlUser) { $testArgs += @("-U", $SqlUser, "-P", $SqlPassword) }
    else { $testArgs += "-E" }
    $testResult = & $sqlcmdPath @testArgs 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Connection successful!" -ForegroundColor Green
        $testOk = $true
    }
}
if (-not $testOk) {
    try {
        Import-Module SqlServer -ErrorAction SilentlyContinue
        $result = Invoke-Sqlcmd -ServerInstance $SqlInstance -Query $testSql -ErrorAction Stop
        Write-Host "  [OK] Connection successful! Found $($result.TableCount) tables" -ForegroundColor Green
    }
    catch {
        Write-Host "  [!] Cannot auto-test. Try running the app to verify." -ForegroundColor Yellow
    }
}

# --- Done ---
Write-Host ""
Write-Host "=== DONE ===" -ForegroundColor Green
Write-Host "  SQL Instance:  $SqlInstance" -ForegroundColor Cyan
Write-Host "  Database:      ReportManagerDB" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Next steps:" -ForegroundColor Yellow
Write-Host "  1. cd IRM" -ForegroundColor White
Write-Host "  2. dotnet run" -ForegroundColor White
Write-Host "  3. Open browser: http://localhost:5050" -ForegroundColor White
Write-Host ""
Write-Host "  Test accounts: admin/123456, nguyenvana/123456, tranthib/123456" -ForegroundColor White
Write-Host ""
