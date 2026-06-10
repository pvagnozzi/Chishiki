// -----------------------------------------------------------------------------
// File:        EspStubSegment.cs
// Author:      Piergiorgio Vagnozzi
// Description: Describes a RAM segment that can be loaded as part of an ESP stub image.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Represents a RAM segment that belongs to a stub-loader image.</summary>
public sealed class EspStubSegment
{
    /// <summary>Gets the absolute target RAM address where the segment must be loaded.</summary>
    public required uint LoadAddress { get; init; }

    /// <summary>Gets the raw segment data to write to RAM.</summary>
    public required byte[] Data { get; init; }
}
