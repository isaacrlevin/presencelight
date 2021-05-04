$ErrorActionPreference  = 'Stop';

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
$InstallDir = Join-Path $env:APPDATA 'PresenceLight'
$url_x86        = "{x86Link}"
$url_x64        = "{x64Link}"
$checksum_x86   = "{ReplaceCheckSumx86}"
$checksum_x64   = "{ReplaceCheckSumx64}"

# Detect Architecture automatically
# Respect user choice first 
$pp = Get-PackageParameters
$Is64Bit = [Environment]::Is64BitOperatingSystem
if ($pp['x86'] -eq 'true')
{
  $url = $url_x86
  $checksum = $checksum_x86
}elseif ($pp['x64'] -eq 'true') {
  $url = $url_x64
  $checksum = $checksum_x64
}elseif ($Is64Bit -eq 'true') {
  $url = $url_x64
  $checksum = $checksum_x64
}else { # Default to 32 bit system
  $url = $url_x86
  $checksum = $checksum_x86
}

if($pp['InstallDir']){
  $InstallDir = $pp['InstallDir']
}

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $InstallDir
  url           = $url
  checksum      = $checksum
  checksumType  = 'SHA256'
}

Install-ChocolateyZipPackage @packageArgs

$exePath = Join-Path $InstallDir 'PresenceLight.exe'


Write-Output "Adding shortcut to Start Menu"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk" -TargetPath $exePath -WorkingDirectory $InstallDir

Write-Output "Adding shortcut to Startup"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk" -TargetPath $exePath -WorkingDirectory $InstallDir

