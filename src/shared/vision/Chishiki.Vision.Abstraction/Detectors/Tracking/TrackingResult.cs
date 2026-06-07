// -----------------------------------------------------------------------------
// File:        TrackingResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the result of a tracking update on a single frame.
// Created:     2026-06-06
// Modified:    2026-06-06
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Tracking;

/// <summary>Represents the result of a tracking update on a single frame.</summary>
public record TrackingResult : DetectionResult<TrackingDetection>
{
    /// <summary>Initializes a new instance of the <see cref="TrackingResult"/> record.</summary>
    /// <param name="originalFrame">The original captured frame.</param>
    /// <param name="annotatedFrame">The annotated frame containing the tracked region highlight.</param>
    /// <param name="detections">The tracked regions for the frame.</param>
    /// <param name="timestamp">The UTC timestamp when the frame was captured.</param>
    public TrackingResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<TrackingDetection>? detections = null,
        DateTimeOffset? timestamp = null) : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary>Gets a value indicating whether the tracker currently has a tracked region for the frame.</summary>
    public bool HasTrack => Detections.Count > 0;
}
