---
name: chishiki-mcp-tools
description: 'Skill for adding MCP tools to Chishiki.MCP.Host. Use when creating tool classes, adding ModelContextProtocol attributes, wiring source-generated logging, serializing tool responses, or extending grouped tool surfaces under src/mcp.'
---

# Chishiki MCP Tools

Use this skill when adding or updating MCP tools in `src/mcp/Chishiki.MCP.Host/Tools/`.

## When to Use This Skill

- Create a new tool group in the MCP host
- Extend an existing tool class with new tool methods
- Standardize logging, serialization, or naming for MCP tools
- Align new tools with Chishiki vertical slices and handler patterns

## Tool Class Conventions

- Place tool classes in `src/mcp/Chishiki.MCP.Host/Tools/<GroupName>Tools.cs`.
- Declare tool classes as `partial` so source-generated logging fits cleanly.
- Use `[McpServerToolType]` on the class.
- Use primary constructor injection for dependencies.
- Prefer `internal sealed partial class` for concrete tool groups.

## Tool Method Conventions

Each tool method should:

- Use `[McpServerTool(Name = "snake_case")]`
- Use `[Description("...")]` on the method and meaningful parameters
- Return `Task<string>`
- Serialize structured results with `System.Text.Json.JsonSerializer.Serialize(...)`
- Forward and observe `CancellationToken`

## Required Logging Pattern

Use source-generated logging only.

```csharp
[LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' invoked")]
private partial void LogToolInvoked(string toolName);

[LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' completed successfully")]
private partial void LogToolCompleted(string toolName);

[LoggerMessage(Level = LogLevel.Error, Message = "MCP tool '{ToolName}' failed")]
private partial void LogToolFailed(string toolName, Exception ex);
```

The exception parameter must be last.

## Complete Tool Class Example

```csharp
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Chishiki.MCP.Host.Tools;

[McpServerToolType]
internal sealed partial class ChishikiSystemTools(ILogger<ChishikiSystemTools> logger)
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' invoked")]
    private partial void LogToolInvoked(string toolName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' completed successfully")]
    private partial void LogToolCompleted(string toolName);

    [LoggerMessage(Level = LogLevel.Error, Message = "MCP tool '{ToolName}' failed")]
    private partial void LogToolFailed(string toolName, Exception ex);

    [McpServerTool(Name = "get_server_info")]
    [Description("Returns version and runtime information for the Chishiki MCP host.")]
    public Task<string> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogToolInvoked("get_server_info");

        try
        {
            var payload = new
            {
                Name = "Chishiki",
                Transport = "StreamableHTTP",
                Endpoint = "/mcp"
            };

            LogToolCompleted("get_server_info");
            return Task.FromResult(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            LogToolFailed("get_server_info", ex);
            throw;
        }
    }
}
```

## Planned Tool Groups

| Class | Group | Purpose |
| --- | --- | --- |
| `ChishikiSystemTools` | `system` | Host metadata, health, diagnostics |
| `RagQueryTools` | `rag` | Semantic retrieval and synthesized answers |
| `HubResourceTools` | `hub` | Hub or resource discovery operations |
| `IngestionTools` | `ingest` | Document import, indexing, source management |
| `SecurityScanTools` | `security` | Security-oriented analysis and guardrails |

## Integration Guidance

- Keep tools thin; place feature orchestration in handlers or slice-specific services.
- Match tool naming to user-facing MCP capability names.
- Prefer one cohesive tool group per domain rather than dumping unrelated tools together.
- Keep descriptions explicit so clients can discover tools accurately.

## Gotchas

- **Do not** return ad hoc object instances without explicit JSON serialization.
- **Do not** call `logger.LogInformation(...)` or similar direct APIs.
- **Do not** hide business workflows inside tool methods; delegate to handlers or services.
- **Always** use snake_case tool names.

## References

- [Chishiki repository conventions](C:\work\personal\Chishiki\skills\chishiki-repo-conventions\SKILL.md)
- [Chishiki RAG development skill](..\chishiki-rag-development\SKILL.md)
