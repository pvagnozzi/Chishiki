// -----------------------------------------------------------------------------
// File:        OpenCVCardplateDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV-based detector for vehicle cardplates with optional text recognition.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Cardplates;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using VisionRect = Chishiki.Vision.Abstraction.Models.Rect;
using OpenCvRect = OpenCvSharp.Rect;
using Chishiki.Vision.Abstraction.Recognizers.Cardplages;


namespace Chishiki.Vision.OpenCV.Detectors.Cardplates;

/// <summary>Detects vehicle cardplates in frames by combining OpenCV contour filtering with an optional text recognition step.</summary>
/// <remarks>Initializes a new <see cref="OpenCVCardplateDetector"/> with the supplied options, recognizer, and logger.</remarks>
/// <param name="options">Detector configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public partial class OpenCVCardplateDetector : OpenCVDetector<CardplateDetectionResult, CardplateDetection, OpenCVCardplateDetectorOptions>, ICardplateDetector
{
    private readonly Mat _closeKernel;

    private OpenCVCardplateDetector(OpenCVCardplateDetectorOptions options, ILogger<OpenCVCardplateDetector> logger)
        : base(options, logger)
    {
        _closeKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(Math.Max(1, options.MorphologyKernelWidth), Math.Max(1, options.MorphologyKernelHeight)));
    }


    /// <inheritdoc/>
    public override void Reset() => CheckDisposed();

    /// <inheritdoc/>
    protected override CardplateDetectionResult CreateEmptyResult(IImage image) => new(image);

    /// <inheritdoc/>
    protected override Task<CardplateDetectionResult> ProcessFrameAsync(IImage originalImage, Mat image, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckDisposed();

        try
        {
            using var grayscale = image.ToGrayscale();
            using var blurred = new Mat();
            using var binary = new Mat();
            using var closed = new Mat();

            var blurKernel = Options.GaussianKernelSize.EnsureOddKernel();
            Cv2.GaussianBlur(grayscale, blurred, new Size(blurKernel, blurKernel), 0);
            Cv2.AdaptiveThreshold(
                blurred,
                binary,
                255,
                AdaptiveThresholdTypes.GaussianC,
                ThresholdTypes.BinaryInv,
                Options.AdaptiveThresholdBlockSize.EnsureOddKernel(),
                Options.AdaptiveThresholdC);
            Cv2.MorphologyEx(binary, closed, MorphTypes.Close, _closeKernel, iterations: Options.MorphologyIterations);

            Cv2.FindContours(closed, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            LogContoursDiscovered(Logger, contours.Length);

            var candidates = contours
                .Select(contour => CreateCandidate(contour, image.Size()))
                .Where(static candidate => candidate is not null)
                .Select(static candidate => candidate!.Value)
                .OrderByDescending(static candidate => candidate.DetectionScore)
                .Take(Math.Max(0, Options.MaxCandidates))
                .ToList();

            LogCandidatesAccepted(Logger, candidates.Count);

            var detections = new List<CardplateDetection>(candidates.Count);
            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                detections.Add(new CardplateDetection(
                    new VisionRect(candidate.Region.X, candidate.Region.Y, candidate.Region.Width, candidate.Region.Height),
                    candidate.DetectionScore));
            }

            var recognizedCount = detections.Count(static detection => detection.HasRecognition);
            LogDetectionCompleted(Logger, detections.Count, recognizedCount);
            return Task.FromResult(new CardplateDetectionResult(originalImage, detections: detections));
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
    protected override Mat DrawDetection(Mat image, CardplateDetection detection, Scalar color)
    {
        image = base.DrawDetection(image, detection, color);
        if (!detection.HasRecognition)
        {
            return image;
        }

        var textOriginY = detection.Rect.Y > 16 ? detection.Rect.Y - 6 : detection.Rect.Y + detection.Rect.Height + 16;
        Cv2.PutText(
            image,
            detection.RecognizedText!,
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
        _closeKernel.Dispose();
        base.DisposeManaged();
    }

    #region Private Helpers
    private CandidateDetection? CreateCandidate(Point[] contour, Size imageSize)
    {
        var area = Cv2.ContourArea(contour);
        if (area < Options.MinContourArea)
        {
            return null;
        }

        var rect = Cv2.BoundingRect(contour);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return null;
        }

        var aspectRatio = rect.Width / (double)rect.Height;
        if (aspectRatio < Options.MinAspectRatio || aspectRatio > Options.MaxAspectRatio)
        {
            return null;
        }

        var rectangularity = area / (rect.Width * (double)rect.Height);
        if (rectangularity < Options.MinRectangularity)
        {
            return null;
        }

        var paddedRect = rect.ExpandRect(imageSize, Options.CandidatePaddingFactor);
        return new CandidateDetection(paddedRect, area, aspectRatio.CalculateDetectionScore(rectangularity));
    }

    private readonly record struct CandidateDetection(OpenCvRect Region, double Area, float DetectionScore);
    #endregion

    #region Log Messages
    /// <summary>Emitted when raw contour extraction completes for the current frame.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate detector discovered {ContourCount} raw contour candidates.")]
    private static partial void LogContoursDiscovered(ILogger logger, int contourCount);

    /// <summary>Emitted when geometric filtering accepts candidate regions for recognition.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate detector accepted {CandidateCount} candidate regions for recognition.")]
    private static partial void LogCandidatesAccepted(ILogger logger, int candidateCount);

    /// <summary>Emitted after the cardplate detection pass completes successfully.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate detection completed with {DetectionCount} detections and {RecognizedCount} recognized cardplates.")]
    private static partial void LogDetectionCompleted(ILogger logger, int detectionCount, int recognizedCount);

    /// <summary>Emitted when recognition of an individual candidate region fails unexpectedly.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Cardplate recognition failed for candidate region at ({CandidateX}, {CandidateY}, {CandidateWidth}, {CandidateHeight}).")]
    private static partial void LogCandidateRecognitionFailed(ILogger logger, int candidateX, int candidateY, int candidateWidth, int candidateHeight, Exception exception);

    /// <summary>Emitted when the detector fails unexpectedly while processing a frame.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Cardplate detection failed.")]
    private static partial void LogDetectionFailed(ILogger logger, Exception exception);
    #endregion
}
