// -----------------------------------------------------------------------------
// File:        CardplateDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the result of a cardplate detection analysis on a single frame.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Represents the result of a cardplate detection analysis on a single frame.</summary>
public record CardplateDetectionResult : DetectionResult<CardplateDetection>
{
    /// <summary>Initializes a new instance of the <see cref="CardplateDetectionResult"/> record with the supplied frame data and detections.</summary>
    /// <param name="originalFrame">The original input frame.</param>
    /// <param name="annotatedFrame">The annotated output frame.</param>
    /// <param name="detections">The collection of detected cardplates.</param>
    /// <param name="timestamp">The capture timestamp associated with the frame.</param>
    public CardplateDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<CardplateDetection>? detections = null,
        DateTimeOffset? timestamp = null)
        : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary>Gets a value indicating whether the frame contains at least one cardplate detection.</summary>
    public bool HasCardplates => Detections.Count > 0;

    /// <summary>Gets a value indicating whether the frame contains at least one recognized cardplate.</summary>
    public bool HasRecognizedCardplates => Detections.Any(static detection => detection.HasRecognition);
}
