[Setup]
AppName=SleepReminder
AppVersion=1.0.0
AppPublisher=SleepReminder
AppPublisherURL=https://github.com/SleepReminder
DefaultDirName={autopf}\SleepReminder
DefaultGroupName=SleepReminder
UninstallDisplayIcon={app}\SleepReminder.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.
OutputBaseFilename=SleepReminder_Setup
PrivilegesRequired=admin
SetupIconFile=
WizardStyle=modern

[Languages]
Name: "chinesesimplified"; MessagesFile: "ChineseSimplified.isl"

[Tasks]
Name: desktopicon; Description: "创建桌面快捷方式"; GroupDescription: "快捷方式："
Name: autostart; Description: "开机自动启动"; GroupDescription: "其他选项："

[Files]
Source: "publish\SleepReminder.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\SleepReminder"; Filename: "{app}\SleepReminder.exe"; Parameters: ""
Name: "{group}\卸载 SleepReminder"; Filename: "{uninstallexe}"
Name: "{autodesktop}\SleepReminder"; Filename: "{app}\SleepReminder.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "SleepReminder"; ValueData: """{app}\SleepReminder.exe"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\SleepReminder.exe"; Parameters: "--first-run"; Description: "运行 SleepReminder"; Flags: postinstall nowait skipifsilent
