// -----------------------------------------------------------------------------
// File:        NotFoundDomainException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Domain exception thrown when a requested entity or resource is not found.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Core;

/// <summary>Domain exception thrown when a requested entity or resource is not found.</summary>
/// <param name="message">The exception message.</param>
/// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
/// <param name="innerException">Optional inner exception.</param>
public class NotFoundDomainException(string message, Guid? correlationId = null, Exception? innerException = null)
    : DomainException(message, correlationId, innerException);
