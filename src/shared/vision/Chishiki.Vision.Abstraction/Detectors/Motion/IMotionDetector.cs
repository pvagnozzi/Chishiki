// -----------------------------------------------------------------------------
// File:        IMotionDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a stateful motion detector operating on sequential video frames.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Motion;

/// <summary>Defines a stateful motion detector that analyses sequential video frames, applies noise reduction, and highlights areas of detected motion.</summary>
public interface IMotionDetector : IDetector<MotionDetectionResult, MotionDetection>
{
    /// <summary> Gets the detector options </summary>
    new MotionDetectorOptions Options { get; }
}
