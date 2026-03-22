# 📋 Changelog

All notable changes to **Chishiki** are documented here.

This project adheres to [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### ✨ Added
- Full infrastructure stack via `.NET Aspire` AppHost (PostgreSQL, Redis, Keycloak, Qdrant, Ollama, Prometheus, Grafana)
- Orleans 10 silo host with Redis-backed clustering, grain storage, PubSubStore, and reminders
- ASP.NET Core Minimal API with OpenAPI / Swagger UI (development only)
- OpenTelemetry instrumentation with Prometheus exporter on `/metrics`
- Health endpoints (`/health`, `/alive`) via `ServiceDefaults`
- Custom Keycloak realm `chishiki` with OIDC client and login theme
- PostgreSQL `pgvector` extension via shell-based init scripts
- Qdrant vector database with HTTP + gRPC support
- Ollama container with startup script for pulling `nomic-embed-text`
- MCP server configuration (`.mcp.json`) with GitHub, Microsoft Learn, Azure, NuGet, fetch, sequential-thinking, postgres, memory servers
- GitHub Copilot hooks: `secrets-scanner`, `session-logger`, `dependency-license-checker`
- Professional project files: `.gitignore`, `.gitattributes`, `.editorconfig`, `README.md`, `CONTRIBUTING.md`, `SECURITY.md`

### 🔧 Fixed
- CRLF → LF line endings in all container shell scripts (fixes exit code 255 in Linux containers)
- Aspire AppHost: corrected Docker build context path (`../../../../` from AppHost)
- `Chishiki.Host` Dockerfile: switched base image from `runtime` to `aspnet` (fixes `Microsoft.AspNetCore.App` missing)
- `Chishiki.Host` Dockerfile: added explicit project path to `dotnet restore` (fixes MSB1003)

---

<!-- Links -->
[Unreleased]: https://github.com/your-org/chishiki/compare/HEAD...HEAD
