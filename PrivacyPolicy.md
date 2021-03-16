# Information Collected And Transmitted By PresenceLight

First, a reminder: PresenceLight is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement.

With that out of the way, here's a breakdown of all the information we may collect via Application Insights.

### Application-Level
Includes:
* Exception information
  * Could, in rare cases, contain paths packages on your computer
* Machine name
* Host name
* Version number (e.g. 2.0.x.x)

### Operating System-Level
Includes:
* Architecture (e.g. 32-bit)
* Version (e.g. Windows 10.0.17763.0)
* Build (e.g. 17134.1.amd64fre.rs4_release.180410-1804)
* Available processors/cores (e.g. 8 cores)
* Machine Name (e.g. MyFastPC)
* .NET Core Common Language Runtime version (e.g. 4.0.30319.42000)

## Package Sources

**OS information and IP address**

When Presence Light makes calls to authenticate users, it uses the Microsoft Graph Api. The author of PresenceLight does not have access to this information, but Microsoft Graph does and logs this information. 
**3rd-party package source**

When user specifies a different package source than the default source at http://nuget.org, he/she will be subjected to the privacy policy of that website. NuGet Package Explorer does not send any such data to its author.

## Third-Party Policies

* LIFX https://www.lifx.com/pages/privacy-policy
* Philips Hue https://www2.meethue.com/en-us/support/privacy-policy
* Microsoft Store https://docs.microsoft.com/en-us/legal/windows/agreements/store-policies
