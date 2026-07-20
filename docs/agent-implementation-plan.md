# Chishiki Agent — Implementation Plan

## Obiettivo

Creare un agente AI completo sotto `src/agents/` con:

- **TUI** (Terminal UI) via Spectre.Console
- **Web UI** stile GitHub Copilot (React embedded in ASP.NET Core)
- **VS Code Extension** (TypeScript + Language Server)
- **Orchestratore / Router** stile OpenRouter (multi-provider, fallback, streaming)
- **Plugin System** via assembly isolati (`AssemblyLoadContext`)
- **Build self-contained** per tutte le piattaforme (nessuna dipendenza da runtime esterno)

---

## Struttura directory target

```
src/agents/
├── Chishiki.Agent.Abstractions/          # Contratti: IPlugin, IProvider, IOrchestrator, modelli
├── Chishiki.Agent.Core/                  # Orchestratore, router, plugin loader, context manager
├── Chishiki.Agent.Providers/             # Adapter provider: OpenAI, Anthropic, Ollama, Azure OpenAI, Gemini
├── Chishiki.Agent.Plugins.Sdk/           # SDK pubblico per sviluppatori di plugin
├── Chishiki.Agent.Plugins.CodeAnalysis/  # Plugin built-in: analisi codice via Roslyn
├── Chishiki.Agent.WebApi/                # ASP.NET Core: API REST OpenAI-compatibile + SignalR streaming
├── Chishiki.Agent.WebUI/                 # React app embedded (servita da WebApi)
├── Chishiki.Agent.Tui/                   # Terminal UI via Spectre.Console
├── Chishiki.Agent.Host/                  # Entry point self-contained (TUI + WebApi in unico processo)
├── vscode-extension/                     # VS Code extension TypeScript
│   ├── src/
│   ├── package.json
│   └── webpack.config.js
tests/
├── Chishiki.Agent.Abstractions.Tests/
├── Chishiki.Agent.Core.Tests/
└── Chishiki.Agent.WebApi.Tests/
```

---

## Namespace e convenzioni

- Prefisso: `Chishiki.Agent.*`
- Target framework: `net10.0`
- C# 14, nullable enabled, implicit usings
- Header standard con `Created:` / `Modified:`
- Logging: `[LoggerMessage]` source-generated
- VS Code extension: TypeScript strict, Node 20+, webpack bundle

---

## Progetto 1: `Chishiki.Agent.Abstractions`

### Interfacce chiave

```csharp
// Modello di completamento (OpenAI-compatibile)
public record ChatMessage(string Role, string Content, string? Name = null);
public record CompletionRequest(string Model, IReadOnlyList<ChatMessage> Messages, ...);
public record CompletionResponse(string Id, string Model, ChatMessage Message, ...);
public record CompletionChunk(string Id, string Model, string? Delta, bool IsFinished);

// Provider (OpenAI, Anthropic, Ollama, ecc.)
public interface IProvider : IDisposable
{
    string Id { get; }
    string DisplayName { get; }
    Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken ct = default);
    IAsyncEnumerable<CompletionChunk> StreamAsync(CompletionRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken ct = default);
}

// Plugin
public interface IPlugin
{
    string Id { get; }
    string DisplayName { get; }
    string Version { get; }
    IReadOnlyList<IPluginCapability> Capabilities { get; }
    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);
    Task ShutdownAsync(CancellationToken ct = default);
}

public interface IPluginCapability
{
    string Name { get; }
    string Description { get; }
    Task<CapabilityResult> ExecuteAsync(CapabilityRequest request, CancellationToken ct = default);
}

// Orchestratore
public interface IOrchestrator
{
    Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken ct = default);
    IAsyncEnumerable<CompletionChunk> StreamAsync(CompletionRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken ct = default);
    IReadOnlyList<IProvider> Providers { get; }
    IReadOnlyList<IPlugin> Plugins { get; }
}
```

---

## Progetto 2: `Chishiki.Agent.Core`

### Componenti

**PluginLoader**: carica assembly da directory `plugins/`, usa `AssemblyLoadContext` per isolamento.

