// -----------------------------------------------------------------------------
// File:        IDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Detector base class.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors;

/// <summary>
/// Defines a detector that can analyze video frames and detect specific features or objects based on a background model.
/// </summary>
/// <typeparam name="TDetection">The type of the detection.</typeparam>
public interface IDetector<TDetection> : IDisposable
    where TDetection : Detection
{
    /// <summary>Analyses the supplied <paramref name="frame"/> against the accumulated background model and returns a <see cref="TResult"/> with annotated image and detected regions.</summary>
    /// <param name="frame">The current video frame to analyze.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="DetectionResult{TDetection}"/> describing motion in the frame.</returns>
    Task<DetectionResult<TDetection>> DetectAsync(IImage frame, CancellationToken cancellationToken = default);

    /// <summary>Resets the internal model, discarding accumulated history.</summary>
    void Reset();
}
