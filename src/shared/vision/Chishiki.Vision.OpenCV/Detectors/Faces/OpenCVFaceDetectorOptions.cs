// -----------------------------------------------------------------------------
// File:        OpenCVFaceDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the OpenCV face detector.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors.Faces;

namespace Chishiki.Vision.OpenCV.Detectors.Faces;

/// <summary>Configuration options for the OpenCV cascade-based face detector.</summary>
public record OpenCVFaceDetectorOptions : FaceDetectorOptions
{
    /// <summary>Gets or sets the path to the cascade classifier XML used to detect faces.</summary>
    public string CascadeModelPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the cascade scale factor. Default is 1.1.</summary>
    public double ScaleFactor { get; set; } = 1.1;

    /// <summary>Gets or sets the minimum number of neighbors required for a retained detection. Default is 5.</summary>
    public int MinNeighbors { get; set; } = 5;

    /// <summary>Gets or sets the minimum face width, in pixels. Default is 48.</summary>
    public int MinFaceWidth { get; set; } = 48;

    /// <summary>Gets or sets the minimum face height, in pixels. Default is 48.</summary>
    public int MinFaceHeight { get; set; } = 48;

    /// <summary>Gets or sets the optional maximum face width, in pixels. Default is 0, which disables the maximum constraint.</summary>
    public int MaxFaceWidth { get; set; }

    /// <summary>Gets or sets the optional maximum face height, in pixels. Default is 0, which disables the maximum constraint.</summary>
    public int MaxFaceHeight { get; set; }

    /// <summary>Gets or sets a value indicating whether histogram equalization is applied before cascade detection. Default is true.</summary>
    public bool EqualizeHistogram { get; set; } = true;

    /// <summary>Gets or sets the recognizer options used when the detector creates its default LBPH recognizer. Default is a new instance.</summary>
    public OpenCVLBPHFaceRecognizerOptions RecognizerOptions { get; set; } = new();
}
