[CmdletBinding()]
param([string]$OutputPath)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'windows-environment.ps1')
$os = Get-CimInstance Win32_OperatingSystem
$windows = Get-IrmWindowsCompatibility -OperatingSystem $os
$report = [ordered]@{
    Caption = $os.Caption
    OSArchitecture = $os.OSArchitecture
    CimVersion = $os.Version
    CimBuildNumber = $os.BuildNumber
    ProductType = $os.ProductType
    DotNetReportedVersion = [Environment]::OSVersion.Version.ToString()
    Is64BitOperatingSystem = [Environment]::Is64BitOperatingSystem
    Is64BitPowerShellProcess = [Environment]::Is64BitProcess
    PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    WindowsCompatibility = $windows
    NetFrameworkCompatibility = (Get-IrmNetFrameworkCompatibility)
}
$json = $report | ConvertTo-Json -Depth 4
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    [IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath), $json, (New-Object Text.UTF8Encoding($true)))
}
Write-Output $json
