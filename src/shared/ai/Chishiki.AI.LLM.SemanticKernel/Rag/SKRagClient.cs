// -----------------------------------------------------------------------------
// File:        SKRagClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: ILLMRagClient implementation using SK VectorStore for retrieval and IChatCompletionService for grounded answers.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Text;
using System.Text.Json;
using Chishiki.AI.LLM.Abstractions;
using Chishiki.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Chishiki.AI.LLM.SemanticKernel.Rag;

/// <summary>Implements <see cref="ILLMRagClient"/> for RAG operations. Note: Vector store integration currently disabled pending SK compatibility resolution.</summary>
/// <param name="embeddingGenerator">The embedding generator used to vectorise queries and documents.</param>
/// <param name="chatService">The chat completion service used to produce grounded answers.</param>
/// <param name="logger">Logger instance.</param>
public sealed partial class SKRagClient(
    // IVectorStore vectorStore,  // TODO: SK 1.76 - IVectorStore not found in Microsoft.Extensions.VectorData.Abstractions 10.6.0
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IChatCompletionService _,  // Temporarily unused pending RAG implementation
    ILogger<SKRagClient> logger) : Service(logger), ILLMRagClient
{
    #region Constants

    private const string RagSystemPrompt =
        "You are a helpful assistant. Answer the question using ONLY the context provided below. " +
        "If the context does not contain enough information to answer, say you don't know. " +
        "Do not make up information.\n\nContext:\n{0}";

    #endregion

    #region Ingest

    /// <inheritdoc/>
    public Task IngestAsync(
        string collectionName,
        LLMRagDocument document,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SK VectorStore integration pending - types not found in Microsoft.Extensions.VectorData.Abstractions 10.6.0");
    }

    /// <inheritdoc/>
    public Task IngestBatchAsync(
        string collectionName,
        IReadOnlyList<LLMRagDocument> documents,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SK VectorStore integration pending - types not found in Microsoft.Extensions.VectorData.Abstractions 10.6.0");
    }

    #endregion

    #region Delete

    /// <inheritdoc/>
    public Task DeleteAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SK VectorStore integration pending - types not found in Microsoft.Extensions.VectorData.Abstractions 10.6.0");
    }

    #endregion

    #region Search

    /// <inheritdoc/>
    public Task<IReadOnlyList<LLMRagSearchResult>> SearchAsync(
        string collectionName,
        string query,
        LLMRagSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SK VectorStore integration pending - types not found in Microsoft.Extensions.VectorData.Abstractions 10.6.0");
    }

    #endregion

    #region Ask

    /// <inheritdoc/>
    public async Task<LLMRagAskResult> AskAsync(
        string collectionName,
        string question,
        LLMRagSearchOptions? searchOptions = null,
        LLMChatOptions? chatOptions = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("SK VectorStore integration pending - types not found in Microsoft.Extensions.VectorData.Abstractions 10.6.0");
    }

    #endregion

    #region Private Helpers

    private async Task<SKRagRecord> ToRecordAsync(LLMRagDocument doc, CancellationToken ct)
    {
        var embeddings = await embeddingGenerator.GenerateAsync([doc.Text], cancellationToken: ct);
        return new SKRagRecord
        {
            Id = doc.Id,
            Text = doc.Text,
            MetadataJson = doc.Metadata is not null ? JsonSerializer.Serialize(doc.Metadata) : null,
            Embedding = embeddings[0].Vector,
        };
    }

    private async Task<ReadOnlyMemory<float>> GenerateQueryEmbeddingAsync(string query, CancellationToken ct)
    {
        var embeddings = await embeddingGenerator.GenerateAsync([query], cancellationToken: ct);
        return embeddings[0].Vector;
    }

    private static LLMRagDocument ToDocument(SKRagRecord record)
    {
        IReadOnlyDictionary<string, string>? metadata = null;
        if (record.MetadataJson is not null)
            metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(record.MetadataJson);

        return new LLMRagDocument(record.Id, record.Text, metadata);
    }
    #endregion

    #region Log Messages

    [LoggerMessage(Level = LogLevel.Debug, Message = "Ingesting {Count} document(s) into collection '{Collection}'")]
    private partial void LogIngestStarted(string collection, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Ingested {Count} document(s) into collection '{Collection}' successfully")]
    private partial void LogIngestCompleted(string collection, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Deleting document '{DocumentId}' from collection '{Collection}'")]
    private partial void LogDeleteStarted(string collection, string documentId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Deleted document '{DocumentId}' from collection '{Collection}'")]
    private partial void LogDeleteCompleted(string collection, string documentId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Searching collection '{Collection}' for '{Query}' (topK={TopK})")]
    private partial void LogSearchStarted(string collection, string query, int topK);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Search on '{Collection}' returned {ResultCount} result(s)")]
    private partial void LogSearchCompleted(string collection, int resultCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "RAG ask on collection '{Collection}': '{Question}'")]
    private partial void LogAskStarted(string collection, string question);

    [LoggerMessage(Level = LogLevel.Debug, Message = "RAG ask on '{Collection}' answered ({AnswerLength} chars) from {SourceCount} source(s)")]
    private partial void LogAskCompleted(string collection, int answerLength, int sourceCount);

    #endregion
}
