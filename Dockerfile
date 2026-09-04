# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project and package files first so that the restore layer stays
# cached when source code changes.
COPY Directory.Build.props Directory.Packages.props ./
COPY eease-web-api.sln ./
COPY Core/EEaseWebAPI.Domain/EEaseWebAPI.Domain.csproj Core/EEaseWebAPI.Domain/
COPY Core/EEaseWebAPI.Application/EEaseWebAPI.Application.csproj Core/EEaseWebAPI.Application/
COPY Infrastructure/EEaseWebAPI.Infrastructure/EEaseWebAPI.Infrastructure.csproj Infrastructure/EEaseWebAPI.Infrastructure/
COPY Infrastructure/EEaseWebAPI.Persistence/EEaseWebAPI.Persistence.csproj Infrastructure/EEaseWebAPI.Persistence/
COPY Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj Presentation/EEaseWebAPI.API/
COPY tests/EEaseWebAPI.UnitTests/EEaseWebAPI.UnitTests.csproj tests/EEaseWebAPI.UnitTests/

RUN dotnet restore Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj

COPY . .

RUN dotnet publish Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

# The aspnet image ships without an HTTP client; curl is needed for the health check.
RUN apt-get update \
    && apt-get install --no-install-recommends -y curl \
    && rm -rf /var/lib/apt/lists/*

# The .NET 8 aspnet image already ships with a non-root "app" user, so there is
# nothing to create here; we just switch to it.
COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "EEaseWebAPI.API.dll"]
