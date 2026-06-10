// -----------------------------------------------------------------------------
// File:        FaceDetection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a detected face region within an image frame.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Represents a detected face region and its optional recognition metadata.</summary>
public record FaceDetection : Detection
{
    /// <summary>Initializes a new instance of the <see cref="FaceDetection"/> record with the specified bounding rectangle and recognition metadata.</summary>
    /// <param name="rect">The detected face rectangle.</param>
    /// <param name="detectionScore">The confidence score assigned to the detection.</param>
    /// <param name="labelId">The numeric label predicted by the recognizer, when available.</param>
    /// <param name="identity">The human-readable identity associated with the prediction, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when available.</param>
    /// <param name="recognitionDistance">The raw recognizer distance, when available.</param>
    /// <param name="area">The area of the detected face region. If not provided, it is derived from the rectangle dimensions.</param>
    public FaceDetection(Rect rect, float detectionScore = 0.0f, int? labelId = null, string? identity = null, float? recognitionScore = null, double? recognitionDistance = null, double area = -1)
        : base(rect, area)
    {
        DetectionScore = detectionScore;
        LabelId = labelId;
        Identity = string.IsNullOrWhiteSpace(identity) ? null : identity.Trim();
        RecognitionScore = recognitionScore;
        RecognitionDistance = recognitionDistance;
    }

    /// <summary>Initializes a new instance of the <see cref="FaceDetection"/> record with the specified position, size, and recognition metadata.</summary>
    /// <param name="point">The top-left corner of the detected face.</param>
    /// <param name="size">The size of the detected face region.</param>
    /// <param name="detectionScore">The confidence score assigned to the detection.</param>
    /// <param name="labelId">The numeric label predicted by the recognizer, when available.</param>
    /// <param name="identity">The human-readable identity associated with the prediction, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when available.</param>
    /// <param name="recognitionDistance">The raw recognizer distance, when available.</param>
    /// <param name="area">The area of the detected face region. If not provided, it is derived from the size.</param>
    public FaceDetection(Point point, Size size, float detectionScore = 0.0f, int? labelId = null, string? identity = null, float? recognitionScore = null, double? recognitionDistance = null, double area = -1)
        : base(point, size, area)
    {
        DetectionScore = detectionScore;
        LabelId = labelId;
        Identity = string.IsNullOrWhiteSpace(identity) ? null : identity.Trim();
        RecognitionScore = recognitionScore;
        RecognitionDistance = recognitionDistance;
    }

    /// <summary>Initializes a new instance of the <see cref="FaceDetection"/> record with the specified coordinates, dimensions, and recognition metadata.</summary>
    /// <param name="x">The x-coordinate of the detected face.</param>
    /// <param name="y">The y-coordinate of the detected face.</param>
    /// <param name="width">The width of the detected face.</param>
    /// <param name="height">The height of the detected face.</param>
    /// <param name="detectionScore">The confidence score assigned to the detection.</param>
    /// <param name="labelId">The numeric label predicted by the recognizer, when available.</param>
    /// <param name="identity">The human-readable identity associated with the prediction, when available.</param>
    /// <param name="recognitionScore">The recognition confidence score, when available.</param>
    /// <param name="recognitionDistance">The raw recognizer distance, when available.</param>
    /// <param name="area">The area of the detected face region. If not provided, it is derived from the dimensions.</param>
    public FaceDetection(int x, int y, int width, int height, float detectionScore = 0.0f, int? labelId = null, string? identity = null, float? recognitionScore = null, double? recognitionDistance = null, double area = -1)
        : base(x, y, width, height, area)
    {
        DetectionScore = detectionScore;
        LabelId = labelId;
        Identity = string.IsNullOrWhiteSpace(identity) ? null : identity.Trim();
        RecognitionScore = recognitionScore;
        RecognitionDistance = recognitionDistance;
    }

    /// <summary>Gets the confidence score assigned to this detection.</summary>
    public float DetectionScore { get; init; }

    /// <summary>Gets the numeric label predicted by the recognizer, when available.</summary>
    public int? LabelId { get; init; }

    /// <summary>Gets the human-readable identity associated with the prediction, when available.</summary>
    public string? Identity { get; init; }

    /// <summary>Gets the recognition confidence score, when available.</summary>
    public float? RecognitionScore { get; init; }

    /// <summary>Gets the raw recognizer distance, when available.</summary>
    public double? RecognitionDistance { get; init; }

    /// <summary>Gets a value indicating whether this detection contains a recognized identity.</summary>
    public bool HasRecognition => LabelId.HasValue || !string.IsNullOrWhiteSpace(Identity);
}
