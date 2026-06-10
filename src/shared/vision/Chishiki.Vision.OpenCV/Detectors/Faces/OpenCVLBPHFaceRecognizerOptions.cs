// -----------------------------------------------------------------------------
// File:        OpenCVLBPHFaceRecognizerOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the OpenCV LBPH face recognizer.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors.Faces;

namespace Chishiki.Vision.OpenCV.Detectors.Faces;

/// <summary>Configuration options for the OpenCV LBPH face recognizer.</summary>
public record OpenCVLBPHFaceRecognizerOptions : FaceRecognizerOptions
{
    /// <summary>Gets or sets the LBPH radius value. Default is 1.</summary>
    public int Radius { get; set; } = 1;

    /// <summary>Gets or sets the LBPH neighbors value. Default is 8.</summary>
    public int Neighbors { get; set; } = 8;

    /// <summary>Gets or sets the LBPH grid width. Default is 8.</summary>
    public int GridX { get; set; } = 8;

    /// <summary>Gets or sets the LBPH grid height. Default is 8.</summary>
    public int GridY { get; set; } = 8;

    /// <summary>Gets or sets a value indicating whether histogram equalization is applied before prediction. Default is true.</summary>
    public bool EqualizeHistogram { get; set; } = true;
}
