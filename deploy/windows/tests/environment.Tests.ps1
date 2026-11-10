$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\windows-environment.ps1')
$script:assertions = 0
function Assert-Equal($Actual, $Expected, [string]$Case) {
    if ($Actual -ne $Expected) { throw "$Case : expected [$Expected], got [$Actual]" }
    $script:assertions++
}
function Get-Case([string]$Build, [string]$Version, [int]$ProductType, [bool]$Os64 = $true, [bool]$Process64 = $true, [string]$ReportedVersion = '10.0.26200.0') {
    $os = [pscustomobject]@{ Caption='Test Windows'; BuildNumber=$Build; Version=$Version; ProductType=$ProductType }
    return Get-IrmWindowsCompatibility -OperatingSystem $os -Is64BitOperatingSystem $Os64 -Is64BitProcess $Process64 -ReportedVersion $ReportedVersion
}

$result = Get-Case '19045' '10.0.19045' 1 $true $true '6.2.9200.0'
Assert-Equal $result.VersionPassed $true 'Actual Windows build must win over compatibility version'
Assert-Equal $result.BuildNumber 19045 'Build comes from CIM'
Assert-Equal $result.ArchitecturePassed $true 'x64 supported regardless of reported .NET version'
Assert-Equal $result.BuildSource 'Win32_OperatingSystem.BuildNumber' 'Authoritative build source'
$result = Get-Case '10586' '10.0.10586' 1
Assert-Equal $result.ArchitecturePassed $true 'Old Windows x64 is still x64'
Assert-Equal $result.VersionPassed $false 'Old Windows must fail version only'
Assert-Equal ($result.VersionDetail -like '*10586*14393*') $true 'Old Windows reports actual and required build'
Assert-Equal (Get-Case '14393' '10.0.14393' 1).VersionPassed $true 'Minimum Windows 10 workstation build'
Assert-Equal (Get-Case '22631' '10.0.22631' 1).VersionPassed $true 'Windows 11'
$result = Get-Case '19045' '10.0.19045' 1 $false $false
Assert-Equal $result.ArchitecturePassed $false '32-bit OS remains unsupported'
Assert-Equal $result.VersionPassed $true 'Bitness and OS version are independent'
$result = Get-Case '19045' '10.0.19045' 1 $true $false
Assert-Equal $result.ArchitecturePassed $true '32-bit PowerShell does not mean 32-bit Windows'
Assert-Equal $result.VersionPassed $true '32-bit host uses real CIM build'
Assert-Equal $result.Is64BitProcess $false 'Process bitness remains visible in diagnostics'
Assert-Equal (Get-Case '17763' '10.0.17763' 3).VersionPassed $true 'Windows Server 2019'
Assert-Equal (Get-Case '17763' '10.0.17763' 2).VersionPassed $true 'Windows Server domain controller'
Assert-Equal (Get-Case '14393' '10.0.14393' 3).VersionPassed $true 'Windows Server 2016 is the technical minimum'
Assert-Equal (Get-Case '19045' '10.0.19045' 0).VersionPassed $false 'Unknown product type fails with diagnostics'
$result = Get-Case 'invalid' '10.0.22631' 1
Assert-Equal $result.VersionPassed $true 'Version fallback comes from CIM, not Environment.OSVersion'
Assert-Equal $result.BuildSource 'Win32_OperatingSystem.Version' 'CIM Version fallback is identified'
$result = Get-Case '' 'invalid' 1
Assert-Equal $result.VersionPassed $false 'Missing real build fails closed'
Assert-Equal (Get-IrmWindowsPowerShellPath -WindowsDirectory 'C:\Windows' -Is64BitOperatingSystem $true -Is64BitProcess $false) 'C:\Windows\Sysnative\WindowsPowerShell\v1.0\powershell.exe' 'Native PowerShell from x86 process'
Assert-Equal (Get-IrmWindowsPowerShellPath -WindowsDirectory 'C:\Windows' -Is64BitOperatingSystem $true -Is64BitProcess $true) 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' 'Native PowerShell from x64 process'

