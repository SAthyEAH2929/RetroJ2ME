#define MyAppName "RetroJ2ME"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "RetroJ2ME"

[Setup]
AppId={{B79F4D26-8C31-4A2E-9F65-2D7A1C83E640}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\RetroJ2ME
DefaultGroupName={#MyAppName}
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\RetroJ2ME.Desktop.exe
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
MinVersion=10.0
OutputDir=publish\installer
OutputBaseFilename=RetroJ2ME-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=src\RetroJ2ME.Desktop\Assets\RetroJ2ME-App-Icon.ico
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\RetroJ2ME"; Filename: "{app}\RetroJ2ME.Desktop.exe"
Name: "{autodesktop}\RetroJ2ME"; Filename: "{app}\RetroJ2ME.Desktop.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\RetroJ2ME.Desktop.exe"; Description: "Launch RetroJ2ME"; Flags: postinstall nowait skipifsilent
