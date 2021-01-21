![Logo](https://github.com/isaacrlevin/PresenceLight/raw/main/Icon.png)
# PresenceLight

![.github/workflows/Worker_Build.yml](https://github.com/isaacrlevin/presencelight/workflows/.github/workflows/Worker_Build.yml/badge.svg)
![.github/workflows/Desktop_Build.yml](https://github.com/isaacrlevin/presencelight/workflows/.github/workflows/Desktop_Build.yml/badge.svg)

### App Versions

| Application Type |  Platforms | Readme
|--- |  ---- | ---- |
| Desktop (.NET 5) | Windows 10 (min Version 1803) | [Desktop Readme](https://github.com/isaacrlevin/PresenceLight/blob/main/desktop-README.md)
| Worker (ASP.NET 5) | Windows, Linux (Debian, AMD x64, ARM, ARM x64),  | [Worker Readme](https://github.com/isaacrlevin/PresenceLight/blob/main/worker-README.md)

## What is PresenceLight?

[PresenceLight](https://isaacl.dev/presence-light) is a solution to broadcast your various statuses to various kinds of smart lights. Some statuses you can broadcast are: your availability in Microsoft Teams, your current Windows 10 theme, and a theme or color of your choosing. There are other solutions that do something similar to sending Teams Availability to a light, but they require a tethered solution (plugging a light into a computer via USB). What PresenceLight does is leverage the [Presence Api](https://docs.microsoft.com/graph/api/presence-get), which is available in [Microsoft Graph](https://docs.microsoft.com/graph/overview), allowing to retrieve your presence without having to be tethered. This could potentially allow someone to update the light bulb from a remote machine they do not use.

#### [Blog Post](https://isaacl.dev/presence-light)

#### [PresenceLight Demos](https://www.youtube.com/playlist?list=PL_IEvQa-oTVtB3fKUclJNNJ1r-Sxtjc-m)

## Supported Hardware

| Light Type  |
| ------------ |
| Phillips Hue (Local and Remote)
| LIFX |
| Yeelight |
| Any light which can be controlled via a GET or POST call to a web API |
| Phillips Wiz **Coming Soon** |

## Configure Hardware
- [Configure Hardware](https://github.com/isaacrlevin/PresenceLight/wiki/Configure-Hardware)

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