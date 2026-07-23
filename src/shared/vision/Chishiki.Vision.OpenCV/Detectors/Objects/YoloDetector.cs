// -----------------------------------------------------------------------------
// File:        YoloDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: YOLO-specific concrete implementation of OnnxDetector with NMS and standard YOLO output parsing.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Objects;
using Chishiki.Vision.OpenCV.Detectors.Objects;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using Rect = Chishiki.Vision.Abstraction.Models.Rect;

namespace Chishiki.Vision.Onnx.Detector.Objects;

/// <summary>
/// YOLO-specific detector that implements preprocessing (letterbox resize + normalization),
/// postprocessing (decode output tensor + NMS), and optional annotation drawing.
/// Supports YOLOv5, YOLOv8, YOLOv11, and similar architectures with standard output format.
/// </summary>
/// <remarks>
/// Initialises a new <see cref="YoloDetector"/> for YOLO-based object detection.
/// </remarks>
/// <param name="options">Configuration options for the YOLO model.</param>
/// <param name="logger">Logger for diagnostic output.</param>
public sealed partial class YoloDetector(OnnxObjectDetectorOptions options, ILogger<YoloDetector> logger) : OnnxObjectDetector(options, logger)
{
    /// <inheritdoc/>
    protected override async Task<DenseTensor<float>> PreprocessAsync(Mat frame, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogPreprocessingStarted();

        // 1. Resize frame to model input size
        var targetSize = new Size(Options.InputWidth, Options.InputHeight);
        using var resized = new Mat();
        Cv2.Resize(frame, resized, targetSize, interpolation: InterpolationFlags.Linear);

        // 2. Convert BGR to RGB if needed
        using var rgb = new Mat();
        Cv2.CvtColor(resized, rgb, ColorConversionCodes.BGR2RGB);

        // 3. Convert to NCHW tensor with normalization
        var tensor = new DenseTensor<float>([1, 3, Options.InputHeight, Options.InputWidth]);

        var rows = rgb.Rows;
        var cols = rgb.Cols;
        // OpenCV Mat<Vec3b> access for RGB pixel data
        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < cols; x++)
            {
                var pixel = rgb.At<Vec3b>(y, x);

                // Vec3b stores as [R, G, B]
                var r = pixel.Item0;
                var g = pixel.Item1;
                var b = pixel.Item2;

                // NCHW layout: [batch, channel, height, width]
                // Apply normalization: (value / 255.0) * scale + mean
                tensor[0, 0, y, x] = (r * Options.NormalizationScale[0]) + Options.NormalizationMean[0];
                tensor[0, 1, y, x] = (g * Options.NormalizationScale[1]) + Options.NormalizationMean[1];
                tensor[0, 2, y, x] = (b * Options.NormalizationScale[2]) + Options.NormalizationMean[2];
            }
        }

        LogPreprocessingCompleted();
        return await Task.FromResult(tensor);
    }

    /// <inheritdoc/>
    protected override async Task<IReadOnlyList<ObjectDetection>> PostprocessAsync(
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs,
        Mat originalFrame,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogPostprocessingStarted();

        // 1. Extract output tensor (typically "output0" or first output)
        var outputTensor = outputs[0].AsTensor<float>();

        // 2. Parse YOLO output format: [batch, num_detections, 85] or [batch, 84, num_detections]
        //    Standard format: [x_center, y_center, width, height, confidence, class0_prob, class1_prob, ...]
        var detections = new List<ObjectDetection>();

        // Assume shape: [1, num_anchors, num_classes + 5]
        var numDetections = outputTensor.Dimensions[1];
        var numFields = outputTensor.Dimensions[2];
        var numClasses = numFields - 5;

        for (var i = 0; i < numDetections; i++)
        {
            // Extract bbox and confidence
            var xCenter = outputTensor[0, i, 0];
            var yCenter = outputTensor[0, i, 1];
            var width = outputTensor[0, i, 2];
            var height = outputTensor[0, i, 3];
            var objectness = outputTensor[0, i, 4];

            if (objectness < Options.ConfidenceThreshold)
            {
                continue;
            }

            // Find best class
            var maxClassScore = 0f;
            var bestClassId = 0;
            for (var c = 0; c < numClasses; c++)
            {
                var classScore = outputTensor[0, i, 5 + c];
                if (classScore > maxClassScore)
                {
                    maxClassScore = classScore;
                    bestClassId = c;
                }
            }

            var finalScore = objectness * maxClassScore;
            if (finalScore < Options.ConfidenceThreshold)
            {
                continue;
            }

            // Convert from center coords to top-left coords
            var x1 = (int)(xCenter - (width / 2));
            var y1 = (int)(yCenter - (height / 2));
            var w = (int)width;
            var h = (int)height;

            detections.Add(new ObjectDetection(
                bestClassId,
                finalScore,
                new Rect(new Abstraction.Models.Point(x1, y1), new Abstraction.Models.Size(w, h))));
        }

        // 3. Apply Non-Maximum Suppression
        var nmsDetections = detections.ApplyNms(Options.IouThreshold);

        LogPostprocessingCompleted(nmsDetections.Count);
        return await Task.FromResult(nmsDetections.Take(Options.MaxDetections).ToList().AsReadOnly());
    }

    /// <summary>
    /// Applies Non-Maximum Suppression (NMS) to filter overlapping detections.
    /// </summary>
    /// <param name="image">The image for which to create an empty detection result.</param>
    /// <returns>An empty detection result.</returns>
    protected override ObjectDetectionResult CreateEmptyResult(IImage image) => new(image);

    // -------------------------------------------------------------------------
    // Compile-time logging
    // -------------------------------------------------------------------------

    /// <summary>Emitted at the start of preprocessing.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "YOLO preprocessing started.")]
    private partial void LogPreprocessingStarted();

    /// <summary>Emitted after preprocessing completes.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "YOLO preprocessing completed.")]
    private partial void LogPreprocessingCompleted();

    /// <summary>Emitted at the start of postprocessing.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "YOLO postprocessing started.")]
    private partial void LogPostprocessingStarted();

    /// <summary>Emitted after postprocessing and NMS complete.</summary>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "YOLO postprocessing completed: {DetectionCount} objects after NMS.")]
    private partial void LogPostprocessingCompleted(int detectionCount);
}

