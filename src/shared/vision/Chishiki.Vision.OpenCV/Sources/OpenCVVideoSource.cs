// -----------------------------------------------------------------------------
// File:        OpenCVVideoSource.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV-backed video source that captures frames from a camera index or file path.
// Created:     2025-01-01
// Modified:    2026-06-05
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Globalization;
using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Common.Sources;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Sources;

/// <summary>OpenCV-backed video source that captures frames from a physical camera or a video file.</summary>
// ReSharper disable once InconsistentNaming
public partial class OpenCVVideoSource : VideoSource
{
    /// <summary>Gets the underlying OpenCV <see cref="VideoCapture"/> instance.</summary>
    private VideoCapture VideoCapture { get; }

    /// <inheritdoc/>
    public override string VideoSourceId => VideoCapture.GetBackendName();

    /// <summary>Initialises a new <see cref="OpenCVVideoSource"/> wrapping the given <paramref name="videoCapture"/>.</summary>
    /// <param name="videoCapture">The OpenCV capture to wrap.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    protected OpenCVVideoSource(VideoCapture videoCapture, ILogger<OpenCVVideoSource> logger)
        : base(logger)
    {
        VideoCapture = videoCapture;
    }

    /// <summary>Opens the camera at the specified device <paramref name="videoIndex"/>.</summary>
    /// <param name="videoIndex">Zero-based index of the camera device (default: 0).</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public OpenCVVideoSource(int videoIndex, ILogger<OpenCVVideoSource> logger)
        : this(CreateVideoCapture(videoIndex, logger), logger)
    {
    }

    /// <summary>Opens a video file at the specified <paramref name="filePath"/>.</summary>
    /// <param name="filePath">Full path to the video file to open.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public OpenCVVideoSource(string filePath, ILogger<OpenCVVideoSource> logger)
        : this(CreateVideoCapture(filePath, logger), logger)
    {
    }

    /// <inheritdoc/>
    public override Task<IImage> GetImageAsync(CancellationToken cancellationToken = default)
    {
        var image = GetImage();
        return Task.FromResult<IImage>(image);
    }

