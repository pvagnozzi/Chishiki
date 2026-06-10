// -----------------------------------------------------------------------------
// File:        EspToolProtocolException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines an exception for ESP ROM protocol-level failures.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Exceptions;

/// <summary>Represents a failure reported by the ESP ROM bootloader protocol.</summary>
/// <remarks>Initializes a new instance of the <see cref="EspToolProtocolException"/> class.</remarks>
/// <param name="message">The exception message.</param>
/// <param name="status">The protocol status byte returned by the target.</param>
/// <param name="errorCode">The protocol error code returned by the target.</param>
public sealed class EspToolProtocolException(string message, byte status, byte errorCode) : EspToolException(message)
{
    /// <summary>Gets the status byte returned by the target.</summary>
    public byte Status { get; } = status;

    /// <summary>Gets the error code returned by the target.</summary>
    public byte ErrorCode { get; } = errorCode;
}
