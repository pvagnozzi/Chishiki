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

/// <summary>Defines a stateful detector that analyses sequential video frames.</summary>
public interface IDetector<TResult, TDetection> : IDisposable
    where TResult : DetectionResult<TDetection>
    where TDetection : Detection
{
    /// <summary> Gets the detector options </summary>
    DetectorOptions Options { get; }

    /// <summary>Analyses the supplied <paramref name="frame"/> against the accumulated background model and returns a <see cref="TResult"/> with annotated image and detected regions.</summary>
    /// <param name="frame">The current video frame to analyze.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="TResult"/> describing motion in the frame.</returns>
    Task<TResult> DetectAsync(IImage frame, CancellationToken cancellationToken = default);

    /// <summary>Resets the internal model, discarding accumulated history.</summary>
    void Reset();
}
