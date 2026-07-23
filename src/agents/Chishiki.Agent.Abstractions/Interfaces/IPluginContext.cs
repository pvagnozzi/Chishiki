// -----------------------------------------------------------------------------
// File:        IPluginContext.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides a plugin with access to host services, configuration, logging, and file-system paths.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Abstractions.Interfaces;

/// <summary>
/// Provides a plugin with access to the host's dependency-injection container, configuration system,
/// logging infrastructure, and file-system paths specific to the plugin.
/// </summary>
public interface IPluginContext
{
    #region Properties

    /// <summary>Gets the host's <see cref="IServiceProvider"/> for resolving registered services.</summary>
    IServiceProvider Services { get; }

    /// <summary>Gets the application configuration, including plugin-scoped configuration sections.</summary>
    IConfiguration Configuration { get; }

    /// <summary>Gets the logger for the plugin to write structured log messages.</summary>
    ILogger Logger { get; }

    /// <summary>Gets the absolute file-system path to the directory that contains the plugin's files.</summary>
    string PluginDirectory { get; }

    #endregion
}
