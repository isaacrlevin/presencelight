![Logo](Icon.png)
# PresenceLight
![Build Status](https://dev.azure.com/isaaclevin/PresenceLight/_apis/build/status/CI-build-wpf?branchName=master)

### [Cross Platform Blazor Version](worker-README.md)

### WPF Installs

| Release Channel | Build Number | Link |
|--- | ------------ | ---- |
| Nightly | [![Nightly build number](https://presencelight.blob.core.windows.net/nightly/ci_badge.svg)](https://presencelight.blob.core.windows.net/nightly/index.html)| [Install](https://presencelight.blob.core.windows.net/nightly/index.html)
| Microsoft Store | [![Stable build number](https://presencelight.blob.core.windows.net/store/stable_badge.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)| [![Install](static/store.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)
| Chocolatey | [![Stable build number](https://presencelight.blob.core.windows.net/store/stable_badge.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)| [Install](https://chocolatey.org/packages/PresenceLight/)
| [Windows Package Manager](https://docs.microsoft.com/en-us/windows/package-manager) | [![Stable build number](https://presencelight.blob.core.windows.net/store/stable_badge.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)| winget install isaaclevin.presencelight

## What is PresenceLight?

### [Blog Post](https://isaacl.dev/presence-light)

### [PresenceLight Demos](https://www.youtube.com/playlist?list=PL_IEvQa-oTVtB3fKUclJNNJ1r-Sxtjc-m)

[PresenceLight](https://isaacl.dev/presence-light) is a solution to broadcast your various statuses to a Phillips Hue or LIFX light bulb. Some statuses you can broadcast are: your availability in Microsoft Teams, your current Windows 10 theme, and a theme or color of your choosing. There are other solutions that do something similar to sending Teams Availability to a light, but they require a tethered solution (plugging a light into a computer via USB). What PresenceLight does is leverage the [Presence Api](https://docs.microsoft.com/graph/api/presence-get), which is available in [Microsoft Graph](https://docs.microsoft.com/graph/overview), allowing to retrieve your presence without having to be tethered. This could potentially allow someone to update the light bulb from a remote machine they do not use.

## Hardware Requirements

| Item  |
| ------------ |
| [Phillips Hue Bridge](https://www2.meethue.com/en-us/p/hue-bridge/046677458478)
| [Phillips Hue Light Bulb](https://www2.meethue.com/en-us/p/hue-white-and-color-ambiance-1-pack-e26/046677548483) |
| [Any LIFX Light](https://www.lifx.com/pages/all-products) |

## Configure Hardware
- [Configure Hardware](https://github.com/isaacrlevin/PresenceLight/wiki/Configure-Hardware)

## App Setup
- [App Setup](https://github.com/isaacrlevin/PresenceLight/wiki/WPF-App-Setup)

## FAQ
- [FAQ](https://github.com/isaacrlevin/PresenceLight/wiki/FAQ)

## Please Contribute

I welcome all contributions here, as I am no expert in WPF/MSIX things.

## Third Party Libraries

Presence Light would not be possibke without the amazing work from the contributors to the following third party libraries!

- [Q42.HueApi](https://github.com/Q42/Q42.HueApi)
- [AppInsights.WindowsDesktop](https://github.com/novotnyllc/AppInsights.WindowsDesktop)
- [IdentityModel.OidcClient](https://github.com/IdentityModel/IdentityModel.OidcClient)
- [wpftoolkit](https://github.com/xceedsoftware/wpftoolkit)
- [OSVersionHelper](https://github.com/onovotny/OSVersionHelper)
- [WpfAnimatedGif](https://github.com/XamlAnimatedGif/WpfAnimatedGif)
