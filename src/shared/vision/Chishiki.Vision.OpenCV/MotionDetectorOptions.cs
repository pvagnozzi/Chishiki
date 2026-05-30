// -----------------------------------------------------------------------------
// File:        MotionDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the OpenCV MOG2-based motion detector.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.OpenCV;

/// <summary>Configuration options for the <see cref="MotionDetector"/> algorithm.</summary>
public sealed class MotionDetectorOptions
{
    /// <summary>Gets or sets the minimum contour area in pixels to be considered a valid motion region. Default is 500.</summary>
    public double MinContourArea { get; set; } = 500.0;

    /// <summary>Gets or sets the Gaussian blur kernel size (must be odd). Default is 5.</summary>
    public int GaussianKernelSize { get; set; } = 5;

    /// <summary>Gets or sets the number of history frames used by the MOG2 background subtractor. Default is 500.</summary>
    public int BackgroundHistory { get; set; } = 500;

    /// <summary>Gets or sets the MOG2 threshold on the squared Mahalanobis distance to classify a pixel as foreground. Default is 16.</summary>
    public double Mog2Threshold { get; set; } = 16.0;

    /// <summary>Gets or sets a value indicating whether MOG2 should detect shadows. Default is false.</summary>
    public bool DetectShadows { get; set; } = false;

    /// <summary>Gets or sets the morphological erosion kernel size used for noise removal. Default is 3.</summary>
    public int ErodeKernelSize { get; set; } = 3;

    /// <summary>Gets or sets the morphological dilation kernel size used to fill gaps in motion masks. Default is 7.</summary>
    public int DilateKernelSize { get; set; } = 7;

    /// <summary>Gets or sets the number of erosion iterations. Default is 2.</summary>
    public int ErodeIterations { get; set; } = 2;

    /// <summary>Gets or sets the number of dilation iterations. Default is 2.</summary>
    public int DilateIterations { get; set; } = 2;

    /// <summary>Gets or sets the BGR colour of the rectangle drawn around each detected motion region. Default is green (0, 255, 0).</summary>
    public (int B, int G, int R) HighlightColour { get; set; } = (0, 255, 0);

    /// <summary>Gets or sets the thickness in pixels of the rectangle drawn around motion regions. Default is 2.</summary>
    public int HighlightThickness { get; set; } = 2;
}
