// -----------------------------------------------------------------------------
// File:        EspToolTimeoutException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines an exception for ESP ROM communication timeouts.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Exceptions;

/// <summary>Represents a timeout while waiting for data from the ESP ROM bootloader.</summary>
public sealed class EspToolTimeoutException : EspToolException
{
    /// <summary>Initializes a new instance of the <see cref="EspToolTimeoutException"/> class.</summary>
    /// <param name="message">The exception message.</param>
    public EspToolTimeoutException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EspToolTimeoutException"/> class.</summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception that caused the timeout handling failure.</param>
    public EspToolTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
