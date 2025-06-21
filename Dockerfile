FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj", "Presentation/EEaseWebAPI.API/"]
COPY ["Core/EEaseWebAPI.Application/EEaseWebAPI.Application.csproj", "Core/EEaseWebAPI.Application/"]
COPY ["Core/EEaseWebAPI.Domain/EEaseWebAPI.Domain.csproj", "Core/EEaseWebAPI.Domain/"]
COPY ["Infrastructure/EEaseWebAPI.Infrastructure/EEaseWebAPI.Infrastructure.csproj", "Infrastructure/EEaseWebAPI.Infrastructure/"]
COPY ["Infrastructure/EEaseWebAPI.Persistence/EEaseWebAPI.Persistence.csproj", "Infrastructure/EEaseWebAPI.Persistence/"]
RUN dotnet restore "Presentation/EEaseWebAPI.API/EEaseWebAPI.API.csproj"
COPY . .
WORKDIR "/src/Presentation/EEaseWebAPI.API"
RUN dotnet build "EEaseWebAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EEaseWebAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EEaseWebAPI.API.dll"] 