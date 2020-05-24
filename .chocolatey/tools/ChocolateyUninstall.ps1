$ErrorActionPreference = 'Stop'
Write-Output "Removing shortcut from Start Menu"
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk"

Write-Output "Removing shortcut from Startup"
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk"

Write-Output "Removing Install Directory"
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$installPath = Join-Path $toolsDir 'PresenceLight'
Remove-Item –path $installPath –recurse -force