// -----------------------------------------------------------------------------
// File:        PluginContext.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides host services to a loaded plugin.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Core.Plugins;

/// <summary>Provides the host's services and configuration to a loaded plugin instance.</summary>
/// <remarks>Initializes a new instance of the <see cref="PluginContext"/> class.</remarks>
/// <param name="services">The host's service provider.</param>
/// <param name="configuration">The host's configuration.</param>
/// <param name="logger">A logger scoped to the plugin.</param>
/// <param name="pluginDirectory">The directory from which the plugin was loaded.</param>
public sealed class PluginContext(
    IServiceProvider services,
    IConfiguration configuration,
    ILogger logger,
    string pluginDirectory) : IPluginContext
{
    /// <inheritdoc/>
    public IServiceProvider Services { get; } = services;

    /// <inheritdoc/>
    public IConfiguration Configuration { get; } = configuration;

    /// <inheritdoc/>
    public ILogger Logger { get; } = logger;

    /// <inheritdoc/>
    public string PluginDirectory { get; } = pluginDirectory;
}
