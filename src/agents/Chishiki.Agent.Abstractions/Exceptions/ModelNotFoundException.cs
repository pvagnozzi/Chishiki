// -----------------------------------------------------------------------------
// File:        ModelNotFoundException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Exception thrown when a requested model alias cannot be resolved to any registered provider.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Exceptions;

/// <summary>Exception thrown when a requested model alias cannot be resolved to any registered provider.</summary>
public class ModelNotFoundException : AgentException
{
    #region Properties

    /// <summary>Gets the model alias that could not be resolved.</summary>
    public string ModelAlias { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelNotFoundException"/> class for the given alias,
    /// using a default error message.
    /// </summary>
    /// <param name="modelAlias">The model alias that was not found.</param>
    public ModelNotFoundException(string modelAlias)
        : base($"Model alias '{modelAlias}' could not be resolved to any registered provider.")
    {
        ModelAlias = modelAlias;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelNotFoundException"/> class with the model alias and a custom message.
    /// </summary>
    /// <param name="modelAlias">The model alias that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    public ModelNotFoundException(string modelAlias, string message)
        : base(message)
    {
        ModelAlias = modelAlias;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelNotFoundException"/> class with the model alias,
    /// a custom message, and an inner exception.
    /// </summary>
    /// <param name="modelAlias">The model alias that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ModelNotFoundException(string modelAlias, string message, Exception innerException)
        : base(message, innerException)
    {
        ModelAlias = modelAlias;
    }

    #endregion
}
