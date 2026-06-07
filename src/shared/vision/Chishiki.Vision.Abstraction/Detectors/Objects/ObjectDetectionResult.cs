// -----------------------------------------------------------------------------
// File:        ObjectDetectionResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the result of an object detection operation within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Objects;

/// <summary>
/// ObjectDetectionResult represents the result of an object detection operation, containing the original frame, an optional annotated frame, a collection of detected objects, and a timestamp. It inherits from DetectionResult<ObjectDetection>, allowing it to encapsulate the specific details of object detections while maintaining a consistent structure for detection results across different types of detectors.
/// </summary>
public record ObjectDetectionResult : DetectionResult<ObjectDetection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectDetectionResult"/> record with the specified original frame, optional annotated frame, collection of detected objects, and timestamp.
    /// </summary>
    /// <param name="originalFrame">The original frame captured from the video source.</param>
    /// <param name="annotatedFrame">An optional frame with annotations, such as bounding boxes or labels, applied to the detected objects.</param>
    /// <param name="detections">A collection of detected objects within the frame.</param>
    /// <param name="timestamp">The timestamp indicating when the frame was captured.</param>
    public ObjectDetectionResult(
        IImage originalFrame,
        IImage? annotatedFrame = null,
        IEnumerable<ObjectDetection>? detections = null,
        DateTimeOffset? timestamp = null) : base(originalFrame, annotatedFrame, detections, timestamp)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectDetectionResult"/> record by copying the properties from an existing <see cref="DetectionResult{ObjectDetection}"/> instance. This constructor allows for easy conversion from a generic detection result to a specific object detection result, preserving all relevant information while providing a more specialized type.
    /// </summary>
    /// <param name="original">The original detection result to copy.</param>
    protected ObjectDetectionResult(DetectionResult<ObjectDetection> original) : base(original)
    {
    }
}