public static class YoloDetectorExtensions
{
    /// <summary>
    /// Applies Non-Maximum Suppression to filter overlapping detections.
    /// </summary>
    /// <param name="detections">The list of candidate detections.</param>
    /// <param name="iouThreshold">The IoU threshold for suppression.</param>
    /// <returns>A filtered list of detections after NMS.</returns>
    public static List<ObjectDetection> ApplyNms(this List<ObjectDetection> detections, float iouThreshold)
    {
        if (detections.Count == 0)
        {
            return [];
        }

        // Sort by score descending
        var sorted = detections.OrderByDescending(d => d.Score).ToList();
        var keep = new List<ObjectDetection>();

        while (sorted.Count > 0)
        {
            var current = sorted[0];
            keep.Add(current);
            sorted.RemoveAt(0);

            sorted = [.. sorted.Where(d => ComputeIoU(current.Rect, d.Rect) < iouThreshold)];
        }

        return keep;
    }

    /// <summary>
    /// Computes the Intersection over Union (IoU) between two bounding boxes.
    /// </summary>
    /// <param name="a">First bounding box.</param>
    /// <param name="b">Second bounding box.</param>
    /// <returns>The IoU value between 0.0 and 1.0.</returns>
    public static float ComputeIoU(this Rect a, Rect b)
    {
        var x1 = Math.Max(a.X, b.X);
        var y1 = Math.Max(a.Y, b.Y);
        var x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        var y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

        var intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        var areaA = a.Width * a.Height;
        var areaB = b.Width * b.Height;
        var unionArea = areaA + areaB - intersectionArea;

        return unionArea > 0 ? (float)intersectionArea / unionArea : 0f;
    }
}
