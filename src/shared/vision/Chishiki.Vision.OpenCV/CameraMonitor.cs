// -----------------------------------------------------------------------------
// File:        CameraMonitor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Single-camera monitor that captures frames at a fixed rate and runs motion detection via a producer/consumer channel.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Threading.Channels;
using Chishiki.Vision.Abstraction;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.OpenCV;

/// <summary>Monitors a single <see cref="IVideoSource"/>, capturing frames at a configurable rate and passing them through an <see cref="IMotionDetector"/> via an internal bounded <see cref="Channel{T}"/> (producer/consumer pattern). The capture loop runs on its own <see cref="Task"/> and uses <see cref="PeriodicTimer"/> for accurate FPS throttling without thread blocking.</summary>
public sealed partial class CameraMonitor : ICameraMonitor
{
    private readonly IVideoSource _source;
    private readonly IMotionDetector _detector;
    private readonly CameraMonitorOptions _options;
    private readonly ILogger<CameraMonitor> _logger;
    private readonly Channel<IImage> _frameChannel;

    private CancellationTokenSource? _cts;
    private Task? _captureTask;
    private Task? _detectTask;
    private bool _disposed;

    /// <inheritdoc/>
    public string CameraId { get; }

    /// <inheritdoc/>
    public bool IsRunning => _captureTask is { IsCompleted: false };

    /// <inheritdoc/>
    public event EventHandler<MotionDetectedEventArgs>? MotionDetected;

    /// <summary>Initialises a new <see cref="CameraMonitor"/> for the given camera. .</summary>
    /// <param name="cameraId">Unique identifier for this camera.</param>
    /// <param name="source">The video source to capture from.</param>
    /// <param name="detector">The motion detector to run on each frame.</param>
    /// <param name="options">Configuration options for this monitor.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public CameraMonitor(
        string cameraId,
        IVideoSource source,
        IMotionDetector detector,
        CameraMonitorOptions options,
        ILogger<CameraMonitor> logger)
    {
        CameraId = cameraId;
        _source = source;
        _detector = detector;
        _options = options;
        _logger = logger;

        _frameChannel = Channel.CreateBounded<IImage>(new BoundedChannelOptions(options.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsRunning)
        {
            LogAlreadyRunning(CameraId);
            return Task.CompletedTask;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        _captureTask = CaptureLoopAsync(token);
        _detectTask = DetectLoopAsync(token);

        LogMonitorStarted(CameraId);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        if (_cts is null)
            return;

        await _cts.CancelAsync();
        _ = _frameChannel.Writer.TryComplete();

        try
        {
            if (_captureTask is not null) await _captureTask;
            if (_detectTask is not null) await _detectTask;
        }
        catch (OperationCanceledException) { /* expected on graceful stop */ }

        LogMonitorStopped(CameraId);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        await StopAsync();
        _cts?.Dispose();
        _source.Dispose();
        _detector.Dispose();
    }

    // -------------------------------------------------------------------------
    // Private loops
    // -------------------------------------------------------------------------

    /// <summary>Producer: captures frames at the configured FPS and writes them to the channel.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task CaptureLoopAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(1.0 / _options.FramesPerSecond);
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var frame = _source.GetFrame();

                if (frame.IsEmpty())
                {
                    frame.Dispose();
                    LogEmptyFrameSkipped(CameraId);
                    continue;
                }

                // DropOldest policy ensures the channel never blocks the producer.
                if (!_frameChannel.Writer.TryWrite(frame))
                    frame.Dispose();
            }
        }
        catch (OperationCanceledException) { /* graceful exit */ }
        finally
        {
            _ = _frameChannel.Writer.TryComplete();
        }
    }

    /// <summary>Consumer: reads frames from the channel and runs motion detection.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task DetectLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var frame in _frameChannel.Reader.ReadAllAsync(cancellationToken))
            {
                using (frame)
                {
                    try
                    {
                        var result = await _detector.DetectAsync(frame, cancellationToken);

                        if (!_options.RaiseOnlyOnMotion || result.HasMotion)
                            RaiseMotionDetected(result);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        LogDetectionError(CameraId, ex);
                    }
                }
            }
        }
        catch (OperationCanceledException) { /* graceful exit */ }
    }

    /// <summary>Raises the <see cref="MotionDetected"/> event on the thread pool.</summary>
    /// <param name="result">The motion detection result to publish.</param>
    private void RaiseMotionDetected(MotionDetectionResult result) =>
        MotionDetected?.Invoke(this, new MotionDetectedEventArgs(CameraId, result));

    // -------------------------------------------------------------------------
    // Compile-time logging
    // -------------------------------------------------------------------------

    /// <summary>Emitted when the monitor is started.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Camera monitor '{CameraId}' started.")]
    private partial void LogMonitorStarted(string cameraId);

    /// <summary>Emitted when the monitor is stopped.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Camera monitor '{CameraId}' stopped.")]
    private partial void LogMonitorStopped(string cameraId);

    /// <summary>Emitted when the monitor is already running.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Camera monitor '{CameraId}' is already running.")]
    private partial void LogAlreadyRunning(string cameraId);

    /// <summary>Emitted when an empty frame is received from the video source.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Camera '{CameraId}': empty frame skipped.")]
    private partial void LogEmptyFrameSkipped(string cameraId);

    /// <summary>Emitted when motion detection raises an unexpected error.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Camera '{CameraId}': error during motion detection.")]
    private partial void LogDetectionError(string cameraId, Exception ex);
}
