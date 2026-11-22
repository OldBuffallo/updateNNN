$ErrorActionPreference = 'SilentlyContinue'
Stop-Service IRM -Force
sc.exe delete IRM | Out-Null
Unregister-ScheduledTask -TaskName 'IRM-DailyBackup' -Confirm:$false
Remove-NetFirewallRule -DisplayName 'IRM HTTPS LAN'
Remove-Item (Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'IRM.lnk') -Force
Remove-Item (Join-Path ([Environment]::GetFolderPath('CommonPrograms')) 'IRM.lnk') -Force
Write-Host 'Đã gỡ dịch vụ IRM. Database và dữ liệu tại C:\ProgramData\IRM được giữ lại để có thể phục hồi.'
