// -----------------------------------------------------------------------------
// File:        IModelRouter.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the contract for resolving model aliases to concrete provider and model pairs.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Abstractions.Interfaces;

/// <summary>
/// Defines the contract for resolving logical model aliases to a concrete <see cref="IProvider"/>
/// and model identifier pair, supporting weighted routing and fallback strategies.
/// </summary>
public interface IModelRouter
{
    #region Properties

    /// <summary>Gets the current route registrations keyed by model alias.</summary>
    IReadOnlyDictionary<string, RouteConfig> Routes { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Resolves the given model alias to an available <see cref="IProvider"/> and model identifier.
    /// </summary>
    /// <param name="modelAlias">The logical alias to resolve, e.g. <c>gpt-4o</c>.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>
    /// A tuple of the resolved <see cref="IProvider"/> and the provider-specific model identifier.
    /// </returns>
    /// <exception cref="Exceptions.ModelNotFoundException">
    /// Thrown when no route is registered for <paramref name="modelAlias"/> or all routes are unavailable.
    /// </exception>
    Task<(IProvider Provider, string Model)> ResolveAsync(string modelAlias, CancellationToken cancellationToken = default);

    /// <summary>Registers or replaces the route configuration for the given alias.</summary>
    /// <param name="modelAlias">The logical alias to register, e.g. <c>gpt-4o</c>.</param>
    /// <param name="config">The <see cref="RouteConfig"/> that specifies the target provider and model.</param>
    void RegisterRoute(string modelAlias, RouteConfig config);

    #endregion
}
