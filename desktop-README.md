![Logo](https://github.com/isaacrlevin/PresenceLight/raw/main/Icon.png)
# PresenceLight - Desktop Version

![.github/workflows/Desktop_Build.yml](https://github.com/isaacrlevin/presencelight/workflows/.github/workflows/Desktop_Build.yml/badge.svg)

### WPF Installs

| Release Channel | Install Type | Build Number | Link |
|--- | ------------ | ---- | --- |
| Nightly | App Installer | [![Nightly build number](https://presencelight.blob.core.windows.net/nightly/ci_badge.svg)](https://presencelight.blob.core.windows.net/nightly/index.html)| [Install Link](https://presencelight.blob.core.windows.net/nightly/index.html)
| Microsoft Store | App Installer | [![Stable build number](https://presencelight.blob.core.windows.net/store/stable_badge.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)| [![Install](https://github.com/isaacrlevin/PresenceLight/raw/main/static/store.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)
| Chocolatey | Standalone exe | [![Stable build number](https://presencelight.blob.core.windows.net/store/stable_badge.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)| [Packge Page](https://chocolatey.org/packages/PresenceLight/)
| GitHub Releases | Standalone exe |  [![Stable build number](https://presencelight.blob.core.windows.net/store/stable_badge.svg)](https://www.microsoft.com/en-us/p/presencelight/9nffkd8gznl7)| [Release Page](https://github.com/isaacrlevin/presencelight/releases)

## Desktop App Setup

**NOTE: These steps are for the WPF (Windows desktop client) application. If you want to get PresenceLight working on non-Windows, please take a look at the [Worker Readme](https://github.com/isaacrlevin/PresenceLight/blob/main/worker-README.md).**

In order for the desktop app to work, you need to be running Windows 10, version 1903 (19H1), as well as provide the following steps.

- Install App via the desired build link above
- Hue Only- Obtain the IP Address for your Phillips Hue Bridge
- LIFX Only - Obtain a LIFX Developer Token from [here](https://cloud.lifx.com/settings)


### Install App

After you have followed the above steps (enable side-loading and installing app), you will have be welcomed with a message like this

   ![Configured](https://github.com/isaacrlevin/PresenceLight/raw/main/static/configured.png)

PresenceLight obtains your Microsoft Teams Availability using a multi-tenant Azure Active Directory Application, meaning you will need to "grant" access to your Presence the first time you use the app. Clicking sign-in will prompt you for a login with your Microsoft 365 credentials, and finally when authenticated, you will be shown your Graph profile image and your presence. If you are curious about what is required to do this on your own tenant, read [Configure an Azure Active Directory Application](https://github.com/isaacrlevin/PresenceLight/wiki/Configure-an-Azure-Active-Directory-Application)

   ![Profile Image](https://github.com/isaacrlevin/PresenceLight/raw/main/static/profile.png)

The application "polls" the Presence Api at a configured value, whican you can set bewteen 1 and 5 seconds on the Settings page. This means that the light and app will update based on your Teams presence with a slight delay.

### Broadcasting to Lights

There are three main ways to currently update your lights using PresenceLight

 - Updating with Teams Presence (status)
 - Syncing with your Windows 10 Theme
 - Setting a fixed color using color picker

You can only do one of these at a time, so if you for instance are syncing with Teams, choosing anohter option will sign you out of Teams. This will happen with the other options as well.

## Customize Icons

One of the features of PresenceLight is that you can minimize the app to the icon tray. When you open the app, you will see an icon similar to this.

   ![white Image](https://github.com/isaacrlevin/PresenceLight/raw/main/static/light-icon.png)

This icon will represent your presence color. There are two "kinds" of icons: Transparent, and White. Here is the transparent icon

   ![Settings 1](https://github.com/isaacrlevin/PresenceLight/raw/main/static/trans-icon.png)

You can change the icon type in the settings pane.

   ![Settings 2](https://github.com/isaacrlevin/PresenceLight/raw/main/static/settings1.png)

After you change and save, the icon will update in the icon tray.

## Wire Up Phillips Light

To connect PresenceLight to Phillips Hue, you can do it 1 of 2 ways

 - Obtain the IP Address of your Phillips Hue Bridge (if you have it)
 - Ask PresenceLight to find it for you (may no work in certain network configurations)

 ![Hue Settings](https://github.com/isaacrlevin/PresenceLight/raw/main/static/hue-settings.png)

Once you have the IP of the bridge, you will need to register a developer account and get an Api Key. This is easily done by clicking the "Register Bridge" button. Clicking the button will popup a window asking you to press the sync button on the bridge, this is needed to register PresenceLight to the bridge.

 ![Sync Button](https://github.com/isaacrlevin/PresenceLight/raw/main/static/sync-button.png)

When PresenceLight is configured, you will see a dropdown of Hue Bulbs connected to the bridge for you to set your presence to.

 ![Registered Bridge](https://github.com/isaacrlevin/PresenceLight/raw/main/static/registered-bridge.png)

## Wire up LIFX

To connect PresenceLight to LIFX colored bulbs, you need to obtain a LIFX Developer Token. When you first arrive at the LIFX tab, you will see a message like this if you try to get Lights or Groups

 ![LIFX Unconfigured](https://github.com/isaacrlevin/PresenceLight/raw/main/static/lifx-unconfigured.png)

After entering an obtained token, you will be able to get a list of either individual lights or groups of lights, selecting one of the options and saving gives you a message like this

 ![LIFX Configured](https://github.com/isaacrlevin/PresenceLight/raw/main/static/lifx-configured.png)

## [Wire-up Custom API](https://github.com/isaacrlevin/presencelight/wiki/Wire-up-Custom-API)


## In Conclusion

At this point PresenceLight should be setup. Feel free to file an issue if you have any problems.