// -----------------------------------------------------------------------------
// File:        LLMRagAskResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the grounded answer produced by a RAG ask operation, including source citations.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Represents the grounded answer produced by a RAG ask operation, including source citations.</summary>
/// <param name="Answer">The LLM-generated answer grounded on the retrieved context.</param>
/// <param name="Sources">The ranked source documents used to ground the answer.</param>
/// <param name="ModelId">The model identifier that produced the answer, if available.</param>
public sealed record LLMRagAskResult(
    string Answer,
    IReadOnlyList<LLMRagSearchResult> Sources,
    string? ModelId = null);
