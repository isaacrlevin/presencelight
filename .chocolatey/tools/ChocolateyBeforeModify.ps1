$InstallDir = Join-Path $env:APPDATA 'PresenceLight'
$SettingsPath = Join-Path $InstallDir 'appsettings.json'

if (Test-Path -Path $SettingsPath -PathType Leaf)
{
    $TempSettingsPath Join-Path $InstallDir 'PresenceLightSettings.json'
    Remove-Item -Path $TempSettingsPath -Recurse
    
    Get-ChildItem -path $SettingsPath | 
    Copy-Item -Destination $env:TEMP -PassThru | Rename-Item -NewName  'PresenceLightSettings.json'
}