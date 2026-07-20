// -----------------------------------------------------------------------------
// File:        ProviderException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Exception thrown when an AI provider encounters an error during an operation.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Exceptions;

/// <summary>Exception thrown when an AI provider encounters an error during an operation.</summary>
public class ProviderException : AgentException
{
    #region Properties

    /// <summary>Gets the identifier of the provider that raised this exception.</summary>
    public string ProviderId { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderException"/> class with the provider identifier and an error message.
    /// </summary>
    /// <param name="providerId">The identifier of the provider that failed.</param>
    /// <param name="message">The message that describes the error.</param>
    public ProviderException(string providerId, string message)
        : base(message)
    {
        ProviderId = providerId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderException"/> class with the provider identifier,
    /// an error message, and an inner exception.
    /// </summary>
    /// <param name="providerId">The identifier of the provider that failed.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ProviderException(string providerId, string message, Exception innerException)
        : base(message, innerException)
    {
        ProviderId = providerId;
    }

    #endregion
}
