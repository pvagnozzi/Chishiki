// -----------------------------------------------------------------------------
// File:        ExceptionBase.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base exception class providing correlation identifier tracking for distributed systems.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;

namespace Chishiki.Exceptions;

/// <summary>Abstract base exception class providing correlation identifier tracking for distributed systems.</summary>
/// <param name="message">The exception message.</param>
/// <param name="correlationId">Optional correlation identifier for distributed tracing across services.</param>
/// <param name="innerException">Optional inner exception.</param>
[SuppressMessage("Design", "CA1710:IdentifiersShouldHaveCorrectSuffix",
    Justification = "ExceptionBase is an intentional name for an abstract base exception class.")]
public abstract class ExceptionBase(string? message, Guid? correlationId = null, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>Gets the correlation identifier used to trace this exception across distributed systems.</summary>
    public Guid? CorrelationId { get; } = correlationId;
}
