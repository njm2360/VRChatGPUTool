!include "MUI2.nsh"
!include "nsDialogs.nsh"
!include "LogicLib.nsh"

Name "VRChatGPUTool"
OutFile "VRCChatGPUTool_Installer.exe"
Unicode True

InstallDir "$LOCALAPPDATA\VRChatGPUTool"
InstallDirRegKey HKCU "Software\VRChatGPUTool" ""

SetCompressor /SOLID lzma
SetDatablockOptimize ON

!define MUI_ABORTWARNING

!define MUI_ICON "VRCGPUTool\VRCGPUTool.ico"
!define MUI_UNICON "VRCGPUTool\VRCGPUTool.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!define REGPATH_UNINSTSUBKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\VRChatGPUTool"

!insertmacro MUI_LANGUAGE "Japanese"

Section "VRChatGPUTool" SecProgram
  SectionIn RO
  SetOutPath "$INSTDIR"

  File "VRCGPUTool\bin\Release\net472\publish\VRCGPUTool.exe"
  File "VRCGPUTool\bin\Release\net472\publish\Microsoft.Bcl.AsyncInterfaces.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Buffers.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Memory.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Numerics.Vectors.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Runtime.CompilerServices.Unsafe.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Text.Encodings.Web.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Text.Json.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.Threading.Tasks.Extensions.dll"
  File "VRCGPUTool\bin\Release\net472\publish\System.ValueTuple.dll"
  File "VRCGPUTool\bin\Release\net472\publish\VRCGPUTool.exe.config"
  File "VRCGPUTool\bin\Release\net472\publish\VRCGPUTool.pdb"

  CreateDirectory "$SMPROGRAMS\VRChatGPUTool"

  CreateShortcut "$SMPROGRAMS\VRChatGPUTool\VRChatGPUTool.lnk" "$INSTDIR\VRCGPUTool.exe" ""
  CreateShortcut "$DESKTOP\VRChatGPUTool.lnk" "$INSTDIR\VRCGPUTool.exe" ""

  WriteRegStr HKCU "Software\VRChatGPUTool" "" $INSTDIR

  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "DisplayName" "VRChatGPUTool"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "Publisher" "njm2360"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "Readme" "https://github.com/njm2360/VRChatGPUTool"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "URLUpdateInfo" "https://github.com/njm2360/VRChatGPUTool/releases"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "URLInfoAbout" "https://github.com/njm2360/VRChatGPUTool"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "HelpLink" "https://twitter.com/njm2360"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "Comments" "VRChatå¸ÇØGPUìdóÕêßå¿ÉcÅ[Éã"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "DisplayIcon" "$INSTDIR\VRChatGPUTool.exe,0"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "QuietUninstallString" '"$INSTDIR\Uninstall.exe" /S'
  WriteRegDWORD HKCU "${REGPATH_UNINSTSUBKEY}" "EstimatedSize" 1120
  WriteRegDWORD HKCU "${REGPATH_UNINSTSUBKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${REGPATH_UNINSTSUBKEY}" "NoRepair" 1
SectionEnd

Section "Uninstall"
  Delete "$INSTDIR\VRCGPUTool.exe"
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$INSTDIR\Microsoft.Bcl.AsyncInterfaces.dll"
  Delete "$INSTDIR\System.Buffers.dll"
  Delete "$INSTDIR\System.Memory.dll"
  Delete "$INSTDIR\System.Numerics.Vectors.dll"
  Delete "$INSTDIR\System.Runtime.CompilerServices.Unsafe.dll"
  Delete "$INSTDIR\System.Text.Encodings.Web.dll"
  Delete "$INSTDIR\System.Text.Json.dll"
  Delete "$INSTDIR\System.Threading.Tasks.Extensions.dll"
  Delete "$INSTDIR\System.ValueTuple.dll"
  Delete "$INSTDIR\VRCGPUTool.exe.config"
  Delete "$INSTDIR\VRCGPUTool.pdb"

  Delete "$SMPROGRAMS\VRChatGPUTool\VRChatGPUTool.lnk"
  Delete "$DESKTOP\VRChatGPUTool.lnk"
  RMDir "$SMPROGRAMS\VRChatGPUTool"

  DeleteRegKey HKCU "${REGPATH_UNINSTSUBKEY}"
SectionEnd
