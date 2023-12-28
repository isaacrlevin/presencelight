
Param
(
    [parameter(Mandatory = $true)]
    [string]
    $Version,
    [parameter(Mandatory = $false)]
    [string]
    $Token
)

$github = Invoke-RestMethod -uri "https://api.github.com/repos/isaacrlevin/presencelight/releases"
$targetRelease = $github | Where-Object -Property name -match "Desktop-v$Version" | Select-Object -First 1

$installerUrl = $targetRelease | Select-Object -ExpandProperty assets | Where-Object { $_.name -like '*.appxbundle' } | Select-Object -ExpandProperty browser_download_url
# Update package using wingetcreate
Invoke-WebRequest https://aka.ms/wingetcreate/latest -OutFile wingetcreate.exe
.\wingetcreate.exe update "isaaclevin.presencelight" --version $Version --urls "$installerUrl" --submit --token $Token