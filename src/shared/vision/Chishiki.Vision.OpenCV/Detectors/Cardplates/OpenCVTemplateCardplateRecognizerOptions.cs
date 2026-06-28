// -----------------------------------------------------------------------------
// File:        OpenCVTemplateCardplateRecognizerOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for the OpenCV template-based cardplate recognizer.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Recognizers.Cardplages;

namespace Chishiki.Vision.OpenCV.Detectors.Cardplates;

/// <summary>Configuration options for the OpenCV template-based cardplate recognizer.</summary>
public record OpenCVTemplateCardplateRecognizerOptions : CardplateRecognizerOptions
{
    /// <summary>Gets or sets the optional Gaussian blur kernel size applied before binarization. Default is 3.</summary>
    public int BinaryBlurKernelSize { get; set; } = 3;

    /// <summary>Gets or sets the width, in pixels, of each generated character template. Default is 24.</summary>
    public int TemplateWidth { get; set; } = 24;

    /// <summary>Gets or sets the height, in pixels, of each generated character template. Default is 36.</summary>
    public int TemplateHeight { get; set; } = 36;

    /// <summary>Gets or sets the font scale used to generate OpenCV character templates. Default is 1.0.</summary>
    public double FontScale { get; set; } = 1.0;

    /// <summary>Gets or sets the font thickness used to generate OpenCV character templates. Default is 2.</summary>
    public int FontThickness { get; set; } = 2;

    /// <summary>Gets or sets the minimum width-to-height ratio accepted for segmented character contours. Default is 0.15.</summary>
    public float MinimumCharacterAspectRatio { get; set; } = 0.15f;

    /// <summary>Gets or sets the maximum width-to-height ratio accepted for segmented character contours. Default is 1.0.</summary>
    public float MaximumCharacterAspectRatio { get; set; } = 1.0f;

    /// <summary>Gets or sets the minimum contour height relative to the normalized cardplate height. Default is 0.35.</summary>
    public float MinimumCharacterHeightRatio { get; set; } = 0.35f;

    /// <summary>Gets or sets the minimum contour area relative to the normalized cardplate area. Default is 0.01.</summary>
    public float MinimumCharacterAreaRatio { get; set; } = 0.01f;

    /// <summary>Gets or sets the maximum contour area relative to the normalized cardplate area. Default is 0.20.</summary>
    public float MaximumCharacterAreaRatio { get; set; } = 0.20f;

    /// <summary>Gets or sets the padding, in pixels, added around each segmented glyph before template matching. Default is 2.</summary>
    public int CharacterPadding { get; set; } = 2;
}
