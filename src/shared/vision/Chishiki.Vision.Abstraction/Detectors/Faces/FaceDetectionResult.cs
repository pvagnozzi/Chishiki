// -----------------------------------------------------------------------------
// File:        FaceDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the result of a face detection analysis on a single frame.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Represents the result of a face detection analysis on a single frame.</summary>
public record FaceDetectionResult : DetectionResult<FaceDetection>
{
    /// <summary>Initializes a new instance of the <see cref="FaceDetectionResult"/> record with the supplied frame data and detections.</summary>
    /// <param name="originalFrame">The original input frame.</param>
    /// <param name="annotatedFrame">The annotated output frame.</param>
    /// <param name="detections">The collection of detected faces.</param>
    /// <param name="timestamp">The capture timestamp associated with the frame.</param>
    public FaceDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<FaceDetection>? detections = null,
        DateTimeOffset? timestamp = null)
        : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary>Gets a value indicating whether the frame contains at least one face detection.</summary>
    public bool HasFaces => Detections.Count > 0;

    /// <summary>Gets a value indicating whether the frame contains at least one recognized face.</summary>
    public bool HasRecognizedFaces => Detections.Any(static detection => detection.HasRecognition);
}
