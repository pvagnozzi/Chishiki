// -----------------------------------------------------------------------------
// File:        CameraMonitorManager.cs
// Author:      Piergiorgio Vagnozzi
// Description: Orchestrates multiple CameraMonitor instances in parallel, aggregating their motion events.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Collections.Concurrent;
using Chishiki.Vision.Abstraction.Detectors.Motion;
using Chishiki.Vision.Abstraction.Monitors;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Monitors;

/// <summary>Manages a collection of <see cref="IVideoSourceMonitor"/> instances, starting and stopping them concurrently via <see>
///         <cref>Task.WhenAll</cref>
///     </see>
///     and aggregating their <see cref="IVideoSourceMonitor.MotionDetected"/> events into a single surface event. All registration operations are thread-safe via a <see cref="ConcurrentDictionary{TKey, TValue}"/>.</summary>
/// <remarks>Initializes a new empty <see cref="VideoSourceMonitorManager"/>.</remarks>
/// <param name="logger">Logger used for diagnostics.</param>
public partial class VideoSourceMonitorManager(ILogger<VideoSourceMonitorManager> logger) : AsyncDisposable(logger), IVideoSourceMonitorManager
{
    /// <summary>
    /// Monitors are stored in a thread-safe concurrent dictionary keyed by their unique camera ID. This allows for safe registration and unregistration of monitors from multiple threads without risking
    /// </summary>
    private readonly ConcurrentDictionary<string, IVideoSourceMonitor> _monitors = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public IReadOnlyCollection<string> CameraIds => [.. _monitors.Keys];

    /// <inheritdoc/>
    public IReadOnlyCollection<IVideoSourceMonitor> Monitors => [.. _monitors.Values];

    /// <inheritdoc/>
    public event EventHandler<MotionDetectedEventArgs>? MotionDetected;

    /// <inheritdoc/>
    public virtual void Register(IVideoSourceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        CheckDisposed();

        if (!_monitors.TryAdd(monitor.VideoSource.VideoSourceId, monitor))
            throw new InvalidOperationException($"A monitor with video source ID '{monitor.VideoSource.VideoSourceId}' is already registered.");

        monitor.MotionDetected += OnCameraMotionDetected;
        LogCameraRegistered(monitor.VideoSource.VideoSourceId);
    }

    /// <inheritdoc/>
    public async Task UnregisterAsync(string cameraId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cameraId);
        CheckDisposed();

        if (!_monitors.TryRemove(cameraId, out var monitor))
        {
            LogCameraNotFound(cameraId);
            return;
        }

        monitor.MotionDetected -= OnCameraMotionDetected;

        if (monitor.IsRunning)
        {
            await monitor.StopAsync();
        }

        await monitor.DisposeAsync();
        LogCameraUnregistered(cameraId);
    }

    /// <inheritdoc/>
    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();

        var monitors = _monitors.Values.ToArray();
        LogStartingAll(monitors.Length);

        var startTasks = monitors
            .Select(m => StartSafeAsync(m, cancellationToken))
            .ToArray();

        await Task.WhenAll(startTasks);
        LogAllStarted(monitors.Length);
    }

    /// <inheritdoc/>
    public async Task StopAllAsync()
    {
        CheckDisposed();

        var monitors = _monitors.Values.ToArray();
        LogStoppingAll(monitors.Length);

        var stopTasks = monitors
            .Where(m => m.IsRunning)
            .Select(StopSafeAsync)
            .ToArray();

        await Task.WhenAll(stopTasks);
        LogAllStopped(monitors.Length);
    }

    /// <summary>
    /// Disposes all registered monitors and clears the collection. This method is called by the base <see cref="AsyncDisposable"/> implementation when the manager itself is being disposed. It ensures that all monitors are properly cleaned up, even if they are still running, and that any exceptions during disposal are logged without preventing the cleanup of sibling monitors.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous disposal of managed resources.</returns>
    protected override async ValueTask DisposeManagedAsync(CancellationToken cancellationToken = default)
    {
        var disposeAll = _monitors.Values
            .Select(async m =>
            {
                m.MotionDetected -= OnCameraMotionDetected;
                await m.DisposeAsync();
            });

        await Task.WhenAll(disposeAll);
        _monitors.Clear();
        await base.DisposeManagedAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Forwards motion events from individual monitors to the aggregated surface event.</summary>
    private void OnCameraMotionDetected(object? sender, MotionDetectedEventArgs e) =>
        MotionDetected?.Invoke(this, e);

    /// <summary>Starts a monitor, swallowing and logging any exceptions to avoid aborting sibling starts.</summary>
    /// <param name="monitor">The monitor to start.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task StartSafeAsync(IVideoSourceMonitor monitor, CancellationToken cancellationToken)
    {
        try
        {
            await monitor.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogStartError(monitor.VideoSource.VideoSourceId, ex);
        }
    }

    /// <summary>Stops a monitor, swallowing and logging any exceptions to avoid aborting sibling stops.</summary>
    /// <param name="monitor">The monitor to stop.</param>
    private async Task StopSafeAsync(IVideoSourceMonitor monitor)
    {
        try
        {
            await monitor.StopAsync();
        }
        catch (Exception ex)
        {
            LogStopError(monitor.VideoSource.VideoSourceId, ex);
        }
    }

    // -------------------------------------------------------------------------
    // Compile-time logging
    // -------------------------------------------------------------------------

    /// <summary>Emitted when a camera monitor is registered.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Camera '{CameraId}' registered.")]
    private partial void LogCameraRegistered(string cameraId);

    /// <summary>Emitted when a camera monitor is unregistered and disposed.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Camera '{CameraId}' unregistered and disposed.")]
    private partial void LogCameraUnregistered(string cameraId);

    /// <summary>Emitted when an unregistered camera ID is referenced during unregister.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Camera '{CameraId}' not found during unregister.")]
    private partial void LogCameraNotFound(string cameraId);

    /// <summary>Emitted before starting all monitors.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting {Count} camera monitor(s).")]
    private partial void LogStartingAll(int count);

    /// <summary>Emitted after all monitors have been started.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "{Count} camera monitor(s) started.")]
    private partial void LogAllStarted(int count);

    /// <summary>Emitted before stopping all monitors.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Stopping {Count} camera monitor(s).")]
    private partial void LogStoppingAll(int count);

    /// <summary>Emitted after all monitors have been stopped.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "{Count} camera monitor(s) stopped.")]
    private partial void LogAllStopped(int count);

    /// <summary>Emitted when a monitor fails to start.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Camera '{CameraId}' failed to start.")]
    private partial void LogStartError(string cameraId, Exception ex);

    /// <summary>Emitted when a monitor fails to stop.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Camera '{CameraId}' failed to stop.")]
    private partial void LogStopError(string cameraId, Exception ex);
}
