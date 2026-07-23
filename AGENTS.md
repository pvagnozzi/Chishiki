# Chishiki — Pi Project Instructions

Use this file as the primary Pi entry point for repository guidance.

## Start here

1. Read `skills/chishiki-repo-conventions/SKILL.md`.
2. Then read:
   - `skills/chishiki-repo-conventions/references/architecture-layout-and-build.md`
   - `skills/chishiki-repo-conventions/references/csharp-conventions.md`
3. Use `.github/copilot-instructions.md` as a secondary reference only when you need the original Copilot wording.

## Repository layout

- `src/infrastructure/aspire/` — Aspire orchestration and shared service defaults
- `src/mcp/Chishiki.MCP.Host/` — MCP host
- `src/shared/ai/` — AI libraries
- `src/shared/core/` — core libraries and EF Core data access
- `src/shared/orleans/` — Orleans-related libraries
- `src/shared/vision/` — vision pipelines and detectors
- `tests/` — test projects

Prefer the live repository tree over stale instructions.

## Commands

```powershell
dotnet build .\Chishiki.slnx
dotnet test .\Chishiki.slnx
dotnet run --project .\src\mcp\Chishiki.MCP.Host\Chishiki.MCP.Host.csproj
dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
```

## Change rules

- Make the smallest safe change that satisfies the task.
- Preserve the standard C# file header and update `Modified:` on meaningful edits.
- Keep XML documentation complete and in English.
- Do not mix requested work with unrelated refactors.
- Prefer project-local conventions already present in nearby files.

## Logging rules

- Use the .NET source-generated logging model with `[LoggerMessage]`.
- Do not introduce direct `logger.LogInformation(...)` / `logger.LogError(...)` style calls.
- Use PascalCase placeholders, for example `{DbContextName}` and `{PendingMigrations}`.
- Keep the `Exception` parameter last.
- Do not use interpolated strings inside log templates.
- Add logs at meaningful boundaries: startup, external I/O, failure paths, and important lifecycle events.

## Verification

- Run the smallest relevant verification for the change.
- For solution-wide validation, prefer `dotnet build .\Chishiki.slnx` and `dotnet test .\Chishiki.slnx` when the working tree is buildable.
- If verification is blocked by pre-existing repository issues, report the blocker clearly instead of masking it.
