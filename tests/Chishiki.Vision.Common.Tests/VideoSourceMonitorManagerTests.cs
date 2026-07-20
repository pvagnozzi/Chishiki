// -----------------------------------------------------------------------------
// File:        VideoSourceMonitorManagerTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers deterministic registration and orchestration behavior for the video source monitor manager.
// Created:     2026-06-10
// Modified:    2026-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Vision.Abstraction.Detectors.Motion;
using Chishiki.Vision.Abstraction.Monitors;
using Chishiki.Vision.Abstraction.Sources;
using Chishiki.Vision.Common.Monitors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Vision.Common.Tests;

/// <summary>Provides focused tests for <see cref="VideoSourceMonitorManager"/>.</summary>
public sealed class VideoSourceMonitorManagerTests
{
    private static readonly string[] ExpectedCamera6Ids = ["camera-6"];

    [Test]
    public void RegisterThrowsWhenVideoSourceIdIsDuplicated()
    {
        var manager = new VideoSourceMonitorManager(NullLogger<VideoSourceMonitorManager>.Instance);
        var firstMonitor = CreateMonitor("camera-1");
        var secondMonitor = CreateMonitor("camera-1");

        manager.Register(firstMonitor);

        var exception = Assert.Throws<InvalidOperationException>(() => manager.Register(secondMonitor));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("camera-1"));
    }

    [Test]
    public async Task UnregisterAsyncStopsRunningMonitorDisposesItAndRemovesIt()
    {
        var manager = new VideoSourceMonitorManager(NullLogger<VideoSourceMonitorManager>.Instance);
        var monitor = CreateMonitor("camera-2", isRunning: true);
        manager.Register(monitor);

        await manager.UnregisterAsync("camera-2");

        await monitor.Received(1).StopAsync();
        await monitor.Received(1).DisposeAsync();
        Assert.That(manager.CameraIds, Is.Empty);
    }

    [Test]
    public void MotionDetectedFromRegisteredMonitorIsForwarded()
    {
        var manager = new VideoSourceMonitorManager(NullLogger<VideoSourceMonitorManager>.Instance);
        var monitor = CreateMonitor("camera-3");
        manager.Register(monitor);

        MotionDetectedEventArgs? forwardedArgs = null;
        manager.MotionDetected += (_, args) => forwardedArgs = args;

        var result = new MotionDetectionResult(new TestImage(), detections: [new MotionDetection(1, 1, 4, 4)]);
        monitor.MotionDetected += Raise.Event<EventHandler<MotionDetectedEventArgs>>(monitor, new MotionDetectedEventArgs("camera-3", result));

        Assert.That(forwardedArgs, Is.Not.Null);
        Assert.That(forwardedArgs!.VideoSourceId, Is.EqualTo("camera-3"));
        Assert.That(forwardedArgs.Result.Detections, Has.Count.EqualTo(1));
    }

    [Test]
    public void StartAllAsyncSwallowsSiblingStartFailures()
    {
        var manager = new VideoSourceMonitorManager(NullLogger<VideoSourceMonitorManager>.Instance);
        var failingMonitor = CreateMonitor("camera-4");
        var healthyMonitor = CreateMonitor("camera-5");
        failingMonitor.StartAsync(Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("boom"));

        manager.Register(failingMonitor);
        manager.Register(healthyMonitor);

        Assert.DoesNotThrowAsync(async () => await manager.StartAllAsync());
        healthyMonitor.Received(1).StartAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void ConstructorRegistersConfiguredMonitorsWhenConfigurationContainsEntries()
    {
        var configuredMonitor = CreateMonitor("camera-6");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Vision:ManagedVideoSources:0:VideoSourceId"] = "camera-6",
                ["Vision:ManagedVideoSources:0:SourceType"] = "Camera",
                ["Vision:ManagedVideoSources:0:CameraIndex"] = "0",
                ["Vision:ManagedVideoSources:0:MonitorOptions:FramesPerSecond"] = "2"
            })
            .Build();
        var factory = Substitute.For<IVideoSourceMonitorFactory>();
        factory.Create(Arg.Any<ManagedVideoSourceOptions>()).Returns(configuredMonitor);

        var manager = new VideoSourceMonitorManager(configuration, factory, NullLogger<VideoSourceMonitorManager>.Instance);

        Assert.That(manager.CameraIds, Is.EqualTo(ExpectedCamera6Ids));
        factory.Received(1).Create(Arg.Is<ManagedVideoSourceOptions>(options =>
            options != null
            && options.VideoSourceId == "camera-6"
            && options.SourceType == "Camera"
            && options.CameraIndex == 0
            && options.MonitorOptions != null
            && Math.Abs(options.MonitorOptions.FramesPerSecond - 2d) < 0.001));
    }

    private static IVideoSourceMonitor CreateMonitor(string videoSourceId, bool isRunning = false)
    {
        var source = Substitute.For<IVideoSource>();
        source.VideoSourceId.Returns(videoSourceId);

        var monitor = Substitute.For<IVideoSourceMonitor>();
        monitor.VideoSource.Returns(source);
        monitor.IsRunning.Returns(isRunning);
        monitor.StopAsync().Returns(Task.CompletedTask);
        monitor.StartAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
#pragma warning disable CA2012 // NSubstitute setup — ValueTask is consumed by the Returns extension method
        monitor.DisposeAsync().Returns(ValueTask.CompletedTask);
#pragma warning restore CA2012

        return monitor;
    }
}
