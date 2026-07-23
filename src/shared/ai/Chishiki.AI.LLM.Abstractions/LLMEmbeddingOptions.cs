// -----------------------------------------------------------------------------
// File:        LLMEmbeddingOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configures parameters sent to the LLM for a text embedding request.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Configures parameters sent to the LLM for a text embedding request.</summary>
public sealed record LLMEmbeddingOptions
{
    /// <summary>Gets the model identifier to use for this request. When <c>null</c> the client default is applied.</summary>
    public string? ModelId { get; init; }

    /// <summary>Gets the number of dimensions for the output embedding. When <c>null</c> the model default is used.</summary>
    public int? Dimensions { get; init; }
}
