// -----------------------------------------------------------------------------
// File:        ICameraMonitor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a single-camera monitor that continuously captures and analyses frames.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Defines a monitor for a single video source that continuously captures frames, runs motion detection, and raises events when motion is found.</summary>
public interface ICameraMonitor : IAsyncDisposable
{
    /// <summary>Gets the unique identifier of this camera monitor.</summary>
    string CameraId { get; }

    /// <summary>Gets a value indicating whether the monitor is currently running.</summary>
    bool IsRunning { get; }

    /// <summary>Raised whenever a motion detection result is produced (with or without motion).</summary>
    event EventHandler<MotionDetectedEventArgs>? MotionDetected;

    /// <summary>Starts the capture-and-detect loop, producing results until <paramref name="cancellationToken"/> is cancelled or <see cref="StopAsync"/> is called. .</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Gracefully stops the capture-and-detect loop and releases camera resources.</summary>
    Task StopAsync();
}
