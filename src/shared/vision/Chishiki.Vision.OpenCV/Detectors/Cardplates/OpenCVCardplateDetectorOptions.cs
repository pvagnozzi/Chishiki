// -----------------------------------------------------------------------------
// File:        OpenCVCardplateDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the OpenCV-based cardplate detector.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors.Cardplates;

namespace Chishiki.Vision.OpenCV.Detectors.Cardplates;

/// <summary>Configuration options for the OpenCV-based cardplate detector.</summary>
public record OpenCVCardplateDetectorOptions : CardplateDetectorOptions
{
    /// <summary>Gets or sets the Gaussian blur kernel size applied before thresholding. Default is 5.</summary>
    public int GaussianKernelSize { get; set; } = 5;

    /// <summary>Gets or sets the odd block size used by adaptive thresholding. Default is 31.</summary>
    public int AdaptiveThresholdBlockSize { get; set; } = 31;

    /// <summary>Gets or sets the subtraction constant used by adaptive thresholding. Default is 9.</summary>
    public double AdaptiveThresholdC { get; set; } = 9.0;

    /// <summary>Gets or sets the width, in pixels, of the morphology kernel used to consolidate cardplate candidates. Default is 17.</summary>
    public int MorphologyKernelWidth { get; set; } = 17;

    /// <summary>Gets or sets the height, in pixels, of the morphology kernel used to consolidate cardplate candidates. Default is 5.</summary>
    public int MorphologyKernelHeight { get; set; } = 5;

    /// <summary>Gets or sets the number of morphology close iterations applied after thresholding. Default is 1.</summary>
    public int MorphologyIterations { get; set; } = 1;

    /// <summary>Gets or sets the recognizer options used when the detector creates its default template-based recognizer. Default is a new instance.</summary>
    public OpenCVTemplateCardplateRecognizerOptions RecognizerOptions { get; set; } = new();
}
