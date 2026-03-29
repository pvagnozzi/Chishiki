// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts.Requests;

/// <summary>Parameters for applying a named collection to the current workspace.</summary>
/// <param name="CollectionName">The name of the collection to apply.</param>
/// <param name="TargetDirectory">Optional directory where assets should be written; defaults to the workspace root.</param>
public record ApplyCollectionRequest(
    string CollectionName,
    string? TargetDirectory = null
);
