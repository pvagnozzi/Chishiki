// -----------------------------------------------------------------------------
// File:        LLMChatResponse.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the result returned by an LLM for a chat or completion request.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Represents the result returned by an LLM for a chat or completion request.</summary>
/// <param name="Content">The generated text content.</param>
/// <param name="ModelId">The model identifier that produced this response.</param>
/// <param name="FinishReason">The reason the model stopped generating tokens, if available.</param>
/// <param name="Usage">Token usage metadata, if provided by the model.</param>
public sealed record LLMChatResponse(
    string Content,
    string? ModelId = null,
    string? FinishReason = null,
    LLMTokenUsage? Usage = null);
