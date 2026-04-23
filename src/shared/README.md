# src/shared — Shared Libraries

Cross-cutting libraries used by all other source areas. No dependency may flow *back* from `shared/` into `api/`, `engine/`, `security/`, `ingestion/`, or `rag/`.

| Project | Description |
|---|---|
| `Chishiki.Abstractions` | Base primitives — `Result<T>`, `Error`, `PagedResult<T>`, `IPagedRequest`. Used as the building block for all service contracts and return types across the platform. |
| `Chishiki.Core` | Domain library — `Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`, `IRepository`, `IUnitOfWork`. Provides the DDD foundation for grain state and domain models. |

## Dependency rules

- `Chishiki.Abstractions` has **no** project references (only BCL).
- `Chishiki.Core` may reference `Chishiki.Abstractions`.
- All other projects in the solution may reference either or both.
- Neither `shared/` project may reference `engine/`, `api/`, `security/`, `ingestion/`, or `rag/` projects.

## Planned projects

| Project | Description |
|---|---|
| `Chishiki.Ingestion.Contracts` | Ingestion source types, chunk model, `ISourceConnector` interface. |
| `Chishiki.RAG.Contracts` | RAG query/response types and retrieval interfaces. |
