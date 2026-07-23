// -----------------------------------------------------------------------------
// File:        AgentException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base exception class for all Chishiki agent subsystem failures.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Exceptions;

/// <summary>Base exception class for all Chishiki agent subsystem failures.</summary>
public class AgentException : Exception
{
    #region Constructors

    /// <summary>Initializes a new instance of the <see cref="AgentException"/> class.</summary>
    public AgentException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AgentException"/> class with a specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public AgentException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AgentException"/> class with a specified error message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public AgentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    #endregion
}
