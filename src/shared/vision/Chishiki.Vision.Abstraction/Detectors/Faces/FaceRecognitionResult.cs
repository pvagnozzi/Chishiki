// -----------------------------------------------------------------------------
// File:        FaceRecognitionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the outcome of a face recognition attempt.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Represents the outcome of a face recognition attempt.</summary>
public record FaceRecognitionResult
{
    /// <summary>Initializes a new instance of the <see cref="FaceRecognitionResult"/> record.</summary>
    /// <param name="labelId">The numeric label predicted by the recognizer, when available.</param>
    /// <param name="identity">The human-readable identity associated with the prediction, when available.</param>
    /// <param name="score">The normalized confidence score associated with the recognition attempt.</param>
    /// <param name="distance">The raw prediction distance reported by the recognizer, when available.</param>
    public FaceRecognitionResult(int? labelId = null, string? identity = null, float score = 0.0f, double? distance = null)
    {
        LabelId = labelId;
        Identity = string.IsNullOrWhiteSpace(identity) ? null : identity.Trim();
        Score = score;
        Distance = distance;
    }

    /// <summary>Gets the numeric label predicted by the recognizer, when available.</summary>
    public int? LabelId { get; init; }

    /// <summary>Gets the human-readable identity associated with the prediction, when available.</summary>
    public string? Identity { get; init; }

    /// <summary>Gets the normalized confidence score associated with the recognition attempt.</summary>
    public float Score { get; init; }

    /// <summary>Gets the raw prediction distance reported by the recognizer, when available.</summary>
    public double? Distance { get; init; }

    /// <summary>Gets a value indicating whether the recognition attempt produced a recognized identity.</summary>
    public bool HasRecognition => LabelId.HasValue || !string.IsNullOrWhiteSpace(Identity);
}
