
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

# Install the latest wingetcreate exe
# Need to do things this way, see https://github.com/PowerShell/PowerShell/issues/13138
Import-Module Appx -UseWindowsPowerShell

# Download and install C++ Runtime framework package.
$vcLibsBundleFile = "$env:TEMP\Microsoft.VCLibs.Desktop.appx"
Invoke-WebRequest https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx -OutFile $vcLibsBundleFile
Add-AppxPackage $vcLibsBundleFile

# Download Winget-Create msixbundle, install, and execute update.
$appxBundleFile = "$env:TEMP\wingetcreate.msixbundle"
Invoke-WebRequest https://aka.ms/wingetcreate/latest/msixbundle -OutFile $appxBundleFile
Add-AppxPackage $appxBundleFile

# Create the PR
wingetcreate submit --token $Token $Version