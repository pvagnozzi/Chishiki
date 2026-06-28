---
name: chishiki-aspire-host
description: 'Skill for evolving the Chishiki Aspire AppHost. Use when adding services to AppHost.cs, wiring container infrastructure, configuring ServiceDefaults, exposing endpoints, or running the full Chishiki stack locally.'
---

# Chishiki Aspire Host

Use this skill when working on `src/infrastructure/aspire/` and the local orchestration layer for Chishiki.

## When to Use This Skill

- Add a new service, worker, or infrastructure dependency to `AppHost.cs`
- Wire references between projects, containers, and supporting infrastructure
- Expose HTTP endpoints or external endpoints for local development
- Configure the Chishiki security profile or other environment-driven behavior
- Update ServiceDefaults for cross-cutting concerns such as telemetry and health checks

## AppHost Patterns

- Use `builder.AddProject<TProject>(...)` for .NET projects.
- Use `builder.AddDockerfile(...)` for repository-owned containers under `containers/`.
- Use `.WithReference(...)` to express dependencies and service discovery.
- Use `.WithExternalHttpEndpoints()` when a service must be reachable outside the internal network.
- Keep orchestration intent in AppHost; keep service-specific code inside the service project itself.

## Adding a Service

```csharp
var worker = builder.AddProject<Projects.Chishiki_Worker>("chishiki-worker")
    .WithReference(kafka)
    .WithReference(ollama)
    .WithReference(redis);

var mcp = builder.AddProject<Projects.Chishiki_MCP_Host>("chishiki-mcp")
    .WithReference(worker)
    .WithReference(qdrant)
    .WithExternalHttpEndpoints();
```

## Container Infrastructure Pattern

Repository-owned images should be declared from the `containers/` folder.

```csharp
var keycloak = builder.AddDockerfile("keycloak", "../../../../containers/keycloak")
    .WithHttpEndpoint(targetPort: 8080)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin");
```

Use clear resource names and keep relative paths aligned with the AppHost project location.

## Environment and Profiles

Use environment variables for opt-in profiles and local feature switches.

```powershell
$env:CHISHIKI_SECURITY_PROFILE = "true"
dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
```

Treat `CHISHIKI_SECURITY_PROFILE` as an orchestration concern that changes which services or behaviors light up locally.

## What Belongs in ServiceDefaults

Put cross-cutting infrastructure in `Chishiki.Infrastructure.Aspire.ServiceDefaults`, such as:

- OpenTelemetry setup
- Health checks and health endpoints
- Service discovery defaults
- Resilience defaults shared across services
- Common HTTP client and diagnostics wiring

Avoid pushing feature-specific business rules into ServiceDefaults.

## Gotchas

- **Do not** duplicate cross-cutting setup in every host; centralize it in ServiceDefaults when it truly applies broadly.
- **Do not** hardcode secret values in AppHost declarations.
- **Always** use `.WithReference(...)` to model dependencies instead of relying on startup ordering assumptions alone.
- **Always** keep AppHost resource names stable because they influence discovery and diagnostics.

## References

- [Chishiki repository conventions](C:\work\personal\Chishiki\skills\chishiki-repo-conventions\SKILL.md)
- [Aspire skill](..\aspire\SKILL.md)
