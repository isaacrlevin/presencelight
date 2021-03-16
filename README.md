![Logo](https://github.com/isaacrlevin/PresenceLight/raw/main/Icon.png)
# PresenceLight

![.github/workflows/Worker_Build.yml](https://github.com/isaacrlevin/presencelight/workflows/.github/workflows/Worker_Build.yml/badge.svg)
![.github/workflows/Desktop_Build.yml](https://github.com/isaacrlevin/presencelight/workflows/.github/workflows/Desktop_Build.yml/badge.svg)

### Get PresenceLight

| Nightly | Microsoft Store | Chocolatey | GitHub Releases | Worker Download Site | Worker Container |
| ------- | --------------- | ---------- | --------------- | -------------------- | ---------------  |
| [<img src="https://github.com/isaacrlevin/PresenceLight/raw/main/Icon.png" width="100">](https://presencelight.blob.core.windows.net/nightly/index.html)| [<img src="https://github.com/isaacrlevin/PresenceLight/raw/main/static/store.svg" width="100">](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7) | [<img src="https://chocolatey.org/content/images/global-shared/logo-square.svg" width="100">](https://chocolatey.org/packages/PresenceLight/) | [<img src="https://user-images.githubusercontent.com/8878502/110871471-55fe7c00-8283-11eb-8ce4-afeeaf62458a.png" width="100">](https://user-images.githubusercontent.com/8878502/110871316-061fb500-8283-11eb-8ad6-db529a86eab0.png) | [<img src="https://github.com/isaacrlevin/PresenceLight/raw/main/Icon.png" width="100">](https://presencelight.blob.core.windows.net/nightly/index.html) | [<img src="https://user-images.githubusercontent.com/8878502/110870857-2602a900-8282-11eb-8846-89c61a219236.png" width="100">](https://hub.docker.com/r/isaaclevin/presencelight)  [<img src="https://user-images.githubusercontent.com/8878502/110871471-55fe7c00-8283-11eb-8ce4-afeeaf62458a.png" width="100">](https://github.com/users/isaacrlevin/packages/container/package/presencelight) |

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
| Philips Hue (Local and Remote)
| LIFX |
| Yeelight |
| Any light which can be controlled via a GET or POST call to a web API |
| Philips Wiz **Coming Soon** |

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