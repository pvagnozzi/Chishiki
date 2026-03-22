# GitHub Copilot Instructions

## Repository context

- This repository is a `.NET 10` solution (`Chishiki.slnx`) — a **developer hub** platform for managing and distributing GitHub Copilot customization assets (project templates, agents, hooks, instructions, skills, collections) via a dedicated MCP server, with an integrated security analysis and exploit-test pipeline.
- The `src/` tree is split into four top-level areas — `backend/`, `frontend/`, `infrastructure/`, and `shared/` — each described in **Source and tests layout** below.
- The main runtime pieces are:
  - `src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.AppHost` — local Aspire orchestration
  - `src/backend/Chishiki.API.Web` — ASP.NET Core minimal API (gateway)
  - `src/backend/Chishiki.Host` — Orleans silo host (grain state)
  - `src/backend/Chishiki.Hub` — Developer Hub MCP server *(planned)*
  - `src/backend/Chishiki.Security` — Security analysis orchestration service *(planned)*
  - `src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.ServiceDefaults` — shared Aspire service defaults
  - `src/shared/Chishiki.Core` and `src/shared/clustering/*` — global cross-layer domain and clustering libraries
  - `src/shared/Chishiki.Hub.Contracts` — Hub DTOs and interfaces *(planned)*
  - `src/shared/Chishiki.Security.Contracts` — Security finding types *(planned)*
- Tests live under `tests/`, mirroring the `src/` structure exactly; see **Testing conventions** below.

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

- Run the full test suite from the repo root:

```powershell
dotnet test .\Chishiki.slnx
```

- Run tests for a single project:

```powershell
dotnet test .\tests\backend\Chishiki.API.Web.UnitTests\Chishiki.API.Web.UnitTests.csproj
dotnet test .\tests\backend\Chishiki.API.Web.IntegrationTests\Chishiki.API.Web.IntegrationTests.csproj
```

- Filter by test tier using the NUnit `[Category]` attribute:

```powershell
dotnet test .\Chishiki.slnx --filter "TestCategory=Unit"
dotnet test .\Chishiki.slnx --filter "TestCategory=Integration"
dotnet test .\Chishiki.slnx --filter "TestCategory=E2E"
```

- The `tests/` tree is not yet fully implemented; the target structure and standards are defined in **Testing conventions** below.
- No dedicated lint command is configured; `dotnet build` is the primary static check.

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
  - the Hub MCP server (`src/backend/Chishiki.Hub/Dockerfile`) *(planned)*
  - security scanners under the `security` launch profile *(planned — see **Security analysis**)*
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

- `src/shared/Chishiki.Core` is the global domain library — entities, value objects, interfaces, and cross-layer abstractions live here.
- `src/shared/clustering/Chishiki.Clustering*` contains the clustering shared projects (`Chishiki.Clustering` = shared contracts, `.Client` = client-side helpers, `.Server` = silo-side helpers).
- The top-level `src/shared/` is the global shared layer and does **not** contain an inner `shared/` subfolder — it is itself that layer.
- The only explicit project references today are from the API and host to `Chishiki.Infrastructure.Aspire.ServiceDefaults`; preserve that dependency direction unless the architecture is intentionally changing.

## Source and tests layout

### `src/` folder convention

Every top-level area under `src/` (except `shared/`) contains a `shared/` subfolder for code shared across projects in that area, plus additional subfolders organised by topic.

```
src/
├── backend/
│   ├── shared/                    ← code shared across backend services
│   ├── Chishiki.API.Web/          ← ASP.NET Core minimal API
│   ├── Chishiki.Host/             ← Orleans silo host
│   ├── Chishiki.Hub/              ← Developer Hub MCP server  *(planned)*
│   └── Chishiki.Security/         ← Security analysis orchestration  *(planned)*
├── frontend/
│   └── shared/                    ← code shared across frontend apps
├── infrastructure/
│   ├── shared/                    ← cross-cutting infra helpers
│   └── aspire/
│       ├── Chishiki.Infrastructure.Aspire.AppHost/
│       └── Chishiki.Infrastructure.Aspire.ServiceDefaults/
└── shared/                        ← global cross-layer libraries
    ├── Chishiki.Core/
    ├── Chishiki.Hub.Contracts/    ← Hub DTOs and interfaces  *(planned)*
    ├── Chishiki.Security.Contracts/ ← Security finding types  *(planned)*
    └── clustering/
        ├── Chishiki.Clustering/
        ├── Chishiki.Clustering.Client/
        └── Chishiki.Clustering.Server/
```

**Rules**:
- `src/shared/` is the **global** shared layer; its projects may be referenced from any other area.
- Each area's inner `shared/` is **area-scoped**: only projects within that same area reference it.
- No cross-area references: `backend/` must not reference `frontend/` and vice versa.
- When adding a new service or library, place it in the matching area folder and add a `shared/` project for the area if it does not already exist.

