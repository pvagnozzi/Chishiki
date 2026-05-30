// -----------------------------------------------------------------------------
// File:        VideoSource.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV-backed video source that captures frames from a camera index or file path.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV;

/// <summary>OpenCV-backed video source that captures frames from a physical camera or a video file.</summary>
public class VideoSource : Disposable, IVideoSource
{
    /// <summary>Gets the underlying OpenCV <see cref="VideoCapture"/> instance.</summary>
    protected internal VideoCapture VideoCapture { get; }

    /// <summary>Initialises a new <see cref="VideoSource"/> wrapping the given <paramref name="videoCapture"/>.</summary>
    /// <param name="videoCapture">The OpenCV capture to wrap.</param>
    protected VideoSource(VideoCapture videoCapture)
    {
        VideoCapture = videoCapture;
    }

    /// <summary>Opens the camera at the specified device <paramref name="videoIndex"/>.</summary>
    /// <param name="videoIndex">Zero-based index of the camera device (default: 0).</param>
    public VideoSource(int videoIndex = 0) : this(new VideoCapture(videoIndex))
    {
    }

    /// <summary>Opens a video file at the specified <paramref name="filePath"/>.</summary>
    /// <param name="filePath">Full path to the video file to open.</param>
    public VideoSource(string filePath) : this(new VideoCapture(filePath))
    {
    }

    /// <inheritdoc/>
    public IImage GetFrame() => GetImage();

    /// <summary>Captures the next frame from the video source and returns it as an <see cref="Image"/>.</summary>
    /// <returns>The captured <see cref="Image"/>, or an empty image when no frame is available.</returns>
    protected internal Image GetImage()
    {
        var mat = new Mat();
        return VideoCapture.Read(mat) ? new Image(mat) : new Image();
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        VideoCapture.Release();
        base.DisposeManaged();
    }
}

