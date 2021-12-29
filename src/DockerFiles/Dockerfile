#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PresenceLight.Web/PresenceLight.Web.csproj", "PresenceLight.Web/"]
COPY ["PresenceLight.Razor/PresenceLight.Razor.csproj", "PresenceLight.Razor/"]
COPY ["PresenceLight.Core/PresenceLight.Core.csproj", "PresenceLight.Core/"]
RUN dotnet restore "PresenceLight.Web/PresenceLight.Web.csproj"
COPY . .
WORKDIR "/src/PresenceLight.Web"
RUN dotnet build "PresenceLight.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PresenceLight.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
LABEL org.opencontainers.image.source=https://github.com/isaacrlevin/presencelight
ENTRYPOINT ["dotnet", "PresenceLight.dll"]