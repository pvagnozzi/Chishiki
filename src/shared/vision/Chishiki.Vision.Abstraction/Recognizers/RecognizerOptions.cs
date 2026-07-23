// -----------------------------------------------------------------------------
// File:        FaceRecognizerOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for face recognition algorithms.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Recognizers;

/// <summary>Defines the common configuration used by face recognition algorithms.</summary>
public record RecognizerOptions
{
    /// <summary>Gets or sets the path to the face-recognition model used by the recognizer.</summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the maximum prediction distance accepted as a valid recognition. Default is 65.</summary>
    public double MaximumDistance { get; set; } = 65.0;

    /// <summary>Gets or sets the normalized width, in pixels, used before recognition. Default is 128.</summary>
    public int TargetWidth { get; set; } = 128;

    /// <summary>Gets or sets the normalized height, in pixels, used before recognition. Default is 128.</summary>
    public int TargetHeight { get; set; } = 128;
}
