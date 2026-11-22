<# SSH helper. Credentials are read from environment variables and never stored in source. #>
param([string]$Command = "echo CONNECTED && uname -a")

$targetHost = $env:IRM_VPS_HOST
$targetUser = if ($env:IRM_VPS_USER) { $env:IRM_VPS_USER } else { "ubuntu" }
$keyPath = $env:IRM_VPS_KEY_PATH

if ([string]::IsNullOrWhiteSpace($targetHost)) {
    throw "Set IRM_VPS_HOST before running this helper."
}

$arguments = @("-o", "BatchMode=yes", "-o", "StrictHostKeyChecking=yes")
if ($keyPath) { $arguments += @("-i", $keyPath) }
$arguments += @("$targetUser@$targetHost", $Command)
& ssh @arguments
exit $LASTEXITCODE
