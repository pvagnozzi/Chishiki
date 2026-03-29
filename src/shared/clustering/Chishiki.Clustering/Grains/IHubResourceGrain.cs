// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Contracts;

namespace Chishiki.Clustering.Grains;

/// <summary>
/// Orleans grain interface for caching a single hub resource, keyed by resource type and name.
/// </summary>
public interface IHubResourceGrain : IGrainWithStringKey
{
    /// <summary>Returns the cached resource DTO, or <see langword="null"/> if the cache is empty.</summary>
    /// <returns>The cached <see cref="HubResourceDto"/>, or <see langword="null"/>.</returns>
    Task<HubResourceDto?> GetAsync();

    /// <summary>Stores a resource DTO in the grain's in-memory cache.</summary>
    /// <param name="resource">The resource DTO to cache.</param>
    Task SetAsync(HubResourceDto resource);

    /// <summary>Clears the cached resource, forcing the next read to go back to the source.</summary>
    Task InvalidateAsync();
}
