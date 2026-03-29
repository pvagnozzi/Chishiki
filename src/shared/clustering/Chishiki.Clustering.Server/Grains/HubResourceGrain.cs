// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Grains;
using Chishiki.Hub.Contracts;

namespace Chishiki.Clustering.Server.Grains;

/// <summary>
/// Orleans grain implementation that caches a single hub resource in grain-local memory.
/// </summary>
/// <remarks>
/// Grain keys use the composite format <c>{HubResourceType}:{name}</c>.
/// The cache is held for the lifetime of the grain activation; calling
/// <see cref="InvalidateAsync"/> forces the next caller to reload from the source.
/// </remarks>
internal sealed class HubResourceGrain : Grain, IHubResourceGrain
{
    private HubResourceDto? _cached;

    /// <inheritdoc/>
    public Task<HubResourceDto?> GetAsync() => Task.FromResult(_cached);

    /// <inheritdoc/>
    public Task SetAsync(HubResourceDto resource)
    {
        _cached = resource;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task InvalidateAsync()
    {
        _cached = null;
        return Task.CompletedTask;
    }
}
