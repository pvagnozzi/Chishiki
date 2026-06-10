// -----------------------------------------------------------------------------
// File:        OpenCVFaceDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV-based detector for faces with optional identity recognition.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Faces;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using VisionRect = Chishiki.Vision.Abstraction.Models.Rect;
using OpenCvRect = OpenCvSharp.Rect;

namespace Chishiki.Vision.OpenCV.Detectors.Faces;

/// <summary>Detects faces in frames by combining an OpenCV cascade classifier with an optional recognition step.</summary>
/// <remarks>Initializes a new <see cref="OpenCVFaceDetector"/> with the supplied options, recognizer, and logger.</remarks>
/// <param name="options">Detector configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public partial class OpenCVFaceDetector : OpenCVDetector<FaceDetectionResult, FaceDetection>, IFaceDetector
{
    private readonly CascadeClassifier _cascadeClassifier;
    private readonly IFaceRecognizer? _recognizer;
    private readonly bool _ownsRecognizer;

    /// <summary>Initializes a new <see cref="OpenCVFaceDetector"/> with a default LBPH recognizer when configured.</summary>
    /// <param name="options">Detector configuration options.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public OpenCVFaceDetector(OpenCVFaceDetectorOptions options, ILogger<OpenCVFaceDetector> logger)
        : this(options, CreateDefaultRecognizer(options, logger), logger, ownsRecognizer: !string.IsNullOrWhiteSpace(options.RecognizerOptions.ModelPath))
    {
    }

    /// <summary>Initializes a new <see cref="OpenCVFaceDetector"/> with an explicit face recognizer.</summary>
    /// <param name="options">Detector configuration options.</param>
    /// <param name="recognizer">Recognizer used to enrich candidate detections with identities.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    public OpenCVFaceDetector(OpenCVFaceDetectorOptions options, IFaceRecognizer recognizer, ILogger<OpenCVFaceDetector> logger)
        : this(options, recognizer, logger, ownsRecognizer: false)
    {
    }

    private OpenCVFaceDetector(OpenCVFaceDetectorOptions options, IFaceRecognizer? recognizer, ILogger<OpenCVFaceDetector> logger, bool ownsRecognizer)
        : base(options, logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        var cascadeModelPath = Path.GetFullPath(options.CascadeModelPath);
        if (!File.Exists(cascadeModelPath))
        {
            throw new FileNotFoundException($"The face cascade model '{cascadeModelPath}' was not found.", cascadeModelPath);
        }

        _cascadeClassifier = new CascadeClassifier(cascadeModelPath);
        if (_cascadeClassifier.Empty())
        {
            throw new InvalidOperationException($"The face cascade model '{cascadeModelPath}' could not be loaded.");
        }

        _recognizer = recognizer;
        _ownsRecognizer = ownsRecognizer;
        if (_recognizer is null)
        {
            LogRecognitionDisabled(Logger!);
        }
    }

    /// <summary>Gets the strongly typed detector options.</summary>
    public new OpenCVFaceDetectorOptions Options => (OpenCVFaceDetectorOptions)base.Options;

    /// <inheritdoc/>
    FaceDetectorOptions IFaceDetector.Options => Options;

    /// <inheritdoc/>
    public override void Reset() => CheckDisposed();

    /// <inheritdoc/>
    protected override FaceDetectionResult CreateEmptyResult(IImage image) => new(image);

    /// <inheritdoc/>
    protected override async Task<FaceDetectionResult> ProcessFrameAsync(IImage originalImage, Mat image, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckDisposed();

        try
        {
            using var grayscale = image.ToGrayscale();
            if (Options.EqualizeHistogram)
            {
                Cv2.EqualizeHist(grayscale, grayscale);
            }

            var minSize = new Size(Math.Max(1, Options.MinFaceWidth), Math.Max(1, Options.MinFaceHeight));
            Size? maxSize = Options.MaxFaceWidth > 0 && Options.MaxFaceHeight > 0
                ? new Size(Options.MaxFaceWidth, Options.MaxFaceHeight)
                : null;

            var faceRegions = _cascadeClassifier.DetectMultiScale(grayscale, Options.ScaleFactor, Options.MinNeighbors, HaarDetectionTypes.ScaleImage, minSize, maxSize);
            LogFacesDetected(Logger, faceRegions.Length);

            var detections = new List<FaceDetection>(faceRegions.Length);
            foreach (var faceRegion in faceRegions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var recognition = await RecognizeFaceAsync(image, faceRegion, cancellationToken);
                if (Options.RequireRecognition && !recognition.HasRecognition)
                {
                    continue;
                }

                detections.Add(new FaceDetection(
                    new VisionRect(faceRegion.X, faceRegion.Y, faceRegion.Width, faceRegion.Height),
                    CalculateDetectionScore(faceRegion, image.Size()),
                    recognition.LabelId,
                    recognition.Identity,
                    recognition.HasRecognition ? recognition.Score : null,
                    recognition.Distance,
                    faceRegion.Width * (double)faceRegion.Height));
            }

            var recognizedCount = detections.Count(static detection => detection.HasRecognition);
            LogDetectionCompleted(Logger, detections.Count, recognizedCount);
            return new FaceDetectionResult(originalImage, detections: detections);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDetectionFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc/>
    protected override Mat DrawDetection(Mat image, FaceDetection detection, Scalar color)
    {
        image = base.DrawDetection(image, detection, color);
        if (!detection.HasRecognition)
        {
            return image;
        }

        var label = !string.IsNullOrWhiteSpace(detection.Identity)
            ? detection.Identity!
            : $"Label {detection.LabelId}";
        var textOriginY = detection.Rect.Y > 16 ? detection.Rect.Y - 6 : detection.Rect.Y + detection.Rect.Height + 16;
        Cv2.PutText(
            image,
            label,
            new Point(detection.Rect.X, textOriginY),
            HersheyFonts.HersheySimplex,
            0.55,
            color,
            Math.Max(1, Options.HighlightThickness - 1),
            LineTypes.AntiAlias);

        return image;
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        _cascadeClassifier.Dispose();
        if (_ownsRecognizer)
        {
            _recognizer?.Dispose();
        }

        base.DisposeManaged();
    }

    #region Private Helpers
    private async Task<FaceRecognitionResult> RecognizeFaceAsync(Mat image, OpenCvRect region, CancellationToken cancellationToken)
    {
        if (_recognizer is null)
        {
            return new FaceRecognitionResult();
        }

        try
        {
            using var face = new Mat(image, region);
            using var faceImage = new OpenCVImage(face.Clone());
            return await _recognizer.RecognizeAsync(faceImage, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCandidateRecognitionFailed(Logger, region.X, region.Y, region.Width, region.Height, ex);
            return new FaceRecognitionResult();
        }
    }

    private static OpenCVLBPHFaceRecognizer? CreateDefaultRecognizer(OpenCVFaceDetectorOptions options, ILogger logger) =>
        !string.IsNullOrWhiteSpace(options.RecognizerOptions.ModelPath)
            ? new OpenCVLBPHFaceRecognizer(options.RecognizerOptions, logger)
            : null;

    private static float CalculateDetectionScore(OpenCvRect region, Size imageSize)
    {
        var imageArea = Math.Max(1.0, imageSize.Width * (double)imageSize.Height);
        var areaRatio = region.Width * (double)region.Height / imageArea;
        return (float)Math.Clamp(areaRatio * 12.0, 0.0, 1.0);
    }
    #endregion

    #region Log Messages
    /// <summary>Emitted when the detector runs without a configured recognizer.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Face detector recognition step is disabled because no recognizer model was configured.")]
    private static partial void LogRecognitionDisabled(ILogger logger);

    /// <summary>Emitted when face regions are detected by the cascade classifier.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Face detector discovered {FaceCount} face candidates.")]
    private static partial void LogFacesDetected(ILogger logger, int faceCount);

    /// <summary>Emitted after the face detection pass completes successfully.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Face detection completed with {DetectionCount} detections and {RecognizedCount} recognized faces.")]
    private static partial void LogDetectionCompleted(ILogger logger, int detectionCount, int recognizedCount);

    /// <summary>Emitted when recognition of an individual face region fails unexpectedly.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Face recognition failed for candidate region at ({CandidateX}, {CandidateY}, {CandidateWidth}, {CandidateHeight}).")]
    private static partial void LogCandidateRecognitionFailed(ILogger logger, int candidateX, int candidateY, int candidateWidth, int candidateHeight, Exception exception);

    /// <summary>Emitted when the detector fails unexpectedly while processing a frame.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Face detection failed.")]
    private static partial void LogDetectionFailed(ILogger logger, Exception exception);
    #endregion
}
