// -----------------------------------------------------------------------------
// File:        CardplateRecognizerOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for cardplate recognition algorithms.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Defines the common configuration used by cardplate recognition algorithms.</summary>
public record CardplateRecognizerOptions
{
    /// <summary>Gets or sets the ordered set of characters that can be emitted by the recognizer. Default is the Latin uppercase alphabet plus digits.</summary>
    public string AllowedCharacters { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>Gets or sets the minimum number of characters required for a recognition result to be accepted. Default is 4.</summary>
    public int MinimumCharacters { get; set; } = 4;

    /// <summary>Gets or sets the maximum number of characters allowed for a recognition result to be accepted. Default is 10.</summary>
    public int MaximumCharacters { get; set; } = 10;

    /// <summary>Gets or sets the minimum per-character matching score required to keep a segmented glyph. Default is 0.35.</summary>
    public float MinimumCharacterScore { get; set; } = 0.35f;

    /// <summary>Gets or sets the minimum average recognition score required to accept the final text. Default is 0.50.</summary>
    public float MinimumRecognitionScore { get; set; } = 0.50f;
}
