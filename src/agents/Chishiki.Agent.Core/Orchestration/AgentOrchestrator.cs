// -----------------------------------------------------------------------------
// File:        AgentOrchestrator.cs
// Author:      Piergiorgio Vagnozzi
// Description: Central orchestrator coordinating routing, providers, and plugins.
// Created:     2026-06-28
// Modified:    2026-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Chishiki.Agent.Abstractions.Exceptions;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;
using Chishiki.Agent.Core.Plugins;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Core.Orchestration;

/// <summary>Central orchestrator that routes requests to providers and dispatches plugin capabilities.</summary>
/// <remarks>Initializes a new instance of the <see cref="AgentOrchestrator"/> class.</remarks>
/// <param name="router">The model router used to resolve provider and model for a request.</param>
/// <param name="pluginLoader">The plugin loader that manages loaded plugin instances.</param>
/// <param name="providers">All registered providers (for listing models).</param>
/// <param name="logger">Logger instance.</param>
public sealed partial class AgentOrchestrator(
    IModelRouter router,
    PluginLoader pluginLoader,
    IEnumerable<IProvider> providers,
    ILogger<AgentOrchestrator> logger) : IOrchestrator
{
    #region Fields

    private readonly IModelRouter _router = router;
    private readonly PluginLoader _pluginLoader = pluginLoader;
    private readonly ILogger<AgentOrchestrator> _logger = logger;

    #endregion
    #region Constructor

    #endregion

    #region IOrchestrator

    /// <inheritdoc/>
    public IReadOnlyList<IProvider> Providers { get; } = [.. providers];

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> Plugins =>
        [.. _pluginLoader.LoadedPlugins.Values];

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var tasks = Providers
            .Where(p => p.IsAvailable)
            .Select(p => SafeListModelsAsync(p, cancellationToken));

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<CompletionResponse> CompleteAsync(
        CompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestId = request.RequestId.ToString();
        var sw = Stopwatch.StartNew();
        LogCompletionRequested(_logger, requestId: requestId, request.Model);

        var (provider, model) = await _router.ResolveAsync(request.Model, cancellationToken);
        var routed = request with { Model = model };

        const int maxAttempts = 2;
        Exception? lastEx = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var response = await provider.CompleteAsync(routed, cancellationToken);
                LogCompletionSucceeded(_logger, requestId, sw.ElapsedMilliseconds, response.TotalTokens);
                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                LogCompletionAttemptFailed(_logger, request.RequestId.ToString(), attempt, provider.Id, ex);

                if (attempt < maxAttempts)
                {
                    // Try to find the next available provider
                    var fallback = Providers
                        .FirstOrDefault(p => p.IsAvailable && p.Id != provider.Id);

                    if (fallback is null)
                    {
                        break;
                    }
                    provider = fallback;
                }
            }
        }

        throw new ProviderException(provider.Id, $"Completion failed after {maxAttempts} attempts.", lastEx!);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<CompletionChunk> StreamAsync(
        CompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var requestId = request.RequestId.ToString();
            LogStreamRequested(_logger, requestId, request.Model);
        }

        var (provider, model) = await _router.ResolveAsync(request.Model, cancellationToken);
        var routed = request with { Model = model, Stream = true };

        await foreach (var chunk in provider.StreamAsync(routed, cancellationToken))
        {
            yield return chunk;
        }
    }

    /// <inheritdoc/>
    public async Task<CapabilityResult> ExecuteCapabilityAsync(
        string pluginId,
        CapabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_pluginLoader.LoadedPlugins.TryGetValue(pluginId, out var plugin))
        {
            throw new PluginException(pluginId, $"Plugin '{pluginId}' is not loaded.");
        }

        var capability = plugin.Capabilities
            .FirstOrDefault(c => string.Equals(c.Name, request.CapabilityName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CapabilityNotFoundException(request.CapabilityName);

        LogCapabilityExecuting(_logger, pluginId, request.CapabilityName);
        var sw = Stopwatch.StartNew();
        var result = await capability.ExecuteAsync(request, cancellationToken);
        LogCapabilityExecuted(_logger, pluginId, request.CapabilityName, sw.ElapsedMilliseconds, result.Success);
        return result;
    }

    #endregion

    #region Private Helpers

    private static async Task<IReadOnlyList<ModelInfo>> SafeListModelsAsync(
        IProvider provider,
        CancellationToken cancellationToken)
    {
        try { return await provider.ListModelsAsync(cancellationToken); }
        catch { return []; }
    }

    #endregion

    #region Logging

    [LoggerMessage(EventId = 2030, Level = LogLevel.Debug,
        Message = "Completion requested: requestId='{RequestId}' model='{Model}'.")]
    private static partial void LogCompletionRequested(ILogger logger, string requestId, string model);

    [LoggerMessage(EventId = 2031, Level = LogLevel.Information,
        Message = "Completion succeeded: requestId='{RequestId}' durationMs={DurationMs} totalTokens={TotalTokens}.")]
    private static partial void LogCompletionSucceeded(ILogger logger, string requestId, long durationMs, int totalTokens);

    [LoggerMessage(EventId = 2032, Level = LogLevel.Warning,
        Message = "Completion attempt {Attempt} failed: requestId='{RequestId}' provider='{ProviderId}'.")]
    private static partial void LogCompletionAttemptFailed(ILogger logger, string requestId, int attempt, string providerId, Exception exception);

    [LoggerMessage(EventId = 2033, Level = LogLevel.Debug,
        Message = "Stream requested: requestId='{RequestId}' model='{Model}'.")]
    private static partial void LogStreamRequested(ILogger logger, string requestId, string model);

    [LoggerMessage(EventId = 2034, Level = LogLevel.Debug,
        Message = "Executing capability '{Capability}' on plugin '{PluginId}'.")]
    private static partial void LogCapabilityExecuting(ILogger logger, string pluginId, string capability);

    [LoggerMessage(EventId = 2035, Level = LogLevel.Debug,
        Message = "Capability '{Capability}' on plugin '{PluginId}' completed in {DurationMs}ms, success={Success}.")]
    private static partial void LogCapabilityExecuted(ILogger logger, string pluginId, string capability, long durationMs, bool success);

    #endregion
}
