// -----------------------------------------------------------------------------
// File:        OpenCVLBPHFaceRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV LBPH-based recognizer for face identity prediction.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Faces;
using Chishiki.Vision.Abstraction.Recognizers;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Face;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Chishiki.Vision.OpenCV.Detectors.Faces;

/// <summary>Recognizes faces by loading an OpenCV LBPH model from disk and predicting the closest identity for a cropped face image.</summary>
/// <remarks>Initializes a new <see cref="OpenCVLBPHFaceRecognizer"/> with the supplied options and logger.</remarks>
/// <param name="options">Recognizer configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public partial class OpenCVLBPHFaceRecognizer(OpenCVLBPHFaceRecognizerOptions options, ILogger logger)
    : Common.Detectors.Faces.FaceRecognizer(options, logger)
{
    private readonly LBPHFaceRecognizer _recognizer = CreateRecognizer(options);

    /// <summary>Gets the strongly typed recognizer options.</summary>
    public new OpenCVLBPHFaceRecognizerOptions Options => (OpenCVLBPHFaceRecognizerOptions)base.Options;

    /// <inheritdoc/>
    public override Task<RecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(image);

        try
        {
            using var grayscale = ToGrayscale(image.ToMat());
            if (grayscale.Empty())
            {
                LogEmptyFaceCrop(Logger);
                return Task.FromResult<RecognitionResult>(new FaceRecognitionResult());
            }

            using var normalized = NormalizeFace(grayscale);
            _recognizer.Predict(normalized, out var labelId, out var distance);
            if (labelId < 0)
            {
                LogNoPrediction(Logger);
                return Task.FromResult<RecognitionResult>(new FaceRecognitionResult(distance: distance));
            }

            if (Options.MaximumDistance > 0.0 && distance > Options.MaximumDistance)
            {
                LogPredictionRejected(Logger, labelId, distance, Options.MaximumDistance);
                return Task.FromResult<RecognitionResult>(new FaceRecognitionResult(distance: distance));
            }

            var identity = _recognizer.GetLabelInfo(labelId);
            var normalizedScore = NormalizeScore(distance, Options.MaximumDistance);
            LogRecognitionCompleted(Logger, labelId, normalizedScore, distance);
            return Task.FromResult<RecognitionResult>(new FaceRecognitionResult(identity: identity, labelId: labelId, score: normalizedScore, distance: distance));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogRecognitionFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        _recognizer.Dispose();
        base.DisposeManaged();
    }

    #region Private Helpers
    private static LBPHFaceRecognizer CreateRecognizer(OpenCVLBPHFaceRecognizerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var modelPath = Path.GetFullPath(options.ModelPath);
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"The face recognition model '{modelPath}' was not found.", modelPath);
        }

        var threshold = options.MaximumDistance > 0.0 ? options.MaximumDistance : double.MaxValue;
        var recognizer = LBPHFaceRecognizer.Create(options.Radius, options.Neighbors, options.GridX, options.GridY, threshold);
        recognizer.Read(modelPath);
        return recognizer;
    }

    private Mat NormalizeFace(Mat grayscale)
    {
        var normalized = new Mat();
        Cv2.Resize(grayscale, normalized, new Size(Math.Max(1, Options.TargetWidth), Math.Max(1, Options.TargetHeight)));
        if (Options.EqualizeHistogram)
        {
            Cv2.EqualizeHist(normalized, normalized);
        }

        return normalized;
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

    private static float NormalizeScore(double distance, double maximumDistance)
    {
        if (maximumDistance > 0.0)
        {
            return (float)Math.Clamp(1.0 - (distance / maximumDistance), 0.0, 1.0);
        }

        return (float)(1.0 / (1.0 + Math.Max(0.0, distance)));
    }
    #endregion

    #region Log Messages
    /// <summary>Emitted when the recognizer receives an empty face crop.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Empty face crop received; skipping recognition.")]
    private static partial void LogEmptyFaceCrop(ILogger logger);

    /// <summary>Emitted when the face recognizer cannot predict a label for the supplied crop.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Face recognition produced no prediction.")]
    private static partial void LogNoPrediction(ILogger logger);

    /// <summary>Emitted when a predicted identity is rejected by the configured maximum distance.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Face recognition rejected label {LabelId} because distance {Distance} exceeded {MaximumDistance}.")]
    private static partial void LogPredictionRejected(ILogger logger, int labelId, double distance, double maximumDistance);

    /// <summary>Emitted after successful recognition of a face crop.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Face recognition completed for label {LabelId} with score {Score} and distance {Distance}.")]
    private static partial void LogRecognitionCompleted(ILogger logger, int labelId, float score, double distance);

    /// <summary>Emitted when face recognition fails unexpectedly.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Face recognition failed.")]
    private static partial void LogRecognitionFailed(ILogger logger, Exception exception);
    #endregion
}
