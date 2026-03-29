// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Grains;
using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace Chishiki.Clustering.Client.Services;

/// <summary>
/// Caching decorator for <see cref="IHubResourceService"/> that stores individually fetched
/// resources in Orleans grain memory, reducing redundant file-system reads across service instances.
/// </summary>
/// <remarks>
/// List, search, scaffold, and collection operations always delegate to the inner service.
/// Only single-resource lookups (<see cref="GetResourceAsync"/>) and creations
/// (<see cref="CreateResourceAsync"/>) interact with the grain cache.
/// </remarks>
internal sealed partial class OrleansHubResourceService : IHubResourceService
{
    private readonly IGrainFactory _grainFactory;
    private readonly IHubResourceService _inner;
    private readonly ILogger<OrleansHubResourceService> _logger;

    /// <summary>Initializes a new instance of <see cref="OrleansHubResourceService"/>.</summary>
    /// <param name="grainFactory">Orleans grain factory used to resolve per-resource cache grains.</param>
    /// <param name="inner">Fallback service that performs actual resource loading on cache misses.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public OrleansHubResourceService(
        IGrainFactory grainFactory,
        IHubResourceService inner,
        ILogger<OrleansHubResourceService> logger)
    {
        _grainFactory = grainFactory;
        _inner = inner;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<HubResourceDto>> ListResourcesAsync(
        HubResourceType type, string? tag = null, CancellationToken cancellationToken = default) =>
        _inner.ListResourcesAsync(type, tag, cancellationToken);

    /// <inheritdoc/>
    public async Task<HubResourceDto?> GetResourceAsync(
        HubResourceType type, string name, CancellationToken cancellationToken = default)
    {
        var grain = GetGrain(type, name);
        var cached = await grain.GetAsync();

        if (cached is not null)
        {
            LogCacheHit(type, name);
            return cached;
        }

        var dto = await _inner.GetResourceAsync(type, name, cancellationToken);

        if (dto is not null)
        {
            await grain.SetAsync(dto);
            LogCacheMiss(type, name);
        }

        return dto;
    }

    /// <inheritdoc/>
    public async Task<HubResourceDto> CreateResourceAsync(
        CreateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var dto = await _inner.CreateResourceAsync(request, cancellationToken);
        await GetGrain(dto.Type, dto.Name).SetAsync(dto);
        return dto;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<HubResourceDto>> ApplyCollectionAsync(
        string collectionName, CancellationToken cancellationToken = default) =>
        _inner.ApplyCollectionAsync(collectionName, cancellationToken);

    /// <inheritdoc/>
    public Task<string> ScaffoldProjectAsync(
        ScaffoldProjectRequest request, CancellationToken cancellationToken = default) =>
        _inner.ScaffoldProjectAsync(request, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<HubResourceDto>> SearchResourcesAsync(
        SearchResourcesRequest request, CancellationToken cancellationToken = default) =>
        _inner.SearchResourcesAsync(request, cancellationToken);

    private IHubResourceGrain GetGrain(HubResourceType type, string name) =>
        _grainFactory.GetGrain<IHubResourceGrain>($"{type}:{name}");

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache hit for {Type}:{Name}")]
    private partial void LogCacheHit(HubResourceType type, string name);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache miss for {Type}:{Name}, loaded from inner service")]
    private partial void LogCacheMiss(HubResourceType type, string name);
}
