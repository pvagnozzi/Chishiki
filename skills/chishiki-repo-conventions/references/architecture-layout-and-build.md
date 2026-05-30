# Chishiki Architecture, Layout, and Build

This reference captures the repository-specific architecture and layout guidance that remains useful after converting the GitHub Copilot instructions to Pi resources. It is intentionally curated rather than copied verbatim so it matches the current repository structure.

## Architecture Summary

Chishiki is a cloud-native distributed RAG platform exposed through an MCP host and built around .NET, Orleans, Aspire, and shared libraries under `src/shared`.

At a high level:

- `src/mcp/Chishiki.MCP.Host/` contains the MCP host
- `src/infrastructure/aspire/` contains Aspire orchestration resources
- `src/shared/` contains reusable libraries such as `core`, `ai`, `orleans`, and `vision`
- `tests/` contains test projects
- `containers/`, `docs/`, and `scripts/` hold supporting assets and operational material

## Current Repository Layout

```text
/
├── src/
│   ├── infrastructure/
│   │   └── aspire/
│   ├── mcp/
│   │   └── Chishiki.MCP.Host/
│   └── shared/
│       ├── ai/
│       ├── core/
│       ├── orleans/
│       └── vision/
├── tests/
├── containers/
├── docs/
├── scripts/
├── .mcp.json
└── Chishiki.slnx
```

## Dependency and Boundary Guidance

Use the repository structure as the source of truth when deciding project boundaries.

- Shared libraries under `src/shared/` should stay reusable and should not take unnecessary dependencies on runnable hosts.
- Prefer communication through defined abstractions and contracts rather than coupling unrelated projects directly.
- Keep edits local to the feature or library being changed.
- If an older instruction references top-level `shared/`, `infrastructure/`, or `k8s/` folders, verify against the current tree before acting; the live repository uses `src/shared/`, `src/infrastructure/`, and `src/mcp/`.

## Build and Test Commands

Use the solution file at the repository root for standard build and test flows.

```powershell
dotnet build .\Chishiki.slnx
dotnet test .\Chishiki.slnx
```

Examples for focused work:

```powershell
dotnet test .\tests\<Project>\<Project>.csproj
dotnet test .\Chishiki.slnx --filter "FullyQualifiedName~MethodName_Scenario"
```

Run the MCP host directly with:

```powershell
dotnet run --project .\src\mcp\Chishiki.MCP.Host\Chishiki.MCP.Host.csproj
```

Run the Aspire orchestration entry point with:

```powershell
dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
```

## MCP Context

The repository root `.mcp.json` registers MCP server configuration used by local tooling. Treat it as environment/configuration context, not as a generic instruction file.

## Curation Notes

The original Copilot instructions contained useful repository context but also some stale path examples. This Pi reference keeps the intent while correcting the structure to the current workspace.
