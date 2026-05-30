// -----------------------------------------------------------------------------
// File:        SKLLMClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: Unified ILLMClient implementation delegating to SK-backed chat, embedding, and RAG clients.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using Chishiki.AI.LLM.SemanticKernel.Chat;
using Chishiki.AI.LLM.SemanticKernel.Embedding;
using Chishiki.AI.LLM.SemanticKernel.Rag;
using Chishiki.Services;
using Microsoft.Extensions.Logging;

namespace Chishiki.AI.LLM.SemanticKernel;

/// <summary>Unified <see cref="ILLMClient"/> implementation delegating to SK-backed chat, embedding, and RAG clients.</summary>
/// <param name="chatClient">The SK chat client.</param>
/// <param name="embeddingClient">The SK embedding client.</param>
/// <param name="ragClient">The SK RAG client.</param>
/// <param name="defaultModelId">The default model identifier reported by this client.</param>
/// <param name="providerName">The provider name reported by this client (e.g. <c>"ollama"</c>).</param>
/// <param name="logger">Logger instance.</param>
public sealed partial class SKLLMClient(
    SKChatClient chatClient,
    SKEmbeddingClient embeddingClient,
    SKRagClient ragClient,
    string defaultModelId,
    string providerName,
    ILogger<SKLLMClient> logger) : Service(logger), ILLMClient
{
    /// <inheritdoc/>
    public string DefaultModelId => defaultModelId;

    /// <inheritdoc/>
    public string ProviderName => providerName;

    // -------------------------------------------------------------------------
    // Chat
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Task<LLMChatResponse> CompleteAsync(
        IReadOnlyList<LLMChatMessage> messages,
        LLMChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LogDelegatingChat(providerName, defaultModelId);
        return chatClient.CompleteAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<LLMStreamingChatChunk> CompleteStreamingAsync(
        IReadOnlyList<LLMChatMessage> messages,
        LLMChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LogDelegatingChatStreaming(providerName, defaultModelId);
        return chatClient.CompleteStreamingAsync(messages, options, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Embedding
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text,
        LLMEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LogDelegatingEmbedding(providerName, 1);
        return embeddingClient.GenerateEmbeddingAsync(text, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        LLMEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        LogDelegatingEmbedding(providerName, texts.Count);
        return embeddingClient.GenerateEmbeddingsAsync(texts, options, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // RAG
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Task IngestAsync(string collectionName, LLMRagDocument document, CancellationToken cancellationToken = default) =>
        ragClient.IngestAsync(collectionName, document, cancellationToken);

    /// <inheritdoc/>
    public Task IngestBatchAsync(string collectionName, IReadOnlyList<LLMRagDocument> documents, CancellationToken cancellationToken = default) =>
        ragClient.IngestBatchAsync(collectionName, documents, cancellationToken);

    /// <inheritdoc/>
    public Task DeleteAsync(string collectionName, string documentId, CancellationToken cancellationToken = default) =>
        ragClient.DeleteAsync(collectionName, documentId, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<LLMRagSearchResult>> SearchAsync(string collectionName, string query, LLMRagSearchOptions? options = null, CancellationToken cancellationToken = default) =>
        ragClient.SearchAsync(collectionName, query, options, cancellationToken);

    /// <inheritdoc/>
    public Task<LLMRagAskResult> AskAsync(string collectionName, string question, LLMRagSearchOptions? searchOptions = null, LLMChatOptions? chatOptions = null, CancellationToken cancellationToken = default) =>
        ragClient.AskAsync(collectionName, question, searchOptions, chatOptions, cancellationToken);

    // -------------------------------------------------------------------------
    // Log messages
    // -------------------------------------------------------------------------

    [LoggerMessage(Level = LogLevel.Debug, Message = "Delegating chat to provider '{Provider}' model '{ModelId}'")]
    private partial void LogDelegatingChat(string provider, string modelId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Delegating streaming chat to provider '{Provider}' model '{ModelId}'")]
    private partial void LogDelegatingChatStreaming(string provider, string modelId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Delegating embedding to provider '{Provider}' for {TextCount} text(s)")]
    private partial void LogDelegatingEmbedding(string provider, int textCount);
}
