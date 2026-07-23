// -----------------------------------------------------------------------------
// File:        CapabilityNotFoundException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Exception thrown when a requested capability name cannot be found on the target plugin.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Exceptions;

/// <summary>Exception thrown when a requested capability name cannot be found on the target plugin.</summary>
public class CapabilityNotFoundException : AgentException
{
    #region Properties

    /// <summary>Gets the capability name that could not be found.</summary>
    public string CapabilityName { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilityNotFoundException"/> class for the given
    /// capability name, using a default error message.
    /// </summary>
    /// <param name="capabilityName">The capability name that was not found.</param>
    public CapabilityNotFoundException(string capabilityName)
        : base($"Capability '{capabilityName}' was not found on the target plugin.")
    {
        CapabilityName = capabilityName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilityNotFoundException"/> class with the capability name
    /// and a custom message.
    /// </summary>
    /// <param name="capabilityName">The capability name that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    public CapabilityNotFoundException(string capabilityName, string message)
        : base(message)
    {
        CapabilityName = capabilityName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilityNotFoundException"/> class with the capability name,
    /// a custom message, and an inner exception.
    /// </summary>
    /// <param name="capabilityName">The capability name that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CapabilityNotFoundException(string capabilityName, string message, Exception innerException)
        : base(message, innerException)
    {
        CapabilityName = capabilityName;
    }

    #endregion
}
