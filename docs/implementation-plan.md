# Chishiki — Implementation Plan

## Current state (baseline)

| Project | Status | Notes |
|---|---|---|
| `Chishiki.Core` | ⬜ Skeleton | No domain types yet |
| `Chishiki.Clustering` | ⬜ Skeleton | No grain interfaces yet |
| `Chishiki.Clustering.Client` | ⬜ Skeleton | Empty |
| `Chishiki.Clustering.Server` | ⬜ Skeleton | Empty |
| `Chishiki.API.Web` | ✅ Running | WeatherForecast placeholder |
| `Chishiki.Host` | ✅ Running | Orleans silo with Redis |
| `Chishiki.Infrastructure.Aspire.AppHost` | ✅ Running | Full container topology |
| `Chishiki.Infrastructure.Aspire.ServiceDefaults` | ✅ Running | OTel + health checks |

## Roadmap

### Phase 1 — Foundation (this iteration) ✅

**Goal**: Core domain primitives + Hub MCP server operational with file-system backing.

#### 1.1 `Chishiki.Core` — DDD primitives
- `Domain/Events/IDomainEvent`
- `Domain/Entities/Entity<TId>` + `AggregateRoot<TId>`
- `Domain/ValueObjects/ValueObject`
- `Domain/Interfaces/IRepository<T,TId>` + `IUnitOfWork`

#### 1.2 `Chishiki.Security.Contracts` — new project
- `Severity` enum
- `SecurityFinding` record (matches copilot-instructions.md spec)
- `ScanResult` record
- `IScannerService` interface

#### 1.3 `Chishiki.Hub.Contracts` — new project
- `HubResourceType` enum
- `HubResourceDto` record
- `CollectionManifest` + `CollectionItem` records
- `IHubResourceService` interface
- `Requests/` — 5 request records

#### 1.4 `Chishiki.Clustering` — grain interfaces
- `IHubResourceGrain` (cache layer for Hub resources)

#### 1.5 `Chishiki.Hub` — new service (Hub MCP server)
- ASP.NET Core minimal API on port 5010
- `ModelContextProtocol.AspNetCore` — HTTP transport
- `FileSystemHubResourceService` — reads from `.github/` + `hub/` directories
- `FrontMatterParser` — parses YAML front-matter without external packages
- `HubMcpTools` — 6 MCP tools: `list_resources`, `get_resource`, `create_resource`,
  `apply_collection`, `scaffold_project`, `search_resources`
- Dockerfile (same pattern as `Chishiki.API.Web`)

#### 1.6 Infrastructure wiring
- `AppHost.cs` — add `chishiki-hub` resource (bind-mounts repo root as `/hub-root`)
- `.mcp.json` — register `hub` server at `http://localhost:5010/mcp`
- `Chishiki.slnx` — add all new projects and test folders

#### 1.7 Tests (Phase 1)
- `Chishiki.Core.UnitTests` — `Entity`, `ValueObject` equality
- `Chishiki.Security.Contracts.UnitTests` — `SecurityFinding` record
- `Chishiki.Hub.Contracts.UnitTests` — `HubResourceDto` record, `HubResourceType` parsing
- `Chishiki.Hub.UnitTests` — `FrontMatterParser`, `HubMcpTools` (NSubstitute mocks)

---

### Phase 2 — Persistence (next iteration)

**Goal**: PostgreSQL persistence via EF Core + Orleans grain caching for Hub resources.

- `Chishiki.Hub` — add EF Core + PostgreSQL persistence
  - `HubDbContext` with `HubResourceEntity` table
  - `EfCoreHubResourceRepository` replacing file-system reads (file system becomes seed/import)
  - `Migrations/` initial schema
- `Chishiki.Clustering.Server` — `HubResourceGrain` implementation
  - Grain reads from `IHubResourceService`, caches with default TTL
  - `InvalidateAsync` on write
- `Chishiki.Clustering.Client` — `OrleansHubResourceClient` wrapping grain calls
- AppHost — add `EF_CONNECTION_STRING` env var for Hub
- Tests — integration tests with Aspire `DistributedApplicationTestingBuilder`

---

### Phase 3 — Security Orchestration (future)

**Goal**: `Chishiki.Security` service that runs scanner containers and generates NUnit exploit tests.

- `Chishiki.Security` — worker service
  - `ScannerOrchestrator` — triggers Docker containers via Docker SDK
  - `FindingPersistenceService` — writes `SecurityFinding` records to PostgreSQL
  - `ExploitTestGenerator` — Roslyn-based test file generator from finding templates
- `Chishiki.Security.Contracts` — extend with `IFindingStore`, `ITestGenerator`
- AppHost — register `chishiki-security` under both default and security profiles
- Tests — `Chishiki.Security.UnitTests` + `Chishiki.Security.IntegrationTests`

---

### Phase 4 — API Gateway (future)

**Goal**: Replace WeatherForecast placeholder in `Chishiki.API.Web` with real endpoints.

- `/hub/*` — proxy to Hub MCP tools via `IHubResourceService`
- `/security/*` — query persisted findings
- OIDC authentication via Keycloak (`Microsoft.AspNetCore.Authentication.OpenIdConnect`)
- OpenAPI docs for all endpoints

---

## Dependency graph (Phase 1)

```
Chishiki.Core
    ← Chishiki.Hub.Contracts
    ← Chishiki.Security.Contracts
    ← Chishiki.Clustering (+ Orleans abstractions + Hub.Contracts)

Chishiki.Hub.Contracts
    ← Chishiki.Hub (MCP server)
    ← Chishiki.Clustering

Chishiki.Infrastructure.Aspire.ServiceDefaults
    ← Chishiki.Hub
    ← Chishiki.API.Web
    ← Chishiki.Host
```

## Hub resource file layout (read-only in Phase 1)

| Type | Directory | File pattern |
|---|---|---|
| `Template` | `hub/templates/` | `*.md`, `*.json` |
| `Agent` | `.github/agents/` | `*.chatmode.md` |
| `Hook` | `.github/` | `hooks.json` |
| `Instruction` | `.github/instructions/` | `*.instructions.md` |
| `Skill` | `.github/skills/{name}/` | `SKILL.md` |
| `Collection` | `hub/collections/` | `*.yaml` |

Every text resource must carry YAML front-matter with `name`, `description`, `tags`, `version`.
