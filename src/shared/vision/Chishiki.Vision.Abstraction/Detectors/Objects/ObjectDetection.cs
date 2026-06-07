// -----------------------------------------------------------------------------
// File:        ObjectDetection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a detected object within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors.Objects;

/// <summary>
/// Represents a detected object within a frame, including its bounding region, class identifier, and confidence score.
/// </summary>
public record ObjectDetection : Detection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectDetection"/> record with the specified class identifier, confidence score, and bounding rectangle.
    /// </summary>
    /// <param name="classId">The class identifier of the detected object.</param>
    /// <param name="score">The confidence score of the detected object.</param>
    /// <param name="rect">The bounding rectangle of the detected object.</param>
    /// <param name="area">The area of the detected object. If not specified, defaults to -1.</param>
    public ObjectDetection(int classId, float score, Rect rect, double area = -1) : base(rect, area)
    {
        ClassId = classId;
        Score = score;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectDetection"/> record with the specified class identifier, confidence score, bounding point, and size.
    /// </summary>
    /// <param name="classId">The class identifier of the detected object.</param>
    /// <param name="score">The confidence score of the detected object.</param>
    /// <param name="point">The top-left corner of the bounding rectangle.</param>
    /// <param name="size">The size of the bounding rectangle.</param>
    /// <param name="area">The area of the detected object. If not specified, defaults to -1.</param>
    public ObjectDetection(int classId, float score, Point point, Size size, double area = -1) : base(point, size, area)
    {
        ClassId = classId;
        Score = score;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectDetection"/> record with the specified class identifier, confidence score, and bounding rectangle defined by its top-left corner and dimensions.
    /// </summary>
    /// <param name="classId">The class identifier of the detected object.</param>
    /// <param name="score">The confidence score of the detected object.</param>
    /// <param name="x">The x-coordinate of the top-left corner of the bounding rectangle.</param>
    /// <param name="y">The y-coordinate of the top-left corner of the bounding rectangle.</param>
    /// <param name="width">The width of the bounding rectangle.</param>
    /// <param name="height">The height of the bounding rectangle.</param>
    /// <param name="area"></param>
    public ObjectDetection(int classId, float score, int x, int y, int width, int height, double area = -1) : base(x, y, width, height, area)
    {
        ClassId = classId;
        Score = score;
    }

    /// <summary>
    /// Gets the class identifier of the detected object, which corresponds to a specific category or label defined in the object detection model's class mapping.
    /// </summary>
    public int ClassId { get; init; }

    /// <summary>
    /// Gets the confidence score of the detected object, representing the model's certainty that the detection is correct. The score is typically a value between 0.0 and 1.0, where higher values indicate greater confidence in the detection.
    /// </summary>
    public float Score { get; init; }
}