### `tests/` folder convention

The `tests/` tree mirrors `src/` exactly. For every source project there are up to three test projects:

| Suffix | Description | Infrastructure |
|---|---|---|
| `.UnitTests` | Fast, fully isolated — no I/O, no real dependencies | NSubstitute mocks only |
| `.IntegrationTests` | One service with real infrastructure | Aspire `DistributedApplicationTestingBuilder` or `WebApplicationFactory` |
| `.E2ETests` | Full-stack user flows end-to-end | Aspire `DistributedApplicationTestingBuilder` with all services up |

```
tests/
├── backend/
│   ├── shared/                                       ← shared builders, fixtures, fakes for backend tests
│   ├── Chishiki.API.Web.UnitTests/
│   ├── Chishiki.API.Web.IntegrationTests/
│   ├── Chishiki.API.Web.E2ETests/
│   ├── Chishiki.Host.UnitTests/
│   ├── Chishiki.Host.IntegrationTests/
│   ├── Chishiki.Host.E2ETests/
│   ├── Chishiki.Hub.UnitTests/
│   ├── Chishiki.Hub.IntegrationTests/
│   ├── Chishiki.Hub.E2ETests/
│   ├── Chishiki.Security.UnitTests/
│   └── Chishiki.Security.IntegrationTests/
├── frontend/
│   └── shared/
├── infrastructure/
│   ├── shared/
│   └── aspire/
│       ├── Chishiki.Infrastructure.Aspire.ServiceDefaults.UnitTests/
│       └── Chishiki.Infrastructure.Aspire.AppHost.IntegrationTests/
└── shared/
    ├── Chishiki.Core.UnitTests/
    ├── Chishiki.Core.IntegrationTests/
    ├── Chishiki.Hub.Contracts.UnitTests/
    ├── Chishiki.Security.Contracts.UnitTests/
    └── clustering/
        ├── Chishiki.Clustering.UnitTests/
        └── Chishiki.Clustering.IntegrationTests/
```

**Rules**:
- A test project's path under `tests/` must match its source project's path under `src/`.
- Each `tests/{area}/shared/` project contains builders, custom assertions, fake implementations, and test fixtures shared across that area's test projects.
- E2E tests are only created for projects that expose an external surface (HTTP endpoints, grain interfaces); library-only projects have Unit + Integration only.

## Testing conventions

### Frameworks and packages

| Package | Purpose |
|---|---|
| `NUnit` 4.x | Test framework — attributes, assertions, test lifecycle |
| `NSubstitute` 5.x | Mocking and argument matching |
| `Aspire.Hosting.Testing` | `DistributedApplicationTestingBuilder` for integration and E2E tests |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` for lightweight API integration tests |

### Project defaults for every test project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="NUnit" Version="4.*" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
  </ItemGroup>
</Project>
```

Integration and E2E projects additionally reference `Aspire.Hosting.Testing`.

### Test class and method conventions

- Test classes are named `{TestedClass}Tests` and decorated with `[TestFixture]`.
- Test methods follow the pattern `{Method}_{Context}_{ExpectedOutcome}` (e.g., `CreateUser_WhenEmailAlreadyExists_ThrowsDomainException`).
- Apply `[Category("Unit")]`, `[Category("Integration")]`, or `[Category("E2E")]` on every test class to enable tier-based filtering.
- Use `[SetUp]` / `[TearDown]` for per-test lifecycle; use `[OneTimeSetUp]` / `[OneTimeTearDown]` for expensive fixtures shared within a class.
- Use `NSubstitute.Substitute.For<T>()` for all external dependencies in unit tests; never perform real I/O in a unit test.

### Integration and E2E infrastructure pattern

```csharp
[TestFixture, Category("Integration")]
public class WeatherForecastApiTests
{
    private DistributedApplication _app = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Chishiki_Infrastructure_Aspire_AppHost>();
        _app    = await appHost.BuildAsync();
        await _app.StartAsync();
        _client = _app.CreateHttpClient("api");
    }

    [OneTimeTearDown]
    public async Task StopAsync() => await _app.DisposeAsync();

    [Test]
    public async Task GetWeatherForecast_ReturnsOk()
    {
        var response = await _client.GetAsync("/weatherforecast");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
```

### Keeping tests in sync with source code

- When a public method, endpoint, grain interface, or constructor signature changes in source, update all corresponding test classes in the **same commit**.
- When a new source project is added, immediately create its test projects (Unit, Integration, and E2E where applicable) under the matching `tests/` path.
- When a source project is deleted, delete its test projects in the same commit.
- Pull requests that modify source code without updating affected tests must include an explicit justification in the PR description.

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

