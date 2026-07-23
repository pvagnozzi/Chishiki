// -----------------------------------------------------------------------------
// File:        OpenCVDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for OpenCV-based detectors.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Common.Detectors;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Detectors;

/// <summary>
/// Base class for OpenCV-based detectors. This abstract class provides a common foundation for all detectors that utilize OpenCV for image processing. It inherits from the generic <see cref="Detector{TResult, TDetection}"/> base class, allowing concrete implementations to specify their own result and detection types while sharing common functionality and configuration management.
/// </summary>
/// <typeparam name="TResult">The concrete detection result type (must derive from <see cref="DetectionResult{TDetection}"/>).</typeparam>
/// <typeparam name="TDetection">The type of the individual detection.</typeparam>
/// <typeparam name="TOptions">The type of the detector options.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="OpenCVDetector{TResult, TDetection, TOptions}"/> class.
/// </remarks>
/// <param name="options">The configuration options for the detector.</param>
/// <param name="logger">The logger used for diagnostics.</param>
public abstract partial class OpenCVDetector<TResult, TDetection, TOptions>(TOptions options, ILogger logger) :
#pragma warning disable CS9107 // Il parametro viene catturato nello stato del tipo di inclusione e il relativo valore viene passato anche al costruttore di base. Il valore potrebbe essere catturato anche dalla classe di base.
    Detector<TDetection, TOptions>(options, logger: logger)
#pragma warning restore CS9107 // Il parametro viene catturato nello stato del tipo di inclusione e il relativo valore viene passato anche al costruttore di base. Il valore potrebbe essere catturato anche dalla classe di base.
    where TResult : DetectionResult<TDetection>
    where TOptions : DetectorOptions
    where TDetection : Detection
{
    /// <inheritdoc/>
    public override async Task<DetectionResult<TDetection>> DetectAsync(IImage frame, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckDisposed();

        var image = frame.ToOpenCVImage();
        var mat = image.Mat;

        if (image.IsEmpty())
        {
            LogEmptyFrame();
            return CreateEmptyResult(frame);
        }

        var result = await ProcessFrameAsync(frame, mat, cancellationToken);
        var annotated = DrawDetections(mat, result.Detections);
        return result with { AnnotatedFrame = new OpenCVImage(annotated) }
            ?? result;
    }

    /// <summary>
    /// Preprocesses the input frame before detection. This virtual method can be overridden by derived classes to apply specific preprocessing steps to the input image, such as resizing, color conversion, or noise reduction, before the actual detection logic is executed. By default, it returns the original image without modification.
    /// </summary>
    /// <param name="image">The OpenCV Mat object representing the image frame.</param>
    /// <returns></returns>
    protected virtual Mat PreprocessFrame(Mat image) => image;

    /// <summary>
    /// Processes a single frame and returns the detection result. This abstract method must be implemented by derived classes to perform the actual detection logic using OpenCV. The method takes an OpenCV Mat object representing the image frame and a cancellation token for cooperative cancellation. The implementation should analyze the image, detect relevant features or objects, and return a result encapsulating the detections found in the frame.
    /// </summary>
    /// <param name="originalImage">The original input image before any preprocessing. This parameter allows derived classes to access the unmodified image data if needed for certain detection algorithms that may require the original pixel values or metadata.</param>
    /// <param name="image">The OpenCV Mat object representing the image frame.</param>
    /// <param name="cancellationToken">A cancellation token for cooperative cancellation.</param>
    /// <returns>A task representing the asynchronous operation, with a result of type <typeparamref name="DetectionResult{TDetection}"/>.</returns>
    protected abstract Task<TResult> ProcessFrameAsync(IImage originalImage, Mat image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Draws the detected objects on the image. This method iterates through the provided detections and draws a rectangle around each detected object using the specified highlight color and thickness from the options. The resulting image with the drawn detections is returned for further use, such as displaying or saving to disk.
    /// </summary>    
    /// <param name="image">The image on which to draw the detections.</param>
    /// <param name="detections">The collection of detections to draw.</param>
    /// <returns>The image with the drawn detections.</returns>
    protected virtual Mat DrawDetections(Mat image, IEnumerable<TDetection> detections)
    {
        var colour = new Scalar(Options.HighlightColour.B, Options.HighlightColour.G, Options.HighlightColour.R);
        foreach (var r in detections)
        {
            image = DrawDetection(image, r, colour);
        }
        return image;
    }

    /// <summary>
    /// Draws a single detection on the image. This method takes an individual detection and draws a rectangle around it using the specified color and thickness from the options. The resulting image with the drawn detection is returned for further use, such as displaying or saving to disk.
    /// </summary>
    /// <param name="image">The image on which to draw the detection.</param>
    /// <param name="detection">The detection to draw.</param>
    /// <param name="color">The color to use for drawing the detection.</param>
    /// <returns>The image with the drawn detection.</returns>
    protected virtual Mat DrawDetection(Mat image, TDetection detection, Scalar color)
    {
        var rect = detection.Rect;
        Cv2.Rectangle(
            image,
            new Rect(rect.X, rect.Y, rect.Width, rect.Height),
            color,
            Options.HighlightThickness);
        return image;
    }

    /// <summary>
    /// Creates an empty detection result. This abstract method must be implemented by derived classes to provide a way to create an empty result object of type <typeparamref name="TResult"/>. This is used when an empty frame is received, allowing the detector to return a valid but empty result without performing any detection logic.
    /// </summary>
    /// <param name="image">The input image for which to create the empty result. This parameter can be used by derived classes to initialize the result with relevant information from the image, such as dimensions or metadata, even when no detections are present.</param>
    /// <returns>An empty detection result of type <typeparamref name="DetectionResult{TDetection}"/>.</returns>
    protected abstract TResult CreateEmptyResult(IImage image);

    /// <summary>Emitted when an empty frame is received and skipped.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Empty frame received; skipping detection.")]
    private partial void LogEmptyFrame();
}

