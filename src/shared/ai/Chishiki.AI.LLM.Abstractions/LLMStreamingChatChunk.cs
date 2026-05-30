// -----------------------------------------------------------------------------
// File:        LLMStreamingChatChunk.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a single streamed token or text chunk emitted during a streaming LLM chat response.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Represents a single streamed token or text chunk emitted during a streaming LLM chat response.</summary>
/// <param name="Content">The partial text content of this chunk.</param>
/// <param name="ModelId">The model identifier that produced this chunk, if available.</param>
/// <param name="IsFinal">Indicates whether this is the last chunk in the stream.</param>
public sealed record LLMStreamingChatChunk(string Content, string? ModelId = null, bool IsFinal = false);
