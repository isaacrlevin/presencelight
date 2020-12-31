$matrices = @()
$Release = New-Object System.Object
$Release | Add-Member -type NoteProperty -name ChannelName -Value "Release"

$Nightly = New-Object System.Object
$Nightly | Add-Member -type NoteProperty -name ChannelName -Value "Nightly"

$Standalone = New-Object System.Object
$Standalone | Add-Member -type NoteProperty -name ChannelName -Value "Standalone"

$matrices += $Release
$matrices += $Standalone, $Nightly

$global:GitBuildVersionSimple = "3.5.14"
$global:VersionNumber = $($GitBuildVersionSimple)
$global:env = New-Object System.Object
$global:github = New-Object System.Object
$github | Add-Member -type NoteProperty -name workspace -Value "C:\temp\"

$DOTNET_CLI_TELEMETRY_OPTOUT = 1
$DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$DOTNET_NOLOGO = $true
$BuildConfiguration = "Release"
$ACTIONS_ALLOW_UNSECURE_COMMANDS = $true

if (Test-Path $github.workspace -PathType Container)
{
    Remove-Item -Recurse -Force $github.workspace
}
git clone https://github.com/isaacrlevin/PresenceLight.git $github.workspace
Set-Location $github.workspace

foreach ($matrix in $matrices) {
    # name: Create Directory for Channel
    $channelDir = $($github.workspace) + $($matrix.ChannelName)
    $ChannelName = $matrix.ChannelName
    mkdir $channelDir

    #dotnet tool install --tool-path . nbgv
    #nbgv cloud -c -a


    Write-Output "BuildNumber - $GitBuildVersionSimple"

    #Update AppxManifest version
    # Update appxmanifest. This must be done before build.
    if ($matrix.ChannelName  -eq 'Release') {
        $manifestFile= $github.workspace + ".\src\DesktopClient\PresenceLight.Package\Package.appxmanifest"
        [xml]$manifest = get-content $manifestFile
        $manifest.Package.Identity.Version = "$GitBuildVersionSimple.0"
        $manifest.save($manifestFile)
    }

    #Update Nightly AppxManifest version
    # Update appxmanifest. This must be done before build.
    if ($matrix.ChannelName  -eq 'Nightly') {
        $manifestFile= $github.workspace + ".\src\DesktopClient\PresenceLight.Package\Package-Nightly.appxmanifest"
        [xml]$manifest = get-content $manifestFile
        $manifest.Package.Identity.Version = "$GitBuildVersionSimple.0"
        $manifest.save($manifestFile)
    }

    #Update appsettings.json for Standalone Version
    # Update AppSettings.json. This must be done before build.
    if ($matrix.ChannelName  -eq 'Standalone') {
        $appSettingsFile = $github.workspace + ".\src\DesktopClient\PresenceLight\appsettings.json"
        $appsettings = get-content $appsettingsFile -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = $false
        $appsettings | ConvertTo-Json -depth 32 | set-content $appSettingsFile
    }

    #Update appsettings.json for AppPackage Version
    # Update AppSettings.json. This must be done before build.
    if ($matrix.ChannelName -ne 'Standalone') {
        $appSettingsFile = $github.workspace + ".\src\DesktopClient\PresenceLight\appsettings.json"
        $appsettings = get-content $appsettingsFile -raw | ConvertFrom-Json
        $appsettings.isAppPackaged = $true
        $appsettings | ConvertTo-Json -depth 32 | set-content $appSettingsFile
    }

    #Add Secrets to appsettings.json
    # Update AppSettings.json. This must be done before build.
    $appSettingsFile = $github.workspace + ".\src\DesktopClient\PresenceLight\appsettings.json"
    $appsettings = get-content $appSettingsFile -raw | ConvertFrom-Json
    # $appsettings.aadSettings.clientId = "$secrets.ApplicationId "
    # $appsettings.lightSettings.lifx.LIFXClientId = "$secrets.LIFXClientId "
    # $appsettings.lightSettings.lifx.LIFXClientSecret = "$secrets.LIFXClientSecret "
    # $appsettings.applicationInsights.instrumentationkey = "$secrets.InstrumentationKey "
    # $appsettings.lightSettings.hue.RemoteHueClientId = "$secrets.RemoteHueClientId "
    # $appsettings.lightSettings.hue.RemoteHueClientSecret = "$secrets.RemoteHueClientSecret "
    # $appsettings.lightSettings.hue.RemoteHueClientAppName = "$secrets.RemoteHueClientAppName "
    $appsettings | ConvertTo-Json -depth 32 | set-content $appSettingsFile

    #Update Badge Versions
    # Update badges
    $buildFolder = $channelDir + "\Build\"
    mkdir $buildFolder

    $ci_badgeFile=$channelDir + "\Build\ci_badge.svg"
    [xml]$badge = Get-Content ".\Build\ci_badge.svg"
    $badge.svg.g[1].text[2].InnerText = "$GitBuildVersionSimple.0"
    $badge.svg.g[1].text[3].InnerText = "$GitBuildVersionSimple.0"
    $badge.Save($ci_badgeFile)

    $store_badgeFile=$channelDir + "\Build\store_badge.svg"
    [xml]$badge = Get-Content ".\Build\store_badge.svg"
    $badge.svg.g[1].text[2].InnerText = "$GitBuildVersionSimple.0"
    $badge.svg.g[1].text[3].InnerText = "$GitBuildVersionSimple.0"
    $badge.Save($store_badgeFile)

    #Build Standalone Presence Light x64
    if ($matrix.ChannelName  -eq 'Standalone') {
        $csprojFile=$github.workspace + ".\src\DesktopClient\PresenceLight\PresenceLight.csproj"
        dotnet publish $csprojFile  -c $BuildConfiguration  /p:PublishProfile=Properties/PublishProfiles/WinX64.pubxml
    }

    #Build Standalone Presence Light x86
    if ($matrix.ChannelName  -eq 'Standalone') {
        $csprojFile=$github.workspace + ".\src\DesktopClient\PresenceLight\PresenceLight.csproj"
        dotnet publish $csprojFile -c $BuildConfiguration  /p:PublishProfile=Properties/PublishProfiles/WinX86.pubxml
    }

    #Zip Standalone PresenceLight x64 Files
    if ($matrix.ChannelName  -eq 'Standalone') {
        $archivePath=$github.workspace + ".\src\DesktopClient\PresenceLight\bin\" + $BuildConfiguration `
        + "\net5.0-windows10.0.19041\win-x64\publish\*"
        $zipFile=$channelDir + "\PresenceLight.$GitBuildVersionSimple-x64.zip"
        Compress-Archive `
        -Path $archivePath  `
        -DestinationPath $zipFile
    }

    #Zip Standalone PresenceLight x86 Files
    if ($matrix.ChannelName  -eq 'Standalone') {
        $archivePath=$github.workspace + ".\src\DesktopClient\PresenceLight\bin\" + $BuildConfiguration `
        + "\net5.0-windows10.0.19041\win-x86\publish\*"
        $zipFile=$channelDir + "\PresenceLight.$GitBuildVersionSimple-x86.zip"
        Compress-Archive `
        -Path $archivePath  `
        -DestinationPath $zipFile
    }

    #Build Appx Package
    if ($matrix.ChannelName  -ne 'Standalone') {
        $packagePath=$github.workspace + ".\src\DesktopClient\PresenceLight.Package\PresenceLight.Package.wapproj"
        $slnPath = $github.workspace + ".\src\PresenceLight.sln"
        $packageDir=$channelDir
        dotnet restore $slnPath
        msbuild $packagePath `
        /p:ChannelName="$ChannelName"  /p:configuration=$BuildConfiguration `
        /p:IncludeSymbols=true /p:AppxPackageDir=$packageDir /bl
    }
}