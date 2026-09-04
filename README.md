# EEase-Web-API

A travel itinerary planning service built on .NET 8 following Clean Architecture.
A user supplies a destination, a date range and a budget level; the system uses Gemini
and Google Places to generate a day-by-day itinerary. Users can like routes, add
friends, and share their routes with those friends.

## Features

- **Authentication:** JWT-based sign-up, sign-in, refresh tokens, email confirmation and password reset
- **Route generation:** day plans via Gemini, place details via Google Places
- **Personalisation:** accommodation, food and sightseeing preferences learned from liked places
- **Social:** friend requests, blocking, route visibility (private / friends / public)
- **Resilience:** per-client rate limiting, a Gemini key pool and retries
- **Operations:** a `/health` endpoint, configurable migrations, one-command start-up with Docker Compose

## Architecture

```
Core/
  EEaseWebAPI.Domain          Entities and enums. Depends on no other layer.
  EEaseWebAPI.Application     CQRS requests/handlers, DTOs, service interfaces,
                              validation, configuration objects (Options)
Infrastructure/
  EEaseWebAPI.Infrastructure  JWT issuing, mail delivery, HTTP helpers
  EEaseWebAPI.Persistence     EF Core DbContext and mappings, repositories,
                              domain services, Gemini and Google Places clients
Presentation/
  EEaseWebAPI.API             Controllers, pipeline setup, Swagger
tests/
  EEaseWebAPI.UnitTests       Unit tests
```

Dependencies point inwards: `API → Persistence/Infrastructure → Application → Domain`.

Package versions are managed centrally in `Directory.Packages.props`; shared MSBuild
settings live in `Directory.Build.props`.

## Technology

.NET 8 · ASP.NET Core · EF Core 8 + Npgsql · PostgreSQL · ASP.NET Core Identity ·
MediatR · FluentValidation · AutoMapper · Swashbuckle · MailKit · xunit

---

## Running the project

### With Docker (recommended)

The Compose file includes PostgreSQL, so there is no separate database to set up.

```sh
cp .env.example .env      # fill in the values (API keys are optional)
docker compose up --build
```

The API listens on `http://localhost:8080` and the root path redirects to Swagger.
The schema is created automatically on first start (`Database__MigrateOnStartup=true`).

Ports can be changed through `.env`:

```sh
API_PORT=8090 POSTGRES_PORT=5433 docker compose up
```

> The PostgreSQL container binds to **5433** by default so it does not clash with a
> PostgreSQL installed on the host.

### Running locally

1. Provide a PostgreSQL instance. To run just the database in Docker:

   ```sh
   docker compose up -d postgres
   ```

2. Supply the connection string and JWT settings. `appsettings.Development.json` is
   preconfigured to talk to the bundled Postgres container; to supply your own values:

   ```sh
   cd Presentation/EEaseWebAPI.API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:PostgreSQL" "Host=localhost;Port=5433;Database=eease;Username=eease;Password=eease"
   dotnet user-secrets set "Token:SecurityKey" "<a key of at least 32 characters>"
   ```

3. Run it:

   ```sh
   dotnet run --project Presentation/EEaseWebAPI.API
   ```

### Neon (or any other managed PostgreSQL)

Supply the connection string through an environment variable; the SSL parameters are
required:

```sh
export ConnectionStrings__PostgreSQL="Host=ep-xxxx.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=xxxx;Password=xxxx;SSL Mode=Require;Trust Server Certificate=true;Pooling=true"
```

In managed environments you may prefer to run migrations as a separate step rather
than on start-up. Leave `Database:MigrateOnStartup` set to `false` and apply the
schema yourself:

```sh
dotnet ef database update --project Infrastructure/EEaseWebAPI.Persistence --startup-project Presentation/EEaseWebAPI.API
```

## Tests

```sh
dotnet test
```

## Configuration

Every setting is declared in `appsettings.json` and can be overridden with environment
variables (use the `__` separator for nested keys, e.g. `Token__SecurityKey`).

| Section | Description |
|---|---|
| `ConnectionStrings:PostgreSQL` | Database connection string. **Required.** |
| `Token` | JWT issuer, audience and signing key. **Required**; the key must be at least 32 characters. |
| `Database` | Command timeout, retries, sensitive data logging, migrate on startup |
| `Cors:AllowedOrigins` | Allowed origins. When empty, every origin is allowed in the Development environment only. |
| `RateLimiting` | The per-client global limit and a separate limit for expensive endpoints |
| `GeminiAI` | Key pool, model id, timeout, retry count |
| `GooglePlaces:ApiKey` | Places API key |
| `MailService` | SMTP settings. With `Enabled: false` mail is only logged. |

The Gemini and Google Places keys are not required at startup; if they are missing the
corresponding endpoint answers with `503` and a descriptive message. This keeps local
development possible without any keys.

### Choosing a model

The Gemini model is selected through `GeminiAI:Model` (default `gemini-3.8-flash`). The
model, API version and base address all come from configuration — no code change needed.

## Database schema

The schema lives in a single `InitialSchema` migration.

**If you have an existing database** created before this rework, run the idempotent
script prepared to bring the schema onto the new layout:

```sh
psql "<connection-string>" -f Scripts/migrate-existing-database.sql
```

The script renames the columns that changed, creates the missing indexes and rebaselines
the migration history. Take a backup before running it.

## API

The Swagger UI is served at the root path: `http://localhost:8080/`

Protected endpoints expect an `Authorization: Bearer <token>` header. Obtain a token
from `POST /api/Auth/Login`.

| Controller | Scope |
|---|---|
| `Auth` | Sign-in, refresh token, password reset, password change |
| `Users` | Sign-up, profile, preferences, search, photo, account deletion |
| `Route` | Route creation, listing, likes, visibility, place like/dislike |
| `Friendship` | Friend requests, friend list, blocking |
| `City` | Country and city search |
| `Currency` | Currency list |
