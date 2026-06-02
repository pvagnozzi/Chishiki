# Chishiki C# Conventions

This reference captures the repository-specific C# conventions that are broadly reusable from the GitHub Copilot instructions.

## File Headers

For repository C# files, use the standard header format already present throughout the codebase:

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

Guidelines:

- `File:` should match the physical filename.
- `Description:` should be a clear one-line English summary.
- Update `Modified:` when making a meaningful change.
- Preserve the copyright and license lines verbatim.

## XML Documentation

Use XML documentation comments consistently, especially for public and internal APIs.

Expected minimums:

- Types: add a `<summary>` describing purpose.
- Methods: add a `<summary>`, document each parameter with `<param>`, and include `<returns>` when the method returns a value.
- Properties: add a `<summary>`.
- Exceptions: add `<exception cref="...">` when a method explicitly throws.
- `CancellationToken` parameters should be documented as: `Token to observe for cancellation.`

When an implementation already uses `<inheritdoc/>`, prefer documenting the abstraction/interface instead of duplicating text in every implementation.

## Region Organization

Use `#region` / `#endregion` in C# files when a type contains clearly distinct groups of members, for example:

- `Properties`
- `Query`
- `Async Methods`
- `Sync Methods`
- `Repository Access`
- `Save Operations`
- `Private Helpers`
- `Log Messages`

Guidelines:

- Prefer short, concrete region names.
- Only add regions when they improve navigation; avoid wrapping every tiny member in its own region.
- Match existing grouping patterns already used in nearby files when available.
- Keep member order unchanged unless the task explicitly asks for a refactor.

## Logging

- Use structured logging.
- Prefer meaningful property names over interpolated free-form strings when logging values.
- Keep log messages concise and actionable.

## Dependency Injection

- Depend on abstractions where practical.
- Register services in the narrowest sensible scope.
- Avoid introducing concrete coupling when an existing interface or option pattern already fits.

## Async

- Prefer async/await for I/O-bound work.
- Flow `CancellationToken` through async call chains when available.
- Avoid blocking async code with synchronous waits.

## Code Style and Change Discipline

- Match the surrounding style of the file and project.
- Prefer simple, standard solutions over clever ones.
- Make the minimum necessary change to satisfy the task.
- Do not mix documentation-only work with unrelated refactors.

## Testing

- Run the smallest reliable verification relevant to the change.
- If a task only changes documentation or instruction resources, manual inspection may be sufficient; say so explicitly when reporting verification.
