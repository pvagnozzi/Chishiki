// -----------------------------------------------------------------------------
// File:        MotionDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the OpenCV MOG2-based motion detector.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Motion;

/// <summary>Configuration options for the <see cref="IMotionDetector"/> algorithm.</summary>
// ReSharper disable once InconsistentNaming
public record MotionDetectorOptions : DetectorOptions
{
    /// <summary>Gets or sets the minimum contour area in pixels to be considered a valid motion region. Default is 500.</summary>
    public new double MinContourArea { get; set; } = 500.0;

    /// <summary>Gets or sets the threshold on the squared Mahalanobis distance to classify a pixel as foreground. Default is 16.</summary>
    public double Threshold { get; set; } = 16.0;

    /// <summary>Gets or sets a value indicating whether motion detector should detect shadows. Default is false.</summary>
    public bool DetectShadows { get; set; }
}