## Developer Hub

Chishiki is a **developer hub** — a self-contained platform for managing and distributing GitHub Copilot customization assets via a dedicated MCP server. The hub is the primary product surface of the repository.

### Hub resource taxonomy

| Resource | Storage location | Description |
|---|---|---|
| **Templates** | `hub/templates/` | Project scaffolding templates (.NET, frontend, infra) |
| **Agents** | `.github/agents/` | Copilot agent definition files (`.chatmode.md`) |
| **Hooks** | `.github/hooks.json` | Copilot session hooks (logging, scanning, license checks) |
| **Instructions** | `.github/instructions/` | Language and domain-specific coding instructions |
| **Skills** | `.github/skills/` | Reusable agent skills (`SKILL.md`) |
| **Collections** | `hub/collections/` | Curated bundles: a named set of agents + instructions + skills |

### Hub MCP server (`Chishiki.Hub`)

- Lives at `src/backend/Chishiki.Hub` — an ASP.NET Core minimal API with MCP-compatible HTTP transport.
- Registered in `.mcp.json` under key `hub` once implemented (HTTP transport, `http://localhost:5010/mcp`).
- Contracts shared with other services live in `src/shared/Chishiki.Hub.Contracts`.
- Hub resource state is persisted in PostgreSQL (via EF Core) with Orleans grains as the caching layer.

**MCP tools exposed by the Hub:**

| Tool | Description |
|---|---|
| `list_resources` | List resources of a given type (templates / agents / hooks / instructions / skills / collections) |
| `get_resource` | Retrieve the full content of a named resource |
| `create_resource` | Scaffold a new resource from a built-in template |
| `apply_collection` | Enable all assets in a named collection for the current workspace |
| `scaffold_project` | Generate a new project from a named template into a target directory |
| `search_resources` | Full-text search across resource descriptions and tags |

### Hub conventions

- Every resource file must carry a YAML front-matter block with `name`, `description`, `tags`, and `version`.
- Collections are YAML manifests in `hub/collections/<name>.yaml` that reference resource names by type and key.
- The Hub service is development-only; it has no production deployment target.
- When a new skill or agent is added to `.github/`, also register it in the hub via a seed file in `hub/seed/`.

## Security analysis and exploit testing

Chishiki integrates open source/free security tooling to scan source code, NuGet dependencies, and container images for vulnerabilities, and converts findings into executable NUnit security tests.

### Integrated scanners

| Tool | Type | Docker image | What it scans |
|---|---|---|---|
| **Semgrep** | SAST | `semgrep/semgrep` | Source code — rules from `semgrep.dev/r` |
| **SecurityCodeScan** | SAST (Roslyn) | NuGet analyzer, runs in `dotnet build` | C# source code |
| **SonarQube Community** | SAST + metrics | `sonarqube:community` | Multi-language source quality and security |
| **OWASP Dependency-Check** | SCA | `owasp/dependency-check` | NuGet package CVEs |
| **Trivy** | SCA + containers | `aquasec/trivy` | Container images, filesystem, NuGet |
| **gitleaks** | Secret scanning | `zricethezav/gitleaks` | Git history and working tree |

All scanners run as Docker containers registered in `AppHost.cs` under the `security` launch profile so they are **not** started in the default dev stack.

### Running security scans locally

```powershell
# Start only the security scanner containers
dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost -- --launch-profile security
```

Tests with category `Security` can be run independently:

```powershell
dotnet test .\Chishiki.slnx --filter "TestCategory=Security"
```

### Security finding model (`Chishiki.Security.Contracts`)

```csharp
public record SecurityFinding(
    string        Tool,
    Severity      Severity,      // Critical | High | Medium | Low | Info
    string        RuleId,
    string        Title,
    string        Description,
    string        FilePath,
    int?          Line,
    string?       Cve,
    string?       Remediation
);
```

### Exploit test generation pattern

When a finding has a known exploit pattern the security service (`Chishiki.Security`) generates a NUnit test that:

1. **Reproduces** the exploit with a representative payload.
2. Is initially **red** (expected to fail) — the test documents the vulnerability.
3. Turns **green** once the fix is applied and stays in the suite as a regression guard.
4. Carries `[Category("Security")]` (combinable with `Unit`, `Integration`, or `E2E`).
5. Lives under `tests/{area}/security/` mirroring the source tree.

```csharp
[TestFixture, Category("Security"), Category("Unit")]
public class SqlInjectionSecurityTests
{
    [Test]
    [Description("SEC-0042 — raw string interpolation in SQL query (Semgrep rule sqli-formatstring)")]
    public void BuildQuery_WithUntrustedInput_ShouldParameterize()
    {
        const string maliciousInput = "' OR '1'='1";

        var query = QueryBuilder.Build(maliciousInput);

        Assert.That(query.CommandText, Does.Not.Contain(maliciousInput));
        Assert.That(query.Parameters,  Has.Count.GreaterThan(0));
    }
}
```

