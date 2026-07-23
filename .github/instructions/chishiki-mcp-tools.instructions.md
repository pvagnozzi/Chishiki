---
applyTo: 'src/mcp/**/*.cs'
description: 'Conventions for MCP tool classes in Chishiki.MCP.Host: attributes, logging, serialization, and vertical slice feature structure.'
---

# Chishiki MCP Host Conventions

Apply these rules to code under `src/mcp/`.

## Tool Class Structure

- Place tool groups in `src/mcp/Chishiki.MCP.Host/Tools/<GroupName>Tools.cs`.
- Use `internal sealed partial class` for concrete tool classes.
- Apply `[McpServerToolType]` to each tool class.
- Use primary constructor injection for dependencies.

## Tool Method Pattern

- Use `[McpServerTool(Name = "snake_case")]`.
- Use `[Description("...")]` on methods and meaningful parameters.
- Return `Task<string>`.
- Serialize payloads with `System.Text.Json.JsonSerializer.Serialize(...)`.
- Keep tool methods thin; delegate workflow logic to handlers or services.

## Logging Pattern

Every tool class should expose source-generated logging methods equivalent to:

```csharp
[LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' invoked")]
private partial void LogToolInvoked(string toolName);

[LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' completed successfully")]
private partial void LogToolCompleted(string toolName);

[LoggerMessage(Level = LogLevel.Error, Message = "MCP tool '{ToolName}' failed")]
private partial void LogToolFailed(string toolName, Exception ex);
```

Do not introduce direct `logger.LogXxx(...)` calls.

## Vertical Slices

- Organize host features as vertical slices under `Features/`.
- Keep request DTOs, handlers, validators, and tool adapters close together.
- Prefer dedicated slices for RAG, ingestion, system, and future domains instead of horizontal layering.

## Response Shaping

- Return structured JSON strings that MCP clients can consume predictably.
- Validate and observe `CancellationToken` parameters.
- Keep method and description text in English.
