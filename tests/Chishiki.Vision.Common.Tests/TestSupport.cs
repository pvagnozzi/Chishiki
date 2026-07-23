// -----------------------------------------------------------------------------
// File:        TestSupport.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides small test doubles for deterministic Vision.Common tests.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Motion;
using Chishiki.Vision.Abstraction.Monitors;
using Chishiki.Vision.Abstraction.Sources;
using Chishiki.Vision.Common.Monitors;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki.Vision.Common.Tests;

/// <summary>Provides a simple detector options implementation for tests.</summary>
internal sealed record TestDetectorOptions : DetectorOptions;

/// <summary>Provides a disposable in-memory image for tests.</summary>
internal sealed class TestImage(bool isEmpty = false) : IImage
{
    /// <summary>Gets a value indicating whether the image has been disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc/>
    public byte[] ImageData { get; } = [0x10, 0x20, 0x30];

    /// <inheritdoc/>
    public bool IsEmpty() => isEmpty;

    /// <inheritdoc/>
    public void Dispose() => IsDisposed = true;
}

/// <summary>Provides a sequence-backed video source for tests.</summary>
internal sealed class SequenceVideoSource(string videoSourceId, IEnumerable<IImage> frames) : IVideoSource
{
    private readonly Queue<IImage> _frames = new(frames);

    /// <summary>Gets the number of frame requests served by the source.</summary>
    public int GetImageCallCount { get; private set; }

    /// <summary>Gets a value indicating whether the source has been disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc/>
    public string VideoSourceId { get; } = videoSourceId;

    /// <inheritdoc/>
    public Task<IImage> GetImageAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetImageCallCount++;

        if (_frames.Count == 0)
        {
            return Task.FromResult<IImage>(new TestImage(isEmpty: true));
        }

        return Task.FromResult(_frames.Dequeue());
    }

    /// <inheritdoc/>
    public void Dispose() => IsDisposed = true;
}

/// <summary>Exposes the abstract <see cref="VideoSourceMonitor"/> for deterministic tests.</summary>
internal sealed class TestVideoSourceMonitor : VideoSourceMonitor
{
    /// <summary>Initializes a new instance of the <see cref="TestVideoSourceMonitor"/> class.</summary>
    /// <param name="source">The video source to capture from.</param>
    /// <param name="detector">The motion detector to run on each frame.</param>
    /// <param name="options">Configuration options for the monitor.</param>
    public TestVideoSourceMonitor(IVideoSource source, IMotionDetector detector, VideoSourceMonitorOptions options)
        : base(source, detector, options, NullLogger<VideoSourceMonitor>.Instance)
    {
    }
}