if ([Environment]::Is64BitOperatingSystem -and -not [Environment]::Is64BitProcess) {
    $nativeResult = & (Get-IrmWindowsPowerShellPath) -NoProfile -NonInteractive -Command '[Environment]::Is64BitProcess'
    Assert-Equal ([string]$nativeResult) 'True' 'x86 host actually launches native x64 PowerShell'
}

Assert-Equal (Get-Case '19043' '10.0.19043' 1).VersionPassed $true 'Windows 10 21H1 must not be rejected as too old'
Assert-Equal (Get-Case '7601' '6.1.7601' 1).VersionPassed $false 'Windows 7 is unsupported'
Assert-Equal (Get-Case '9600' '6.3.9600' 1).VersionPassed $false 'Windows 8.1 is unsupported'
Assert-Equal (Get-Case '9600' '6.3.9600' 3).VersionPassed $false 'Windows Server 2012 R2 is below SQL Server minimum'
Assert-Equal (Get-Case '14392' '10.0.14392' 3).VersionPassed $false 'One build below minimum server version'
Assert-Equal (Get-Case '14393' '10.0.14393' 2).VersionPassed $true 'Server 2016 domain controller build threshold'
Assert-Equal (Get-IrmNetFrameworkCompatibility -Release 461807).Passed $false 'One below .NET Framework 4.7.2'
Assert-Equal (Get-IrmNetFrameworkCompatibility -Release 461808).Passed $true '.NET Framework 4.7.2 on Windows 10 1803'
Assert-Equal (Get-IrmNetFrameworkCompatibility -Release 461814).Passed $true '.NET Framework 4.7.2 on other supported Windows'
Assert-Equal (Get-IrmNetFrameworkCompatibility -Release 528040).Passed $true '.NET Framework 4.8'
Assert-Equal (Get-IrmNetFrameworkCompatibility -Release $null).Passed $false 'Missing .NET Framework'
Assert-Equal (Get-IrmNetFrameworkCompatibility -Release 'invalid').Passed $false 'Unknown .NET Framework release'
$manifest = Get-Content (Join-Path $PSScriptRoot '..\..\release-manifest.json') -Raw | ConvertFrom-Json
Assert-Equal $manifest.windows.minimumOs.build (Get-Case '14393' '10.0.14393' 1).MinimumBuild 'Manifest and runtime agree on minimum OS build'
Assert-Equal $manifest.windows.netFramework.minimumRelease (Get-IrmNetFrameworkCompatibility -Release 0).MinimumRelease 'Manifest and runtime agree on .NET Framework'
$definition = Get-Content (Join-Path $PSScriptRoot '..\irm.iss') -Raw
Assert-Equal ($definition -match '(?m)^MinVersion=10\.0\.14393\s*$') $true 'Inno entry gate agrees with preflight'

# Query only: never execute the installer or change services/registry/database.
$actualOs = Get-CimInstance Win32_OperatingSystem
$actual = Get-IrmWindowsCompatibility -OperatingSystem $actualOs
Assert-Equal $actual.BuildNumber ([int]$actualOs.BuildNumber) 'Actual machine uses CIM BuildNumber'
Assert-Equal $actual.ArchitecturePassed ([Environment]::Is64BitOperatingSystem) 'Actual OS bitness independent of process'

foreach ($file in 'windows-environment.ps1','installer-ui.ps1','installer-theme.ps1','install.ps1','build-windows-installer.ps1','diagnose-environment.ps1') {
    $tokens=$null; $errors=$null
    [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot "..\$file"),[ref]$tokens,[ref]$errors) | Out-Null
    Assert-Equal @($errors).Count 0 "PowerShell syntax: $file"
}
Write-Host "PASS: $script:assertions assertions; OS build=$($actual.BuildNumber); OS64=$($actual.Is64BitOperatingSystem); Process64=$($actual.Is64BitProcess)"
