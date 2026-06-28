// -----------------------------------------------------------------------------
// File:        OpenCVMotionDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: MOG2-based motion detector with Gaussian denoising, morphological cleanup, and contour highlighting.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Motion;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Detectors.Motion;

/// <summary>Stateful motion detector based on OpenCV MOG2 background subtraction. The processing pipeline applies Gaussian blur for noise suppression, morphological erosion and dilation to remove artefacts and fill gaps, then extracts contours to identify and annotate motion regions.</summary>
/// <remarks>Initializes a new <see cref="OpenCVMotionDetector"/> with the supplied options and logger.</remarks>
/// <param name="options">Algorithm configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public partial class OpenCVMotionDetector(OpenCVMotionDetectorOptions options, ILogger<OpenCVMotionDetector> logger) :
    OpenCVDetector<MotionDetectionResult, MotionDetection, OpenCVMotionDetectorOptions>(options, logger), IMotionDetector
{
    /// <summary>
    /// Background subtractor instance using the MOG2 algorithm. It is initialized in the constructor with parameters from the provided options and is responsible for maintaining the background model and generating foreground masks for motion detection. The subtractor is a stateful component that updates its model over time as new frames are processed, allowing it to adapt to changes in the scene while effectively distinguishing between background and moving objects.
    /// </summary>
    private readonly BackgroundSubtractorMOG2 _subtractor = BackgroundSubtractorMOG2.Create(
            history: options.BackgroundHistory,
            varThreshold: options.Mog2Threshold,
            detectShadows: options.DetectShadows);

    /// <summary>
    /// Erosion kernel used for morphological erosion to remove noise from motion masks. It is initialized in the constructor based on the provided options and remains constant for the lifetime of the detector.
    /// </summary>
    private readonly Mat _erodeKernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(options.ErodeKernelSize, options.ErodeKernelSize));

    /// <summary>
    /// The dilation kernel used for morphological dilation to fill gaps in motion masks. It is initialized in the constructor based on the provided options and remains constant for the lifetime of the detector.
    /// </summary>
    private readonly Mat _dilateKernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(options.DilateKernelSize, options.DilateKernelSize));

    /// <summary>
    /// Gets the strongly-typed options for this motion detector, allowing access to specific configuration parameters defined in <see cref="OpenCVMotionDetectorOptions"/> without needing to cast from the base <see cref="DetectorOptions"/> type. This property provides convenient access to the motion detector's settings, such as Gaussian kernel size, background history, MOG2 threshold, and morphological operation parameters, enabling derived classes and internal methods to easily reference these options when processing frames and performing motion detection logic.
    /// </summary>
    public new OpenCVMotionDetectorOptions Options => (OpenCVMotionDetectorOptions)base.Options;
    MotionDetectorOptions IDetector<MotionDetection, MotionDetectorOptions>.Options => Options;

    /// <summary>
    /// Gets the name of the motion detector, which is used for logging and identification purposes. This property returns a string that uniquely identifies this specific implementation of a motion detector, allowing it to be distinguished from other detectors in logs, diagnostics, and when managing multiple detectors within the application.
    /// </summary>

    /// <inheritdoc/>
    public override void Reset()
    {
        CheckDisposed();
        _subtractor.Dispose();
        LogBackgroundReset();
    }

    /// <summary>
    /// Creates an empty motion detection result for the given image. This method is called when the detector is unable to process the frame or when no motion is detected, allowing it to return a consistent result object with an empty list of motion regions and a flag indicating that no motion was found. The implementation of this method is currently not provided and will throw a <see cref="NotImplementedException"/> if called, indicating that it needs to be implemented to return a valid <see cref="MotionDetectionResult"/> instance based on the input image.
    /// </summary>
    /// <param name="image">The input image for which to create the empty result.</param>
    /// <returns>An empty motion detection result.</returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override MotionDetectionResult CreateEmptyResult(IImage image) => new(image);

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        _subtractor.Dispose();
        _erodeKernel.Dispose();
        _dilateKernel.Dispose();
        base.DisposeManaged();
    }

    /// <inheritdoc/>
    protected override Task<MotionDetectionResult> ProcessFrameAsync(
        IImage originalImage,
        Mat image,
        CancellationToken cancellationToken = default)
    {
        using var fgMask = new Mat();
        using var eroded = new Mat();
        using var dilated = new Mat();

        // 1. MOG2 background subtraction — produces foreground binary mask.
        _subtractor.Apply(image, fgMask);

        // 2. Morphological erosion — removes small noise pixels (salt).
        Cv2.Erode(fgMask, eroded, _erodeKernel, iterations: Options.ErodeIterations);

        // 3. Morphological dilation — restores object size and fills internal holes.
        Cv2.Dilate(eroded, dilated, _dilateKernel, iterations: Options.DilateIterations);

        // 4. Find external contours of motion blobs.
        Cv2.FindContours(
            dilated,
            out var contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // 5. Filter by minimum area and build MotionRegion list.
        var detections = new List<MotionDetection>(contours.Length);

        foreach (var contour in contours)
        {
            var area = Cv2.ContourArea(contour);
            if (!(area < Options.MinContourArea))
            {
                var rect = Cv2.BoundingRect(contour);
                detections.Add(new MotionDetection(rect.X, rect.Y, rect.Width, rect.Height, area));
            }
        }

        var hasMotion = detections.Count > 0;
        if (hasMotion)
        {
            LogMotionDetected(detections.Count);
        }

        var result = new MotionDetectionResult(originalImage, detections: detections);
        return Task.FromResult(result);
    }

    #region Compile-time logging    
    /// <summary>Emitted after successful background model reset.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Motion detector background model reset.")]
    private partial void LogBackgroundReset();

    /// <summary>Emitted when at least one motion region is found.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Motion detected: {RegionCount} region(s) found.")]
    private partial void LogMotionDetected(int regionCount);
    #endregion
}
