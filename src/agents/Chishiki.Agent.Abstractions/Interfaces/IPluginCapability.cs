// -----------------------------------------------------------------------------
// File:        IPluginCapability.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the contract for a single executable capability exposed by a plugin.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Abstractions.Interfaces;

/// <summary>Defines the contract for a single named, executable capability exposed by an <see cref="IPlugin"/>.</summary>
public interface IPluginCapability
{
    #region Properties

    /// <summary>Gets the unique name of this capability within its plugin.</summary>
    string Name { get; }

    /// <summary>Gets a brief description of what this capability does.</summary>
    string Description { get; }

    /// <summary>
    /// Gets a dictionary describing the parameters this capability accepts.
    /// Keys are parameter names; values are human-readable descriptions.
    /// </summary>
    IReadOnlyDictionary<string, string> ParameterSchema { get; }

    #endregion

    #region Async Methods

    /// <summary>Executes the capability with the provided request and returns the result.</summary>
    /// <param name="request">The capability request containing the capability name and input parameters.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="CapabilityResult"/> describing the outcome of the execution.</returns>
    Task<CapabilityResult> ExecuteAsync(CapabilityRequest request, CancellationToken cancellationToken = default);

    #endregion
}
