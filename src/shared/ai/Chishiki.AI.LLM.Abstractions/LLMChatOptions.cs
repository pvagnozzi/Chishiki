// -----------------------------------------------------------------------------
// File:        LLMChatOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configures inference parameters sent to the LLM for a chat or completion request.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Configures inference parameters sent to the LLM for a chat or completion request.</summary>
public sealed record LLMChatOptions
{
    /// <summary>Gets the model identifier to use for this request. When <c>null</c> the client default is applied.</summary>
    public string? ModelId { get; init; }

    /// <summary>Gets the sampling temperature controlling randomness. Range [0, 2]. Defaults to <c>null</c> (provider default).</summary>
    public float? Temperature { get; init; }

    /// <summary>Gets the nucleus-sampling probability mass. Range (0, 1]. Defaults to <c>null</c> (provider default).</summary>
    public float? TopP { get; init; }

    /// <summary>Gets the maximum number of tokens to generate. Defaults to <c>null</c> (provider default).</summary>
    public int? MaxTokens { get; init; }

    /// <summary>Gets stop sequences that instruct the model to stop generating further tokens.</summary>
    public IReadOnlyList<string>? StopSequences { get; init; }
}
