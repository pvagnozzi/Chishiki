// -----------------------------------------------------------------------------
// File:        RouteConfig.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines routing configuration for resolving a model alias to a specific provider and model.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Defines routing configuration that maps a model alias to a concrete provider and model identifier.</summary>
/// <param name="Provider">The identifier of the provider to route requests to.</param>
/// <param name="Model">The model identifier exposed by the provider.</param>
/// <param name="Weight">
/// Relative weight used when multiple routes share the same alias for load-balancing purposes.
/// Defaults to <c>1.0</c>.
/// </param>
/// <param name="Fallback">Optional alias to fall back to when this route is unavailable.</param>
public sealed record RouteConfig(
    string Provider,
    string Model,
    double Weight = 1.0,
    string? Fallback = null);
