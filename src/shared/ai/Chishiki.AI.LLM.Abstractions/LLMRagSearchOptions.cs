// -----------------------------------------------------------------------------
// File:        LLMRagSearchOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configures parameters for a semantic similarity search in the RAG vector store.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Configures parameters for a semantic similarity search in the RAG vector store.</summary>
public sealed record LLMRagSearchOptions
{
    /// <summary>Gets the maximum number of results to return. Defaults to <c>5</c>.</summary>
    public int TopK { get; init; } = 5;

    /// <summary>Gets the minimum relevance score threshold in the range [0, 1]. Results below this value are discarded. Defaults to <c>0.0</c>.</summary>
    public double MinRelevanceScore { get; init; }

    /// <summary>Gets optional metadata filters applied before the vector search. Only documents matching all pairs are considered.</summary>
    public IReadOnlyDictionary<string, string>? Filters { get; init; }
}
