// -----------------------------------------------------------------------------
// File:        FaceDetection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a detected face region and its optional recognition metadata.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Represents a detected face region within a video frame, including optional recognition metadata.</summary>
public record FaceDetection : Detection
{
    /// <summary>Initializes a new instance of the <see cref="FaceDetection"/> record.</summary>
    /// <param name="rect">The bounding rectangle of the detected face.</param>
    /// <param name="detectionScore">The confidence score for the geometric detection step.</param>
    /// <param name="labelId">The recognized label identifier, when available.</param>
    /// <param name="identity">The recognized identity name, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when available.</param>
    /// <param name="distance">The raw prediction distance reported by the recognizer, when available.</param>
    /// <param name="area">The area of the detected region. Defaults to rectangle area when negative.</param>
    public FaceDetection(
        Rect rect,
        float detectionScore = 0.0f,
        string? labelId = null,
        string? identity = null,
        float? recognitionScore = null,
        double? distance = null,
        double area = -1)
        : base(rect, detectionScore, area)
    {
        LabelId = labelId;
        Identity = identity;
        RecognitionScore = recognitionScore;
        Distance = distance;
    }

    /// <summary>Gets the numeric label identifier assigned by the recognizer, when available.</summary>
    public string? LabelId { get; init; }

    /// <summary>Gets the recognized identity name, when available.</summary>
    public string? Identity { get; init; }

    /// <summary>Gets the recognition confidence score, when available.</summary>
    public float? RecognitionScore { get; init; }

    /// <summary>Gets the raw prediction distance reported by the recognizer, when available.</summary>
    public double? Distance { get; init; }

    /// <summary>Gets a value indicating whether this detection produced a recognized identity.</summary>
    public bool HasRecognition => Identity is not null || LabelId is not null;
}
