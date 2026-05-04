# 🧠 Chishiki — GitHub Copilot Instructions

> **Chishiki** (知識 — *knowledge* in Japanese) is a cloud-native, distributed **RAG platform** exposed as a
> **Model Context Protocol (MCP) server**, built on **.NET 10**, **Microsoft Orleans**, **.NET Aspire**,
> and **Microsoft Kernel Memory**. Designed for on-premises or Azure deployment, including **Kubernetes**.

---

## 📋 Table of Contents

- [🏗️ Architecture](#-architecture)
- [🗂️ Repository Layout](#-repository-layout)
- [🔨 Build, Test and Run](#-build-test-and-run)
- [📐 Design Principles](#-design-principles)
- [📁 File Header Convention](#-file-header-convention)
- [📝 XML Documentation](#-xml-documentation)
- [🧩 MCP Tool Conventions](#-mcp-tool-conventions)
- [🌾 Orleans Grain Conventions](#-orleans-grain-conventions)
- [📚 Kernel Memory Integration](#-kernel-memory-integration)
- [🪵 Logging](#-logging)
- [💉 Dependency Injection](#-dependency-injection)
- [⚡ Async](#-async)
- [🎨 Code Style](#-code-style)
- [🧪 Testing](#-testing)
- [☸️ Kubernetes](#-kubernetes)

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

- **Grain interfaces** in `Chishiki.Clustering` — no implementation, no infra dependencies.
- **Implementations** in `Chishiki.Clustering.Server`.
- **Client helpers** (`IGrainFactory` extensions) in `Chishiki.Clustering.Client`.
- Use `IGrainWithStringKey` or `IGrainWithGuidKey` — choose one per domain, never mix.
- All grain methods: `Task<T>` or `ValueTask<T>` — no synchronous grain methods.
- Grain state stored in Redis (`[StorageName("Default")]`).
- Cluster: `ClusterId = "chishiki-cluster"`, `ServiceId = "chishiki"`, Silo `11111`, Gateway `30000`.

```csharp
// Grain interface — Chishiki.Clustering
public interface IDocumentGrain : IGrainWithStringKey
{
    Task<DocumentStatus> GetStatusAsync();
    Task IngestAsync(IngestDocumentRequest request, CancellationToken ct = default);
}

// Grain implementation — Chishiki.Clustering.Server
internal sealed partial class DocumentGrain(
    [PersistentState("document")] IPersistentState<DocumentState> state,
    IKernelMemory memory,
    ILogger<DocumentGrain> logger) : Grain, IDocumentGrain
{
    // ...
}
```

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
