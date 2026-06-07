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
using Chishiki.Vision.Abstraction.Detectors.Motion;
using Chishiki.Vision.Abstraction.Monitors;
using Chishiki.Vision.Abstraction.Sources;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Monitors;

/// <summary>Monitors a single <see cref="IVideoSource"/>, capturing frames at a configurable rate and passing them through an <see cref="IMotionDetector"/> via an internal bounded <see cref="Channel{T}"/> (producer/consumer pattern). The capture loop runs on its own <see cref="Task"/> and uses <see cref="PeriodicTimer"/> for accurate FPS throttling without thread blocking.</summary>
public abstract partial class VideoSourceMonitor : AsyncDisposable, IVideoSourceMonitor
{
    /// <summary>
    /// Frame channel.
    /// </summary>
    private readonly Channel<IImage> _frameChannel;

    /// <summary>
    /// Cancellation token source.
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Capture and detection tasks run concurrently: the capture task produces frames at the configured FPS and writes them to the channel, while the detection task consumes frames from the channel and runs motion detection. Both tasks observe the same cancellation token for coordinated shutdown. The channel's DropOldest policy ensures that if the producer outpaces the consumer, the oldest frame will be discarded to maintain real-time performance without blocking the capture loop.
    /// </summary>
    private Task? _captureTask;

    /// <summary>
    /// Detection task consumes frames from the channel and runs motion detection. Observes the same cancellation token for coordinated shutdown.
    /// </summary>
    private Task? _detectTask;

    /// <inheritdoc/>
    public IVideoSource VideoSource { get; }

    /// <inheritdoc/>
    public IMotionDetector MotionDetector { get; }

    /// <inheritdoc/>
    public VideoSourceMonitorOptions Options { get; }

    /// <inheritdoc/>
    public bool IsRunning => _captureTask is { IsCompleted: false };

    /// <summary> Gets the video source id. </summary>
    public string VideoSourceId => VideoSource.VideoSourceId;

    /// <inheritdoc/>
    public event EventHandler<MotionDetectedEventArgs>? MotionDetected;

    /// <summary>Initializes a new <see cref="VideoSourceMonitor"/> for the given camera. .</summary>
    /// <param name="source">The video source to capture from.</param>
    /// <param name="detector">The motion detector to run on each frame.</param>
   
    /// <param name="options">Configuration options for this monitor.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    protected VideoSourceMonitor(
        IVideoSource source,
        IMotionDetector detector,
        VideoSourceMonitorOptions options,
        ILogger<VideoSourceMonitor> logger)
        : base(logger)
    {
        VideoSource = source;
        MotionDetector = detector;
        Options = options;

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
        CheckDisposed();
        if (IsRunning)
        {
            LogAlreadyRunning(Logger, VideoSourceId);
            return Task.CompletedTask;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        _captureTask = CaptureLoopAsync(token);
        _detectTask = DetectLoopAsync(token);

        LogMonitorStarted(Logger, VideoSourceId);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        if (_cts is null)
        {
            return;
        }

        await _cts.CancelAsync();
        _ = _frameChannel.Writer.TryComplete();

        try
        {
            if (_captureTask is not null)
            {
                await _captureTask;
            }

            if (_detectTask is not null)
            {
                await _detectTask;
            }
        }
        catch (OperationCanceledException) { /* expected on graceful stop */ }

        LogMonitorStopped(Logger, VideoSourceId);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeManagedAsync(CancellationToken cancellationToken = default)
    {
        await StopAsync();
        _cts?.Dispose();
        VideoSource.Dispose();
        MotionDetector.Dispose();
        await base.DisposeManagedAsync(cancellationToken);
    }

    #region Private loops    
    /// <summary>Producer: captures frames at the configured FPS and writes them to the channel.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task CaptureLoopAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(1.0 / Options.FramesPerSecond);
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var frame = await VideoSource.GetImageAsync(cancellationToken);

                if (frame.IsEmpty())
                {
                    frame.Dispose();
                    LogEmptyFrameSkipped(Logger, VideoSourceId);
                    continue;
                }

                // DropOldest policy ensures the channel never blocks the producer.
                if (!_frameChannel.Writer.TryWrite(frame))
                {
                    frame.Dispose();
                }
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
                        var result = await MotionDetector.DetectAsync(frame, cancellationToken);

                        if (!Options.RaiseOnlyOnMotion || result.HasMotion)
                        {
                            RaiseMotionDetected(result);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        LogDetectionError(Logger, VideoSourceId, ex);
                    }
                }
            }
        }
        catch (OperationCanceledException) { /* graceful exit */ }
    }

    /// <summary>Raises the <see cref="MotionDetected"/> event on the thread pool.</summary>
    /// <param name="result">The motion detection result to publish.</param>
    private void RaiseMotionDetected(MotionDetectionResult result) =>
        MotionDetected?.Invoke(this, new MotionDetectedEventArgs(VideoSourceId, result));
    #endregion

    #region Compile-time logging    
    /// <summary>Emitted when the monitor is started.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Camera monitor '{CameraId}' started.")]
    private static partial void LogMonitorStarted(ILogger logger, string cameraId);

    /// <summary>Emitted when the monitor is stopped.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Camera monitor '{CameraId}' stopped.")]
    private static partial void LogMonitorStopped(ILogger logger, string cameraId);

    /// <summary>Emitted when the monitor is already running.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Camera monitor '{CameraId}' is already running.")]
    private static partial void LogAlreadyRunning(ILogger logger, string cameraId);

    /// <summary>Emitted when an empty frame is received from the video source.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Camera '{CameraId}': empty frame skipped.")]
    private static partial void LogEmptyFrameSkipped(ILogger logger, string cameraId);

    /// <summary>Emitted when motion detection raises an unexpected error.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Camera '{CameraId}': error during motion detection.")]
    private static partial void LogDetectionError(ILogger logger, string cameraId, Exception ex);
    #endregion
}