    /// <summary>Captures the next frame from the video source and returns it as an <see cref="OpenCVImage"/>.</summary>
    /// <returns>The captured <see cref="OpenCVImage"/>, or an empty image when no frame is available.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    protected internal OpenCVImage GetImage()
    {
        var mat = new Mat();

        try
        {
            if (VideoCapture.Read(mat))
            {
                if (Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
                {
                    var videoSourceId = GetVideoSourceIdentifier();
                    LogFrameRead(Logger, videoSourceId);
                }

                return new OpenCVImage(mat);
            }

            if (Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                var videoSourceId = GetVideoSourceIdentifier();
                LogFrameUnavailable(Logger, videoSourceId);
            }

            mat.Dispose();
            return new OpenCVImage();
        }
        catch (Exception ex)
        {
            mat.Dispose();
            LogFrameReadFailed(Logger, GetVideoSourceIdentifier(), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        var videoSourceId = GetVideoSourceIdentifier();
        LogReleasingVideoCapture(Logger, videoSourceId);

        try
        {
            VideoCapture.Release();
            LogVideoCaptureReleased(Logger, videoSourceId);
        }
        catch (Exception ex)
        {
            LogVideoCaptureReleaseFailed(Logger, videoSourceId, ex);
            throw;
        }

        base.DisposeManaged();
    }

    #region Private helpers

    /// <summary>Creates and opens a video capture for the specified camera device index.</summary>
    /// <param name="videoIndex">Zero-based index of the camera device.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <returns>The created <see cref="VideoCapture"/> instance.</returns>
    private static VideoCapture CreateVideoCapture(int videoIndex, ILogger logger) =>
        CreateVideoCaptureCore(videoIndex.ToString(CultureInfo.InvariantCulture), "Camera", () => new VideoCapture(videoIndex), logger);

    /// <summary>Creates and opens a video capture for the specified file path.</summary>
    /// <param name="filePath">Full path of the video file.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <returns>The created <see cref="VideoCapture"/> instance.</returns>
    private static VideoCapture CreateVideoCapture(string filePath, ILogger logger) =>
        CreateVideoCaptureCore(filePath, "File", () => new VideoCapture(filePath), logger);

    /// <summary>Creates a video capture instance and emits the appropriate open diagnostics.</summary>
    /// <param name="source">Human-readable source descriptor, such as a camera index or file path.</param>
    /// <param name="sourceType">Type of source being opened.</param>
    /// <param name="factory">Factory that creates the capture instance.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <returns>The created <see cref="VideoCapture"/> instance.</returns>
    private static VideoCapture CreateVideoCaptureCore(string source, string sourceType, Func<VideoCapture> factory, ILogger logger)
    {
        try
        {
            var videoCapture = factory();

            if (videoCapture.IsOpened())
            {
                LogVideoCaptureOpened(logger, sourceType, source);
            }
            else
            {
                LogVideoCaptureOpenReturnedClosed(logger, sourceType, source);
            }

            return videoCapture;
        }
        catch (Exception ex)
        {
            LogVideoCaptureOpenFailed(logger, sourceType, source, ex);
            throw;
        }
    }

    /// <summary>Returns a stable identifier for diagnostics without throwing when the capture backend is unavailable.</summary>
    /// <returns>The backend name when available; otherwise <c>Unknown</c>.</returns>
    private string GetVideoSourceIdentifier()
    {
        try
        {
            var backendName = VideoCapture.GetBackendName();
            return string.IsNullOrWhiteSpace(backendName) ? "Unknown" : backendName;
        }
        catch
        {
            return "Unknown";
        }
    }

    #endregion

    #region Log messages

    /// <summary>Emits a debug log entry when a video capture source opens successfully.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="sourceType">Type of source being opened.</param>
    /// <param name="source">Human-readable source descriptor.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Opened OpenCV video capture for {SourceType} {Source}")]
    private static partial void LogVideoCaptureOpened(ILogger logger, string sourceType, string source);

    /// <summary>Emits a warning log entry when a video capture instance is created but not opened.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="sourceType">Type of source being opened.</param>
    /// <param name="source">Human-readable source descriptor.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Warning, Message = "OpenCV video capture for {SourceType} {Source} was created but is not open")]
    private static partial void LogVideoCaptureOpenReturnedClosed(ILogger logger, string sourceType, string source);

    /// <summary>Emits an error log entry when opening a video capture source fails with an exception.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="sourceType">Type of source being opened.</param>
    /// <param name="source">Human-readable source descriptor.</param>
    /// <param name="ex">The exception raised while opening the capture source.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Failed to open OpenCV video capture for {SourceType} {Source}")]
    private static partial void LogVideoCaptureOpenFailed(ILogger logger, string sourceType, string source, Exception ex);

    /// <summary>Emits a trace log entry when a frame is read successfully.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="videoSourceId">Identifier of the video source backend.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Trace, Message = "Read frame from OpenCV video source {VideoSourceId}")]
    private static partial void LogFrameRead(ILogger logger, string videoSourceId);

    /// <summary>Emits a debug log entry when no frame is available from the capture source.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="videoSourceId">Identifier of the video source backend.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "No frame available from OpenCV video source {VideoSourceId}")]
    private static partial void LogFrameUnavailable(ILogger logger, string videoSourceId);

    /// <summary>Emits an error log entry when reading a frame fails with an exception.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="videoSourceId">Identifier of the video source backend.</param>
    /// <param name="ex">The exception raised while reading the frame.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Failed to read a frame from OpenCV video source {VideoSourceId}")]
    private static partial void LogFrameReadFailed(ILogger logger, string videoSourceId, Exception ex);

    /// <summary>Emits a debug log entry before releasing the capture source.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="videoSourceId">Identifier of the video source backend.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Releasing OpenCV video source {VideoSourceId}")]
    private static partial void LogReleasingVideoCapture(ILogger logger, string videoSourceId);

    /// <summary>Emits a debug log entry after the capture source is released.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="videoSourceId">Identifier of the video source backend.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Released OpenCV video source {VideoSourceId}")]
    private static partial void LogVideoCaptureReleased(ILogger logger, string videoSourceId);

    /// <summary>Emits an error log entry when releasing the capture source fails.</summary>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="videoSourceId">Identifier of the video source backend.</param>
    /// <param name="ex">The exception raised while releasing the capture source.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Failed to release OpenCV video source {VideoSourceId}")]
    private static partial void LogVideoCaptureReleaseFailed(ILogger logger, string videoSourceId, Exception ex);

    #endregion
}

