// -----------------------------------------------------------------------------
// File:        CardplateDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for cardplate detection algorithms.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Defines the common configuration used by cardplate detection algorithms.</summary>
public record CardplateDetectorOptions : DetectorOptions
{
    /// <summary>Gets or sets the minimum contour area in pixels required for a candidate region to be considered a cardplate. Default is 1200.</summary>
    public double MinContourArea { get; set; } = 1200.0;

    /// <summary>Gets or sets the minimum width-to-height ratio allowed for a candidate cardplate region. Default is 2.0.</summary>
    public double MinAspectRatio { get; set; } = 2.0;

    /// <summary>Gets or sets the maximum width-to-height ratio allowed for a candidate cardplate region. Default is 6.5.</summary>
    public double MaxAspectRatio { get; set; } = 6.5;

    /// <summary>Gets or sets the minimum rectangularity score required for a contour to be considered a plausible cardplate. Default is 0.45.</summary>
    public double MinRectangularity { get; set; } = 0.45;

    /// <summary>Gets or sets the relative padding factor applied around each accepted candidate before recognition. Default is 0.08.</summary>
    public double CandidatePaddingFactor { get; set; } = 0.08;

    /// <summary>Gets or sets the maximum number of candidates to recognize per frame after geometric filtering. Default is 5.</summary>
    public int MaxCandidates { get; set; } = 5;

    /// <summary>Gets or sets a value indicating whether detections without recognized text should be discarded. Default is false.</summary>
    public bool RequireRecognition { get; set; }
}
