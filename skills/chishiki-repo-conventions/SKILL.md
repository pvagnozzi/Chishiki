---
name: chishiki-repo-conventions
description: 'Repository-specific conventions for working in Chishiki. Use when changing code in this repo, especially for architecture/layout guidance, C# file headers, XML documentation, MCP-related context, and minimal safe edits aligned with the existing codebase.'
---

# Chishiki Repository Conventions

Pi-compatible repository guidance distilled from the existing GitHub Copilot instructions.

## When to Use This Skill

- You are making or reviewing changes in the Chishiki repository
- You need the current repository layout and project-boundary guidance
- You need C# documentation conventions such as file headers or XML comments
- You need repo-specific guidance for MCP host context, build commands, or minimal-change expectations

## Core Rules

- Follow direct user or task instructions first.
- Prefer factual inspection of the repository over assumptions when layout or implementation details may have changed.
- Preserve existing structure and style; make the smallest safe change that satisfies the request.
- Do not broaden edits into unsolicited refactors or cleanup.
- Track reasoning and work in task artifacts/notes, not in ad-hoc repo-root scratch files.

## References

- [Architecture, layout, and build guidance](./references/architecture-layout-and-build.md)
- [C# conventions for headers, XML docs, and common coding patterns](./references/csharp-conventions.md)

## Mapping Notes

This skill is a curated conversion of repository GitHub Copilot instructions into Pi-friendly project resources:

- Copilot `applyTo` frontmatter was not migrated because Pi skills use skill metadata plus explicit references instead of file-glob instruction targeting.
- Copilot-specific response-format directives were reduced to durable repository rules that remain useful across runtimes.
- The Copilot thought-logging workflow was intentionally not ported because it requires creating/updating a repo-root scratch file; Pi task artifacts and notes should be used instead.