```csharp
public class PluginLoader
{
    public Task<IPlugin> LoadAsync(string assemblyPath);
    public Task UnloadAsync(string pluginId);
    public IReadOnlyList<IPlugin> LoadedPlugins { get; }
}
```

**ModelRouter (OpenRouter-like)**:

- Mappatura alias → provider+model (es. `"fast"` → `"ollama/llama3.2"`)
- Fallback chain: se provider A fallisce, prova B
- Load balancing round-robin
- Statistiche di latenza e costo

```csharp
public class ModelRouter
{
    public Task<IProvider> ResolveAsync(string modelAlias);
    public void RegisterRoute(string alias, RouteConfig config);
}
```

**ConversationContext**: gestione history multi-turn, context window, summarization automatica.

**AgentOrchestrator**: implementa `IOrchestrator`, coordina router + plugins + context.

---

## Progetto 3: `Chishiki.Agent.Providers`

Provider da implementare con client HTTP standard (`HttpClient`):

| Provider | API Base | Auth | Streaming |
|---|---|---|---|
| OpenAI | `https://api.openai.com/v1` | Bearer token | SSE |
| Anthropic | `https://api.anthropic.com/v1` | x-api-key | SSE |
| Ollama | `http://localhost:11434/api` | nessuna | SSE |
| Azure OpenAI | `https://{resource}.openai.azure.com/` | api-key | SSE |
| Google Gemini | `https://generativelanguage.googleapis.com/v1` | api-key | SSE |
| OpenRouter | `https://openrouter.ai/api/v1` | Bearer token | SSE (OpenAI-compat) |

Ogni provider implementa `IProvider`. Configurazione via `IConfiguration` / `appsettings.json`.

---

## Progetto 4: `Chishiki.Agent.Plugins.Sdk`

SDK pubblico per plugin di terze parti:

- Attributi: `[Plugin]`, `[Capability]`
- Base class: `PluginBase`
- Helpers: `PluginContext`, `CapabilityBuilder`
- NuGet-publishable (separato dal core)

---

## Progetto 5: `Chishiki.Agent.Plugins.CodeAnalysis`

Plugin built-in, caricato di default:

**Capabilities**:

- `analyze-complexity`: McCabe cyclomatic complexity via Roslyn
- `find-issues`: problemi di qualità (null ref, unused vars, ecc.)
- `suggest-refactor`: suggerimenti di refactoring
- `explain-code`: genera spiegazione naturale del codice
- `generate-tests`: genera stub di test unitari

Implementazione: Microsoft.CodeAnalysis (Roslyn) + Microsoft.CodeAnalysis.CSharp.

---

## Progetto 6: `Chishiki.Agent.WebApi`

ASP.NET Core minimal API, **OpenAI-API-compatibile**:

```
GET  /v1/models                      → lista modelli disponibili
POST /v1/chat/completions            → completamento (standard o streaming SSE)
GET  /v1/plugins                     → lista plugin caricati
POST /v1/plugins/{id}/capabilities/{cap} → esegui capability plugin
WS   /ws/chat                        → WebSocket per UI real-time
GET  /                               → serve Web UI statica (SPA)
GET  /health                         → health check
```

**SignalR Hub** per streaming real-time alla Web UI.

**Middleware**:

- CORS
- Autenticazione opzionale (API key)
- Rate limiting
- OpenAPI/Swagger

---

## Progetto 7: `Chishiki.Agent.WebUI`

React 18 + TypeScript, servita come file statici embedded nell'assembly WebApi.

**Features**:

- Chat panel con markdown rendering e syntax highlighting
- Sidebar: model selector, provider status, plugin list
- Code viewer con diff e suggerimenti inline
- Settings page: provider API keys, routing rules, plugin directory
- Theme: dark/light, stile GitHub Copilot Chat

**Build**: `npm run build` → output in `wwwroot/` → embedded via `EmbeddedResource`.

**Tecnologie**: React, Vite, TailwindCSS, Markdown-it, Highlight.js, React Router.

---

## Progetto 8: `Chishiki.Agent.Tui`

Spectre.Console-based TUI:

**Modalità**:

