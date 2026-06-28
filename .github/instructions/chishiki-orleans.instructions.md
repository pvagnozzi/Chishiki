---
applyTo: 'src/shared/orleans/**/*.cs'
description: 'Orleans grain conventions for Chishiki: interfaces, implementations, streaming-first architecture, Kafka streams, Redis persistence.'
---

# Chishiki Orleans Conventions

Apply these rules to Orleans-related code under `src/shared/orleans/`.

## Placement Rules

- Put grain interfaces in `Chishiki.Clustering`.
- Put implementations in `Chishiki.Clustering.Server`.
- Put helper extensions for clients in `Chishiki.Clustering.Client`.

## Architecture Rules

- Grains are orchestration and state holders only.
- Long-running external I/O belongs in workers or background services.
- Use Kafka-backed Orleans Streams for token streaming.
- Use `llm-tokens` as the standard topic for LLM token events unless a repository change says otherwise.
- Kafka message keys must use `GrainId` to preserve ordering.

## Grain Implementation Rules

- Use `Task<T>` or `ValueTask<T>` for all grain methods.
- Mark stream-subscribing grains with `[Reentrant]`.
- Use `[StorageName("Default")]` for Redis persistence where applicable.
- Buffer streaming results per job identifier, not in a single global accumulator.
- Design stream handling to tolerate at-least-once delivery.

## Cluster Defaults

- `ClusterId = "chishiki-cluster"`
- `ServiceId = "chishiki"`
- Silo port `11111`
- Gateway port `30000`

## Logging and Documentation

- Use `[LoggerMessage]` source-generated logging only.
- Add XML documentation to public and internal types and members.
- Keep comments and documentation in English.
- Preserve standard Chishiki file headers.
