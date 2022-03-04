
Param
(
    [parameter(Mandatory = $true)]
    [string]
    $Version,
    [parameter(Mandatory = $false)]
    [string]
    $Token
)

function Get-HashForArchitecture {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $Architecture,
        [parameter(Mandatory = $true)]
        [string]
        $Version
    )
    Invoke-WebRequest -Uri "https://github.com/isaacrlevin/presencelight/releases/download/Desktop-v$Version/PresenceLight.$Version-$Architecture.zip"  -OutFile "download$Architecture.zip"
    $hash=Get-Filehash "download$Architecture.zip"
   
    return $hash
}

function Write-MetaData {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $FileName,
        [parameter(Mandatory = $true)]
        [string]
        $Version,
        [parameter(Mandatory = $true)]
        [string]
        $HashAmd64,
        [parameter(Mandatory = $true)]
        [string]
        $HashArm64
    )
    $content = Get-Content $FileName -Raw
    $content = $content.Replace('<VERSION>', $Version)
    $content = $content.Replace('<HASH-AMD64>', $HashAmd64)
    $content = $content.Replace('<HASH-ARM64>', $HashArm64)
    $date = Get-Date -Format "yyyy-MM-dd"
    $content = $content.Replace('<DATE>', $date)
    $content | Out-File -Encoding 'UTF8' "./$Version/$FileName"
}

New-Item -Path $PWD -Name $Version -ItemType "directory"
# Get all files inside the folder and adjust the version/hash
$HashAmd64 = Get-HashForArchitecture -Architecture 'x64' -Version $Version
$HashArm64 = Get-HashForArchitecture -Architecture 'win-arm64' -Version $Version
Get-ChildItem '*.yaml' | ForEach-Object -Process {
    Write-MetaData -FileName $_.Name -Version $Version -HashAmd64 $HashAmd64 -HashArm64 $HashArm64
}
if (-not $Token) {
    return
}
# Get the latest wingetcreate exe
Invoke-WebRequest 'https://aka.ms/wingetcreate/latest/self-contained' -OutFile wingetcreate.exe
# Create the PR
./wingetcreate.exe submit --token $Token $Version