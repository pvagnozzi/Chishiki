// -----------------------------------------------------------------------------
// File:        CompletionChunk.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a single streaming chunk emitted during a chat-completion stream.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Represents a single incremental chunk emitted during a streamed chat-completion response.</summary>
/// <param name="Id">Provider-assigned identifier shared across all chunks of the same completion.</param>
/// <param name="Model">The model identifier that is generating the stream.</param>
/// <param name="Delta">The incremental text fragment produced in this chunk. <c>null</c> on the final chunk.</param>
/// <param name="IsFinished">Indicates whether this is the final chunk in the stream.</param>
/// <param name="FinishReason">The reason the stream finished. Only set on the final chunk.</param>
public sealed record CompletionChunk(
    string Id,
    string Model,
    string? Delta,
    bool IsFinished,
    string? FinishReason = null);
