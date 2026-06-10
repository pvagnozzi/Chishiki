// -----------------------------------------------------------------------------
// File:        EspFlashRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Describes a firmware flash write request for an ESP device.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Represents a firmware image write request.</summary>
public sealed class EspFlashRequest
{
    /// <summary>Gets the destination flash offset where the binary image will be written.</summary>
    public uint FlashOffset { get; init; }

    /// <summary>Gets the raw binary image data to write.</summary>
    public required byte[] ImageData { get; init; }

    /// <summary>Gets a value indicating whether the write should use the stub-loader compressed flash flow when available.</summary>
    public bool UseCompression { get; init; }

    /// <summary>Gets a value indicating whether the target should reboot after the write completes.</summary>
    public bool ResetAfterFlash { get; init; } = true;
}
