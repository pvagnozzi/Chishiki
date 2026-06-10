// -----------------------------------------------------------------------------
// File:        OpenCVTemplateCardplateRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV template-based recognizer for vehicle cardplate text.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Cardplates;
using Chishiki.Vision.Common.Detectors.Cardplates;
using Chishiki.Vision.OpenCV;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvRect = OpenCvSharp.Rect;

namespace Chishiki.Vision.OpenCV.Detectors.Cardplates;

/// <summary>Recognizes vehicle cardplate text by segmenting glyphs and matching them against OpenCV-generated templates.</summary>
/// <remarks>Initializes a new <see cref="OpenCVTemplateCardplateRecognizer"/> with the supplied options and logger.</remarks>
/// <param name="options">Recognizer configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public partial class OpenCVTemplateCardplateRecognizer(OpenCVTemplateCardplateRecognizerOptions options, ILogger logger)
    : CardplateRecognizer(options, logger)
{
    private readonly Dictionary<char, Mat> _templates = CreateTemplates(options);

    /// <summary>Gets the strongly typed recognizer options.</summary>
    public new OpenCVTemplateCardplateRecognizerOptions Options => (OpenCVTemplateCardplateRecognizerOptions)base.Options;

    /// <inheritdoc/>
    public override Task<CardplateRecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(image);

        try
        {
            using var grayscale = ToGrayscale(image.ToMat());
            if (grayscale.Empty())
            {
                LogEmptyCardplate(Logger);
                return Task.FromResult(new CardplateRecognitionResult());
            }

            using var normalized = NormalizePlate(grayscale);
            using var binary = BinarizePlate(normalized);
            var characterRegions = ExtractCharacterRegions(binary);

            if (characterRegions.Count == 0)
            {
                LogNoCharacterCandidates(Logger);
                return Task.FromResult(new CardplateRecognitionResult());
            }

            var recognizedCharacters = new List<char>(characterRegions.Count);
            var scores = new List<float>(characterRegions.Count);

            foreach (var region in characterRegions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var match = MatchCharacter(binary, region);
                if (match.Score < Options.MinimumCharacterScore)
                {
                    continue;
                }

                recognizedCharacters.Add(match.Character);
                scores.Add(match.Score);
            }

            if (recognizedCharacters.Count < Options.MinimumCharacters || recognizedCharacters.Count > Options.MaximumCharacters)
            {
                LogRecognitionRejectedByLength(Logger, recognizedCharacters.Count);
                return Task.FromResult(new CardplateRecognitionResult());
            }

            var averageScore = scores.Count == 0 ? 0.0f : scores.Average();
            if (averageScore < Options.MinimumRecognitionScore)
            {
                LogRecognitionRejectedByScore(Logger, recognizedCharacters.Count, averageScore);
                return Task.FromResult(new CardplateRecognitionResult());
            }

            LogRecognitionCompleted(Logger, recognizedCharacters.Count, averageScore);
            return Task.FromResult(new CardplateRecognitionResult(new string(recognizedCharacters.ToArray()), averageScore));
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
        foreach (var template in _templates.Values)
        {
            template.Dispose();
        }

        base.DisposeManaged();
    }

    #region Private Helpers
    private Mat NormalizePlate(Mat grayscale)
    {
        var normalizedHeight = Math.Max(1, Options.TargetHeight);
        var normalizedWidth = Math.Max(1, (int)Math.Round(grayscale.Width * (normalizedHeight / (double)Math.Max(1, grayscale.Height))));
        var normalized = new Mat();
        Cv2.Resize(grayscale, normalized, new Size(normalizedWidth, normalizedHeight));
        Cv2.EqualizeHist(normalized, normalized);
        return normalized;
    }

    private Mat BinarizePlate(Mat grayscale)
    {
        var working = grayscale.Clone();
        var blurKernel = EnsureOddKernel(Options.BinaryBlurKernelSize);
        if (blurKernel > 1)
        {
            Cv2.GaussianBlur(working, working, new Size(blurKernel, blurKernel), 0);
        }

        Cv2.Threshold(working, working, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

        var whiteRatio = Cv2.CountNonZero(working) / (double)(working.Rows * working.Cols);
        if (whiteRatio > 0.5)
        {
            Cv2.BitwiseNot(working, working);
        }

        return working;
    }

    private List<OpenCvRect> ExtractCharacterRegions(Mat binary)
    {
        Cv2.FindContours(binary, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        var minHeight = binary.Height * Options.MinimumCharacterHeightRatio;
        var minArea = binary.Width * binary.Height * Options.MinimumCharacterAreaRatio;
        var maxArea = binary.Width * binary.Height * Options.MaximumCharacterAreaRatio;

        return contours
            .Select(Cv2.BoundingRect)
            .Where(rect => rect.Width > 0 && rect.Height > 0)
            .Where(rect => rect.Height >= minHeight)
            .Where(rect =>
            {
                var aspectRatio = rect.Width / (double)rect.Height;
                return aspectRatio >= Options.MinimumCharacterAspectRatio && aspectRatio <= Options.MaximumCharacterAspectRatio;
            })
            .Where(rect =>
            {
                var area = rect.Width * rect.Height;
                return area >= minArea && area <= maxArea;
            })
            .OrderBy(rect => rect.X)
            .ToList();
    }

    private CharacterMatch MatchCharacter(Mat binary, OpenCvRect region)
    {
        var paddedRegion = ExpandRect(region, binary.Width, binary.Height, Options.CharacterPadding);
        using var character = new Mat(binary, paddedRegion);
        using var resized = new Mat();
        Cv2.Resize(character, resized, new Size(Options.TemplateWidth, Options.TemplateHeight));

        char bestCharacter = '?';
        var bestScore = float.MinValue;

        foreach (var template in _templates)
        {
            using var result = new Mat();
            Cv2.MatchTemplate(resized, template.Value, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out var maxScore, out _, out _);

            if (maxScore > bestScore)
            {
                bestScore = (float)maxScore;
                bestCharacter = template.Key;
            }
        }

        return new CharacterMatch(bestCharacter, Math.Max(0.0f, bestScore));
    }

    private static Dictionary<char, Mat> CreateTemplates(OpenCVTemplateCardplateRecognizerOptions options)
    {
        var templates = new Dictionary<char, Mat>();
        foreach (var candidate in options.AllowedCharacters.Where(static c => !char.IsWhiteSpace(c)).Select(static c => char.ToUpperInvariant(c)).Distinct())
        {
            var template = new Mat(new Size(options.TemplateWidth, options.TemplateHeight), MatType.CV_8UC1, Scalar.Black);
            var textSize = Cv2.GetTextSize(candidate.ToString(), HersheyFonts.HersheySimplex, options.FontScale, options.FontThickness, out var baseline);
            var origin = new Point(
                Math.Max(0, (options.TemplateWidth - textSize.Width) / 2),
                Math.Max(textSize.Height, (options.TemplateHeight + textSize.Height) / 2 - baseline));

            Cv2.PutText(template, candidate.ToString(), origin, HersheyFonts.HersheySimplex, options.FontScale, Scalar.White, options.FontThickness, LineTypes.AntiAlias);
            Cv2.Threshold(template, template, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            templates[candidate] = template;
        }

        return templates;
    }

    private static OpenCvRect ExpandRect(OpenCvRect rect, int width, int height, int padding)
    {
        var x = Math.Max(0, rect.X - padding);
        var y = Math.Max(0, rect.Y - padding);
        var right = Math.Min(width, rect.Right + padding);
        var bottom = Math.Min(height, rect.Bottom + padding);
        return new OpenCvRect(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
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

    private static int EnsureOddKernel(int size)
    {
        if (size <= 1)
        {
            return 1;
        }

        return size % 2 == 0 ? size + 1 : size;
    }

    private readonly record struct CharacterMatch(char Character, float Score);
    #endregion

    #region Log Messages
    /// <summary>Emitted when the recognizer receives an empty cardplate crop.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Empty cardplate crop received; skipping recognition.")]
    private static partial void LogEmptyCardplate(ILogger logger);

    /// <summary>Emitted when cardplate segmentation produces no plausible character candidates.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate recognition found no character candidates.")]
    private static partial void LogNoCharacterCandidates(ILogger logger);

    /// <summary>Emitted when the segmented glyph count falls outside the accepted range.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate recognition rejected by character count {CharacterCount}.")]
    private static partial void LogRecognitionRejectedByLength(ILogger logger, int characterCount);

    /// <summary>Emitted when the aggregate recognition score is below the accepted threshold.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate recognition rejected with {CharacterCount} characters because score {RecognitionScore} is below threshold.")]
    private static partial void LogRecognitionRejectedByScore(ILogger logger, int characterCount, float recognitionScore);

    /// <summary>Emitted after successful recognition of a cardplate crop.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Cardplate recognition completed with {CharacterCount} characters and score {RecognitionScore}.")]
    private static partial void LogRecognitionCompleted(ILogger logger, int characterCount, float recognitionScore);

    /// <summary>Emitted when cardplate recognition fails unexpectedly.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Cardplate recognition failed.")]
    private static partial void LogRecognitionFailed(ILogger logger, Exception exception);
    #endregion
}
