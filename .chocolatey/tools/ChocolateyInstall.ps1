$ErrorActionPreference  = 'Stop';


$WindowsVersion=[Environment]::OSVersion.Version
if ($WindowsVersion.Major -ne "10") {
  throw "This package requires Windows 10."
}

$IsCorrectBuild=[Environment]::OSVersion.Version.Build
if ($IsCorrectBuild -lt "17134") {
  throw "This package requires at least Windows 10 version build 17134.x."
}

$version        = "{ReplaceVersion}"

$packageName    = "presencelight"
$toolsDir       = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url_x86        = "https://github.com/isaacrlevin/PresenceLight/releases/download/v${version}/PresenceLight.${version}-x32.zip"
$url_x64        = "https://github.com/isaacrlevin/PresenceLight/releases/download/v${version}/PresenceLight.${version}-x64.zip"
$checksum_x86   = "{ReplaceCheckSumx86}"
$checksum_x64   = "{ReplaceCheckSumx64}"

# By default, we want 32-bit installer
$url = $url_x86
$checksum = $checksum_x86

# If specifically asked for 64-bit, we use that
$pp = Get-PackageParameters
if ($pp['x64'] -eq 'true')
{
  $url = $url_x64
  $checksum = $checksum_x64
}

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  url           = $url
  checksum      = $checksum
  checksumType  = 'SHA512'
}

Install-ChocolateyZipPackage @packageArgs

$installPath    = Join-Path $toolsDir 'PresenceLight'
$exePath = Join-Path installPath 'PresenceLight.exe'


Write-Output "Adding shortcut to Start Menu"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk" -TargetPath $exePath

Write-Output "Adding shortcut to Startup"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk" -TargetPath $exePath
