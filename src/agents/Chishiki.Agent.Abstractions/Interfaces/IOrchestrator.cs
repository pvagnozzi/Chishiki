// -----------------------------------------------------------------------------
// File:        IOrchestrator.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the central orchestration contract that coordinates providers, plugins, and routing.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Abstractions.Interfaces;

/// <summary>
/// Defines the central orchestration contract that coordinates registered providers and plugins,
/// routes completion requests, and dispatches capability executions.
/// </summary>
public interface IOrchestrator
{
    #region Properties

    /// <summary>Gets the list of providers currently registered with the orchestrator.</summary>
    IReadOnlyList<IProvider> Providers { get; }

    /// <summary>Gets the list of plugins currently registered with the orchestrator.</summary>
    IReadOnlyList<IPlugin> Plugins { get; }

    #endregion

    #region Async Methods

    /// <summary>Returns the aggregated list of models available across all registered providers.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A read-only list of <see cref="ModelInfo"/> descriptors.</returns>
    Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Routes a chat-completion request to the appropriate provider and returns the full response.
    /// </summary>
    /// <param name="request">The completion request, including the model alias or identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The <see cref="CompletionResponse"/> returned by the resolved provider.</returns>
    Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Routes a chat-completion request to the appropriate provider and streams the response.
    /// </summary>
    /// <param name="request">The completion request. <see cref="CompletionRequest.Stream"/> should be <c>true</c>.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>An async sequence of <see cref="CompletionChunk"/> instances.</returns>
    IAsyncEnumerable<CompletionChunk> StreamAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Executes a named capability on the specified plugin.</summary>
    /// <param name="pluginId">The identifier of the plugin that owns the capability.</param>
    /// <param name="request">The capability request containing the capability name and parameters.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="CapabilityResult"/> describing the outcome of the execution.</returns>
    Task<CapabilityResult> ExecuteCapabilityAsync(string pluginId, CapabilityRequest request, CancellationToken cancellationToken = default);

    #endregion
}
