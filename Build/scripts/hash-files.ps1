Param
(
    [parameter(Mandatory = $true)]    [string]
    $Version
)

$GitHubReleaseUrl = "https://github.com/isaacrlevin/presencelight/releases/download/Desktop-"

# Hash the Zip Files
mkdir .\Download

#https://github.com/isaacrlevin/presencelight/releases/download/Desktop-v5.6.101/PresenceLight.5.6.101-x86-zip.sha256
Invoke-WebRequest -Uri "${GitHubReleaseUrl}v${Version}\PresenceLight.${Version}-x86-zip.sha256" -OutFile ".\Download\x86-zip.sha256"
Invoke-WebRequest -Uri "${GitHubReleaseUrl}v${Version}\PresenceLight.${Version}-x64-zip.sha256" -OutFile ".\Download\x64-zip.sha256"
Invoke-WebRequest -Uri "${GitHubReleaseUrl}v${Version}\PresenceLight.${Version}-win-arm64-zip.sha256" -OutFile ".\Download\win-arm64-zip.sha256"
$hash86 = get-content ".\Download\x86-zip.sha256"
$hash64 = get-content ".\Download\x64-zip.sha256"
$hashARM = get-content ".\Download\win-arm64-zip.sha256"

# Update ChocolateyInstall.ps1
$installFile = Get-Content -path ".\Chocolatey\tools\ChocolateyInstall.ps1" -Raw
$installFile = $installFile -replace '{ReplaceCheckSumARM}', $hashARM
$installFile = $installFile -replace '{ReplaceCheckSumx86}', $hash86
$installFile = $installFile -replace '{ReplaceCheckSumx64}', $hash64
$installFile = $installFile -replace '{ARMLink}' , "${GitHubReleaseUrl}v${Version}/PresenceLight.${Version}-win-arm64.zip"
$installFile = $installFile -replace '{x86Link}' , "$GitHubReleaseUrl}v${Version}/PresenceLight.${Version}-x86.zip"
$installFile = $installFile -replace '{x64Link}' , "${GitHubReleaseUrl}v${Version}/PresenceLight.${Version}-x64.zip"
$installFile | Set-Content -Path ".\Chocolatey\tools\ChocolateyInstall.ps1"

# Update Verification.txt
$verificationFile = Get-Content -path ".\Chocolatey\tools\Verification.txt"
$verificationFile = $verificationFile -replace '{ARMLink}' , "${GitHubReleaseUrl}v${Version}/PresenceLight.${Version}-win-arm64.zip"
$verificationFile = $verificationFile -replace '{x64Link}' , "${GitHubReleaseUrl}v${Version}/PresenceLight.${Version}-x64.zip"
$verificationFile = $verificationFile -replace '{x86Link}' , "${GitHubReleaseUrl}v${Version}/PresenceLight.${Version}-x86.zip"
$verificationFile = $verificationFile -replace '{HASHx64}', $hash64
$verificationFile = $verificationFile -replace '{HASHx86}', $hash86
$verificationFile = $verificationFile -replace '{HASHARM}', $hashARM
$verificationFile | Set-Content -Path ".\Chocolatey\tools\Verification.txt"