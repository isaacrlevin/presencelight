$ErrorActionPreference  = 'Stop';

# Make sure to kill any presencelight processes before attempting an
# installation. This covers the case that PresenceLight is currently
# installed outside of Chocolatey
Get-Process presencelight* -ErrorAction SilentlyContinue | Stop-Process

$WindowsVersion=[Environment]::OSVersion.Version
if ($WindowsVersion.Major -ne "10") {
  throw "This package requires Windows 10."
}

$IsCorrectBuild=[Environment]::OSVersion.Version.Build
if ($IsCorrectBuild -lt "17134") {
  throw "This package requires at least Windows 10 version build 17134.x."
}

$packageName    = "presencelight"
$toolsDir       = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$InstallDir     = Join-Path $env:APPDATA 'PresenceLight'

$pp = Get-PackageParameters

if($pp['InstallDir']){
  $InstallDir = $pp['InstallDir']
}

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $InstallDir
  urlARM         = "{ARMLink}"
  url86bit       = "{x86Link}"
  url64bit       = "{x64Link}"
  checksumARM    = "{ReplaceCheckSumARM}"
  checksum       = "{ReplaceCheckSumx86}"
  checksum64     = "{ReplaceCheckSumx64}"
  checksumType   = 'SHA256'
}

Install-ChocolateyZipPackage @packageArgs

$exePath = Join-Path $InstallDir 'PresenceLight.exe'


Write-Output "Adding shortcut to Start Menu"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk" -TargetPath $exePath -WorkingDirectory $InstallDir

Write-Output "Adding shortcut to Startup"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk" -TargetPath $exePath -WorkingDirectory $InstallDir
