$ErrorActionPreference = 'Stop'

Write-Output "Removing Install Directory"
$installPath = "C:\ProgramData\chocolatey\lib\PresenceLight"
Remove-Item –path $installPath –recurse -force

Write-Output "Removing shortcut from Start Menu"
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk"

Write-Output "Removing shortcut from Startup"
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk"