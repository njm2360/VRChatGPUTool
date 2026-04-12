#define AppName        "VRChatGPUTool"
#define AppVersion     "3.0.0"
#define AppExeName     "VRCGPUTool.exe"
#define ServiceExeName "NvidiaSmiProxy.exe"
#define ServiceName    "VRCGPUToolNvidiaSmiProxy"
#define ServiceDisplay "VRCGPUTool NvidiaSmi Proxy"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=njm2360
DefaultDirName={autopf64}\{#AppName}
DefaultGroupName={#AppName}
OutputDir=dist
OutputBaseFilename=VRCGPUTool-v{#AppVersion}-setup
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
Filename: "{sys}\sc.exe"; \
    Parameters: "create ""{#ServiceName}"" binPath= ""{app}\Service\{#ServiceExeName}"" DisplayName= ""{#ServiceDisplay}"" start= auto obj= LocalSystem"; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "サービスを登録しています..."

; サービス起動
Filename: "{sys}\sc.exe"; \
    Parameters: "start ""{#ServiceName}"""; \
    Flags: runhidden waituntilterminated; \
    StatusMsg: "サービスを起動しています..."

; インストール完了後にアプリを起動（任意）
Filename: "{app}\{#AppExeName}"; \
    Description: "{#AppName} を起動する"; \
    Flags: nowait postinstall skipifsilent runascurrentuser

[UninstallRun]
; サービスを停止（起動していない場合もエラーを無視して続行）
Filename: "{sys}\sc.exe"; \
    Parameters: "stop ""{#ServiceName}"""; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "StopService"

; 停止完了を少し待つ（SCM が非同期で止めるため）
Filename: "{sys}\timeout.exe"; \
    Parameters: "/t 3 /nobreak"; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "WaitServiceStop"

; サービス登録を削除
Filename: "{sys}\sc.exe"; \
    Parameters: "delete ""{#ServiceName}"""; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "DeleteService"
