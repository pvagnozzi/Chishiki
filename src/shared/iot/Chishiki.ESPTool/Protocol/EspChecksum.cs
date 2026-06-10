// -----------------------------------------------------------------------------
// File:        EspChecksum.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides checksum helpers for ESP ROM bootloader commands.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Protocol;

/// <summary>Provides checksum helpers for ESP ROM bootloader packets.</summary>
internal static class EspChecksum
{
    private const byte InitialState = 0xEF;

    /// <summary>Computes the ROM loader XOR checksum for a flash data block.</summary>
    /// <param name="data">The data block to checksum.</param>
    /// <returns>The checksum encoded as an unsigned 32-bit integer.</returns>
    public static uint Compute(ReadOnlySpan<byte> data)
    {
        var state = InitialState;

        foreach (var value in data)
        {
            state ^= value;
        }

        return state;
    }
}
