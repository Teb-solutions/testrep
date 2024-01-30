#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

ENV ASPNETCORE_ENVIRONMENT=Staging
ENV ASPNETCORE_URLS=https://+:443

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["services/profiles/Profiles.API/Profiles.API.csproj", "services/profiles/Profiles.API/"]
RUN dotnet restore "services/profiles/Profiles.API/Profiles.API.csproj"
COPY . .
WORKDIR "/src/services/profiles/Profiles.API"
RUN dotnet build "Profiles.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Profiles.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profiles.API.dll"]
