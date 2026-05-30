// -----------------------------------------------------------------------------
// File:        MotionDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Holds the result of a single motion detection pass on a video frame.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Holds the result of a single motion detection analysis on a video frame.</summary>
/// <param name="OriginalFrame">Gets the original captured frame.</param>
/// <param name="AnnotatedFrame">Gets the frame with motion regions drawn as highlighted rectangles.</param>
/// <param name="Regions">Gets the collection of detected motion regions.</param>
/// <param name="Timestamp">Gets the UTC timestamp when the frame was captured.</param>
/// <param name="HasMotion">Gets a value indicating whether any motion was detected in this frame.</param>
public record MotionDetectionResult(
    IImage OriginalFrame,
    IImage AnnotatedFrame,
    IReadOnlyList<MotionRegion> Regions,
    DateTimeOffset Timestamp,
    bool HasMotion);