- **Interactive**: REPL con prompt, history, completamento tab
- **Pipe**: stdin → stdout (per scripting)
- **Watch**: monitora directory e analizza file al salvataggio

**Comandi slash**:

```
/models           → lista modelli
/providers        → stato provider
/plugins          → plugin caricati
/route <alias>    → info routing
/analyze <file>   → analisi codice
/clear            → pulisci history
/help             → aiuto
```

**Layout Spectre.Console**:

```
┌─ Chishiki Agent ─────────────────────────────────────────────────────┐
│ Model: gpt-4o │ Provider: OpenAI │ Tokens: 1,234 │ Cost: $0.02      │
├──────────────────────────────────────────────────────────────────────┤
│ [chat history area - scrollable]                                      │
├──────────────────────────────────────────────────────────────────────┤
│ > _                                                                   │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Progetto 9: `Chishiki.Agent.Host`

Entry point unico che:

1. Avvia Web API su `http://localhost:5100` (background)
2. Avvia TUI in foreground (o usa `--headless` per solo WebApi)
3. Carica plugin dalla directory `~/.chishiki/plugins/`

**CLI args**:

```
chishiki-agent [--port 5100] [--headless] [--plugins <dir>] [--config <path>]
chishiki-agent --tui-only
chishiki-agent --web-only
```

### Build self-contained

`Chishiki.Agent.Host.csproj`:

```xml
<PublishSingleFile>true</PublishSingleFile>
<PublishTrimmed>true</PublishTrimmed>
<SelfContained>true</SelfContained>
```

**Script `scripts/Build-Agent.ps1`**:

```
win-x64, win-arm64, linux-x64, linux-arm64, osx-x64, osx-arm64
```

Output in `dist/agent/{platform}/`.

---

## Progetto 10: `vscode-extension/`

**Package**: `chishiki-agent-vscode`  
**Publisher**: chishiki  
**VSCode engine**: `^1.90.0`

**Contributes**:

- `chishiki.chat`: panel webview (carica l'UI del WebApi locale)
- `chishiki.analyzeFile`: analisi file corrente via plugin
- `chishiki.suggestRefactor`: suggerimenti inline
- Inline completions provider (copilot-like ghost text)
- Hover provider: spiegazione codice on-hover
- Code actions: quick fixes suggeriti dal plugin

**Comunicazione**: chiama `http://localhost:5100` (il processo host).  
Se il processo non è avviato, lo avvia automaticamente via `child_process`.

**Bundle**: webpack + ts-loader → `dist/extension.js` (no node_modules nel vsix).  
**Package**: `vsce package` → `chishiki-agent-*.vsix`.

---

## Test da creare

| Progetto | Test |
|---|---|
| Abstractions.Tests | Serializzazione modelli, validazione |
| Core.Tests | Router fallback, plugin loader, context window |
| WebApi.Tests | Endpoint OpenAI-compatibili, health check |

---

## Aggiornamento solution file

Aggiungere tutti i nuovi `.csproj` al `Chishiki.slnx` sotto la folder `/src/agents/`.

---

## File di configurazione `appsettings.json` per Host

```json
{
  "Agent": {
    "Port": 5100,
    "PluginsDirectory": "plugins",
    "Providers": {
      "OpenAI": { "ApiKey": "", "Enabled": true },
      "Anthropic": { "ApiKey": "", "Enabled": false },
      "Ollama": { "BaseUrl": "http://localhost:11434", "Enabled": true },
      "AzureOpenAI": { "ApiKey": "", "Endpoint": "", "Enabled": false },
      "Gemini": { "ApiKey": "", "Enabled": false },
      "OpenRouter": { "ApiKey": "", "Enabled": false }
    },
    "Routing": {
      "DefaultModel": "ollama/llama3.2",
      "Routes": {
        "fast":   { "Provider": "Ollama",  "Model": "llama3.2" },
        "smart":  { "Provider": "OpenAI",  "Model": "gpt-4o" },
        "code":   { "Provider": "Anthropic","Model": "claude-3-5-sonnet-20241022" },
        "local":  { "Provider": "Ollama",  "Model": "codellama" }
      },
      "Fallback": ["Ollama", "OpenAI"]
    }
  }
}
```
