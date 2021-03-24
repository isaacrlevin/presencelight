$light = Get-process presencelight*

if($light){
	$light | Stop-Process -Force
}

Remove-Item $Env:AppData\PresenceLight -Recurse -Force
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PresenceLight.lnk"
Remove-Item "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\PresenceLight.lnk"