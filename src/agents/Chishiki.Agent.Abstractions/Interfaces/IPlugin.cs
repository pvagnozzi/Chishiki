// -----------------------------------------------------------------------------
// File:        IPlugin.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the contract for a Chishiki agent plugin with lifecycle management.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Interfaces;

/// <summary>
/// Defines the contract for a Chishiki agent plugin.
/// Plugins expose named capabilities and manage their own initialization and shutdown lifecycle.
/// </summary>
public interface IPlugin : IAsyncDisposable
{
    #region Properties

    /// <summary>Gets the unique identifier for this plugin.</summary>
    string Id { get; }

    /// <summary>Gets the human-readable display name of this plugin.</summary>
    string DisplayName { get; }

    /// <summary>Gets the semantic version string for this plugin, e.g. <c>1.0.0</c>.</summary>
    string Version { get; }

    /// <summary>Gets a brief description of what this plugin does.</summary>
    string Description { get; }

    /// <summary>Gets the list of capabilities exposed by this plugin.</summary>
    IReadOnlyList<IPluginCapability> Capabilities { get; }

    #endregion

    #region Async Methods

    /// <summary>Initializes the plugin using the supplied context.</summary>
    /// <param name="context">The plugin context providing services, configuration, and logging.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default);

    /// <summary>Shuts the plugin down gracefully, releasing any held resources.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    #endregion
}
