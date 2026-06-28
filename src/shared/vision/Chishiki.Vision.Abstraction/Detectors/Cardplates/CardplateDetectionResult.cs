// -----------------------------------------------------------------------------
// File:        CardplateDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Holds the result of a cardplate detection pass on a video frame.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Holds the result of a cardplate detection pass on a video frame.</summary>
public record CardplateDetectionResult : DetectionResult<CardplateDetection>
{
    /// <inheritdoc />
    public CardplateDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<CardplateDetection>? detections = null,
        DateTimeOffset? timestamp = null)
        : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary>Gets a value indicating whether any cardplates were detected in the frame.</summary>
    public bool HasCardplates => Detections.Count > 0;

    /// <summary>Gets a value indicating whether at least one detected cardplate was successfully recognized.</summary>
    public bool HasRecognizedCardplates => Detections.Any(static d => d.HasRecognition);
}
