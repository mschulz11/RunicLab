; Inno Setup script for RunicLab (Windows, unpackaged WinUI/MAUI build)
; Build output: dotnet publish -f net9.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained true
;               -p:TargetFrameworks=net9.0-windows10.0.19041.0 -p:WindowsPackageType=None
;               -p:WindowsAppSDKSelfContained=true -o ..\dist\RunicLab-win-x64
;
; Compile:  "C:\Users\<user>\AppData\Local\Programs\Inno Setup 6\ISCC.exe" RunicLab.iss

#define AppName        "RunicLab"
#define AppPublisher   "mschulz11"
#define AppURL         "https://github.com/mschulz11/RunicLab"
#define AppExeName     "RunicLab.exe"

; Von außen überschreibbar: ISCC.exe RunicLab.iss /DAppVersion=1.1
#ifndef AppVersion
  #define AppVersion   "1.0"
#endif

#ifndef PublishDir
  #define PublishDir   "..\..\dist\RunicLab-win-x64"
#endif

#ifndef OutputDir
  #define OutputDir    "..\..\dist"
#endif

[Setup]
AppId={{64FA7B27-FA70-478B-9109-F51D699300E2}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE.txt
OutputDir={#OutputDir}
OutputBaseFilename={#AppName}-Setup-{#AppVersion}-win-x64
SetupIconFile={#PublishDir}\appicon.ico
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
CloseApplications=yes
RestartApplications=no
; Code signing (optional): uncomment and point to your cert
; SignTool=signtool
; SignedUninstaller=yes

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
