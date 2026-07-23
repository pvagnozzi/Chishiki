---
name: chishiki-orleans-grains
description: 'Skill for implementing Orleans 10 grains in Chishiki. Use when adding grain interfaces, streaming subscribers, Kafka-backed token flows, Redis grain state, or worker-orchestrated LLM jobs.'
---

# Chishiki Orleans Grains

Use this skill when working in the Orleans-specific shared libraries that power Chishiki clustering and distributed orchestration.

## When to Use This Skill

- Add or modify grain interfaces in `Chishiki.Clustering`
- Implement grain classes in `Chishiki.Clustering.Server`
- Build token-streaming flows over Orleans Streams and Kafka
- Add worker-to-grain orchestration for long-running LLM processing
- Update Redis-backed grain state or cluster configuration assumptions

## Placement Rules

| Artifact | Location |
| --- | --- |
| Grain interface | `Chishiki.Clustering` |
| Grain implementation | `Chishiki.Clustering.Server` |
| Grain client helpers | `Chishiki.Clustering.Client` |

Keep contracts free from infrastructure-heavy dependencies.

## Architecture Rules

- Grains orchestrate and hold state; they do not run heavy external I/O.
- Long-running LLM or embedding calls belong in workers or background services.
- Use Kafka-backed Orleans Streams for token-by-token communication.
- Use `Task<T>` or `ValueTask<T>` for all grain methods.
- Mark grains that subscribe to streams with `[Reentrant]`.
- Persist grain state in Redis with `[StorageName("Default")]`.

## Cluster Configuration

Use these Chishiki defaults unless the repository changes explicitly:

| Setting | Value |
| --- | --- |
| ClusterId | `chishiki-cluster` |
| ServiceId | `chishiki` |
| Silo port | `11111` |
| Gateway port | `30000` |
| Token topic | `llm-tokens` |

## Streaming Pattern

Subscribe on activation and buffer tokens by job identifier.

```csharp
[Reentrant]
internal sealed partial class LlmJobGrain : Grain, ILlmJobGrain
{
    private readonly Dictionary<Guid, StringBuilder> tokenBuffers = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = GetStreamProvider("kafka-stream");
        var stream = streamProvider.GetStream<LlmToken>("llm-tokens", this.GetPrimaryKeyAsGuid());
        await stream.SubscribeAsync<LlmToken>(OnTokenReceivedAsync);
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<string> GetResultAsync(Guid jobId) =>
        Task.FromResult(tokenBuffers.GetValueOrDefault(jobId)?.ToString() ?? string.Empty);

    private Task OnTokenReceivedAsync(LlmToken token)
    {
        if (!tokenBuffers.ContainsKey(token.JobId))
        {
            tokenBuffers[token.JobId] = new StringBuilder();
        }

        tokenBuffers[token.JobId].Append(token.Text);
        return Task.CompletedTask;
    }
}
```

## Message Contract Pattern

```csharp
/// <summary>Represents a single LLM token streamed from a worker through Kafka-backed Orleans Streams.</summary>
public sealed record LlmToken
{
    /// <summary>Gets the grain identifier that owns the request.</summary>
    public Guid GrainId { get; init; }

    /// <summary>Gets the logical streaming job identifier.</summary>
    public Guid JobId { get; init; }

    /// <summary>Gets the token text payload.</summary>
    public string Text { get; init; } = string.Empty;
}
```

## Interface and Implementation Pattern

```csharp
public interface ILlmJobGrain : IGrainWithGuidKey
{
    Task<Guid> StartJobAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> GetResultAsync(Guid jobId);
}
```

```csharp
[Reentrant]
[StorageName("Default")]
internal sealed partial class LlmJobGrain(
    [PersistentState("llm-job")] IPersistentState<LlmJobState> state,
    ILogger<LlmJobGrain> logger) : Grain, ILlmJobGrain
{
    public Task<Guid> StartJobAsync(string prompt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var jobId = Guid.NewGuid();
        return Task.FromResult(jobId);
    }
}
```

## Worker Pattern

Workers execute external I/O and stream results back through Kafka.

- Use `BackgroundService` or a dedicated service class.
- Publish tokens immediately to `llm-tokens`.
- Key Kafka messages by `GrainId` to preserve per-grain ordering.
- Never call expensive external services inside a grain loop.

## Gotchas

- **Never** perform heavy network I/O directly inside a grain.
- **Always** use `[Reentrant]` for grains that receive stream callbacks.
- **Always** preserve per-job buffering instead of a single shared text accumulator.
- **Always** treat Kafka delivery as at-least-once and design token handling to tolerate duplicates.

## References

- [Chishiki repository conventions](C:\work\personal\Chishiki\skills\chishiki-repo-conventions\SKILL.md)
- [Chishiki MCP tools skill](..\chishiki-mcp-tools\SKILL.md)
