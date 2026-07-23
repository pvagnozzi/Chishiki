// -----------------------------------------------------------------------------
// File:        ComposedObjectDetectorTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers deterministic aggregation behavior for the composed object detector.
// Created:     2026-06-10
// Modified:    2026-06-11
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Objects;
using Chishiki.Vision.Common.Detectors.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Vision.Common.Tests;

/// <summary>Provides focused tests for <see cref="ComposedObjectDetector"/>.</summary>
public sealed class ComposedObjectDetectorTests
{
    private static readonly int[] ExpectedClassIds = [1, 2];

    [Test]
    public async Task DetectAsyncAggregatesDetectionsFromAllInnerDetectors()
    {
        var frame = new TestImage();
        var detector1 = Substitute.For<IObjectDetector>();
        var detector2 = Substitute.For<IObjectDetector>();
        _ = detector1.DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DetectionResult<ObjectDetection>>(new ObjectDetectionResult(frame, detections: [new ObjectDetection(1, 0.9f, 1, 2, 3, 4)])));
        _ = detector2.DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DetectionResult<ObjectDetection>>(new ObjectDetectionResult(frame, detections: [new ObjectDetection(2, 0.8f, 5, 6, 7, 8)])));

        var composedDetector = new ComposedObjectDetector(new TestDetectorOptions(), [detector1, detector2], NullLogger<ComposedObjectDetector>.Instance);

        var result = await composedDetector.DetectAsync(frame);

        Assert.That(result.OriginalFrame, Is.SameAs(frame));
        Assert.That(result.Detections.Select(detection => detection.ClassId), Is.EqualTo(ExpectedClassIds));
    }

    [Test]
    public void ResetCallsResetOnAllInnerDetectors()
    {
        var detector1 = Substitute.For<IObjectDetector>();
        var detector2 = Substitute.For<IObjectDetector>();
        var composedDetector = new ComposedObjectDetector(new TestDetectorOptions(), [detector1, detector2], NullLogger<ComposedObjectDetector>.Instance);

        composedDetector.Reset();

        detector1.Received(1).Reset();
        detector2.Received(1).Reset();
    }
}
