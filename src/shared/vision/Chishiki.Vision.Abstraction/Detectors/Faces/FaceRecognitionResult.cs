// -----------------------------------------------------------------------------
// File:        FaceRecognitionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the outcome of a face recognition attempt.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Globalization;
using Chishiki.Vision.Abstraction.Recognizers;

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Represents the outcome of a face recognition attempt, including optional identity and label information.</summary>
public record FaceRecognitionResult : RecognitionResult
{
    /// <summary>Initializes a new instance of the <see cref="FaceRecognitionResult"/> record.</summary>
    /// <param name="identity">The recognized identity name, when available. Whitespace-only values are treated as absent.</param>
    /// <param name="labelId">The numeric label predicted by the recognizer, when available.</param>
    /// <param name="score">The normalized confidence score for the recognition attempt.</param>
    /// <param name="distance">The raw prediction distance reported by the recognizer, when available.</param>
    public FaceRecognitionResult(string? identity = null, int? labelId = null, float score = 0.0f, double? distance = null)
        : base(labelId?.ToString(CultureInfo.InvariantCulture), score, distance)
    {
        Identity = string.IsNullOrWhiteSpace(identity) ? null : identity.Trim();
    }

    /// <summary>Gets the recognized identity name, or <see langword="null"/> when recognition was not successful.</summary>
    public string? Identity { get; init; }

    /// <summary>Gets a value indicating whether the recognition attempt produced an identity or a label.</summary>
    public new bool HasRecognition => Identity is not null || !string.IsNullOrWhiteSpace(LabelId);
}
