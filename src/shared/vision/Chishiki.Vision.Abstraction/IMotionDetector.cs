// -----------------------------------------------------------------------------
// File:        IMotionDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a stateful motion detector operating on sequential video frames.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Defines a stateful motion detector that analyses sequential video frames, applies noise reduction, and highlights areas of detected motion.</summary>
public interface IMotionDetector : IDisposable
{
    /// <summary>Analyses the supplied <paramref name="frame"/> against the accumulated background model and returns a <see cref="MotionDetectionResult"/> with annotated image and detected regions. .</summary>
    /// <param name="frame">The current video frame to analyse.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="MotionDetectionResult"/> describing motion in the frame.</returns>
    Task<MotionDetectionResult> DetectAsync(IImage frame, CancellationToken cancellationToken = default);

    /// <summary>Resets the internal background model, discarding accumulated history.</summary>
    void Reset();
}
