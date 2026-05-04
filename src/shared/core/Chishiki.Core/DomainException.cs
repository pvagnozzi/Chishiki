// -----------------------------------------------------------------------------
// File:        DomainException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base exception class for domain-specific errors with correlation tracking.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Core;

/// <summary>Base exception class for domain-specific errors with correlation tracking.</summary>
/// <param name="message">The exception message.</param>
/// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
/// <param name="innerException">Optional inner exception.</param>
public class DomainException(string message, Guid? correlationId = null, Exception? innerException = null)
    : ExceptionBase(message, correlationId, innerException);
