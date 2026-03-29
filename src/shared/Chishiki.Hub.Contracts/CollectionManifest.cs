// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts;

/// <summary>
/// Represents a named collection manifest that groups hub resources into a curated bundle.
/// </summary>
/// <param name="Name">Unique name of the collection.</param>
/// <param name="Description">Human-readable description of the collection's purpose.</param>
/// <param name="Items">Ordered list of resource references included in this collection.</param>
public record CollectionManifest(
    string Name,
    string Description,
    IReadOnlyList<CollectionItem> Items
);

/// <summary>Identifies a single resource entry within a <see cref="CollectionManifest"/>.</summary>
/// <param name="Type">The resource type of the referenced item.</param>
/// <param name="ResourceName">The unique name of the referenced resource.</param>
public record CollectionItem(
    HubResourceType Type,
    string ResourceName
);
