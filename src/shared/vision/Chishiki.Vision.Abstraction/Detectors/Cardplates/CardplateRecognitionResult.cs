// -----------------------------------------------------------------------------
// File:        CardplateRecognitionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the outcome of a cardplate text recognition attempt.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Represents the outcome of a cardplate text recognition attempt.</summary>
public record CardplateRecognitionResult
{
    /// <summary>Initializes a new instance of the <see cref="CardplateRecognitionResult"/> record.</summary>
    /// <param name="text">The recognized cardplate text, when available.</param>
    /// <param name="score">The aggregate confidence score of the recognition attempt.</param>
    public CardplateRecognitionResult(string? text = null, float score = 0.0f)
    {
        Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim().ToUpperInvariant();
        Score = score;
    }

    /// <summary>Gets the recognized cardplate text, when recognition succeeds.</summary>
    public string? Text { get; init; }

    /// <summary>Gets the aggregate confidence score associated with the recognition attempt.</summary>
    public float Score { get; init; }

    /// <summary>Gets a value indicating whether the recognition attempt produced a non-empty text.</summary>
    public bool HasRecognition => !string.IsNullOrWhiteSpace(Text);
}
