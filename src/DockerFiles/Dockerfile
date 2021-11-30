FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy everything else and build app
COPY PresenceLight.Web/. ./PresenceLight.Web/
COPY PresenceLight.Razor/. ./PresenceLight.Razor/
COPY PresenceLight.Core/. ./PresenceLight.Core/

WORKDIR /source/PresenceLight.Web
RUN dotnet publish -c release -o /app /p:Version={VERSION}

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
EXPOSE 5000
EXPOSE 5001

WORKDIR /app
COPY --from=build /app ./
LABEL org.opencontainers.image.source=https://github.com/isaacrlevin/presencelight
ENTRYPOINT ["dotnet", "PresenceLight.dll"]
