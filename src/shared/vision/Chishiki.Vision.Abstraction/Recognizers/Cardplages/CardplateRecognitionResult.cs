// -----------------------------------------------------------------------------
// File:        CardplateRecognitionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the outcome of a cardplate text recognition attempt.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Recognizers.Cardplages;

/// <summary>Represents the outcome of a cardplate text recognition attempt.</summary>
public record CardplateRecognitionResult : RecognitionResult
{
    /// <summary>Initializes a new instance of the <see cref="CardplateRecognitionResult"/> record.</summary>
    /// <param name="text">The raw recognized text, when available. Whitespace-only values are treated as absent. The text is normalized to uppercase.</param>
    /// <param name="score">The normalized confidence score for the recognition attempt.</param>
    public CardplateRecognitionResult(string? text = null, float score = 0.0f)
        : base(NormalizeText(text), score)
    {
        Text = NormalizeText(text);
    }

    /// <summary>Gets the normalized recognized cardplate text, or <see langword="null"/> when recognition was not successful.</summary>
    public string? Text { get; init; }

    /// <summary>Gets a value indicating whether the recognition attempt produced a non-empty cardplate text.</summary>
    public new bool HasRecognition => Text is not null;

    /// <summary>Normalizes the raw recognized text by trimming whitespace and converting to uppercase.</summary>
    private static string? NormalizeText(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim().ToUpperInvariant();
}
