// -----------------------------------------------------------------------------
// File:        OpenCVVideoTrackerOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the minimal OpenCV template-matching video tracker.
// Created:     2026-06-06
// Modified:    2026-06-06
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors;

namespace Chishiki.Vision.OpenCV.Tracking;

/// <summary>Configuration options for <see cref="OpenCVVideoTracker"/>.</summary>
public record OpenCVVideoTrackerOptions : DetectorOptions
{
    /// <summary>Gets or sets the minimum normalized template-matching score required to keep the track. Default is 0.70.</summary>
    public double MinimumScore { get; set; } = 0.70;

    /// <summary>Gets or sets the number of pixels added around the previous tracked region when searching for the next match. Default is 32.</summary>
    public int SearchPadding { get; set; } = 32;
}
