// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts;

/// <summary>Classifies the kind of hub resource managed by the developer hub.</summary>
public enum HubResourceType
{
    /// <summary>A project scaffolding template (.NET, frontend, infra, etc.).</summary>
    Template,

    /// <summary>A Copilot agent definition file (<c>.chatmode.md</c>).</summary>
    Agent,

    /// <summary>A Copilot session hook definition (e.g. <c>hooks.json</c>).</summary>
    Hook,

    /// <summary>A language or domain-specific coding instruction file.</summary>
    Instruction,

    /// <summary>A reusable agent skill (<c>SKILL.md</c>).</summary>
    Skill,

    /// <summary>A curated bundle of agents, instructions, and skills.</summary>
    Collection
}
