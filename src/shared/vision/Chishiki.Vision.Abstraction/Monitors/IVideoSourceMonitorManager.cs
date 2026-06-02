// -----------------------------------------------------------------------------
// File:        IVideoSourceMonitorManager.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for managing and orchestrating multiple camera monitors in parallel.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors.Motion;

namespace Chishiki.Vision.Abstraction.Monitors;

/// <summary>Manages a collection of <see cref="IVideoSourceMonitor"/> instances, starting and stopping them in parallel and aggregating their motion-detected events.</summary>
public interface IVideoSourceMonitorManager : IAsyncDisposable
{
    /// <summary>Gets the identifiers of all registered cameras.</summary>
    IReadOnlyCollection<string> CameraIds { get; }

    /// <summary>Gets the list of all registered cameras.</summary>
    IReadOnlyCollection<IVideoSourceMonitor> Monitors { get; }

    /// <summary>Raised when any managed camera monitor detects motion.</summary>
    event EventHandler<MotionDetectedEventArgs> MotionDetected;

    /// <summary>Registers a camera monitor with the manager.</summary>
    /// <param name="monitor">The camera monitor to register.</param>
    /// <exception cref="InvalidOperationException">Thrown when a monitor with the same <see cref="IVideoSourceMonitor.CameraId"/> is already registered.</exception>
    void Register(IVideoSourceMonitor monitor);

    /// <summary>Removes and disposes the camera monitor identified by <paramref name="cameraId"/>.</summary>
    /// <param name="cameraId">The identifier of the camera to unregister.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task UnregisterAsync(string cameraId, CancellationToken cancellationToken = default);

    /// <summary>Starts all registered camera monitors concurrently.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task StartAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops all running camera monitors concurrently.</summary>
    Task StopAllAsync();
}
