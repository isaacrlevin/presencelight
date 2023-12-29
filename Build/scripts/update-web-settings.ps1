Param
(
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

# Update AppSettings.json. This must be done before build.
$appsettings = get-content ".\src\PresenceLight.Web\appsettings.json" -raw | ConvertFrom-Json

$appsettings.AADSettings.clientId = $ApplicationId
$appsettings.AADSettings.clientSecret = $ClientSecret

$appsettings.appVersion = $GitBuildVersionSimple

$appsettings.applicationInsights.instrumentationkey = $InstrumentationKey
$appsettings | ConvertTo-Json -depth 32 | set-content '.\src\PresenceLight.Web\appsettings.json'

# Update PresenceLightSettings.json. This must be done before build.
$PresenceLightSettings = get-content ".\src\PresenceLight.Web\PresenceLightSettings.json" -raw | ConvertFrom-Json

$PresenceLightSettings.lightSettings.lifx.LIFXClientId = $LIFXClientId
$PresenceLightSettings.lightSettings.lifx.LIFXClientSecret = $LIFXClientSecret

$PresenceLightSettings.lightSettings.hue.RemoteHueClientId = $RemoteHueClientId
$PresenceLightSettings.lightSettings.hue.RemoteHueClientSecret = $RemoteHueClientSecret
$PresenceLightSettings.lightSettings.hue.RemoteHueClientAppName = $RemoteHueClientAppName

$PresenceLightSettings | ConvertTo-Json -depth 32 | set-content '.\src\PresenceLight.Web\PresenceLightSettings.json'