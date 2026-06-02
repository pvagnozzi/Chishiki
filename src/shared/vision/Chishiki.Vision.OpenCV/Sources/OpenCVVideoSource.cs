// -----------------------------------------------------------------------------
// File:        OpenCVVideoSource.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV-backed video source that captures frames from a camera index or file path.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Common.Sources;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Sources;

/// <summary>OpenCV-backed video source that captures frames from a physical camera or a video file.</summary>
// ReSharper disable once InconsistentNaming
public class OpenCVVideoSource : VideoSource
{
    /// <summary>Gets the underlying OpenCV <see cref="VideoCapture"/> instance.</summary>
    private VideoCapture VideoCapture { get; }

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
    public OpenCVVideoSource(int videoIndex, ILogger<OpenCVVideoSource> logger) : this(new VideoCapture(videoIndex), logger)
    {
    }

    /// <summary>Opens a video file at the specified <paramref name="filePath"/>.</summary>
    /// <param name="filePath">Full path to the video file to open.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public OpenCVVideoSource(string filePath, ILogger<OpenCVVideoSource> logger) : this(new VideoCapture(filePath), logger)
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
        return VideoCapture.Read(mat) ? new OpenCVImage(mat) : new OpenCVImage();
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        VideoCapture.Release();
        base.DisposeManaged();
    }
}

