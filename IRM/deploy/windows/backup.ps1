$ErrorActionPreference = 'Stop'
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$appRoot = Join-Path $env:ProgramData 'IRM\app'
Push-Location $appRoot
try {
    & (Join-Path $appRoot 'IRM.exe') --backup
    if ($LASTEXITCODE -ne 0) { throw "Backup IRM thất bại (mã $LASTEXITCODE)." }
}
finally {
    Pop-Location
}
