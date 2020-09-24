#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
#EXPOSE 80
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["PresenceLight.Worker/PresenceLight.Worker.csproj", "PresenceLight.Worker/"]
COPY ["PresenceLight.Core/PresenceLight.Core.csproj", "PresenceLight.Core/"]
RUN dotnet restore "PresenceLight.Worker/PresenceLight.Worker.csproj"
COPY . .
WORKDIR "/src/PresenceLight.Worker"
RUN dotnet build "PresenceLight.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PresenceLight.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PresenceLight.dll"]