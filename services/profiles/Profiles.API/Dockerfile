#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
#USER app
ENV ASPNETCORE_ENVIRONMENT=Staging
ENV ASPNETCORE_URLS=https://+:443

WORKDIR /
EXPOSE 80
EXPOSE 443

COPY services/profiles/Profiles.API/tebpfx2023.pfx /https/tebpfx2023.pfx

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/profiles/Profiles.API/Profiles.API.csproj", "services/profiles/Profiles.API/"]
COPY ["BuildingBlocks/EasyGas.Shared/EasyGas.Shared.csproj", "BuildingBlocks/EasyGas.Shared/"]
COPY ["BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]
COPY ["BuildingBlocks/IntegrationEventLogEF/IntegrationEventLogEF.csproj", "BuildingBlocks/IntegrationEventLogEF/"]
RUN dotnet restore "./services/profiles/Profiles.API/./Profiles.API.csproj"
COPY . .
WORKDIR "/src/services/profiles/Profiles.API"
RUN dotnet build "./Profiles.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Profiles.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profiles.API.dll"]