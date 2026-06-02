// -----------------------------------------------------------------------------
// File:        IVideoSourceMonitor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a single-camera monitor that continuously captures and analyses frames.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors.Motion;
using Chishiki.Vision.Abstraction.Sources;

namespace Chishiki.Vision.Abstraction.Monitors;

/// <summary>Defines a monitor for a single video source that continuously captures frames, runs motion detection, and raises events when motion is found.</summary>
public interface IVideoSourceMonitor : IAsyncDisposable
{
    /// <summary>
    /// Gets the video source.
    /// </summary>
    IVideoSource VideoSource { get; }

    /// <summary>
    /// Gets the motion detector.
    /// </summary>
    IMotionDetector Detector { get; }

    /// <summary>
    /// Gets the configuration options for this camera monitor, including parameters such as capture FPS, channel capacity, and motion detection settings. These options control the behaviour of the capture-and-detect loop and can be used to fine-tune performance and sensitivity.
    /// </summary>
    VideoSourceMonitorOptions Options { get; }

    /// <summary>Gets a value indicating whether the monitor is currently running.</summary>
    bool IsRunning { get; }

    /// <summary>Raised whenever a motion detection result is produced (with or without motion).</summary>
    event EventHandler<MotionDetectedEventArgs>? MotionDetected;

    /// <summary>Starts the capture-and-detect loop, producing results until <paramref name="cancellationToken"/> is cancelled or <see cref="StopAsync"/> is called.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Gracefully stops the capture-and-detect loop and releases camera resources.</summary>
    Task StopAsync();
}
