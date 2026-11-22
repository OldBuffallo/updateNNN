# ============================================
# Install SQL Server 2014 Express - Database Engine Only
# Run as Administrator!
# ============================================

$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Host "=== SQL Server 2014 Express Installer ===" -ForegroundColor Cyan
Write-Host ""

# Check admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")
if (-not $isAdmin) {
    Write-Host "[!] MUST run as Administrator!" -ForegroundColor Red
    Write-Host "    Right-click PowerShell -> Run as Administrator" -ForegroundColor Yellow
    exit 1
}

# Download SQL Server 2014 Express (Database Engine only, ~280MB)
$downloadUrl = "https://download.microsoft.com/download/E/A/E/EAE6F7FC-767A-4038-A954-49B8B05D04EB/SQLEXPR_x64_ENU.exe"
$installerPath = Join-Path $env:TEMP "SQLEXPR_x64_2014.exe"
$extractPath = Join-Path $env:TEMP "SQL2014Express"

if (Test-Path $installerPath) {
    Write-Host "[1/3] Installer already downloaded: $installerPath" -ForegroundColor Green
} else {
    Write-Host "[1/3] Downloading SQL Server 2014 Express (~280MB)..." -ForegroundColor Yellow
    Write-Host "  URL: $downloadUrl" -ForegroundColor Gray
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -UseBasicParsing
        Write-Host "  [OK] Downloaded!" -ForegroundColor Green
    } catch {
        Write-Host "  [!] Download failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "  Please download manually from:" -ForegroundColor Yellow
        Write-Host "  https://www.microsoft.com/en-us/download/details.aspx?id=42299" -ForegroundColor Cyan
        Write-Host "  Choose: SQLEXPR_x64_ENU.exe" -ForegroundColor Cyan
        Write-Host "  Save to: $installerPath" -ForegroundColor Cyan
        Write-Host "  Then run this script again." -ForegroundColor Yellow
        exit 1
    }
}

# Extract
Write-Host "[2/3] Extracting..." -ForegroundColor Yellow
if (Test-Path $extractPath) { Remove-Item $extractPath -Recurse -Force }
Start-Process -FilePath $installerPath -ArgumentList "/Q /x:$extractPath" -Wait -NoNewWindow
Write-Host "  [OK] Extracted to $extractPath" -ForegroundColor Green

# Install
Write-Host "[3/3] Installing SQL Server 2014 Express..." -ForegroundColor Yellow
Write-Host "  Instance: SQLEXPRESS" -ForegroundColor Gray
Write-Host "  Auth: Mixed Mode (Windows + SQL)" -ForegroundColor Gray
Write-Host "  SA Password: IRM@2026!Dev" -ForegroundColor Gray
Write-Host ""
Write-Host "  This may take 5-10 minutes..." -ForegroundColor Yellow

$setupExe = Join-Path $extractPath "setup.exe"
$setupArgs = @(
    "/Q"
    "/ACTION=Install"
    "/FEATURES=SQLEngine"
    "/INSTANCENAME=SQLEXPRESS"
    "/SQLSYSADMINACCOUNTS=`"BUILTIN\Administrators`""
    "/SECURITYMODE=SQL"
    "/SAPWD=IRM@2026!Dev"
    "/TCPENABLED=1"
    "/NPENABLED=1"
    "/IACCEPTSQLSERVERLICENSETERMS"
    "/UPDATEENABLED=0"
)

$process = Start-Process -FilePath $setupExe -ArgumentList ($setupArgs -join " ") -Wait -NoNewWindow -PassThru

if ($process.ExitCode -eq 0) {
    Write-Host ""
    Write-Host "=== SUCCESS! ===" -ForegroundColor Green
    Write-Host "  Instance: .\SQLEXPRESS" -ForegroundColor Cyan
    Write-Host "  SA Password: IRM@2026!Dev" -ForegroundColor Cyan
    Write-Host ""

    # Verify service
    $svc = Get-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
    if ($svc) {
        Write-Host "  Service: $($svc.DisplayName) - $($svc.Status)" -ForegroundColor Green
        if ($svc.Status -ne "Running") {
            Start-Service "MSSQL`$SQLEXPRESS"
            Write-Host "  Service started!" -ForegroundColor Green
        }
    }

    Write-Host ""
    Write-Host "  Next: Run setup-sqlserver-localdb.ps1 -SkipInstall -SqlInstance '.\SQLEXPRESS'" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "  [!] Installation returned exit code: $($process.ExitCode)" -ForegroundColor Red
    Write-Host "  Check logs at: C:\Program Files\Microsoft SQL Server\120\Setup Bootstrap\Log" -ForegroundColor Yellow
}

# Cleanup
Write-Host ""
Write-Host "Cleaning up temp files..." -ForegroundColor Gray
Remove-Item $extractPath -Recurse -Force -ErrorAction SilentlyContinue
