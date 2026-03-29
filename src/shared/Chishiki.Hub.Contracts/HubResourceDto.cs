// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts;

/// <summary>
/// Data transfer object representing a single hub resource with its metadata and content.
/// </summary>
/// <param name="Name">Unique logical name of the resource within its type.</param>
/// <param name="Description">Human-readable description of the resource's purpose.</param>
/// <param name="Type">The type classification of the resource.</param>
/// <param name="Tags">Searchable tags associated with the resource.</param>
/// <param name="Version">Semantic version string declared in the resource's front-matter.</param>
/// <param name="Content">Full body content of the resource file after front-matter is stripped.</param>
/// <param name="LastModified">UTC timestamp of the resource file's last modification.</param>
public record HubResourceDto(
    string Name,
    string Description,
    HubResourceType Type,
    IReadOnlyList<string> Tags,
    string Version,
    string Content,
    DateTimeOffset LastModified
);
