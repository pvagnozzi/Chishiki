// -----------------------------------------------------------------------------
// File:        VideoSourceMonitorTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers deterministic producer/consumer monitor behavior with fake sources and substituted detectors.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Motion;
using Chishiki.Vision.Abstraction.Monitors;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Vision.Common.Tests;

/// <summary>Provides focused tests for <see cref="TestVideoSourceMonitor"/>.</summary>
public sealed class VideoSourceMonitorTests
{
    [Test]
    public async Task StartAsyncRaisesMotionDetectedWhenDetectorFindsMotion()
    {
        var source = new SequenceVideoSource("camera-1", [new TestImage(), new TestImage(isEmpty: true)]);
        var detector = Substitute.For<IMotionDetector>();
        detector.DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(new MotionDetectionResult((IImage)callInfo[0]!, detections: [new MotionDetection(1, 1, 4, 4)])));

        await using var monitor = new TestVideoSourceMonitor(source, detector, new VideoSourceMonitorOptions
        {
            FramesPerSecond = 200,
            ChannelCapacity = 1,
            RaiseOnlyOnMotion = true
        });

        var eventTaskSource = new TaskCompletionSource<MotionDetectedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        monitor.MotionDetected += (_, args) => eventTaskSource.TrySetResult(args);

        await monitor.StartAsync();
        var completedTask = await Task.WhenAny(eventTaskSource.Task, Task.Delay(TimeSpan.FromSeconds(2)));
        await monitor.StopAsync();

        Assert.That(completedTask, Is.SameAs(eventTaskSource.Task));
        var eventArgs = await eventTaskSource.Task;
        Assert.That(eventArgs.VideoSourceId, Is.EqualTo("camera-1"));
        Assert.That(eventArgs.Result.Detections, Has.Count.EqualTo(1));
        await detector.Received(1).DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task StartAsyncSkipsEmptyFramesWithoutInvokingDetector()
    {
        var source = new SequenceVideoSource("camera-2", [new TestImage(isEmpty: true), new TestImage(isEmpty: true)]);
        var detector = Substitute.For<IMotionDetector>();

        await using var monitor = new TestVideoSourceMonitor(source, detector, new VideoSourceMonitorOptions
        {
            FramesPerSecond = 200,
            ChannelCapacity = 1,
            RaiseOnlyOnMotion = true
        });

        await monitor.StartAsync();
        await Task.Delay(100);
        await monitor.StopAsync();

        await detector.DidNotReceive().DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task StartAsyncSuppressesEventsWhenRaiseOnlyOnMotionIsEnabled()
    {
        var source = new SequenceVideoSource("camera-3", [new TestImage(), new TestImage(isEmpty: true)]);
        var detector = Substitute.For<IMotionDetector>();
        detector.DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(new MotionDetectionResult((IImage)callInfo[0]!)));

        await using var monitor = new TestVideoSourceMonitor(source, detector, new VideoSourceMonitorOptions
        {
            FramesPerSecond = 200,
            ChannelCapacity = 1,
            RaiseOnlyOnMotion = true
        });

        var eventRaised = false;
        monitor.MotionDetected += (_, _) => eventRaised = true;

        await monitor.StartAsync();
        await Task.Delay(150);
        await monitor.StopAsync();

        Assert.That(eventRaised, Is.False);
        await detector.Received(1).DetectAsync(Arg.Any<IImage>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DisposeAsyncDisposesOwnedSourceAndDetector()
    {
        var source = new SequenceVideoSource("camera-4", [new TestImage()]);
        var detector = Substitute.For<IMotionDetector>();

        var monitor = new TestVideoSourceMonitor(source, detector, new VideoSourceMonitorOptions());

        await monitor.DisposeAsync();

        Assert.That(source.IsDisposed, Is.True);
        detector.Received(1).Dispose();
    }
}
