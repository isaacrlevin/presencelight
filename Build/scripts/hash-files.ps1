Param
(
    [parameter(Mandatory = $true)]    [string]
    $Version
)

# Get the latest release from GitHub
$github = Invoke-RestMethod -uri "https://api.github.com/repos/isaacrlevin/presencelight/releases"

$targetRelease = $github | Where-Object -Property name -match "Desktop-v$Version" | Select-Object -First 1

mkdir .\Download
$fileNames = (, "x86", "x64", "win-arm64")

# Get the Hashes from the GitHub Release and save them to files
$hashUrls = $targetRelease | Select-Object -ExpandProperty assets -First 1 | Where-Object -Property name -match '.*?.zip.sha256' | Select-Object -ExpandProperty browser_download_url

foreach ($url in $hashUrls) {
    foreach ($fileName in $fileNames) {
        if ($url -like "*$fileName*") {
            $filePath = ".\Download\$fileName.zip.sha256"
            Invoke-WebRequest -Uri $url -OutFile $filePath
            break
        }
    }
}

$hash86 = get-content ".\Download\x86.zip.sha256"
$hash64 = get-content ".\Download\x64.zip.sha256"
$hashARM = get-content ".\Download\win-arm64.zip.sha256"

# Update ChocolateyInstall.ps1 with Hashes
$installFile = Get-Content -path ".\Chocolatey\tools\ChocolateyInstall.ps1" -Raw
$installFile = $installFile -replace '{ReplaceCheckSumARM}', $hashARM
$installFile = $installFile -replace '{ReplaceCheckSumx86}', $hash86
$installFile = $installFile -replace '{ReplaceCheckSumx64}', $hash64

# Update Verification.txt with Hashes
$verificationFile = Get-Content -path ".\Chocolatey\tools\Verification.txt"
$verificationFile = $verificationFile -replace '{HASHx64}', $hash64
$verificationFile = $verificationFile -replace '{HASHx86}', $hash86
$verificationFile = $verificationFile -replace '{HASHARM}', $hashARM

# Get the Download Urls for the Zip files and update ChocolateyInstall.ps1 and Verification.txt with Urls
$zipUrls = $targetRelease | Select-Object -ExpandProperty assets | Where-Object { $_.name -like '*.zip' } | Select-Object -ExpandProperty browser_download_url

foreach ($url in $zipUrls) {
    if ($url -like "*x64*") {
        $installFile = $installFile -replace '{x64Link}' , $url
        $verificationFile = $verificationFile -replace '{x64Link}' , $url
    }
    if ($url -like "*x86*") {
        $installFile = $installFile -replace '{x86Link}' , $url
        $verificationFile = $verificationFile -replace '{x86Link}' , $url
    }
    if ($url -like "*arm64*") {
        $installFile = $installFile -replace '{ARMLink}' , $url
        $verificationFile = $verificationFile -replace '{ARMLink}' , $url
    }
}

# Save the updated files
$verificationFile | Set-Content -Path ".\Chocolatey\tools\Verification.txt"
$installFile | Set-Content -Path ".\Chocolatey\tools\ChocolateyInstall.ps1"


