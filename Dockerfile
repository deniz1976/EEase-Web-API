FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY eease-web-api.sln ./
COPY Core/EEaseWebAPI.Domain/*.csproj Core/EEaseWebAPI.Domain/
COPY Core/EEaseWebAPI.Application/*.csproj Core/EEaseWebAPI.Application/
COPY Infrastructure/EEaseWebAPI.Infrastructure/*.csproj Infrastructure/EEaseWebAPI.Infrastructure/
COPY Infrastructure/EEaseWebAPI.Persistence/*.csproj Infrastructure/EEaseWebAPI.Persistence/
COPY Presentation/EEaseWebAPI.API/*.csproj Presentation/EEaseWebAPI.API/

RUN echo '<?xml version="1.0" encoding="utf-8"?>\
<configuration>\
  <packageSources>\
    <clear />\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />\
  </packageSources>\
  <config>\
    <add key="globalPackagesFolder" value="/root/.nuget/packages" />\
  </config>\
  <fallbackPackageFolders>\
    <clear />\
  </fallbackPackageFolders>\
</configuration>' > /root/.nuget/NuGet/NuGet.Config

ENV NUGET_PACKAGES=/root/.nuget/packages \
    MSBuildSDKsPath=/usr/share/dotnet/sdk/8.0.408/Sdks \
    MSBUILDDISABLENODEREUSE=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true

RUN echo '<Project>\
  <PropertyGroup>\
    <RestorePackagesPath>/root/.nuget/packages</RestorePackagesPath>\
    <MSBuildProjectExtensionsPath>$(MSBuildProjectDirectory)/obj</MSBuildProjectExtensionsPath>\
    <BaseIntermediateOutputPath>$(MSBuildProjectDirectory)/obj</BaseIntermediateOutputPath>\
    <RootNamespace>$(MSBuildProjectName)</RootNamespace>\
    <DisableImplicitNuGetFallbackFolder>true</DisableImplicitNuGetFallbackFolder>\
    <NuGetFallbackFolders></NuGetFallbackFolders>\
  </PropertyGroup>\
</Project>' > Directory.Build.props

RUN dotnet nuget locals all --clear

RUN dotnet restore Core/EEaseWebAPI.Domain/EEaseWebAPI.Domain.csproj /p:DisableImplicitNuGetFallbackFolder=true || true && \
    dotnet restore Core/EEaseWebAPI.Application/EEaseWebAPI.Application.csproj /p:DisableImplicitNuGetFallbackFolder=true || true && \
    dotnet restore Infrastructure/EEaseWebAPI.Infrastructure/EEaseWebAPI.Infrastructure.csproj /p:DisableImplicitNuGetFallbackFolder=true || true && \
    dotnet restore Infrastructure/EEaseWebAPI.Persistence/EEaseWebAPI.Persistence.csproj /p:DisableImplicitNuGetFallbackFolder=true || true && \
    dotnet restore Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj /p:DisableImplicitNuGetFallbackFolder=true || true

COPY . .

RUN dotnet publish Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:DisableImplicitNuGetFallbackFolder=true \
    /p:NuGetFallbackFolders='' \
    /p:MSBuildEnableWorkloadResolver=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80

RUN apt-get update && apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/* && \
    adduser --disabled-password --gecos "" appuser && \
    chown -R appuser:appuser /app
USER appuser

HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

EXPOSE 80

ENTRYPOINT ["dotnet", "EEaseWebAPI.API.dll"] 