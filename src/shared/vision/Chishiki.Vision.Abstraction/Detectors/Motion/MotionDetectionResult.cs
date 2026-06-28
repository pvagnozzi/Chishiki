// -----------------------------------------------------------------------------
// File:        MotionDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Holds the result of a single motion detection pass on a video frame.
// Created:     2025-01-01
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Motion;

/// <summary>Holds the result of a single motion detection pass on a video frame.</summary>
public record MotionDetectionResult : DetectionResult<MotionDetection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MotionDetectionResult"/> record with all detection data.
    /// </summary>
    /// <param name="originalFrame">The original captured frame.</param>
    /// <param name="annotatedFrame">The frame with detected motion regions highlighted. Defaults to <paramref name="originalFrame"/> when <see langword="null"/>.</param>
    /// <param name="detections">The collection of detected motion regions.</param>
    /// <param name="timestamp">The UTC timestamp when the frame was captured.</param>
    public MotionDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<MotionDetection>? detections = null,
        DateTimeOffset? timestamp = null)
        : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary>Gets a value indicating whether any motion regions were detected in this frame.</summary>
    public bool HasMotion => Detections.Count > 0;
}
