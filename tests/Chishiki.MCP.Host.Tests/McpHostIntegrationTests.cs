// -----------------------------------------------------------------------------
// File:        McpHostIntegrationTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers MCP host endpoint wiring and environment-dependent exposure using in-memory hosting.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Chishiki.MCP.Host.Tests;

/// <summary>Provides focused integration tests for the MCP host startup pipeline.</summary>
public sealed class McpHostIntegrationTests
{
    [Test]
    public async Task DevelopmentHost_MapsDefaultEndpointsOpenApiAndMcpRoute()
    {
        await using var factory = new McpHostWebApplicationFactory(Environments.Development);
        using var client = factory.CreateClient();

        using var healthResponse = await client.GetAsync("/health");
        using var aliveResponse = await client.GetAsync("/alive");
        using var openApiResponse = await client.GetAsync("/openapi/v1.json");

        var endpoints = factory.Services.GetRequiredService<IEnumerable<EndpointDataSource>>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Where(pattern => pattern is not null)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(healthResponse.IsSuccessStatusCode, Is.True);
            Assert.That(aliveResponse.IsSuccessStatusCode, Is.True);
            Assert.That(openApiResponse.IsSuccessStatusCode, Is.True);
            Assert.That(endpoints, Does.Contain("/mcp/"));
            Assert.That(endpoints, Does.Contain("/metrics"));
        });
    }

    [Test]
    public async Task ProductionHost_DoesNotExposeDevelopmentOnlyRoutes()
    {
        await using var factory = new McpHostWebApplicationFactory(Environments.Production);
        using var client = factory.CreateClient();

        using var healthResponse = await client.GetAsync("/health");
        using var aliveResponse = await client.GetAsync("/alive");
        using var openApiResponse = await client.GetAsync("/openapi/v1.json");
        using var metricsResponse = await client.GetAsync("/metrics");

        Assert.Multiple(() =>
        {
            Assert.That(healthResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(aliveResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(openApiResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(metricsResponse.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound));
        });
    }

    private sealed class McpHostWebApplicationFactory(string environmentName) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environmentName);
        }
    }
}
