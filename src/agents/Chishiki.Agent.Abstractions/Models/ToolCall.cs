// -----------------------------------------------------------------------------
// File:        ToolCall.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a tool or function call requested by the AI model.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Represents a tool or function call requested by the AI model during a completion.</summary>
/// <param name="Id">Unique identifier for this tool call, assigned by the model.</param>
/// <param name="Name">The name of the function or tool to invoke.</param>
/// <param name="ArgumentsJson">JSON-encoded arguments for the tool call.</param>
public sealed record ToolCall(
    string Id,
    string Name,
    string ArgumentsJson);
