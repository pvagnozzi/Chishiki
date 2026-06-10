// -----------------------------------------------------------------------------
// File:        ChishikiSystemToolsTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers the deterministic system metadata and capability MCP tools via reflection-based activation.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Chishiki.MCP.Host.Tests;

/// <summary>Provides focused unit tests for the internal Chishiki system MCP tools.</summary>
public sealed class ChishikiSystemToolsTests
{
    [Test]
    public async Task GetServerInfoAsync_ReturnsExpectedMetadataPayload()
    {
        var instance = CreateToolInstance();
        var method = instance.GetType().GetMethod("GetServerInfoAsync")!;

        var json = await InvokeAsync(method, instance, CancellationToken.None);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Multiple(() =>
        {
            Assert.That(root.GetProperty("name").GetString(), Is.EqualTo("Chishiki MCP Server"));
            Assert.That(root.GetProperty("version").GetString(), Is.Not.Null.And.Not.Empty);
            Assert.That(root.GetProperty("framework").GetString(), Is.Not.Null.And.Not.Empty);
            Assert.That(root.GetProperty("buildTime").GetString(), Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task ListCapabilitiesAsync_ReturnsAvailableSystemCapabilityAndPlannedCapabilities()
    {
        var instance = CreateToolInstance();
        var method = instance.GetType().GetMethod("ListCapabilitiesAsync")!;

        var json = await InvokeAsync(method, instance, CancellationToken.None);
        using var document = JsonDocument.Parse(json);
        var capabilities = document.RootElement.EnumerateArray().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(capabilities.Length, Is.GreaterThanOrEqualTo(5));
            Assert.That(capabilities.Any(c => c.GetProperty("group").GetString() == "system" && c.GetProperty("status").GetString() == "available"), Is.True);
            Assert.That(capabilities.Any(c => c.GetProperty("status").GetString() == "planned"), Is.True);
        });
    }

    [Test]
    public void GetServerInfoAsync_WhenCanceled_ThrowsOperationCanceledException()
    {
        var instance = CreateToolInstance();
        var method = instance.GetType().GetMethod("GetServerInfoAsync")!;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            _ = await InvokeAsync(method, instance, cts.Token));

        Assert.That(exception, Is.Not.Null);
    }

    private static object CreateToolInstance()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var toolType = typeof(Program).Assembly.GetType("Chishiki.MCP.Host.Tools.ChishikiSystemTools", throwOnError: true)!;
        return ActivatorUtilities.CreateInstance(services, toolType);
    }

    private static async Task<string> InvokeAsync(MethodInfo method, object instance, CancellationToken cancellationToken)
    {
        try
        {
            var task = (Task<string>)method.Invoke(instance, new object?[] { cancellationToken })!;
            return await task;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is OperationCanceledException operationCanceledException)
        {
            throw operationCanceledException;
        }
    }
}
