$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$installPath = Join-Path $toolsDir 'PresenceLight'
$exePath = Join-Path $toolsDir 'PresenceLight\PresenceLight.exe'
$zipPath = Join-Path $toolsDir 'Release.zip'

Install-ChocolateyZipPackage -PackageName "$packageName" `
                             -Url "$zipPath" `
                             -UnzipLocation "$installPath"

Write-Output "Adding shortcut to Start Menu"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk" -TargetPath $exePath

Write-Output "Adding shortcut to Startup"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk" -TargetPath $exePath