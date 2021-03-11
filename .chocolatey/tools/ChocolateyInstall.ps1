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
$url_x86        = "https://github.com/isaacrlevin/presencelight/releases/download/Desktop-v4.0.1/PresenceLight.4.0.1-x86.zip"
$url_x64        = "https://github.com/isaacrlevin/presencelight/releases/download/Desktop-v4.0.1/PresenceLight.4.0.1-x64.zip"
$checksum_x86   = "579CEBA3771C0287C268D8D80E1C6FB2BA5D91CA6C87C1C0666540CA2D509A91"
$checksum_x64   = "1055A76F5E5500D0BC4D8617DF6928487558F55A6DF467D7EC5EFA47492C08BB"

# Detect Architecture automatically
# Respect user choice first 
$pp = Get-PackageParameters
$Is64Bit = [Environment]::Is64BitOperatingSystem
if ($pp['x86'] -eq 'true' -Or $Is64Bit -eq 'false')
{
  $url = $url_x86
  $checksum = $checksum_x86
}elseif ($pp['x64'] -eq 'true' -Or $Is64Bit -eq 'true') {
  $url = $url_x64
  $checksum = $checksum_x64
}else { # Default to 32 bit system
  $url = $url_x86
  $checksum = $checksum_x86
}

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  url           = $url
  checksum      = $checksum
  checksumType  = 'SHA256'
}

Install-ChocolateyZipPackage @packageArgs

$exePath = Join-Path $toolsDir 'PresenceLight.exe'


Write-Output "Adding shortcut to Start Menu"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk" -TargetPath $exePath

Write-Output "Adding shortcut to Startup"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk" -TargetPath $exePath

