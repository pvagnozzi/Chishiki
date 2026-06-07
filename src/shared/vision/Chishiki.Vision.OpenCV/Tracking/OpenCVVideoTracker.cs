// -----------------------------------------------------------------------------
// File:        OpenCVVideoTracker.cs
// Author:      Piergiorgio Vagnozzi
// Description: Minimal single-object video tracker based on OpenCV template matching.
// Created:     2026-06-06
// Modified:    2026-06-06
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Tracking;
using Chishiki.Vision.OpenCV.Detectors;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using VisionRect = Chishiki.Vision.Abstraction.Models.Rect;
using OpenCvRect = OpenCvSharp.Rect;
using OpenCvSize = OpenCvSharp.Size;

namespace Chishiki.Vision.OpenCV.Tracking;

/// <summary>Minimal single-object video tracker based on OpenCV template matching.</summary>
/// <remarks>Initializes a new <see cref="OpenCVVideoTracker"/> with the supplied options and logger.</remarks>
/// <param name="options">Tracker configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public class OpenCVVideoTracker(OpenCVVideoTrackerOptions options, ILogger<OpenCVVideoTracker> logger) :
    OpenCVDetector<TrackingResult, TrackingDetection>(options, logger)
{
    private Mat? _template;
    private VisionRect? _lastKnownRegion;

    /// <summary>Gets the strongly-typed tracker options.</summary>
    public new OpenCVVideoTrackerOptions Options => (OpenCVVideoTrackerOptions)base.Options;

    /// <summary>Gets a value indicating whether the tracker has already been initialized with an initial region.</summary>
    public bool IsInitialized => _template is not null && _lastKnownRegion is not null;

    /// <summary>Initializes the tracker with the first frame and the region to follow in subsequent frames.</summary>
    /// <param name="frame">The frame that contains the initial tracked region.</param>
    /// <param name="initialRegion">The initial region to track.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="frame"/> or <paramref name="initialRegion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="frame"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialRegion"/> is invalid or outside the frame bounds.</exception>
    public void Initialize(IImage frame, VisionRect initialRegion)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(initialRegion);

        var openCvImage = frame as OpenCVImage;
        using var ownedImage = openCvImage is null ? new OpenCVImage(Mat.FromImageData(frame.ImageData, ImreadModes.AnyColor)) : null;
        var image = openCvImage ?? ownedImage!;

        if (image.IsEmpty())
        {
            throw new ArgumentException("Cannot initialize the tracker with an empty frame.", nameof(frame));
        }

        ValidateRegion(image.Mat, initialRegion, nameof(initialRegion));

        ResetTemplate();

        using var grayFrame = ToGrayscale(image.Mat);
        using var region = new Mat(grayFrame, ToOpenCvRect(initialRegion));

        _template = region.Clone();
        _lastKnownRegion = new VisionRect(initialRegion.X, initialRegion.Y, initialRegion.Width, initialRegion.Height);
    }

    /// <inheritdoc/>
    public override void Reset()
    {
        CheckDisposed();
        ResetTemplate();
        _lastKnownRegion = null;
    }

    /// <inheritdoc/>
    protected override TrackingResult CreateEmptyResult(IImage image) => new(image);

    /// <inheritdoc/>
    protected override Task<TrackingResult> ProcessFrameAsync(
        IImage originalImage,
        Mat image,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckDisposed();

        if (!IsInitialized)
        {
            throw new InvalidOperationException("The tracker must be initialized before calling DetectAsync.");
        }

        using var grayFrame = ToGrayscale(image);
        var template = _template!;
        var searchRegion = GetSearchRegion(grayFrame.Size(), _lastKnownRegion!);

        if (searchRegion.Width < template.Width || searchRegion.Height < template.Height)
        {
            return Task.FromResult(new TrackingResult(originalImage));
        }

        using var searchWindow = new Mat(grayFrame, searchRegion);
        using var matchResult = new Mat();

        Cv2.MatchTemplate(searchWindow, template, matchResult, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(matchResult, out _, out var maxScore, out _, out var maxLocation);

        if (maxScore < Options.MinimumScore)
        {
            return Task.FromResult(new TrackingResult(originalImage));
        }

        var trackedRegion = new VisionRect(
            searchRegion.X + maxLocation.X,
            searchRegion.Y + maxLocation.Y,
            template.Width,
            template.Height);

        _lastKnownRegion = trackedRegion;

        var detection = new TrackingDetection(trackedRegion);
        return Task.FromResult(new TrackingResult(originalImage, detections: [detection]));
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        ResetTemplate();
        base.DisposeManaged();
    }

    #region Private Helpers
    private void ResetTemplate()
    {
        _template?.Dispose();
        _template = null;
    }

    private OpenCvRect GetSearchRegion(OpenCvSize frameSize, VisionRect region)
    {
        var padding = Math.Max(0, Options.SearchPadding);
        var x = Math.Max(0, region.X - padding);
        var y = Math.Max(0, region.Y - padding);
        var right = Math.Min(frameSize.Width, region.X + region.Width + padding);
        var bottom = Math.Min(frameSize.Height, region.Y + region.Height + padding);

        return new OpenCvRect(x, y, Math.Max(0, right - x), Math.Max(0, bottom - y));
    }

    private static OpenCvRect ToOpenCvRect(VisionRect rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    private static void ValidateRegion(Mat image, VisionRect region, string paramName)
    {
        if (region.Width <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "The tracked region width must be greater than zero.");
        }

        if (region.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "The tracked region height must be greater than zero.");
        }

        if (region.X < 0 || region.Y < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "The tracked region must be inside the frame bounds.");
        }

        if (region.X + region.Width > image.Width || region.Y + region.Height > image.Height)
        {
            throw new ArgumentOutOfRangeException(paramName, "The tracked region must fit entirely inside the frame bounds.");
        }
    }

    private static Mat ToGrayscale(Mat image)
    {
        if (image.Channels() == 1)
        {
            return image.Clone();
        }

        var grayscale = new Mat();
        var conversionCode = image.Channels() switch
        {
            4 => ColorConversionCodes.BGRA2GRAY,
            _ => ColorConversionCodes.BGR2GRAY
        };

        Cv2.CvtColor(image, grayscale, conversionCode);
        return grayscale;
    }
    #endregion
}
