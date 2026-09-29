#ifndef StageDir
  #error StageDir is required
#endif
#ifndef OutputDir
  #error OutputDir is required
#endif

[Setup]
AppId={{A7A7AA33-A680-4E29-93C4-9F4720376A6B}
AppName=Immigration Report Manager
AppVersion=1.0.2
AppPublisher=IRM
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
CreateAppDir=no
DisableProgramGroupPage=yes
DisableReadyPage=yes
DisableFinishedPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=IRM-v1.0.2-windows-x64-offline-setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
Uninstallable=no

[Files]
Source: "{#StageDir}\*"; DestDir: "{tmp}\irm-v1.0.2"; Flags: recursesubdirs createallsubdirs deleteafterinstall

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  PowerShell: String;
  Parameters: String;
begin
  if CurStep = ssPostInstall then
  begin
    WizardForm.StatusLabel.Caption := 'Đang mở trình cài đặt IRM offline...';
    PowerShell := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
    Parameters := '-NoProfile -ExecutionPolicy Bypass -File "' +
      ExpandConstant('{tmp}\irm-v1.0.2\installer-ui.ps1') + '" -PayloadRoot "' +
      ExpandConstant('{tmp}\irm-v1.0.2') + '"';
    if not Exec(PowerShell, Parameters, '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      RaiseException('Không thể khởi chạy quy trình cài đặt IRM.');
    if ResultCode <> 0 then
      RaiseException(Format('Cài đặt IRM thất bại (mã %d). Xem log để biết chi tiết.', [ResultCode]));
  end;
end;
