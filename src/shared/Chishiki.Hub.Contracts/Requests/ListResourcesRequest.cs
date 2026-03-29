// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts.Requests;

/// <summary>Parameters for listing hub resources of a given type.</summary>
/// <param name="Type">The resource type to list.</param>
/// <param name="Tag">Optional tag filter; only resources carrying this tag are returned.</param>
public record ListResourcesRequest(HubResourceType Type, string? Tag = null);
