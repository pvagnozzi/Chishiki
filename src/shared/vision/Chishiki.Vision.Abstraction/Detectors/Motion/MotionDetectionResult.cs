// -----------------------------------------------------------------------------
// File:        MotionDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the result of a motion detection analysis on a single frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Motion;

/// <summary>Represents the result of a motion detection analysis on a single frame.</summary>
// ReSharper disable once ClassNeverInstantiated.Global
public record MotionDetectionResult : DetectionResult<MotionDetection>
{
    public MotionDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<MotionDetection>? detections = null,
        DateTimeOffset? timestamp = null) : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary> Gets a value indicating whether if has motion detection. </summary>
    public bool HasMotion => Detections.Count > 0;
}
