# src/infrastructure — Infrastructure

Aspire orchestration and shared service defaults for the Chishiki platform.

| Project | Description |
|---|---|
| `Chishiki.Infrastructure.Aspire.AppHost` | Aspire AppHost — declares all services (PostgreSQL, Redis, Keycloak, Qdrant, Ollama, Prometheus, Grafana, Orleans silo, API Web, Hub MCP server) and wires up their dependencies for one-command local startup. |
| `Chishiki.Infrastructure.Aspire.ServiceDefaults` | Shared service defaults — configures OpenTelemetry, health checks, resilience (retry/circuit-breaker), and service discovery for all .NET services in the stack. |

## Starting the full stack

```powershell
dotnet run --project src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.AppHost/Chishiki.Infrastructure.Aspire.AppHost.csproj
```

The Aspire Dashboard is available at **http://localhost:15888**.

## Security scanners (opt-in)

Set `CHISHIKI_SECURITY_PROFILE=true` before starting AppHost to activate the Semgrep, SonarQube, OWASP Dependency-Check, Trivy, and gitleaks containers.
