// -----------------------------------------------------------------------------
// File:        LLMRagDocument.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a document to be ingested into the RAG vector store.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Represents a document to be ingested into the RAG vector store.</summary>
/// <param name="Id">The unique identifier of the document within the collection.</param>
/// <param name="Text">The textual content to embed and store.</param>
/// <param name="Metadata">Optional key/value metadata attached to the document for filtering.</param>
public sealed record LLMRagDocument(
    string Id,
    string Text,
    IReadOnlyDictionary<string, string>? Metadata = null);
