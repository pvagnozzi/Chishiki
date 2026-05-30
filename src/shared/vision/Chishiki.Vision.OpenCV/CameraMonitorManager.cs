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
using Chishiki.Vision.Abstraction;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.OpenCV;

/// <summary>Manages a collection of <see cref="ICameraMonitor"/> instances, starting and stopping them concurrently via <see cref="Task.WhenAll"/> and aggregating their <see cref="ICameraMonitor.MotionDetected"/> events into a single surface event. All registration operations are thread-safe via a <see cref="ConcurrentDictionary{TKey, TValue}"/>.</summary>
public sealed partial class CameraMonitorManager : ICameraMonitorManager
{
    private readonly ConcurrentDictionary<string, ICameraMonitor> _monitors = new(StringComparer.Ordinal);
    private readonly ILogger<CameraMonitorManager> _logger;
    private bool _disposed;

    /// <summary>Initialises a new empty <see cref="CameraMonitorManager"/>.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    public CameraMonitorManager(ILogger<CameraMonitorManager> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> CameraIds => _monitors.Keys.ToArray();

    /// <inheritdoc/>
    public event EventHandler<MotionDetectedEventArgs>? MotionDetected;

    /// <inheritdoc/>
    public void Register(ICameraMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_monitors.TryAdd(monitor.CameraId, monitor))
            throw new InvalidOperationException($"A monitor with camera ID '{monitor.CameraId}' is already registered.");

        monitor.MotionDetected += OnCameraMotionDetected;
        LogCameraRegistered(monitor.CameraId);
    }

    /// <inheritdoc/>
    public async Task UnregisterAsync(string cameraId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cameraId);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_monitors.TryRemove(cameraId, out var monitor))
        {
            LogCameraNotFound(cameraId);
            return;
        }

        monitor.MotionDetected -= OnCameraMotionDetected;

        if (monitor.IsRunning)
            await monitor.StopAsync();

        await monitor.DisposeAsync();
        LogCameraUnregistered(cameraId);
    }

    /// <inheritdoc/>
    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

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
        ObjectDisposedException.ThrowIf(_disposed, this);

        var monitors = _monitors.Values.ToArray();
        LogStoppingAll(monitors.Length);

        var stopTasks = monitors
            .Where(m => m.IsRunning)
            .Select(StopSafeAsync)
            .ToArray();

        await Task.WhenAll(stopTasks);
        LogAllStopped(monitors.Length);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        var disposeAll = _monitors.Values
            .Select(async m =>
            {
                m.MotionDetected -= OnCameraMotionDetected;
                await m.DisposeAsync();
            });

        await Task.WhenAll(disposeAll);
        _monitors.Clear();
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
    private async Task StartSafeAsync(ICameraMonitor monitor, CancellationToken cancellationToken)
    {
        try
        {
            await monitor.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogStartError(monitor.CameraId, ex);
        }
    }

    /// <summary>Stops a monitor, swallowing and logging any exceptions to avoid aborting sibling stops.</summary>
    /// <param name="monitor">The monitor to stop.</param>
    private async Task StopSafeAsync(ICameraMonitor monitor)
    {
        try
        {
            await monitor.StopAsync();
        }
        catch (Exception ex)
        {
            LogStopError(monitor.CameraId, ex);
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
