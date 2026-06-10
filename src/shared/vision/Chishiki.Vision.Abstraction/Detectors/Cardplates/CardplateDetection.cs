// -----------------------------------------------------------------------------
// File:        CardplateDetection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a detected vehicle cardplate region within an image frame.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Represents a detected vehicle cardplate region and its optional recognition metadata.</summary>
public record CardplateDetection : Detection
{
    /// <summary>Initializes a new instance of the <see cref="CardplateDetection"/> record with the specified bounding rectangle and recognition metadata.</summary>
    /// <param name="rect">The detected cardplate rectangle.</param>
    /// <param name="detectionScore">The geometric confidence score assigned to the detection.</param>
    /// <param name="recognizedText">The recognized cardplate text, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when text recognition succeeds.</param>
    /// <param name="area">The area of the detected cardplate region. If not provided, it is derived from the rectangle dimensions.</param>
    public CardplateDetection(Rect rect, float detectionScore = 0.0f, string? recognizedText = null, float? recognitionScore = null, double area = -1)
        : base(rect, area)
    {
        DetectionScore = detectionScore;
        RecognizedText = NormalizeText(recognizedText);
        RecognitionScore = recognitionScore;
    }

    /// <summary>Initializes a new instance of the <see cref="CardplateDetection"/> record with the specified position, size, and recognition metadata.</summary>
    /// <param name="point">The top-left corner of the detected cardplate.</param>
    /// <param name="size">The size of the detected cardplate region.</param>
    /// <param name="detectionScore">The geometric confidence score assigned to the detection.</param>
    /// <param name="recognizedText">The recognized cardplate text, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when text recognition succeeds.</param>
    /// <param name="area">The area of the detected cardplate region. If not provided, it is derived from the size.</param>
    public CardplateDetection(Point point, Size size, float detectionScore = 0.0f, string? recognizedText = null, float? recognitionScore = null, double area = -1)
        : base(point, size, area)
    {
        DetectionScore = detectionScore;
        RecognizedText = NormalizeText(recognizedText);
        RecognitionScore = recognitionScore;
    }

    /// <summary>Initializes a new instance of the <see cref="CardplateDetection"/> record with the specified coordinates, dimensions, and recognition metadata.</summary>
    /// <param name="x">The x-coordinate of the detected cardplate.</param>
    /// <param name="y">The y-coordinate of the detected cardplate.</param>
    /// <param name="width">The width of the detected cardplate.</param>
    /// <param name="height">The height of the detected cardplate.</param>
    /// <param name="detectionScore">The geometric confidence score assigned to the detection.</param>
    /// <param name="recognizedText">The recognized cardplate text, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when text recognition succeeds.</param>
    /// <param name="area">The area of the detected cardplate region. If not provided, it is derived from the dimensions.</param>
    public CardplateDetection(int x, int y, int width, int height, float detectionScore = 0.0f, string? recognizedText = null, float? recognitionScore = null, double area = -1)
        : base(x, y, width, height, area)
    {
        DetectionScore = detectionScore;
        RecognizedText = NormalizeText(recognizedText);
        RecognitionScore = recognitionScore;
    }

    /// <summary>Gets the geometric confidence score assigned to this detection.</summary>
    public float DetectionScore { get; init; }

    /// <summary>Gets the recognized cardplate text, when recognition succeeds.</summary>
    public string? RecognizedText { get; init; }

    /// <summary>Gets the recognition confidence score associated with <see cref="RecognizedText"/>, when available.</summary>
    public float? RecognitionScore { get; init; }

    /// <summary>Gets a value indicating whether this detection contains a recognized cardplate text.</summary>
    public bool HasRecognition => !string.IsNullOrWhiteSpace(RecognizedText);

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
