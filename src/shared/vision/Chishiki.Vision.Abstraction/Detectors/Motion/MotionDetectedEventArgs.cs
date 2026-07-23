// -----------------------------------------------------------------------------
// File:        MotionDetectedEventArgs.cs
// Author:      Piergiorgio Vagnozzi
// Description: Motion detection result.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Motion;

/// <summary>
/// Motion detection event arguments raised when motion is detected on a monitored camera. Contains the identifier of the video source that produced the detection result and the motion detection result itself, which includes frame data and information about detected motion regions.
/// </summary>
/// <param name="videoSourceId">Video source id</param>
/// <param name="result">Motion detection result</param>
public class MotionDetectedEventArgs(string videoSourceId, MotionDetectionResult result)
    : DetectedEventArgs<MotionDetectionResult, MotionDetection>(videoSourceId, result);
