// -----------------------------------------------------------------------------
// File:        CameraMonitorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the CameraMonitor capture-and-detect loop.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.OpenCV;

/// <summary>Configuration options for the <see cref="CameraMonitor"/> capture-and-detect loop.</summary>
public sealed class CameraMonitorOptions
{
    /// <summary>Gets or sets the target capture rate in frames per second. Default is 10.</summary>
    public double FramesPerSecond { get; set; } = 10.0;

    /// <summary>Gets or sets the maximum number of unprocessed results buffered in the internal channel. Default is 32.</summary>
    public int ChannelCapacity { get; set; } = 32;

    /// <summary>Gets or sets a value indicating whether only frames with detected motion are raised via the event. Default is true.</summary>
    public bool RaiseOnlyOnMotion { get; set; } = true;

    /// <summary>Gets or sets the motion detector options applied by this monitor.</summary>
    public MotionDetectorOptions DetectorOptions { get; set; } = new();
}
