// -----------------------------------------------------------------------------
// File:        ComposedObjectDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Composed object detector class.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Objects;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Objects;

/// <summary>
/// Object detector that composes multiple object detectors together. This allows for running multiple detectors on the same frame and aggregating their results.
/// </summary>
/// <param name="options">The options for the detector.</param>
/// <param name="detectors">The collection of object detectors to compose.</param>
/// <param name="logger">The logger for the detector.</param>
public class ComposedObjectDetector(DetectorOptions options, IEnumerable<IObjectDetector> detectors, ILogger<ComposedObjectDetector> logger) : ObjectDetector(options, logger)
{
    /// <summary>
    /// The collection of object detectors to compose.
    /// </summary>
    public IReadOnlyCollection<IObjectDetector> Detectors { get; } = detectors.ToList().AsReadOnly();

    /// <summary>
    /// Runs the composed object detectors on the given frame and aggregates their results.
    /// </summary>
    /// <param name="frame">The image frame to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the aggregated object detection result.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override async Task<ObjectDetectionResult> DetectAsync(IImage frame, CancellationToken cancellationToken = default)
    {
        var results = new List<ObjectDetection>();

        foreach (var detector in Detectors)
        {
            var result = await detector.DetectAsync(frame, cancellationToken);
            results.AddRange(result.Detections);
        }

        return new ObjectDetectionResult(frame, null, results);
    }

    /// <summary>
    /// Resets the state of all composed detectors.
    /// </summary>
    public override void Reset()
    {
        foreach (var detector in Detectors)
        {
            detector.Reset();
        }
    }
}
