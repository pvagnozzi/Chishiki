// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts.Requests;

/// <summary>Parameters for creating a new hub resource.</summary>
/// <param name="Type">The resource type to create.</param>
/// <param name="Name">Unique name for the new resource, used as the file name.</param>
/// <param name="Description">Short human-readable description of the resource.</param>
/// <param name="Tags">Tags to attach to the resource for filtering and discovery.</param>
/// <param name="TemplateName">Optional built-in template to scaffold the initial content from.</param>
public record CreateResourceRequest(
    HubResourceType Type,
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    string? TemplateName = null
);
