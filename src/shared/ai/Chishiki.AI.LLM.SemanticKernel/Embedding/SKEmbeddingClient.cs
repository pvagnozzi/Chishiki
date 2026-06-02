// -----------------------------------------------------------------------------
// File:        SKEmbeddingClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: ILLMEmbeddingClient implementation backed by a Microsoft.Extensions.AI IEmbeddingGenerator.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using Chishiki.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Chishiki.AI.LLM.SemanticKernel.Embedding;

/// <summary>Implements <see cref="ILLMEmbeddingClient"/> backed by a <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/>.</summary>
/// <param name="embeddingGenerator">The Microsoft.Extensions.AI embedding generator.</param>
/// <param name="logger">Logger instance.</param>
public sealed partial class SKEmbeddingClient(
    IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>> embeddingGenerator,
    ILogger<SKEmbeddingClient> logger) : Service(logger), ILLMEmbeddingClient
{
    /// <summary>Gets the underlying embedding generator for use by other components.</summary>
    internal IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>> Generator => embeddingGenerator;

    /// <inheritdoc/>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text,
        LLMEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogEmbeddingStarted(1);

        try
        {
            var results = await embeddingGenerator.GenerateAsync([text], cancellationToken: cancellationToken);
            var embedding = results[0].Vector;
            LogEmbeddingCompleted(1, embedding.Length);
            return embedding;
        }
        catch (Exception ex)
        {
            LogEmbeddingFailed(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        LLMEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogEmbeddingStarted(texts.Count);

        try
        {
            var results = await embeddingGenerator.GenerateAsync(texts, cancellationToken: cancellationToken);
            IReadOnlyList<ReadOnlyMemory<float>> embeddings = [.. results.Select(e => e.Vector)];
            LogEmbeddingCompleted(texts.Count, embeddings.Count > 0 ? embeddings[0].Length : 0);
            return embeddings;
        }
        catch (Exception ex)
        {
            LogEmbeddingFailed(ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Generating embeddings for {TextCount} text(s)")]
    private partial void LogEmbeddingStarted(int textCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Generated {TextCount} embedding(s) with {Dimensions} dimensions")]
    private partial void LogEmbeddingCompleted(int textCount, int dimensions);

    [LoggerMessage(Level = LogLevel.Error, Message = "Embedding generation failed")]
    private partial void LogEmbeddingFailed(Exception ex);
}
