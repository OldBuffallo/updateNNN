# Shared read-only Windows checks for both installer entry points.
# Do not use Environment.OSVersion to decide support: compatibility manifests can alter it.
function Get-IrmWindowsCompatibility {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][object]$OperatingSystem,
        [bool]$Is64BitOperatingSystem = [Environment]::Is64BitOperatingSystem,
        [bool]$Is64BitProcess = [Environment]::Is64BitProcess,
        [string]$ReportedVersion = [Environment]::OSVersion.Version.ToString()
    )

    $build = 0
    $buildDetected = [int]::TryParse([string]$OperatingSystem.BuildNumber, [ref]$build) -and $build -gt 0
    $buildSource = 'Win32_OperatingSystem.BuildNumber'
    if (-not $buildDetected) {
        $version = $null
        if ([version]::TryParse([string]$OperatingSystem.Version, [ref]$version) -and $version.Build -gt 0) {
            $build = $version.Build
            $buildDetected = $true
            $buildSource = 'Win32_OperatingSystem.Version'
        }
    }

    $productType = 0
    $productTypeKnown = [int]::TryParse([string]$OperatingSystem.ProductType, [ref]$productType) -and $productType -in 1,2,3
    $minimumBuild = 14393
    $requirement = if ($productType -eq 1) { 'Windows 10 phiên bản 1607 x64 (build 14393) hoặc mới hơn, bao gồm Windows 11' } else { 'Windows Server 2016 x64 (build 14393) hoặc mới hơn' }
    $osBits = if ($Is64BitOperatingSystem) { '64-bit' } else { '32-bit' }
    $processBits = if ($Is64BitProcess) { '64-bit' } else { '32-bit' }
    $architectureDetail = "Windows $osBits; tiến trình PowerShell $processBits."
    $versionPassed = $productTypeKnown -and $buildDetected -and $build -ge $minimumBuild
    if (-not $productTypeKnown) {
        $versionDetail = "Không xác định được loại Windows từ CIM (ProductType=$($OperatingSystem.ProductType))."
    } elseif (-not $buildDetected) {
        $versionDetail = 'Không đọc được số build Windows thật từ CIM; không dùng số phiên bản tương thích của PowerShell để suy đoán.'
    } elseif ($versionPassed) {
        $versionDetail = "$($OperatingSystem.Caption), phiên bản $($OperatingSystem.Version), build $build. Yêu cầu tối thiểu: $requirement."
    } else {
        $versionDetail = "Đang dùng $($OperatingSystem.Caption), phiên bản $($OperatingSystem.Version), build $build. Yêu cầu: $requirement. Máy 64-bit vẫn cần đủ phiên bản Windows."
    }
    $versionDetail += " Nguồn: $buildSource; .NET báo: $ReportedVersion."

    return [pscustomobject]@{
        ArchitecturePassed = $Is64BitOperatingSystem
        ArchitectureDetail = $architectureDetail
        VersionPassed = $versionPassed
        VersionDetail = $versionDetail
        BuildNumber = $build
        BuildSource = $buildSource
        ProductType = $productType
        MinimumBuild = $minimumBuild
        Is64BitOperatingSystem = $Is64BitOperatingSystem
        Is64BitProcess = $Is64BitProcess
    }
}

function Get-IrmWindowsPowerShellPath {
    [CmdletBinding()]
    param(
        [string]$WindowsDirectory = $env:SystemRoot,
        [bool]$Is64BitOperatingSystem = [Environment]::Is64BitOperatingSystem,
        [bool]$Is64BitProcess = [Environment]::Is64BitProcess
    )
    $systemFolder = if ($Is64BitOperatingSystem -and -not $Is64BitProcess) { 'Sysnative' } else { 'System32' }
    return Join-Path $WindowsDirectory "$systemFolder\WindowsPowerShell\v1.0\powershell.exe"
}

# SQL Server 2022 requires .NET Framework 4.7.2 even though IRM ships its own .NET 8 runtime.
function Get-IrmNetFrameworkRelease {
    $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
    try {
        $key = $base.OpenSubKey('SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full')
        if ($null -eq $key) { return 0 }
        try { return [int]$key.GetValue('Release', 0) }
        finally { $key.Dispose() }
    } finally { $base.Dispose() }
}

function Get-IrmNetFrameworkCompatibility {
    [CmdletBinding()]
    param([AllowNull()][object]$Release = (Get-IrmNetFrameworkRelease))
    $releaseNumber = 0
    $known = [int]::TryParse([string]$Release, [ref]$releaseNumber)
    $passed = $known -and $releaseNumber -ge 461808
    $detail = if ($passed) {
        ".NET Framework 4.7.2 trở lên đã có (Release=$releaseNumber), đáp ứng yêu cầu SQL Server 2022."
    } else {
        'SQL Server 2022 cần .NET Framework 4.7.2 trở lên. Hãy cài thành phần này trước rồi chạy lại bộ cài. Gói hiện tại không kèm bộ cài .NET Framework.'
    }
    return [pscustomobject]@{ Passed=$passed; Detail=$detail; Release=$releaseNumber; MinimumRelease=461808; MinimumVersion='4.7.2' }
}