### Security conventions

- Security test classes follow the naming `{TestedClass}SecurityTests`.
- Every generated test must reference the originating finding ID in its `[Description]` attribute.
- Findings that have no automated exploit pattern are tracked as GitHub issues with label `security-finding`.
- Scanner raw output is **not** committed; only generated test files and finding metadata (persisted to PostgreSQL via `Chishiki.Security`) are kept.

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

## Cross-platform scripts library

The repository contains a `scripts/` folder with developer automation scripts organised by topic. Every script is available for all three supported platforms and behaves identically across them.

### Folder layout

```
scripts/
├── README.md             ← index, standards, and usage examples
├── windows/              ← PowerShell 7+ (.ps1)
│   ├── setup/
│   ├── dev/
│   └── docker/
├── linux/                ← Bash 4+ (.sh) — all distributions
│   ├── setup/
│   ├── dev/
│   └── docker/
└── macos/                ← Zsh 5.8+ (.zsh)
    ├── setup/
    ├── dev/
    └── docker/
```

### Script topics

| Topic      | Scripts                                       | Purpose                                          |
|------------|-----------------------------------------------|--------------------------------------------------|
| `setup/`   | `setup-devenv`                                | One-command orchestrator — runs all setup scripts in order |
|            | `install-prereqs`                             | Package managers (Chocolatey/Scoop/Homebrew) + .NET SDK |
|            | `container-prereqs` *(Windows only)*          | Hyper-V, WSL2, Ubuntu                           |
|            | `container-runtime`                           | Docker / Podman detection and install            |
|            | `dev-env`                                     | Git, VS Code, PowerShell, Oh My Posh             |
|            | `mcp-setup`                                   | Node.js, uv, GitHub CLI, MCP server packages     |
|            | `install-ide`                                 | IDE — VS 2026 Professional (Win) or JetBrains Rider |
| `dev/`     | `start`, `stop`, `reset-data`                 | Local development lifecycle via Aspire AppHost   |
| `docker/`  | `build-all`, `clean`                          | Docker image build and cleanup                   |

### Authoring conventions — follow these for all new scripts

- **Same base name across all three platforms**: `foo.ps1` / `foo.sh` / `foo.zsh`
- **`--help` / `-Help` flag** must be supported by every script and must print the synopsis block
- **ANSI color helpers** — copy the 6-function block (`header`, `step`, `ok`, `warn`, `fail`, `info`) from any existing script in the same platform folder
- **Idempotent** — running a script multiple times must produce the same observable result
- **Strict error mode**: `set -euo pipefail` + `trap ERR` (bash/zsh), `Set-StrictMode -Version Latest` + `$ErrorActionPreference = 'Stop'` (PowerShell)
- **Repo-root resolution** — resolve the repo root relative to the script's own path so the script works from any working directory
  - PowerShell: `(Get-Item $PSScriptRoot).Parent.Parent.Parent.FullName`
  - bash: `"$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"`
  - zsh: `"$(cd "${0:A:h}/../../.." && pwd)"`
- **Line endings** are enforced by `.gitattributes`: `.ps1` → CRLF, `.sh`/`.zsh` → LF

### Platform install backends

| Platform | Backend    | Auto-install flag |
|----------|------------|-------------------|
| Windows  | `winget`   | `-Install`        |
| Linux    | apt/dnf/pacman | `--install`   |
| macOS    | Homebrew   | `--install`       |

## Working assumptions for future sessions

- Prefer the Aspire AppHost workflow when you need the full stack locally.
- When introducing new backend code, stay aligned with the existing split between:
  - orchestration in AppHost
  - service startup in each `Program.cs`
  - shared observability and resilience setup in `ServiceDefaults`
- When adding or changing source code, immediately create or update the corresponding test projects under `tests/` following the **Testing conventions** section. Unit, Integration, and E2E test projects must be kept in sync with their source counterpart.
- `src/frontend/` is the intended location for frontend projects; structure it following the same `shared/` + topic subfolder convention described in **Source and tests layout**.
- When adding a new developer script, create it in all three platform folders (`windows/`, `linux/`, `macos/`) under the same topic subfolder, and update `scripts/README.md`.
- When adding a new skill or agent to `.github/`, add the corresponding seed entry under `hub/seed/` so the hub database stays in sync with the file system.
- Security scanners are **never** started in the default `dotnet run` profile; always gate them behind the `security` launch profile in AppHost.
- Every new public API surface (controller action, grain method, MCP tool) must have at minimum a unit test and, where applicable, a security test if the input is user-controlled.
