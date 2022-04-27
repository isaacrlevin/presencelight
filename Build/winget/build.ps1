
Param
(
    [parameter(Mandatory = $true)]
    [string]
    $Version,
    [parameter(Mandatory = $false)]
    [string]
    $Token
)

function Get-Hash {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $Version
    )
    $hash = (new-object Net.WebClient).DownloadString("https://github.com/isaacrlevin/presencelight/releases/download/Desktop-v$Version/appx.sha256")
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
        $Hash
    )
    $content = Get-Content $FileName -Raw
    $content = $content.Replace('<VERSION>', $Version)
    $content = $content.Replace('<HASH>', $Hash)
    $date = Get-Date -Format "yyyy-MM-dd"
    $content = $content.Replace('<DATE>', $date)
    $content | Out-File -Encoding 'UTF8' "./$Version/$FileName"
}

New-Item -Path $PWD -Name $Version -ItemType "directory"
# Get all files inside the folder and adjust the version/hash
$Hash = Get-Hash -Version $Version
Get-ChildItem '*.yaml' | ForEach-Object -Process {
    Write-MetaData -FileName $_.Name -Version $Version -Hash $Hash
}
if (-not $Token) {
    return
}
# Get the latest wingetcreate exe
Invoke-WebRequest 'https://aka.ms/wingetcreate/latest/self-contained' -OutFile wingetcreate.exe
# Create the PR
./wingetcreate.exe submit --token $Token $Version
