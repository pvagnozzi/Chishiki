// -----------------------------------------------------------------------------
// File:        MotionDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: MOG2-based motion detector with Gaussian denoising, morphological cleanup, and contour highlighting.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV;

/// <summary>Stateful motion detector based on OpenCV MOG2 background subtraction. The processing pipeline applies Gaussian blur for noise suppression, morphological erosion and dilation to remove artefacts and fill gaps, then extracts contours to identify and annotate motion regions.</summary>
public sealed partial class MotionDetector : IMotionDetector
{
    private readonly MotionDetectorOptions _options;
    private readonly ILogger<MotionDetector> _logger;
    private readonly BackgroundSubtractorMOG2 _subtractor;
    private readonly Size _gaussianKernel;
    private readonly Mat _erodeKernel;
    private readonly Mat _dilateKernel;
    private bool _disposed;

    /// <summary>Initialises a new <see cref="MotionDetector"/> with the supplied options and logger.</summary>
    /// <param name="options">Algorithm configuration options.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public MotionDetector(MotionDetectorOptions options, ILogger<MotionDetector> logger)
    {
        _options = options;
        _logger = logger;

        _subtractor = BackgroundSubtractorMOG2.Create(
            history: options.BackgroundHistory,
            varThreshold: options.Mog2Threshold,
            detectShadows: options.DetectShadows);

        _gaussianKernel = new Size(options.GaussianKernelSize, options.GaussianKernelSize);

        _erodeKernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(options.ErodeKernelSize, options.ErodeKernelSize));

        _dilateKernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(options.DilateKernelSize, options.DilateKernelSize));
    }

    /// <inheritdoc/>
    public Task<MotionDetectionResult> DetectAsync(IImage frame, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (frame is not Image openCvImage)
            throw new ArgumentException("Frame must be an OpenCV Image instance.", nameof(frame));

        if (openCvImage.IsEmpty())
        {
            LogEmptyFrame();
            var emptyRegions = Array.Empty<MotionRegion>();
            var emptyResult = new MotionDetectionResult(frame, frame, emptyRegions, DateTimeOffset.UtcNow, false);
            return Task.FromResult(emptyResult);
        }

        var result = ProcessFrame(openCvImage);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _subtractor.Dispose();
        LogBackgroundReset();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _subtractor.Dispose();
        _erodeKernel.Dispose();
        _dilateKernel.Dispose();
        _disposed = true;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Executes the full detection pipeline on a single non-empty frame.</summary>
    /// <param name="frame">The source OpenCV frame.</param>
    /// <returns>A populated <see cref="MotionDetectionResult"/>.</returns>
    private MotionDetectionResult ProcessFrame(Image frame)
    {
        using var gray = new Mat();
        using var blurred = new Mat();
        using var fgMask = new Mat();
        using var eroded = new Mat();
        using var dilated = new Mat();

        // 1. Convert to grayscale to reduce data volume.
        Cv2.CvtColor(frame.Mat, gray, ColorConversionCodes.BGR2GRAY);

        // 2. Gaussian blur — suppresses high-frequency noise before subtraction.
        Cv2.GaussianBlur(gray, blurred, _gaussianKernel, sigmaX: 0);

        // 3. MOG2 background subtraction — produces foreground binary mask.
        _subtractor.Apply(blurred, fgMask);

        // 4. Morphological erosion — removes small noise pixels (salt).
        Cv2.Erode(fgMask, eroded, _erodeKernel, iterations: _options.ErodeIterations);

        // 5. Morphological dilation — restores object size and fills internal holes.
        Cv2.Dilate(eroded, dilated, _dilateKernel, iterations: _options.DilateIterations);

        // 6. Find external contours of motion blobs.
        Cv2.FindContours(
            dilated,
            out var contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // 7. Filter by minimum area and build MotionRegion list.
        var regions = new List<MotionRegion>(contours.Length);
        foreach (var contour in contours)
        {
            var area = Cv2.ContourArea(contour);
            if (area < _options.MinContourArea)
                continue;

            var rect = Cv2.BoundingRect(contour);
            regions.Add(new MotionRegion(rect.X, rect.Y, rect.Width, rect.Height, area));
        }

        // 8. Clone original and draw highlights.
        var annotatedMat = frame.Mat.Clone();
        var colour = new Scalar(_options.HighlightColour.B, _options.HighlightColour.G, _options.HighlightColour.R);
        foreach (var r in regions)
        {
            Cv2.Rectangle(
                annotatedMat,
                new Rect(r.X, r.Y, r.Width, r.Height),
                colour,
                _options.HighlightThickness);
        }

        var hasMotion = regions.Count > 0;
        if (hasMotion)
            LogMotionDetected(regions.Count);

        var annotated = new Image(annotatedMat);
        return new MotionDetectionResult(frame, annotated, regions, DateTimeOffset.UtcNow, hasMotion);
    }

    // -------------------------------------------------------------------------
    // Compile-time logging
    // -------------------------------------------------------------------------

    /// <summary>Emitted when an empty frame is received and skipped.</summary>
    [LoggerMessage(Level = global::Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Empty frame received; skipping motion detection.")]
    private partial void LogEmptyFrame();

    /// <summary>Emitted after successful background model reset.</summary>
    [LoggerMessage(Level = global::Microsoft.Extensions.Logging.LogLevel.Information, Message = "Motion detector background model reset.")]
    private partial void LogBackgroundReset();

    /// <summary>Emitted when at least one motion region is found.</summary>
    [LoggerMessage(Level = global::Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Motion detected: {RegionCount} region(s) found.")]
    private partial void LogMotionDetected(int regionCount);
}
