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
using Chishiki.Vision.Abstraction.Sources;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Sources;

/// <summary>Video source that captures frames from a physical camera or a video file.</summary>
public abstract class VideoSource(ILogger<VideoSource> logger) : Disposable(logger), IVideoSource
{
    /// <inheritdoc/>
    public abstract string VideoSourceId { get; }

    /// <inheritdoc/>
    public abstract Task<IImage> GetImageAsync(CancellationToken cancellationToken = default);
}

