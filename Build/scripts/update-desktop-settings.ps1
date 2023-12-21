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
        # Update appxmanifest. This must be done before build.
        [xml]$manifest = get-content ".\src\DesktopClient\PresenceLight.Package\Package.appxmanifest"
        $manifest.Package.Identity.Version = "${Version}.0"
        $manifest.save(".\src\DesktopClient\PresenceLight.Package\Package.appxmanifest")

        # Update AppSettings.json. This must be done before build.
        $appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = "true"
        $appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'
    }
    "Nightly" {
        # Update appxmanifest. This must be done before build.
        [xml]$manifest = Get-Content ".\src\DesktopClient\PresenceLight.Package\Package-Nightly.appxmanifest"
        $manifest.Package.Identity.Version = "${Version}.0"
        $manifest.Save(".\src\DesktopClient\PresenceLight.Package\Package-Nightly.appxmanifest")

        # Update AppSettings.json. This must be done before build.
        $appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = "true"
        $appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'
    }
    "Standalone" {
        # Update AppSettings.json. This must be done before build.
        $appsettings = get-content ".\src\DesktopClient\PresenceLight\appsettings.json" -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = "false"
        $appsettings | ConvertTo-Json -depth 32 | set-content '.\src\DesktopClient\PresenceLight\appsettings.json'
    }
}


# Update AppSettings.json. This must be done before build.
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