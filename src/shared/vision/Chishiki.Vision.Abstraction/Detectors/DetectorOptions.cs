// -----------------------------------------------------------------------------
// File:        DetectionOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a detector options.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors;

/// <summary>
/// Detection options are used to configure the behavior of a detector, such as sensitivity, thresholds, or specific parameters relevant to the detection algorithm. They provide a way to customize how the detector processes video frames and identifies objects or motion.
/// </summary>
public abstract record DetectorOptions
{
    /// <summary>Gets or sets the BGR color of the rectangle drawn around each detected motion region. Default is green (0, 255, 0).</summary>
    public Color HighlightColour { get; set; } = new(0, 255, 0);

    /// <summary>Gets or sets the thickness in pixels of the rectangle drawn around motion regions. Default is 2.</summary>
    public int HighlightThickness { get; set; } = 2;
}
