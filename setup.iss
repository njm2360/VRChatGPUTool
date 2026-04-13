#define AppName        "VRChatGPUTool"
#define AppVersion     "3.0.0"
#define AppExeName     "VRChatGPUTool.exe"
#define ServiceExeName "NvidiaSmiProxy.exe"
#define ServiceName    "VRCGPUToolNvidiaSmiProxy"
#define ServiceDisplay "VRCGPUTool NvidiaSmi Proxy"

[Setup]
AppId={{e94c57b2-b490-46d5-9c46-5f6a0517c973}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName}
AppPublisher=njm2360
DefaultDirName={autopf64}\{#AppName}
DefaultGroupName={#AppName}
OutputDir=dist
OutputBaseFilename=VRChatGPUTool-v{#AppVersion}-setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExeName}
CloseApplications=no

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "デスクトップにショートカットを作成"; GroupDescription: "追加タスク:"

[Files]
Source: "VRCGPUTool\bin\Release\net10.0-windows\win-x64\publish\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "NvidiaSmiProxy\bin\Release\net10.0-windows\win-x64\publish\{#ServiceExeName}"; DestDir: "{app}\Service"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}";                    Filename: "{app}\{#AppExeName}"
Name: "{group}\{#AppName} のアンインストール"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";              Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
; Always create — existing service removed in [Code] before install
Filename: "{sys}\sc.exe"; \
    Parameters: "create ""{#ServiceName}"" binPath= ""{app}\Service\{#ServiceExeName}"" DisplayName= ""{#ServiceDisplay}"" start= auto obj= LocalSystem"; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "サービスを登録しています..."

Filename: "{sys}\sc.exe"; \
    Parameters: "start ""{#ServiceName}"""; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "サービスを起動しています..."

Filename: "{app}\{#AppExeName}"; \
    Description: "{#AppName} を起動する"; \
    Flags: nowait postinstall skipifsilent runascurrentuser

[UninstallRun]
Filename: "{sys}\sc.exe"; \
    Parameters: "stop ""{#ServiceName}"""; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "StopService"

; Wait for SCM to stop the service asynchronously
Filename: "{sys}\timeout.exe"; \
    Parameters: "/t 3 /nobreak"; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "WaitServiceStop"

Filename: "{sys}\sc.exe"; \
    Parameters: "delete ""{#ServiceName}"""; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "DeleteService"

Filename: "{sys}\reg.exe"; \
    Parameters: "delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v ""{#AppName}"" /f"; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "DeleteStartupReg"

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssInstall then
  begin
    // Kill app to avoid file lock during upgrade
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/IM "{#AppExeName}" /F', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);

    // Stop and delete existing service so sc create in [Run] always succeeds
    Exec(ExpandConstant('{sys}\sc.exe'), 'stop "{#ServiceName}"', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(2000);
    Exec(ExpandConstant('{sys}\sc.exe'), 'delete "{#ServiceName}"', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1000);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/IM "{#AppExeName}" /F', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(500);

    if MsgBox('設定ファイルを削除しますか？', mbConfirmation, MB_YESNO) = IDYES then
      DelTree(ExpandConstant('{localappdata}\{#AppName}'), True, True, True);
  end;
end;
