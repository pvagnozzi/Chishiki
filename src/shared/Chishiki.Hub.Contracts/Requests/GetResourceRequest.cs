// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts.Requests;

/// <summary>Parameters for retrieving a single hub resource by type and name.</summary>
/// <param name="Type">The resource type to search within.</param>
/// <param name="Name">Exact name of the resource to retrieve.</param>
public record GetResourceRequest(HubResourceType Type, string Name);
