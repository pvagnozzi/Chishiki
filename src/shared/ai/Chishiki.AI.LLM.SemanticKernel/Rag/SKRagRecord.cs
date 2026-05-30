// -----------------------------------------------------------------------------
// File:        SKRagRecord.cs
// Author:      Piergiorgio Vagnozzi
// Description: Internal vector store record schema used by the SK RAG client.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Microsoft.Extensions.VectorData;

namespace Chishiki.AI.LLM.SemanticKernel.Rag;

/// <summary>Internal vector store record schema used by the SK RAG client.</summary>
internal sealed class SKRagRecord
{
    /// <summary>Gets or sets the unique identifier of the record.</summary>
    [VectorStoreKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the original text content of the document.</summary>
    [VectorStoreData(IsFullTextIndexed = true)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized metadata JSON attached to the document.</summary>
    [VectorStoreData]
    public string? MetadataJson { get; set; }

    /// <summary>Gets or sets the embedding vector for this record.</summary>
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
