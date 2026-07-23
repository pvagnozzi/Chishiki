// -----------------------------------------------------------------------------
// File:        EspToolException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the base exception type for ESPTool library failures.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Exceptions;

/// <summary>Represents the base exception for ESP ROM bootloader operations.</summary>
public class EspToolException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="EspToolException"/> class.</summary>
    public EspToolException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EspToolException"/> class with a specific message.</summary>
    /// <param name="message">The exception message.</param>
    public EspToolException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EspToolException"/> class with a specific message and inner exception.</summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception that caused the failure.</param>
    public EspToolException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
