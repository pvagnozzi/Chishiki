# Chishiki — GitHub Copilot Instructions

## Project overview

Chishiki is a .NET 10 / C# 14 solution that exposes a Model Context Protocol (MCP) server backed by
Aspire orchestration, PostgreSQL + pgvector, Qdrant, Redis, Keycloak, and Ollama.

---

## Code style

- Follow `.editorconfig` for all formatting and style rules.
- Use **file-scoped namespaces** (`namespace Foo;`).
- Use **primary constructors** for dependency injection.
- Prefer **expression-bodied members** for single-line implementations.
- Use **`var`** whenever the type is apparent.
- Every `.cs` file must begin with the copyright header:
  ```csharp
  // Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
  // Licensed under the MIT License. See LICENSE in the project root for license information.
  ```

---

## Logging — .NET 10 standard

### Rule: always use `[LoggerMessage]` source generation

Never call `logger.LogInformation(...)` / `logger.LogError(...)` directly.
Use the **compile-time source generator** introduced in .NET 6 and refined in .NET 9/10.

#### Pattern A — instance partial methods (preferred for classes with DI)

Declare the class as `partial` and inject `ILogger<T>` via the primary constructor.
The generator emits zero-allocation, strongly-typed log methods.

```csharp
internal sealed partial class MyService(ILogger<MyService> logger)
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing item {ItemId}")]
    private partial void LogProcessingItem(string itemId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Item {ItemId} not found")]
    private partial void LogItemNotFound(string itemId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process item {ItemId}")]
    private partial void LogProcessingFailed(string itemId, Exception ex);

    public void Process(string itemId)
    {
        LogProcessingItem(itemId);
        // ...
    }
}
```

#### Pattern B — static extension class (preferred for static helpers / top-level code)

```csharp
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Application started on {Urls}")]
    public static partial void AppStarted(this ILogger logger, string urls);

    [LoggerMessage(Level = LogLevel.Information, Message = "Application shutting down")]
    public static partial void AppShuttingDown(this ILogger logger);
}
```

Usage in `Program.cs`:
```csharp
var app = builder.Build();
app.Logger.AppStarted(string.Join(", ", app.Urls));
```

### Rules

| Rule | Detail |
|------|--------|
| **No string interpolation in log messages** | All variable parts must be template parameters — the source generator enforces this. |
| **Event IDs** | Omit `EventId` unless you need structured filtering; the generator assigns stable IDs automatically. |
| **Log levels** | `Trace` = diagnostic noise · `Debug` = dev-time detail · `Information` = operational milestones · `Warning` = recoverable anomaly · `Error` = failure that needs attention · `Critical` = fatal, requires immediate action |
| **Sensitive data** | Never log passwords, tokens, PII, or connection strings. |
| **Structured keys** | Use PascalCase for log message parameter names (`{ItemId}`, `{UserId}`). |
| **Exceptions** | Pass `Exception` as the **last** parameter — the generator picks it up as the exception argument automatically. |

### OpenTelemetry integration

Logging is automatically forwarded to the OpenTelemetry pipeline configured in
`Chishiki.Infrastructure.Aspire.ServiceDefaults` (`Extensions.ConfigureOpenTelemetry`).
No additional wiring is required in individual projects.

---

## Dependency injection

- Use **primary constructors** for constructor injection.
- Register services in `Program.cs` via `builder.Services.AddXxx(...)`.
- Prefer `internal sealed` for concrete service classes.

---

## Async

- All I/O-bound methods must be `async Task<T>` and end with the `Async` suffix.
- Always pass and honour `CancellationToken`.
- Never use `.Result` or `.Wait()` — always `await`.

---

## Testing

- Unit tests live under `tests/`.
- Use xUnit + FluentAssertions.
- Name test methods: `MethodName_Scenario_ExpectedResult`.
