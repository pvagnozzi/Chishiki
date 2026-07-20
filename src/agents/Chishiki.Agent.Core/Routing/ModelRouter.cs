// -----------------------------------------------------------------------------
// File:        ModelRouter.cs
// Author:      Piergiorgio Vagnozzi
// Description: Thread-safe model router that resolves model aliases to providers.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Collections.Concurrent;
using Chishiki.Agent.Abstractions.Exceptions;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;
using Chishiki.Agent.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chishiki.Agent.Core.Routing;

/// <summary>Thread-safe model router that resolves model aliases to (provider, model) pairs.</summary>
public sealed partial class ModelRouter : IModelRouter
{
    #region Fields

    private readonly ConcurrentDictionary<string, RouteConfig> _routes = new(StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyList<IProvider> _providers;
    private readonly AgentOptions _options;
    private readonly ILogger<ModelRouter> _logger;

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="ModelRouter"/> class.</summary>
    /// <param name="options">Agent configuration options.</param>
    /// <param name="providers">All registered providers.</param>
    /// <param name="logger">Logger instance.</param>
    public ModelRouter(
        IOptions<AgentOptions> options,
        IEnumerable<IProvider> providers,
        ILogger<ModelRouter> logger)
    {
        _options = options.Value;
        _providers = [..providers];
        _logger = logger;

        foreach (var (alias, config) in _options.Routing.Routes)
        {
            _routes[alias] = config;
        }

        LogRouterInitialized(_logger, _routes.Count, _providers.Count);
    }

    #endregion

    #region IModelRouter

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, RouteConfig> Routes => _routes;

    /// <inheritdoc/>
    public void RegisterRoute(string modelAlias, RouteConfig config)
    {
        _routes[modelAlias] = config;
        LogRouteRegistered(_logger, modelAlias, config.Provider, config.Model);
    }

    /// <inheritdoc/>
    public Task<(IProvider Provider, string Model)> ResolveAsync(
        string modelAlias,
        CancellationToken cancellationToken = default)
    {
        // 1. Direct route lookup
        if (_routes.TryGetValue(modelAlias, out var route))
        {
            return ResolveRouteAsync(route, modelAlias, cancellationToken);
        }

        // 2. Default model fallback
        var defaultAlias = _options.Routing.DefaultModel;
        if (!string.IsNullOrWhiteSpace(defaultAlias) &&
            !string.Equals(defaultAlias, modelAlias, StringComparison.OrdinalIgnoreCase) &&
            _routes.TryGetValue(defaultAlias, out var defaultRoute))
        {
            LogAliasNotFound(_logger, modelAlias, defaultAlias);
            return ResolveRouteAsync(defaultRoute, defaultAlias, cancellationToken);
        }

        // 3. Try to find a provider that supports this exact model name
        var provider = _providers.FirstOrDefault(p => p.IsAvailable);
        if (provider is not null)
        {
            LogUsingFirstAvailable(_logger, modelAlias, provider.Id);
            return Task.FromResult((provider, modelAlias));
        }

        throw new ModelNotFoundException(modelAlias);
    }

    #endregion

    #region Private Helpers

    private Task<(IProvider, string)> ResolveRouteAsync(
        RouteConfig route,
        string alias,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var provider = _providers.FirstOrDefault(p =>
            string.Equals(p.Id, route.Provider, StringComparison.OrdinalIgnoreCase) && p.IsAvailable);

        if (provider is null)
        {
            // Try fallback providers
            foreach (var fallbackId in _options.Routing.Fallback)
            {
                provider = _providers.FirstOrDefault(p =>
                    string.Equals(p.Id, fallbackId, StringComparison.OrdinalIgnoreCase) && p.IsAvailable);

                if (provider is not null)
                {
                    LogFallbackProvider(_logger, alias, route.Provider, fallbackId);
                    return Task.FromResult((provider, route.Model));
                }
            }

            throw new ProviderException(route.Provider, $"Provider '{route.Provider}' is unavailable and no fallback succeeded.");
        }

        LogRouteResolved(_logger, alias, provider.Id, route.Model);
        return Task.FromResult((provider, route.Model));
    }

    #endregion

    #region Logging

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information,
        Message = "ModelRouter initialised with {RouteCount} routes and {ProviderCount} providers.")]
    private static partial void LogRouterInitialized(ILogger logger, int routeCount, int providerCount);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug,
        Message = "Route registered: alias='{Alias}' → provider='{Provider}' model='{Model}'.")]
    private static partial void LogRouteRegistered(ILogger logger, string alias, string provider, string model);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Debug,
        Message = "Alias '{Alias}' not found; falling back to default route '{Default}'.")]
    private static partial void LogAliasNotFound(ILogger logger, string alias, string @default);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Debug,
        Message = "Route '{Alias}' resolved → provider='{ProviderId}' model='{Model}'.")]
    private static partial void LogRouteResolved(ILogger logger, string alias, string providerId, string model);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning,
        Message = "Primary provider '{Primary}' unavailable for route '{Alias}'; using fallback '{Fallback}'.")]
    private static partial void LogFallbackProvider(ILogger logger, string alias, string primary, string fallback);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Warning,
        Message = "No route found for '{Alias}'; routing to first available provider '{ProviderId}'.")]
    private static partial void LogUsingFirstAvailable(ILogger logger, string alias, string providerId);

    #endregion
}
