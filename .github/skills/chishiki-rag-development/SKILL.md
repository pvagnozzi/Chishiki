---
name: chishiki-rag-development
description: 'Skill for building and evolving Chishiki RAG features. Use when adding semantic search, document ingestion, Kernel Memory handlers, Qdrant collection operations, or MCP-facing RAG tools in the Chishiki repository.'
---

# Chishiki RAG Development

Use this skill when working on retrieval-augmented generation flows in Chishiki, especially in the MCP host and shared AI libraries.

## When to Use This Skill

- Add a new RAG feature or vertical slice in `src/mcp/Chishiki.MCP.Host/Features/Rag/`
- Modify Microsoft Kernel Memory integration or `IKernelMemory` usage
- Implement document ingestion, semantic search, or answer synthesis flows
- Add or tune Qdrant-backed vector operations and collection management
- Expose RAG capabilities through MCP tools or request handlers

## Core Architecture Rules

- Inject and depend on `IKernelMemory`, not concrete Kernel Memory implementations.
- Keep ingestion and query responsibilities separate.
- Preserve the vertical-slice layout: request, handler, and tool adapter stay together.
- Keep shared RAG logic reusable; do not couple it unnecessarily to runnable hosts.
- Prefer `System.Text.Json` for tool responses and data interchange.

## Vertical Slice Pattern

Organize RAG features as self-contained slices:

```text
src/mcp/Chishiki.MCP.Host/
└── Features/
    └── Rag/
        ├── QueryKnowledgeBase/
        │   ├── QueryKnowledgeBaseRequest.cs
        │   ├── QueryKnowledgeBaseHandler.cs
        │   └── QueryKnowledgeBaseTool.cs
        └── IngestDocument/
            ├── IngestDocumentRequest.cs
            ├── IngestDocumentHandler.cs
            └── IngestDocumentTool.cs
```

Each slice should own its DTO, orchestration logic, validation, and MCP-facing adapter.

## Kernel Memory Usage Pattern

Use dependency injection and forward cancellation tokens consistently.

```csharp
internal sealed class QueryKnowledgeBaseHandler(IKernelMemory memory)
{
    public async Task<string> HandleAsync(string question, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var answer = await memory.AskAsync(question, cancellationToken: cancellationToken);
        return answer.Result;
    }
}
```

## Ingestion vs Query Separation

Keep these concerns distinct:

| Concern | Typical responsibility |
| --- | --- |
| Ingestion | Accept source content, normalize metadata, import into Kernel Memory, manage indexing |
| Query | Accept a user question, run semantic retrieval, synthesize an answer, shape MCP output |

**Never** mix indexing workflows into answer-query handlers unless the feature explicitly requires a controlled pre-query ingestion step.

## MCP Tool Pattern for RAG

Use MCP tools as thin adapters over feature handlers.

```csharp
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Chishiki.MCP.Host.Tools;

[McpServerToolType]
internal sealed partial class RagQueryTools(
    IKernelMemory memory,
    ILogger<RagQueryTools> logger)
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' invoked")]
    private partial void LogToolInvoked(string toolName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' completed successfully")]
    private partial void LogToolCompleted(string toolName);

    [LoggerMessage(Level = LogLevel.Error, Message = "MCP tool '{ToolName}' failed")]
    private partial void LogToolFailed(string toolName, Exception ex);

    [McpServerTool(Name = "rag_query"), Description("Answers a question using the Chishiki knowledge base.")]
    public async Task<string> QueryAsync(
        [Description("Natural-language question.")] string question,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogToolInvoked("rag_query");

        try
        {
            var answer = await memory.AskAsync(question, cancellationToken: cancellationToken);
            LogToolCompleted("rag_query");
            return JsonSerializer.Serialize(answer);
        }
        catch (Exception ex)
        {
            LogToolFailed("rag_query", ex);
            throw;
        }
    }
}
```

## Qdrant Guidance

- Treat Qdrant as the primary vector store unless the feature explicitly targets PostgreSQL pgvector.
- Use stable collection naming and metadata conventions so retrieval flows remain diagnosable.
- Keep collection lifecycle changes explicit and reviewable.
- Prefer repository- or handler-level abstractions over scattering Qdrant-specific logic across tools.

## Gotchas

- **Do not** inject concrete memory server types into host features. Use `IKernelMemory` so implementations remain swappable.
- **Do not** combine ingestion and query logic in one handler just because both touch the same collection.
- **Always** propagate `CancellationToken` into Kernel Memory calls.
- **Always** serialize MCP results explicitly with `JsonSerializer.Serialize(...)`.

## References

- [Chishiki repository conventions](C:\work\personal\Chishiki\skills\chishiki-repo-conventions\SKILL.md)
- [Chishiki MCP tools skill](..\chishiki-mcp-tools\SKILL.md)
