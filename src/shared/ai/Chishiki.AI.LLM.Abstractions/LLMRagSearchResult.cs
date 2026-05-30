// -----------------------------------------------------------------------------
// File:        LLMRagSearchResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a single result returned by a RAG vector similarity search.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Represents a single result returned by a RAG vector similarity search.</summary>
/// <param name="Document">The matched document.</param>
/// <param name="RelevanceScore">The cosine similarity score in the range [0, 1].</param>
public sealed record LLMRagSearchResult(LLMRagDocument Document, double RelevanceScore);
