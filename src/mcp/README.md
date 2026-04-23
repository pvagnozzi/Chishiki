# src/mcp — MCP Servers

ASP.NET Core services that expose functionality as Model Context Protocol (MCP) tools,
consumed directly by VS 2026 / GitHub Copilot via `.mcp.json`.

| Project | Port | `.mcp.json` key | Status |
|---|---|---|---|
| `Chishiki.MCP.Host` | 5010 | `hub` | ✅ Running |

## Running locally

```powershell
# Standalone
dotnet run --project src/mcp/Chishiki.MCP.Host/Chishiki.MCP.Host.csproj

# Via Aspire (full stack)
dotnet run --project src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.AppHost/Chishiki.Infrastructure.Aspire.AppHost.csproj
```

The MCP endpoint is available at `http://localhost:5010/mcp` once the server is running.
VS 2026 and GitHub Copilot pick it up automatically from `.mcp.json`.

## Tool groups

| Group | Class | Status |
|---|---|---|
| `system` | `ChishikiSystemTools` | ✅ Available |
| `hub` | `HubResourceTools` *(planned)* | Phase 2 |
| `rag` | `RagQueryTools` *(planned)* | Phase 4 |
| `security` | `SecurityScanTools` *(planned)* | Phase 6 |
| `ingest` | `IngestionTools` *(planned)* | Phase 3 |
