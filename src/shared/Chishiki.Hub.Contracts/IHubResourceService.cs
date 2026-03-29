// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Contracts.Requests;

namespace Chishiki.Hub.Contracts;

/// <summary>Defines the operations for managing and querying developer hub resources.</summary>
public interface IHubResourceService
{
    /// <summary>Lists all resources of the given type, optionally filtered by tag.</summary>
    /// <param name="type">The resource type to list.</param>
    /// <param name="tag">Optional tag filter; only resources carrying this tag are returned.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A read-only list of matching <see cref="HubResourceDto"/> instances.</returns>
    Task<IReadOnlyList<HubResourceDto>> ListResourcesAsync(
        HubResourceType type, string? tag = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the full DTO for a named resource, or <see langword="null"/> if not found.</summary>
    /// <param name="type">The resource type to search within.</param>
    /// <param name="name">Exact name of the resource to retrieve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The matching <see cref="HubResourceDto"/>, or <see langword="null"/>.</returns>
    Task<HubResourceDto?> GetResourceAsync(
        HubResourceType type, string name, CancellationToken cancellationToken = default);

    /// <summary>Creates a new resource from <paramref name="request"/> and persists it to the hub.</summary>
    /// <param name="request">Details of the resource to create.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The newly created <see cref="HubResourceDto"/>.</returns>
    Task<HubResourceDto> CreateResourceAsync(
        CreateResourceRequest request, CancellationToken cancellationToken = default);

    /// <summary>Applies all resources listed in the named collection and returns those that were resolved.</summary>
    /// <param name="collectionName">Name of the collection to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A read-only list of the applied <see cref="HubResourceDto"/> instances.</returns>
    Task<IReadOnlyList<HubResourceDto>> ApplyCollectionAsync(
        string collectionName, CancellationToken cancellationToken = default);

    /// <summary>Generates a new project from the named template into a target directory.</summary>
    /// <param name="request">Scaffold parameters including template name and target path.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The absolute path of the generated project directory.</returns>
    Task<string> ScaffoldProjectAsync(
        ScaffoldProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>Performs full-text search across resource names, descriptions, and tags.</summary>
    /// <param name="request">Search parameters including query string, optional type, and tag filters.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A read-only list of matching <see cref="HubResourceDto"/> instances.</returns>
    Task<IReadOnlyList<HubResourceDto>> SearchResourcesAsync(
        SearchResourcesRequest request, CancellationToken cancellationToken = default);
}
