Param
(
    [parameter(Mandatory = $true)]    [string]
    $Release,

    [parameter(Mandatory = $true)]    [string]
    $Version,

    [parameter(Mandatory = $true)]
    [string]
    $ApplicationId,

    [parameter(Mandatory = $true)]
    [string]
    $ClientSecret,

    [parameter(Mandatory = $true)]
    [string]
    $InstrumentationKey,

    [parameter(Mandatory = $true)]
    [string]
    $LIFXClientId,

    [parameter(Mandatory = $true)]
    [string]
    $LIFXClientSecret,

    [parameter(Mandatory = $true)]
    [string]
    $RemoteHueClientId,

    [parameter(Mandatory = $true)]
    [string]
    $RemoteHueClientSecret,

    [parameter(Mandatory = $true)]
    [string]
    $RemoteHueClientAppName
)


switch ($Release) {
    "Release" {
        Write-Host "Updating AppxManifest for Release"
        [xml]$manifest = get-content ".\src\DesktopClient\PresenceLight.Package\Package.appxmanifest"
        $manifest.Package.Identity.Version = "${Version}.0"
        $manifest.save(".\src\DesktopClient\PresenceLight.Package\Package.appxmanifest")

        Write-Host "Updating AppSettings for Release"
        $appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = "true"
        $appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'
    }
    "Nightly" {
        Write-Host "Updating AppxManifest for Nightly"
        [xml]$manifest = Get-Content ".\src\DesktopClient\PresenceLight.Package\Package-Nightly.appxmanifest"
        Write-Host "Setting Version to ${Version}.0"
        $manifest.Package.Identity.Version = "${Version}.0"
        Write-Host $manifest.Package.Identity.Version
        $manifest.Save(".\src\DesktopClient\PresenceLight.Package\Package-Nightly.appxmanifest")

        Write-Host "Updating AppSettings for Nightly"
        $appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = "true"
        $appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'
    }
    "Standalone" {
        Write-Host "Updating AppSettings for Standalone"
        $appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = "false"
        $appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'
    }
}


Write-Host "Updating AppSettings for All Channels"
$appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
$appsettings.aadSettings.clientId = "${ApplicationId}"
$appsettings.appVersion = "${Version}"
$appsettings.lightSettings.lifx.LIFXClientId = "${LIFXClientId}"
$appsettings.lightSettings.lifx.LIFXClientSecret = "${LIFXClientSecret}"
$appsettings.applicationInsights.instrumentationkey = "${InstrumentationKey}"
$appsettings.lightSettings.hue.RemoteHueClientId = "${RemoteHueClientId}"
$appsettings.lightSettings.hue.RemoteHueClientSecret = "${RemoteHueClientSecret}"
$appsettings.lightSettings.hue.RemoteHueClientAppName = "${RemoteHueClientAppName}"
$appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'