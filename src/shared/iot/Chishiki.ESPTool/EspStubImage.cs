// -----------------------------------------------------------------------------
// File:        EspStubImage.cs
// Author:      Piergiorgio Vagnozzi
// Description: Describes a stub-loader image that can be uploaded to ESP RAM.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Represents a complete stub-loader image that can be uploaded to target RAM.</summary>
public sealed class EspStubImage
{
    /// <summary>Gets the entry point that should be executed after all segments have been loaded.</summary>
    public required uint EntryPoint { get; init; }

    /// <summary>Gets the ordered set of RAM segments that make up the stub image.</summary>
    public required IReadOnlyList<EspStubSegment> Segments { get; init; }
}
