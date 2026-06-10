// -----------------------------------------------------------------------------
// File:        VisionAbstractionResultTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers deterministic DTO and result behaviors in the vision abstraction library.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Cardplates;
using Chishiki.Vision.Abstraction.Detectors.Faces;
using Chishiki.Vision.Abstraction.Detectors.Motion;
using NUnit.Framework;

namespace Chishiki.Vision.Abstraction.Tests;

/// <summary>Provides deterministic coverage for result and recognition DTOs.</summary>
public sealed class VisionAbstractionResultTests
{
    [Test]
    public void DetectionResultDefaultsAnnotatedFrameToOriginalAndUsesCurrentTimestamp()
    {
        var originalFrame = new TestImage();
        var before = DateTimeOffset.UtcNow;

        var result = new MotionDetectionResult(originalFrame);

        var after = DateTimeOffset.UtcNow;
        Assert.That(result.OriginalFrame, Is.SameAs(originalFrame));
        Assert.That(result.AnnotatedFrame, Is.SameAs(originalFrame));
        Assert.That(result.Detections, Is.Empty);
        Assert.That(result.Timestamp, Is.InRange(before, after));
        Assert.That(result.HasMotion, Is.False);
    }

    [Test]
    public void DetectionResultCreateExtensionUsesProvidedValues()
    {
        var originalFrame = new TestImage();
        var annotatedFrame = new TestImage();
        var timestamp = new DateTimeOffset(2026, 06, 10, 11, 0, 0, TimeSpan.Zero);
        var detections = new[] { new MotionDetection(10, 20, 30, 40) };

        var result = originalFrame.Create<TestDetectionResult, MotionDetection>(annotatedFrame, detections, timestamp);

        Assert.That(result.OriginalFrame, Is.SameAs(originalFrame));
        Assert.That(result.AnnotatedFrame, Is.SameAs(annotatedFrame));
        Assert.That(result.Detections, Has.Count.EqualTo(1));
        Assert.That(result.Detections[0].Area, Is.EqualTo(1200));
        Assert.That(result.Timestamp, Is.EqualTo(timestamp));
    }

    [Test]
    public void FaceRecognitionResultTrimsIdentityAndRecognizesEitherIdentityOrLabel()
    {
        var identityResult = new FaceRecognitionResult(identity: "  Alice  ", score: 0.9f);
        var labelResult = new FaceRecognitionResult(labelId: 7);
        var emptyResult = new FaceRecognitionResult(identity: "   ");

        Assert.That(identityResult.Identity, Is.EqualTo("Alice"));
        Assert.That(identityResult.HasRecognition, Is.True);
        Assert.That(labelResult.HasRecognition, Is.True);
        Assert.That(emptyResult.Identity, Is.Null);
        Assert.That(emptyResult.HasRecognition, Is.False);
    }

    [Test]
    public void CardplateRecognitionResultNormalizesTextAndSuppressesWhitespaceRecognition()
    {
        var recognized = new CardplateRecognitionResult("  ab123cd  ", 0.8f);
        var empty = new CardplateRecognitionResult("   ");

        Assert.That(recognized.Text, Is.EqualTo("AB123CD"));
        Assert.That(recognized.HasRecognition, Is.True);
        Assert.That(empty.Text, Is.Null);
        Assert.That(empty.HasRecognition, Is.False);
    }

    [Test]
    public void CardplateDetectionNormalizesRecognizedTextAndHasRecognitionFlag()
    {
        var detection = new CardplateDetection(1, 2, 30, 10, detectionScore: 0.7f, recognizedText: "  xy  ", recognitionScore: 0.6f);
        var emptyDetection = new CardplateDetection(1, 2, 30, 10, recognizedText: "   ");

        Assert.That(detection.Area, Is.EqualTo(300));
        Assert.That(detection.RecognizedText, Is.EqualTo("XY"));
        Assert.That(detection.HasRecognition, Is.True);
        Assert.That(emptyDetection.RecognizedText, Is.Null);
        Assert.That(emptyDetection.HasRecognition, Is.False);
    }

    [Test]
    public void CardplateDetectionResultAggregatesCardplateAndRecognitionFlags()
    {
        var frame = new TestImage();
        var result = new CardplateDetectionResult(
            frame,
            detections:
            [
                new CardplateDetection(0, 0, 30, 10, recognizedText: "AB12"),
                new CardplateDetection(40, 0, 30, 10)
            ]);

        Assert.That(result.HasCardplates, Is.True);
        Assert.That(result.HasRecognizedCardplates, Is.True);
        Assert.That(result.Detections, Has.Count.EqualTo(2));
    }

    private sealed record TestDetectionResult : DetectionResult<MotionDetection>
    {
        public TestDetectionResult() : base(new TestImage())
        {
        }
    }

    private sealed class TestImage(bool empty = false) : IImage
    {
        public byte[] ImageData { get; } = [0x01, 0x02, 0x03];

        public bool IsDisposed { get; private set; }

        public bool IsEmpty() => empty;

        public void Dispose() => IsDisposed = true;
    }
}
