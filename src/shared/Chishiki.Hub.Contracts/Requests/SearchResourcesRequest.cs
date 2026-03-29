// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts.Requests;

/// <summary>Parameters for performing a full-text search across hub resources.</summary>
/// <param name="Query">Free-text query matched against resource name, description, and tags.</param>
/// <param name="Type">Optional type filter; restricts results to a single resource type when set.</param>
/// <param name="Tags">Optional tag filters; only resources carrying all listed tags are returned.</param>
public record SearchResourcesRequest(
    string Query,
    HubResourceType? Type = null,
    IReadOnlyList<string>? Tags = null
);
