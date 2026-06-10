// -----------------------------------------------------------------------------
// File:        OpenCVImageExtensionsTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers deterministic OpenCV image helper behaviors without external assets.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.OpenCV;
using NUnit.Framework;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Tests;

/// <summary>Provides focused tests for <see cref="OpenCVImageExtensions"/>.</summary>
public sealed class OpenCVImageExtensionsTests
{
    [Test]
    public void ExpandRect_ClampsToImageBoundsAndKeepsPositiveSize()
    {
        var rect = new Rect(2, 2, 4, 4);

        var expanded = rect.ExpandRect(new Size(8, 8), 1.0);

        Assert.Multiple(() =>
        {
            Assert.That(expanded.X, Is.EqualTo(0));
            Assert.That(expanded.Y, Is.EqualTo(0));
            Assert.That(expanded.Width, Is.EqualTo(8));
            Assert.That(expanded.Height, Is.EqualTo(8));
        });
    }

    [Test]
    public void CalculateDetectionScore_ReturnsHigherScoreForNearIdealCandidates()
    {
        var idealScore = 4.0.CalculateDetectionScore(0.95);
        var poorScore = 1.2.CalculateDetectionScore(0.30);

        Assert.That(idealScore, Is.GreaterThan(poorScore));
        Assert.That(idealScore, Is.InRange(0f, 1f));
        Assert.That(poorScore, Is.InRange(0f, 1f));
    }

    [Test]
    public void ToGrayscale_WhenSourceAlreadyGrayscale_ReturnsClone()
    {
        using var grayscale = new Mat(2, 2, MatType.CV_8UC1, Scalar.All(127));

        using var converted = grayscale.ToGrayscale();

        Assert.Multiple(() =>
        {
            Assert.That(converted.Channels(), Is.EqualTo(1));
            Assert.That(converted.Empty(), Is.False);
            Assert.That(ReferenceEquals(converted, grayscale), Is.False);
        });
    }

    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(2, 3)]
    [TestCase(7, 7)]
    public void EnsureOddKernel_NormalizesKernelSize(int input, int expected)
    {
        Assert.That(input.EnsureOddKernel(), Is.EqualTo(expected));
    }
}
