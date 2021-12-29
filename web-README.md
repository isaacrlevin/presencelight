![Logo](Icon.png)
# PresenceLight - Web Version
![Build Status](https://dev.azure.com/isaaclevin/PresenceLight/_apis/build/status/CI-build-web?branchName=main)

### Web Version Installs

 Web Download Site | Web Container |
| ------- | ---------------   |
| [<img src="https://github.com/isaacrlevin/PresenceLight/raw/main/Icon.png" width="100">](https://presencelightapp.azurewebsites.net/) | [<img src="https://user-images.githubusercontent.com/8878502/110870857-2602a900-8282-11eb-8846-89c61a219236.png" width="100">](https://hub.docker.com/r/isaaclevin/presencelight)  [<img src="https://user-images.githubusercontent.com/8878502/110871471-55fe7c00-8283-11eb-8ce4-afeeaf62458a.png" width="100">](https://github.com/users/isaacrlevin/packages/container/package/presencelight) |

The cross platform version of PresenceLight runs as a .NET 6 single file executable application that runs a Server-Side Blazor Web Application and a ASP.NET Core Worker Service. The Blazor App is used as the mechanism to log the user in and configure settings of the app, while the Worker Service is responsible for interaction with Graph Api as well as the Smart Lights. This allows users to not need to have a UI version of the app open at all time, since the worker runs as a process.
## App Setup

### Prerequisites

For PresenceLight to run out of the box, you need to setup a local SSL Cert for the app to run under. Here are two ways to do this

- dotnet dev-certs
  - dotnet dev-certs https -ep C:\Users\youruserid\.aspnet\https\presencelight.pfx -p presencelight
  - dotnet dev-certs https --trust
- openssl (Linux)
  - [Go here make your life easier](https://www.digicert.com/easy-csr/openssl.htm)
  - openssl x509 -signkey my_web_domain.key -in my_web_domain.csr -req -days 365 -out my_web_domain.crt
  - openssl pkcs12 -inkey my_web_domain.key -in my_web_domain.crt -export -out %PATHTOYOURCERT%/presencelight.pfx

### Install

There is no installer for PresenceLight, so all that needs to be done is to download the zip folder from the [install site](http://presencelightapp.azurewebsites.net/), unzip, and run the .exe. At this point, a terminal window will open showing

 ![Terminal](static/blazor-terminal.png)

Here you will the Url for the Kestrel hosted Web Application, which will be `https://localhost:5001`. Going to that Url will take you through the login process for Azure Active Directory (for the Graph call). After login, you will see a similar look and feel to the client app.

 ![Index](static/blazor-index.png)

 From here you can use PresenceLight in a similar way to the client app. You can enable and operate lights, push custom lights and configure polling. When done, you can close the browser and PresenceLight will continue to run in the background.

 To make the process even cleaner, you can configure a startup task to run the exe at startup, and PresenceLight will be available at the url listed the first time you ran it.

## Running PresenceLight as a container

PresenceLight can be configured to run in a Docker container, and I have images on my [DockerHub](https://hub.docker.com/repository/docker/isaaclevin/presencelight) for the primary Linux distros.

- x64 Linux (latest tag)
- ARM64 (debian-arm64 tag)
- ARM32 (debian-arm32 tag) **This is the 4GB Raspberry Pi one**

### How are you handling SSL?

In order for PresenceLight to work, you need to have a redirect url to AAD
that is https. In order to make it easy for folks, I provided a self-signed cert that will allow PresenceLight to do Https redirection out of the box. Isaac is this secure? Weeeeeeelllll not the best, but since PresenceLight runs locally you should be fine. If you want to expose PresenceLight over the internet, it more than likely won't work as I have to register EACH redirect uri with Azure AD.

For my particular use-case I do not need SSL. WHAT?!?! Actually it is pretty cool. My personal setup is that PresenceLight runs in a docker container on a Raspberry Pi. I have Traefik, which is a well-known
reverse proxy that allows me to forward applications through my domain, so I can access the application from anywhere by going to

presencelight.mydomain.com

The best part about this is that [Traefik](https://traefik.io/) can be configured to pull LetsEncrypt Certificates and integration with CloudFlare SSL. There is a [great blog post on this](https://www.smarthomebeginner.com/traefik-2-docker-tutorial/), that I highly reccomend if you are interested.

### SSL for Docker Containers

To get PresenceLight to work in a Docker container, you will need to obtain (or generate like above) a certificate and mount it as a volume to your container.

**[Doc on subject](https://docs.microsoft.com/dotnet/core/additional-tools/self-signed-certificates-guide)**

Once you have a valid .pfx file, you will need to wire up the app to use that cert, the way you do that depends on how you host your app. If you app is just running locally on the machine,
you can just set environment variables for your app.

- ASPNETCORE_Kestrel__Certificates__Default__Path
- ASPNETCORE_Kestrel__Certificates__Default__Password

Or if you are running in docker, you will need to mount a volume that has your cert in it.

Here are some examples for this

**docker run example**

```bash
docker run --rm -it -p 5000:80 -p 5001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=5001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="presencelight" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/presencelight.pfx -v $env:USERPROFILE\.aspnet\https:/https/ docker pull isaaclevin/presencelight
```


**docker-compose example**

```bash
ports:
  - 5000:80
  - 5001:443
volumes:
  C:\Users\isaac\.aspnet\https:/https/ #Windows Way
  /mnt/c/Users/isaac/.aspnet/https:/https #Linux Way
environment:
  ASPNETCORE_HTTPS_PORT: "5001"
  ASPNETCORE_URLS: "https://+;http://+"
  ASPNETCORE_Kestrel__Certificates__Default__Password: "presencelight"
  ASPNETCORE_Kestrel__Certificates__Default__Path: "/https/presencelight.pfx"
```

### Mounting settings path to host

If you want to configure PresenceLight to use your own settings (maybe your own AAD, your own smart light registered app), you can do that by editing the appsettings.json

To do this in docker, just run the container once, and than stop and rerun by mounting the appsettings via a local volume.**

Log data and Configuration file will need to be written to a directory that has read/write enabled.   This is accomplished using
volumes.
```dotnetcli
volumes:
    /somedirectory:/app/config
```

`/app/config/appsettings.json` contains settings for AAD and `/app/config/PresenceLightSettings.json` contains settings for Lights, in case you wanted to configure lights outside of the UI. If you need to customize your configuration. Add/edit one or more of the nec`essary configuration files in this attached directory. This will get you host access to the appsettings.json and PresenceLightSettings.json

When running under a container, logs will save to  `/app/config/logs` as well.

### Third Party Libraries

PresenceLight would not be possible without the amazing work from the contributors to the following third party libraries!

- [Blazored.Modal](https://github.com/Blazored/Modal)
- [Blazorise](https://github.com/stsrki/Blazorise)
- [IdentityModel.OidcClient](https://github.com/IdentityModel/IdentityModel.OidcClient)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
