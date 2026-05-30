// -----------------------------------------------------------------------------
// File:        MotionDetectedEventArgs.cs
// Author:      Piergiorgio Vagnozzi
// Description: Event arguments raised when motion is detected on a monitored camera.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Provides data for the motion-detected event raised by a camera monitor.</summary>
/// <param name="CameraId">Gets the identifier of the camera that detected motion.</param>
/// <param name="Result">Gets the motion detection result containing frame data and region information.</param>
public sealed class MotionDetectedEventArgs(string CameraId, MotionDetectionResult Result) : EventArgs
{
    /// <summary>Gets the identifier of the camera that detected motion.</summary>
    public string CameraId { get; } = CameraId;

    /// <summary>Gets the motion detection result containing frame data and region information.</summary>
    public MotionDetectionResult Result { get; } = Result;
}
