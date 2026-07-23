// -----------------------------------------------------------------------------
// File:        CapabilityBase.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class providing a convenient starting point for IPluginCapability implementations.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Plugins.Sdk.Base;

/// <summary>
/// Abstract base class providing a convenient starting point for <see cref="IPluginCapability"/> implementations.
/// </summary>
/// <remarks>
/// Derive from this class and override <see cref="ExecuteAsync"/> to implement custom capability logic.
/// Override <see cref="ParameterSchema"/> to expose a schema describing the expected parameters.
/// </remarks>
public abstract class CapabilityBase : IPluginCapability
{
    #region Private Fields

    private static readonly Dictionary<string, string> s_emptySchema = [];

    #endregion

    #region Properties

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string Description { get; }

    /// <summary>
    /// Gets a dictionary describing the parameters this capability accepts.
    /// Returns an empty dictionary by default; override to provide parameter documentation.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string> ParameterSchema => s_emptySchema;

    #endregion

    #region Async Methods

    /// <summary>Executes the capability with the provided request and returns the result.</summary>
    /// <param name="request">The capability request containing the capability name and input parameters.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="CapabilityResult"/> describing the outcome of the execution.</returns>
    public abstract Task<CapabilityResult> ExecuteAsync(
        CapabilityRequest request,
        CancellationToken cancellationToken = default);

    #endregion
}
