// -----------------------------------------------------------------------------
// File:        OnnxObjectDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for ONNX-based object detectors including YOLO models.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors;

namespace Chishiki.Vision.OpenCV.Detectors.Objects;

/// <summary>Configuration options for ONNX Runtime-based object detection models.</summary>
public record OnnxObjectDetectorOptions : DetectorOptions
{
    /// <summary>Gets or sets the ONNX model file path. This is a required field.</summary>
    public required string ModelPath { get; set; }

    /// <summary>Gets or sets the input tensor width in pixels. Default is 640.</summary>
    public int InputWidth { get; set; } = 640;

    /// <summary>Gets or sets the input tensor height in pixels. Default is 640.</summary>
    public int InputHeight { get; set; } = 640;

    /// <summary>Gets or sets the minimum confidence threshold for accepting detections. Default is 0.25.</summary>
    public float ConfidenceThreshold { get; set; } = 0.25f;

    /// <summary>Gets or sets the IoU (Intersection over Union) threshold for Non-Maximum Suppression. Default is 0.45.</summary>
    public float IouThreshold { get; set; } = 0.45f;

    /// <summary>Gets or sets the maximum number of detections to return per image. Default is 300.</summary>
    public int MaxDetections { get; set; } = 300;

    /// <summary>Gets or sets the list of class names corresponding to the model's output indices. If not set, numeric IDs will be used.</summary>
    public IReadOnlyList<string>? ClassNames { get; set; }

    /// <summary>Gets or sets the mean values for RGB normalization. Default is [0.0, 0.0, 0.0] (no mean subtraction).</summary>
    public float[] NormalizationMean { get; set; } = [0.0f, 0.0f, 0.0f];

    /// <summary>Gets or sets the scale factors for RGB normalization. Default is [1.0/255, 1.0/255, 1.0/255].</summary>
    public float[] NormalizationScale { get; set; } = [1.0f / 255, 1.0f / 255, 1.0f / 255];

    /// <summary>Gets or sets a value indicating whether to use GPU acceleration if available. Default is false (CPU only).</summary>
    public bool UseGpu { get; set; }

    /// <summary>Gets or sets the number of intra-operation threads used by ONNX Runtime. Default is 0 (auto).</summary>
    public int IntraOpNumThreads { get; set; }

    /// <summary>Gets or sets the number of inter-operation threads used by ONNX Runtime. Default is 0 (auto).</summary>
    public int InterOpNumThreads { get; set; }
}
