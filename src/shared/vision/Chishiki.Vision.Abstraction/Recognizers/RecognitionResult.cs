// -----------------------------------------------------------------------------
// File:        RecognitionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the outcome of a recognition attempt.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Recognizers;

/// <summary>Represents the outcome of a recognition attempt.</summary>
public record RecognitionResult
{
    /// <summary>Initializes a new instance of the <see cref="RecognitionResult"/> record.</summary>
    /// <param name="labelId">The numeric label predicted by the recognizer, when available.</param>    
    /// <param name="score">The normalized confidence score associated with the recognition attempt.</param>
    /// <param name="distance">The raw prediction distance reported by the recognizer, when available.</param>
    public RecognitionResult(string? labelId = null, float score = 0.0f, double? distance = null)
    {
        LabelId = labelId;
        Score = score;
        Distance = distance;
    }

    /// <summary>Gets the numeric label predicted by the recognizer, when available.</summary>
    public string? LabelId { get; init; }

    /// <summary>Gets the normalized confidence score associated with the recognition attempt.</summary>
    public float Score { get; init; }

    /// <summary>Gets the raw prediction distance reported by the recognizer, when available.</summary>
    public double? Distance { get; init; }

    /// <summary>Gets a value indicating whether the recognition attempt produced a non-empty text.</summary>
    public bool HasRecognition => !string.IsNullOrWhiteSpace(LabelId);
}
