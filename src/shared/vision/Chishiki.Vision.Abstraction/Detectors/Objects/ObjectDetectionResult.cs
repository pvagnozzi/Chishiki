// -----------------------------------------------------------------------------
// File:        ObjectDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Holds the result of an object detection pass on a video frame.
// Created:     2025-01-01
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Objects;

/// <summary>Holds the result of an object detection pass on a video frame.</summary>
public record ObjectDetectionResult : DetectionResult<ObjectDetection>
{
    /// <inheritdoc />
    public ObjectDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<ObjectDetection>? detections = null,
        DateTimeOffset? timestamp = null)
        : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }
}
