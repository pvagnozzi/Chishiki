# 🧠 Chishiki — GitHub Copilot Instructions

> **Chishiki** (知識 — *knowledge* in Japanese) is a cloud-native, distributed **RAG platform** exposed as a
> **Model Context Protocol (MCP) server**, built on **.NET 10**, **Microsoft Orleans**, **.NET Aspire**,
> and **Microsoft Kernel Memory**. Designed for on-premises or Azure deployment, including **Kubernetes**.

> Pi users should start from the repo-local guidance in `skills/chishiki-repo-conventions/SKILL.md` and its
> reference files. Those Pi resources are the maintained conversion of these repository conventions for Pi-based
> workflows.

---

## 📋 Table of Contents

- [🤖 Pi Guidance](#-pi-guidance)
- [🏗️ Architecture](#-architecture)
- [🗂️ Repository Layout](#-repository-layout)
- [🔨 Build, Test and Run](#-build-test-and-run)
- [📐 Design Principles](#-design-principles)
- [📁 File Header Convention](#-file-header-convention)
- [📝 XML Documentation](#-xml-documentation)
- [🧩 MCP Tool Conventions](#-mcp-tool-conventions)
- [🌾 Orleans Grain Conventions](#-orleans-grain-conventions)
- [👷 Worker Pattern: External Long-Running Services](#-worker-pattern-external-long-running-services)
- [📚 Kernel Memory Integration](#-kernel-memory-integration)
- [🪵 Logging](#-logging)
- [💉 Dependency Injection](#-dependency-injection)
- [⚡ Async](#-async)
- [🎨 Code Style](#-code-style)
- [🧪 Testing](#-testing)
- [☸️ Kubernetes](#-kubernetes)

---

## 🤖 Pi Guidance

If you are working in this repository with Pi, prefer these repo-local resources first:

- `skills/chishiki-repo-conventions/SKILL.md`
- `skills/chishiki-repo-conventions/references/architecture-layout-and-build.md`
- `skills/chishiki-repo-conventions/references/csharp-conventions.md`

They capture the Pi-compatible version of the repository conventions, including the current project layout, build
commands, file header format, and XML documentation expectations.

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        .NET Aspire AppHost                          │
│                    (Local & Kubernetes Orchestration)               │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
          ┌────────────────────┼────────────────────┐
          ▼                    ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌──────────────────────┐
│  MCP Server     │  │  Orleans Silo   │  │    Infrastructure    │
│  (ASP.NET Core) │  │  (Engine Host)  │  │                      │
│                 │  │                 │  │  • PostgreSQL+pgvec  │
│  /mcp  :5010   │  │  Grains         │  │  • Qdrant            │
│  StreamableHTTP │  │  Streams        │  │  • Redis             │
│  Tools/Prompts  │  │  Reminders      │  │  • Keycloak (OIDC)   │
└────────┬────────┘  └────────┬────────┘  │  • Ollama (LLM)      │
         │                    │           │  • Prometheus+Grafana │
         └────────────────────┘           └──────────────────────┘
                    │ Orleans Redis Clustering
                    │ Kernel Memory RAG Pipeline
```

### Key Technology Choices

| Concern | Technology |
|---|---|
| RAG pipeline | Microsoft Kernel Memory |
| Actor model | .NET Orleans 10 (virtual actors / grains) |
| Orchestration | .NET Aspire 9.x (local + K8s) |
| MCP transport | `ModelContextProtocol.AspNetCore` — StreamableHTTP at `/mcp` |
| Vector store | Qdrant (primary) + PostgreSQL pgvector (secondary) |
| Embedding / LLM | Ollama (`nomic-embed-text` default) |
| Caching / clustering | Redis 7 |
| Identity | Keycloak 26 — realm `chishiki`, client `chishiki-api` |
| Observability | OpenTelemetry → Prometheus → Grafana |

---

## 🗂️ Repository Layout

```
/
├── src/                                    # All runnable services
│   ├── mcp/
│   │   └── Chishiki.MCP.Host/             # MCP server — tools auto-discovered via assembly scan
│   ├── engine/
│   │   ├── Chishiki.Host/                 # Orleans Silo host
│   │   └── clustering/
│   │       ├── Chishiki.Clustering/           # Grain interface contracts (no impl)
│   │       ├── Chishiki.Clustering.Client/    # Orleans client extensions
│   │       └── Chishiki.Clustering.Server/    # Silo-side grain registration
│   ├── api/
│   │   └── Chishiki.API.Web/              # ASP.NET Core Minimal API (planned)
│   ├── ingestion/                          # Ingestion pipeline services (planned)
│   ├── rag/                               # RAG query services (planned)
│   └── security/                          # Security scanning services (planned)
│
├── shared/                                # Cross-cutting libraries — NO back-refs into src/
│   └── core/
│       ├── Chishiki.Core/                 # DDD primitives: Entity, AggregateRoot, Disposable
│       ├── Chishiki.Data/                 # Repository + UoW abstractions
│       └── Chishiki.Data.EFCore/          # EF Core implementation
│
├── infrastructure/
│   └── aspire/
│       ├── Chishiki.Infrastructure.Aspire.AppHost/         # Aspire orchestration root
│       └── Chishiki.Infrastructure.Aspire.ServiceDefaults/ # Shared OTel / health / resilience
│
├── tests/                                 # Mirrors src/ + shared/ layout
├── containers/                            # Per-service Dockerfiles + configs
├── k8s/                                   # Kubernetes manifests / Helm charts
├── docs/                                  # Architecture Decision Records (ADRs)
└── scripts/                               # Build, deploy, seed scripts
```

### Dependency Rules

```
shared/core
  Chishiki.Core          — no project refs (BCL only)
  Chishiki.Data          — may ref Chishiki.Core
  Chishiki.Data.EFCore   — may ref Core + Data

infrastructure/aspire/ServiceDefaults
  — may ref BCL + Aspire SDK only

src/ services
  — may reference shared/core + ServiceDefaults
  — must NOT reference other service projects directly
    (communicate via Orleans grains or HTTP service discovery)
```

> ⚠️ `shared/core` projects are reusable across solutions — keep them free of any Chishiki-specific service layer.

---

## 🔨 Build, Test and Run

```powershell
# Build
dotnet build .\Chishiki.slnx

# Test — full suite
dotnet test .\Chishiki.slnx

# Test — single project
dotnet test .\tests\<Project>\<Project>.csproj

# Test — single test by name filter
dotnet test .\Chishiki.slnx --filter "FullyQualifiedName~MethodName_Scenario"

# Test — with coverage
dotnet test .\Chishiki.slnx --collect:"XPlat Code Coverage"

# Run — full stack via Aspire (requires Docker)
dotnet run --project .\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj

# Run — MCP server standalone
dotnet run --project .\src\mcp\Chishiki.MCP.Host\Chishiki.MCP.Host.csproj

# Run — with security scanner profile
$env:CHISHIKI_SECURITY_PROFILE = "true"
dotnet run --project .\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
```

| Endpoint | URL |
|---|---|
| Aspire Dashboard | http://localhost:15888 |
| MCP endpoint | http://localhost:5010/mcp |
| API | http://localhost:8080 |
| Grafana | http://localhost:3000 |
| Keycloak | http://localhost:8180 |

> 💡 Infrastructure containers use `builder.AddDockerfile("name", '../../../../containers/<name>')` in `AppHost.cs` —
> not Aspire built-in helpers. The root `.mcp.json` registers the MCP server for VS 2026 / GitHub Copilot.

---

## 📐 Design Principles

### SOLID + Vertical Slice

Each feature is a **self-contained vertical slice** — a folder owning its own request DTO, handler, validator,
and tool adapter, rather than spreading across horizontal layers.

```
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

| Principle | Application |
|---|---|
| **S**ingle Responsibility | One class, one reason to change |
| **O**pen/Closed | Add new slices; never modify existing handlers to add features |
| **L**iskov Substitution | Grain interfaces must be fully implementable by any grain |
| **I**nterface Segregation | Small, focused interfaces — no fat `IService` |
| **D**ependency Inversion | Depend on `IKernelMemory`, `IRepository<T>` — never on concrete infra |

---

## 📁 File Header Convention

> 🚫 **Mandatory** — every `.cs` file without exception must start with this header. PRs missing it will be rejected.

Every `.cs` file **must** begin with this exact header:

```csharp
// -----------------------------------------------------------------------------
// File:        FileName.cs
// Author:      Piergiorgio Vagnozzi
// Description: One-line summary of what this file contains or does.
// Created:     YYYY-MM-DD
// Modified:    YYYY-MM-DD
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
```

**Rules:**
- `File:` matches the physical filename including extension.
- `Description:` is a single clear sentence — never blank, never "TODO".
- `Modified:` is updated on every meaningful change to the file.
- All text is in **English** — no other languages.
- The three copyright lines are verbatim — do not alter them.
- Auto-generated files under `obj/` are exempt.

### Standardization of File Headers

- All files in **Chishiki.Core** and **Chishiki.Data** must adhere to the standard file header format.
- Ensure XML documentation is complete for all members in these files.

---

## 📝 XML Documentation

> 🚫 **Mandatory** — every type, member, and parameter **must** carry an XML doc comment. No exceptions.

### Rules

| Target | Requirement |
|---|---|
| Classes / interfaces / records / enums | `<summary>` — one clear sentence describing purpose |
| Public & internal methods | `<summary>` + `<param>` for every parameter + `<returns>` if non-void |
| Private methods (incl. `[LoggerMessage]` partials) | `<summary>` — one sentence |
| Properties | `<summary>` — one sentence |
| Constructor parameters (primary constructors) | Document on the class `<summary>` if self-evident; otherwise add `<param>` |
| Exceptions thrown | `<exception cref="ExType">` when explicitly thrown |
| `CancellationToken` parameters | `<param name="cancellationToken">Token to observe for cancellation.</param>` |

### Examples

```csharp
/// <summary>Provides a disposable base class with structured logging for managed and unmanaged resource cleanup.</summary>
public abstract partial class Disposable : IDisposable
{
    /// <summary>Gets the logger used to emit disposal diagnostics.</summary>
    protected ILogger Logger { get; }

    /// <summary>Releases managed resources. Override to dispose owned <see cref="IDisposable"/> members.</summary>
    protected virtual void DisposeManaged() { }

    /// <summary>Releases unmanaged resources. Override only when holding raw OS handles.</summary>
    protected virtual void DisposeUnmanaged() { }
}
```

```csharp
/// <summary>Returns version and build metadata for the running Chishiki MCP server.</summary>
/// <param name="cancellationToken">Token to observe for cancellation.</param>
/// <returns>JSON object with <c>name</c>, <c>version</c>, <c>framework</c>, and <c>buildTime</c> fields.</returns>
public Task<string> GetServerInfoAsync(CancellationToken cancellationToken = default) { ... }
```

- All doc text is in **English**.
- `<summary>` must be a complete sentence ending with a period.
- Never leave `<summary>` empty or with placeholder text such as "TODO".
- Use `<see cref="..."/>` to cross-reference types and members.

---

## 🧩 MCP Tool Conventions

Tools and prompts are auto-discovered via `WithToolsFromAssembly()` + `WithPromptsFromAssembly()` in `Program.cs`.

### Adding a new tool group

1. Create `src/mcp/Chishiki.MCP.Host/Tools/<GroupName>Tools.cs` — `partial` class with the file header.
2. Annotate with `[McpServerToolType]`; inject dependencies via primary constructor.
3. Each tool method: `[McpServerTool(Name = "snake_case")]` + `[Description("…")]`, returns `Task<string>`.
4. Use `async`/`await` only when truly async; otherwise `return Task.FromResult(JsonSerializer.Serialize(...))`.
5. Emit `LogToolInvoked` (Debug) + `LogToolCompleted` (Debug) + `LogToolFailed` (Error) via `[LoggerMessage]`.
6. Serialize results with `System.Text.Json.JsonSerializer.Serialize(result)`.

```csharp
// -----------------------------------------------------------------------------
// File:        RagQueryTools.cs
// Author:      Piergiorgio Vagnozzi
// Description: MCP tools for semantic search and AI-synthesised answers via RAG.
// Created:     2026-04-26
// Modified:    2026-04-26
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// ----------------------------------------------------------------------------- 

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

    [McpServerTool(Name = "rag_query"),
     Description("Answers a question using the Chishiki knowledge base (RAG).")]
    public async Task<string> QueryAsync(
        [Description("Natural-language question")] string question,
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

### Planned tool groups

| Class | MCP group | Status |
|---|---|---|
| `ChishikiSystemTools` | `system` | ✅ Available |
| `RagQueryTools` | `rag` | 🔲 Planned |
| `HubResourceTools` | `hub` | 🔲 Planned |
| `IngestionTools` | `ingest` | 🔲 Planned |
| `SecurityScanTools` | `security` | 🔲 Planned |

---

## 🌾 Orleans Grain Conventions

### Architecture: Distributed Event-Driven Streaming

The system follows a strict streaming-first architecture:

```
Client → Orleans Grain → (trigger worker) → Worker (Ollama) → Kafka topic (llm-tokens)
  → Orleans Streams (Kafka provider) → Grain (subscriber) → Client
```

**Core principles:**
- Orleans grains are **orchestration and state holders only**
- Long-running I/O (LLM calls) **MUST NOT** execute inside grains
- All worker ↔ grain communication uses **Orleans Streams backed by Kafka**
- System is **streaming-first** (token-by-token), not batch-based

### Grain Rules

- **Grain interfaces** in `Chishiki.Clustering` — no implementation, no infra dependencies.
- **Implementations** in `Chishiki.Clustering.Server`.
- **Client helpers** (`IGrainFactory` extensions) in `Chishiki.Clustering.Client`.
- Use `IGrainWithStringKey` or `IGrainWithGuidKey` — choose one per domain, never mix.
- **All grain methods: `Task<T>` or `ValueTask<T>`** — no synchronous grain methods.
- Grains that receive stream events **MUST be marked with `[Reentrant]`**.
- Grain state stored in Redis (`[StorageName("Default")]`).
- Cluster: `ClusterId = "chishiki-cluster"`, `ServiceId = "chishiki"`, Silo `11111`, Gateway `30000`.

### Streaming Configuration

```csharp
// In grain OnActivateAsync:
var streamProvider = GetStreamProvider("kafka-stream");
var stream = streamProvider.GetStream<LlmToken>("llm-tokens", this.GetPrimaryKeyAsGuid());
await stream.SubscribeAsync<LlmToken>(OnTokenReceivedAsync);
```

**Grain token buffer:**
```csharp
private Dictionary<Guid, StringBuilder> _tokenBuffers = new();

private Task OnTokenReceivedAsync(LlmToken token)
{
    if (!_tokenBuffers.ContainsKey(token.JobId))
        _tokenBuffers[token.JobId] = new();

    _tokenBuffers[token.JobId].Append(token.Text);
    return Task.CompletedTask;
}

public Task<string> GetResultAsync(Guid jobId) => 
    Task.FromResult(_tokenBuffers.GetValueOrDefault(jobId)?.ToString() ?? "");
```

### Message Contract

```csharp
/// <summary>Represents a single LLM token streamed from Ollama via Kafka.</summary>
public record LlmToken
{
    /// <summary>Gets the grain ID that requested this token.</summary>
    public Guid GrainId { get; init; }

    /// <summary>Gets the unique job ID for this streaming session.</summary>
    public Guid JobId { get; init; }

    /// <summary>Gets the token text content.</summary>
    public string Text { get; init; }
}
```

### Example Grain Implementation

```csharp
// Grain interface — Chishiki.Clustering
public interface ILlmJobGrain : IGrainWithGuidKey
{
    Task<Guid> StartJobAsync(string prompt, CancellationToken ct = default);
    Task<string> GetResultAsync(Guid jobId);
}

// Grain implementation — Chishiki.Clustering.Server
[Reentrant]
internal sealed partial class LlmJobGrain(
    [PersistentState("llm-job")] IPersistentState<LlmJobState> state,
    ILogger<LlmJobGrain> logger) : Grain, ILlmJobGrain
{
    private Dictionary<Guid, StringBuilder> _tokenBuffers = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = GetStreamProvider("kafka-stream");
        var stream = streamProvider.GetStream<LlmToken>("llm-tokens", this.GetPrimaryKeyAsGuid());
        await stream.SubscribeAsync<LlmToken>(OnTokenReceivedAsync);
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<Guid> StartJobAsync(string prompt, CancellationToken ct = default)
    {
        var jobId = Guid.NewGuid();
        _tokenBuffers[jobId] = new();
        Log.JobStarted(Logger, jobId);
        // Trigger worker via separate grain call or event
        return Task.FromResult(jobId);
    }

    public Task<string> GetResultAsync(Guid jobId) =>
        Task.FromResult(_tokenBuffers.GetValueOrDefault(jobId)?.ToString() ?? "");

    private Task OnTokenReceivedAsync(LlmToken token)
    {
        if (!_tokenBuffers.ContainsKey(token.JobId))
            _tokenBuffers[token.JobId] = new();
        _tokenBuffers[token.JobId].Append(token.Text);
        return Task.CompletedTask;
    }
}
```

### Kafka Configuration Rules

- Topic name: **`llm-tokens`**
- Message key **MUST be `GrainId`** to preserve per-grain ordering
- Expect **at-least-once delivery** → handle duplicates safely via idempotent token buffering
- Configure `PubSubStore` (required for Orleans Streams)

---

## 👷 Worker Pattern: External Long-Running Services

Workers execute **outside Orleans grains** and handle heavy I/O workloads (Ollama calls, processing, etc.).

### Architecture

```
Job Request (Grain)
    ↓
Worker Service (BackgroundService)
    ├─ Poll/listen for job requests
    ├─ Call Ollama (OllamaSharp) with streaming
    ├─ Emit each token to Kafka immediately
    └─ (Optional) Grain gets notified when complete
```

### Worker Implementation Rules

- **Do NOT inherit from `Grain`** — use `BackgroundService` or separate service class.
- **Must be fully async** — no `.Wait()`, `.Result`, or `Thread.Sleep`.
- **Communicate with grains via:**
  - Kafka (primary for token streaming)
  - Orleans `IGrainFactory` for metadata/state updates
  - Never direct grain calls for heavy work
- **Stream tokens incrementally** — emit each LLM token to Kafka immediately, not aggregated.
- **Inject dependencies via primary constructor** — `ILogger<T>`, `IKernelMemory`, Ollama client, `IProducer<>`, etc.
- **Handle backpressure** — implement exponential backoff if Kafka is slow.

### Example Worker Implementation

```csharp
// Worker interface — Chishiki.Clustering (message contract)
public record LlmJobRequest
{
    public Guid GrainId { get; init; }
    public Guid JobId { get; init; }
    public string Prompt { get; init; }
    public string Model { get; init; } = "llama2";
}

// Worker implementation — Chishiki.Engine (or separate service)
internal sealed partial class OllamaStreamingWorker(
    ILogger<OllamaStreamingWorker> logger,
    OllamaApiClient ollamaClient,
    IProducer<Guid, LlmToken> kafkaProducer,
    IChannel<LlmJobRequest> jobQueue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in jobQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                Log.ProcessingJob(logger, request.JobId);

                var response = ollamaClient.GenerateStreamAsync(
                    request.Model,
                    request.Prompt,
                    cancellationToken: stoppingToken);

                var tokenCount = 0;
                await foreach (var chunk in response)
                {
                    var token = new LlmToken
                    {
                        GrainId = request.GrainId,
                        JobId = request.JobId,
                        Text = chunk.Response
                    };

                    var message = new Message<Guid, LlmToken>
                    {
                        Key = request.GrainId,
                        Value = token
                    };

                    await kafkaProducer.ProduceAsync("llm-tokens", message, stoppingToken);
                    tokenCount++;
                    Log.TokenEmitted(logger, request.JobId, tokenCount);
                }

                Log.JobCompleted(logger, request.JobId, tokenCount);
            }
            catch (Exception ex)
            {
                Log.JobFailed(logger, request.JobId, ex);
            }
        }
    }
}
```

### Worker Configuration in Aspire

```csharp
// In AppHost.cs:
var chishiki = builder.AddProject<Chishiki.Host>("chishiki-engine")
    .WithReference(kafka)
    .WithReference(ollama)
    .WithExternalHttpEndpoints();

var worker = builder.AddProject<Chishiki.Worker>("chishiki-worker")
    .WithReference(kafka)
    .WithReference(ollama)
    .WithReference(chishiki);

var mcp = builder.AddProject<Chishiki.MCP.Host>("chishiki-mcp")
    .WithReference(chishiki);
```

### Worker Constraints (Strict)

- ✅ Use `IChannel<T>` or Kafka topic for job requests
- ✅ Emit each token to Kafka immediately (no buffering)
- ✅ Use `await foreach` for streaming responses
- ✅ Always forward `CancellationToken`
- ✅ Log job lifecycle (start, token emission, completion, failure)
- ❌ Never call grain methods inside token loop
- ❌ Never buffer full response before emitting
- ❌ Never use synchronous I/O

---

## 📚 Kernel Memory Integration

- Register with `builder.Services.AddKernelMemory(...)` in each service that needs RAG.
- Default embedding model: `nomic-embed-text` via Ollama at `http://ollama:11434`.
- Primary vector store: **Qdrant** at `http://qdrant:6333`.
- Always inject and use `IKernelMemory` — never the concrete `MemoryServerless` type.
- Keep **ingestion** and **query** as separate handlers/grains — do not mix in the same class.

---

## 🪵 Logging

> 🚫 Never call `logger.LogInformation(...)` / `logger.LogError(...)` directly.
> Always use compile-time `[LoggerMessage]` source generation.

### Pattern A — instance partial methods *(DI classes)*

```csharp
internal sealed partial class MyService(ILogger<MyService> logger)
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing item {ItemId}")]
    private partial void LogProcessingItem(string itemId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Item {ItemId} not found")]
    private partial void LogItemNotFound(string itemId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process {ItemId}")]
    private partial void LogProcessingFailed(string itemId, Exception ex);
}
```

### Pattern B — static extension class *(Program.cs / top-level code)*

```csharp
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Application started")]
    public static partial void AppStarted(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Application terminated unexpectedly")]
    public static partial void AppTerminatedUnexpectedly(this ILogger logger, Exception ex);
}
```

### Rules

| Rule | Detail |
|---|---|
| No string interpolation | All variable parts are named template parameters |
| Exception parameter | Always **last** — picked up as the log record exception automatically |
| Log level | `Debug` dev detail · `Information` milestones · `Warning` recoverable · `Error` needs attention · `Critical` fatal |
| Sensitive data | Never log passwords, tokens, PII, or connection strings |
| Parameter naming | **PascalCase**: `{ItemId}`, `{UserId}`, `{ToolName}` |
| EventId | Omit unless you need structured filtering |

---

## 💉 Dependency Injection

- **Primary constructors** for all constructor injection.
- Register services in `Program.cs` via `builder.Services.AddXxx(...)`.
- Concrete service classes: `internal sealed`.
- Publicly exposed interfaces belong in the contracts / clustering project.
- Never use `ServiceLocator` or resolve `IServiceProvider` inside business logic.

---

## ⚡ Async

- All I/O-bound methods: `async Task<T>`, suffix `Async`.
- Always accept and forward `CancellationToken` — never ignore it.
- Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
- Prefer `ValueTask<T>` for hot paths with synchronous fast-paths (e.g., grain state cache hits).
- Use `ConfigureAwait(false)` in `shared/core` library code; omit in ASP.NET Core and Orleans grain code.

---

## 🎨 Code Style

- Follow `.editorconfig` — max line length **120**.
- All comments and documentation are written in **English**.
- File-scoped namespaces: `namespace Chishiki.MCP.Host.Tools;`
- Primary constructors for DI.
- Expression-bodied members for single-line implementations.
- `var` when the type is apparent from the right-hand side.
- Prefer `System.Text.Json` over `Newtonsoft.Json` everywhere.
- `System` usings sorted first (`dotnet_sort_system_directives_first = true`).
- No `this.` qualifier unless required for disambiguation.

### Namespace Standardization

- All namespaces must follow the format `Chishiki.<Layer>.<Subdomain>` (e.g., `Chishiki.Data.Abstractions`, `Chishiki.Data.Specifications`, `Chishiki.Core`).

---

## 🧪 Testing

- Test projects under `tests/` mirror the source layout (e.g., `tests/mcp/`, `tests/shared/core/`).
- Framework: **xUnit** + **FluentAssertions** + **NSubstitute**.
- Test method naming: `MethodName_Scenario_ExpectedResult`.
- Every test file must include the standard file header.
- Unit tests mock `IKernelMemory`, `IGrainFactory`, `ILogger<T>` — no real infra in unit tests.
- Integration tests live in a separate `*.IntegrationTests` project.

```powershell
dotnet test .\Chishiki.slnx --filter "FullyQualifiedName~RagQueryTools"
```

---

## ☸️ Kubernetes

- Kubernetes manifests and Helm charts: `k8s/`.
- Use Aspire's `PublishingContext` to generate initial K8s manifests — avoid hand-authoring what Aspire can generate.
- All secrets are Kubernetes `Secret` objects — never in `ConfigMap` or image layers.
- Orleans clustering on K8s uses the Redis provider.
- Health probes: `/health` (readiness) and `/alive` (liveness) — wired by `ServiceDefaults`, referenced in every deployment manifest.
- Resource naming: `chishiki-<service>` (e.g., `chishiki-mcp`, `chishiki-engine`, `chishiki-api`).

---
