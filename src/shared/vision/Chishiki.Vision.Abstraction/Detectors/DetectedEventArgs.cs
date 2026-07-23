// -----------------------------------------------------------------------------
// File:        DetectedEventArgs.cs
// Author:      Piergiorgio Vagnozzi
// Description: Event arguments raised when motion is detected on a monitored camera.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors;

/// <summary>Provides data for the motion-detected event raised by a camera monitor.</summary>
/// <param name="videoSourceId">Gets the identifier of the camera that detected motion.</param>
/// <param name="result">Gets the motion detection result containing frame data and region information.</param>
public class DetectedEventArgs<TResult, TDetection>(string videoSourceId, TResult result) : EventArgs
    where TResult : DetectionResult<TDetection>
    where TDetection : Detectors.Detection
{
    /// <summary>Gets the identifier of the camera that detected motion.</summary>
    public string VideoSourceId { get; } = videoSourceId;

    /// <summary>Gets the motion detection result containing frame data and region information.</summary>
    public DetectionResult<TDetection> Result { get; } = result;
}
