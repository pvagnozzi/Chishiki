# GitHub Copilot Instructions

## Repository context

- This repository is a `.NET 10` solution (`Chishiki.slnx`) with a distributed backend and supporting infrastructure under `src/`.
- The main runtime pieces are:
  - `src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.AppHost` for local orchestration
  - `src/backend/Chishiki.API.Web` for the ASP.NET Core API
  - `src/backend/Chishiki.Host` for the Orleans silo host
  - `src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults` for shared Aspire defaults
  - `src/shared/Chishiki.Core` and `src/shared/clustering/*` for shared/domain and clustering libraries

## Build and run commands

- Build the full solution from the repo root:

```powershell
dotnet build .\Chishiki.slnx
```

- Run the full local stack through Aspire AppHost:

```powershell
dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
```

- Run only the API:

```powershell
dotnet run --project .\src\backend\Chishiki.API.Web\Chishiki.API.Web.csproj
```

- Run only the Orleans host:

```powershell
dotnet run --project .\src\backend\Chishiki.Host\Chishiki.Host.csproj
```

## Test and lint commands

- The solution currently has no real test projects under `tests/`, so `dotnet test` does not execute a meaningful suite yet.
- If you need to check the current solution state, use:

```powershell
dotnet test .\Chishiki.slnx
```

- No dedicated lint command or lint configuration was found in the repository.

## High-level architecture

### Distributed application layout

- `Chishiki.Infrastructure.Aspire.AppHost` is the local composition root. It starts the application dependencies and service containers with `DistributedApplication.CreateBuilder(args)`.
- AppHost wires together Dockerfile-based resources for:
  - PostgreSQL (`containers/postgresql`)
  - Redis (`containers/redis`)
  - Keycloak (`containers/keycloak`)
  - Prometheus (`containers/prometheus`)
  - Grafana (`containers/grafana`)
  - Qdrant (`containers/qdrant`)
  - Ollama (`containers/ollama`)
  - the Orleans host (`src/backend/Chishiki.Host/Dockerfile`)
  - the API (`src/backend/Chishiki.API.Web/Dockerfile`)
- Service startup order is encoded with `.WaitFor(...)` in `AppHost.cs`, so treat AppHost as the authoritative local-development topology.

### API and host split

- `Chishiki.API.Web` is a minimal ASP.NET Core app. It currently exposes a minimal API endpoint (`/weatherforecast`) and enables OpenAPI in development.
- `Chishiki.Host` is the Orleans silo process. It is not an ASP.NET app; it uses `Host.CreateApplicationBuilder(args)` and configures Orleans directly.
- The Orleans host uses Redis for:
  - clustering
  - default grain storage
  - `PubSubStore`
  - reminder service

### Shared infrastructure defaults

- Both runtime services call `builder.AddServiceDefaults()`, which comes from `Chishiki.Infrastructure.Aspire.ServiceDefaults`.
- That shared project centralizes:
  - OpenTelemetry logging, tracing, and metrics
  - service discovery
  - default HTTP resilience handlers
  - default health checks
  - Prometheus scraping
- `app.MapDefaultEndpoints()` is the matching web-side convention for health and metrics endpoints. In development it maps:
  - `/health`
  - `/alive`
  - `/metrics`

### Shared libraries

- `src/shared/Chishiki.Core` is the placeholder shared/domain library.
- `src/shared/clustering/Chishiki.Clustering*` contains clustering-related shared projects that are part of the solution even though they are still mostly skeletal.
- Right now, the only explicit project references are from the API and host to `Chishiki.Infrastructure.Aspire.ServiceDefaults`; keep that dependency direction intact unless the architecture is intentionally changing.

## Repository-specific conventions

### Service defaults are the shared backend convention

- For new backend services, reuse `AddServiceDefaults()` instead of re-declaring telemetry, health checks, service discovery, or resilience setup in each service.
- For ASP.NET Core services, pair that with `app.MapDefaultEndpoints()` so health and metrics behavior stays consistent with the existing services.

### AppHost is the source of truth for local dependencies

- If you add a new infrastructure dependency or service-to-service dependency, update `src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.AppHost/AppHost.cs` rather than inventing a separate local orchestration path.
- Container paths in AppHost are relative to the repo root and existing `containers/*` Dockerfiles; follow that pattern.

### Redis-backed Orleans configuration is explicit

- Keep Orleans configuration in `src/backend/Chishiki.Host/Program.cs`.
- Existing conventions there are explicit values rather than extracted helpers:
  - cluster ID: `chishiki-cluster`
  - service ID: `chishiki`
  - silo port: `11111`
  - gateway port: `30000`
- Redis connection resolution currently comes from `ConnectionStrings__redis` with a fallback of `redis:6379`.

### Modern .NET project defaults are consistent across projects

- Every current `.csproj` targets `net10.0`.
- Every current `.csproj` enables:
  - `<Nullable>enable</Nullable>`
  - `<ImplicitUsings>enable</ImplicitUsings>`
- The API and host projects also set `DockerDefaultTargetOS` to `Linux`.
- Match these project-level defaults when adding new projects unless there is a strong repository-specific reason to differ.

### Development-only API surface

- OpenAPI and Swagger UI are only enabled in development in `Chishiki.API.Web/Program.cs`.
- Health endpoints are also development-only in `ServiceDefaults/Extensions.cs`.
- Preserve that environment-gated behavior unless the repo intentionally changes its exposure policy.

## Existing Copilot and MCP setup

- The repository already has a substantial `.github/instructions/` and `.github/agents/` library. Before adding task-specific guidance, check whether an existing instruction file or agent already covers the language or domain you are working in.
- `.mcp.json` is present at the repo root and defines MCP servers for:
  - GitHub
  - Microsoft Learn
  - Azure
  - NuGet
  - fetch
  - sequential-thinking
  - PostgreSQL
  - memory
- `.github/hooks.json` is also present and configures Copilot session hooks for:
  - session logging
  - secret scanning on session end
  - dependency license checks on session end

## Working assumptions for future sessions

- Prefer the Aspire AppHost workflow when you need the full stack locally.
- Treat `src/frontend/` and `tests/` as not-yet-implemented areas unless the repository changes.
- When introducing new backend code, stay aligned with the existing split between:
  - orchestration in AppHost
  - service startup in each `Program.cs`
  - shared observability and resilience setup in `ServiceDefaults`
