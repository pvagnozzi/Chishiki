// -----------------------------------------------------------------------------
// File:        IProvider.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the contract for an AI model provider capable of executing completions and streaming.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Abstractions.Interfaces;

/// <summary>
/// Defines the contract for an AI model provider that can execute chat completions,
/// stream responses, and enumerate available models.
/// </summary>
public interface IProvider : IAsyncDisposable
{
    #region Properties

    /// <summary>Gets the unique identifier for this provider.</summary>
    string Id { get; }

    /// <summary>Gets the human-readable display name of this provider.</summary>
    string DisplayName { get; }

    /// <summary>Gets a value indicating whether this provider is currently available and healthy.</summary>
    bool IsAvailable { get; }

    #endregion

    #region Async Methods

    /// <summary>Returns the list of AI models available through this provider.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A read-only list of <see cref="ModelInfo"/> descriptors.</returns>
    Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>Executes a chat-completion request and returns the full response.</summary>
    /// <param name="request">The completion request parameters.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The <see cref="CompletionResponse"/> produced by the model.</returns>
    Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Executes a chat-completion request and streams the response as incremental chunks.</summary>
    /// <param name="request">The completion request parameters. <see cref="CompletionRequest.Stream"/> should be <c>true</c>.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>An async sequence of <see cref="CompletionChunk"/> instances.</returns>
    IAsyncEnumerable<CompletionChunk> StreamAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    #endregion
}
