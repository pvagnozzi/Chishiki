// -----------------------------------------------------------------------------
// File:        FaceDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Holds the result of a face detection pass on a video frame.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Holds the result of a face detection pass on a video frame.</summary>
public record FaceDetectionResult : DetectionResult<FaceDetection>
{
    /// <inheritdoc />
    public FaceDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<FaceDetection>? detections = null,
        DateTimeOffset? timestamp = null)
        : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }
}
