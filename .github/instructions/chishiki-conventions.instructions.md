---
applyTo: '**/*.cs'
description: 'Chishiki project-specific C# coding conventions: file headers, XML docs, source-generated logging, primary constructors, vertical slice structure.'
---

# Chishiki C# Conventions

Apply these rules to C# code in the Chishiki repository.

## File Header

Every `.cs` file must start with this exact header format:

```csharp
// -----------------------------------------------------------------------------
// File:        FileName.cs
// Author:      Piergiorgio Vagnozzi
// Description: One-line summary.
// Created:     YYYY-MM-DD
// Modified:    YYYY-MM-DD
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
```

Rules:

- `File:` must match the physical file name.
- `Description:` must be a clear English sentence.
- Update `Modified:` on every meaningful edit.
- Do not alter the copyright and license lines.

## XML Documentation

Add XML documentation to all public and internal types and members.

- Use complete English sentences ending with a period.
- Document every parameter with `<param>`.
- Add `<returns>` for non-void methods.
- Document explicitly thrown exceptions with `<exception>` when relevant.
- For `CancellationToken`, use: `<param name="cancellationToken">Token to observe for cancellation.</param>`

## Logging

Use source-generated logging only.

- Use `[LoggerMessage]` partial methods.
- Never introduce direct `logger.LogTrace(...)`, `logger.LogDebug(...)`, `logger.LogInformation(...)`, `logger.LogWarning(...)`, `logger.LogError(...)`, or `logger.LogCritical(...)` calls.
- Use PascalCase placeholders such as `{ToolName}`, `{GrainId}`, and `{ItemId}`.
- Keep the `Exception` parameter last.

## Construction and Types

- Prefer primary constructors for dependency injection.
- Use file-scoped namespaces such as `namespace Chishiki.MCP.Host.Tools;`
- Declare concrete service classes as `internal sealed` unless an existing pattern requires otherwise.
- Favor vertical-slice structure for host features.

## Async and Serialization

- Use `ConfigureAwait(false)` in shared libraries.
- Omit `ConfigureAwait(false)` in ASP.NET Core and Orleans grain code.
- Use `System.Text.Json` only. Do not introduce Newtonsoft.Json.
- Forward `CancellationToken` consistently.

## Formatting

- Respect the repository `.editorconfig`.
- Keep line length at or below 120 characters.
- Keep all code comments and documentation in English.
