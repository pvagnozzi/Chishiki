// -----------------------------------------------------------------------------
// File:        ILLMEmbeddingClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the contract for generating vector embeddings from text using an LLM provider.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Defines the contract for generating vector embeddings from text using an LLM provider.</summary>
public interface ILLMEmbeddingClient
{
    /// <summary>Generates a vector embedding for the specified text.</summary>
    /// <param name="text">The input text to embed.</param>
    /// <param name="options">Optional embedding parameters. When <c>null</c> client defaults apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="ReadOnlyMemory{T}"/> of floats representing the embedding vector.</returns>
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text,
        LLMEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Generates vector embeddings for a batch of texts.</summary>
    /// <param name="texts">The collection of input texts to embed.</param>
    /// <param name="options">Optional embedding parameters. When <c>null</c> client defaults apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A list of <see cref="ReadOnlyMemory{T}"/> vectors, one per input text in order.</returns>
    Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        LLMEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default);
}
