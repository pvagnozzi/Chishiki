// -----------------------------------------------------------------------------
// File:        PluginException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Exception thrown when a plugin encounters an error during initialization, execution, or shutdown.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Exceptions;

/// <summary>Exception thrown when a plugin encounters an error during initialization, capability execution, or shutdown.</summary>
public class PluginException : AgentException
{
    #region Properties

    /// <summary>Gets the identifier of the plugin that raised this exception.</summary>
    public string PluginId { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginException"/> class with the plugin identifier and an error message.
    /// </summary>
    /// <param name="pluginId">The identifier of the plugin that failed.</param>
    /// <param name="message">The message that describes the error.</param>
    public PluginException(string pluginId, string message)
        : base(message)
    {
        PluginId = pluginId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginException"/> class with the plugin identifier,
    /// an error message, and an inner exception.
    /// </summary>
    /// <param name="pluginId">The identifier of the plugin that failed.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PluginException(string pluginId, string message, Exception innerException)
        : base(message, innerException)
    {
        PluginId = pluginId;
    }

    #endregion
}
