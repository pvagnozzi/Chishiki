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
public record ObjectDetection : Detectors.Detection
{
    public ObjectDetection(int classId, float score, Rect rect, double area = -1) : base(rect, area)
    {
        ClassId = classId;
        Score = score;
    }

    public ObjectDetection(int classId, float score, Point point, Size size, double area = -1) : base(point, size, area)
    {
        ClassId = classId;
        Score = score;
    }

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
