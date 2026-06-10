// -----------------------------------------------------------------------------
// File:        ExtensionsTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers the highest-value Aspire ServiceDefaults registration and endpoint behaviors.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using NUnit.Framework;

namespace Chishiki.Infrastructure.Aspire.ServiceDefaults.Tests;

/// <summary>Provides focused tests for Aspire ServiceDefaults registrations and default endpoints.</summary>
public sealed class ExtensionsTests
{
    [Test]
    public void AddServiceDefaults_ConfiguresHttpsAsTheOnlyAllowedServiceDiscoveryScheme()
    {
        var builder = CreateBuilder(Environments.Development);

        builder.AddServiceDefaults();

        using var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

        Assert.That(options.AllowedSchemes, Is.EquivalentTo(new[] { "https" }));
    }

    [Test]
    public async Task MapDefaultEndpoints_InDevelopment_MapsHealthAliveAndMetrics()
    {
        var builder = CreateBuilder(Environments.Development);
        builder.AddServiceDefaults();

        await using var app = builder.Build();
        app.MapDefaultEndpoints();
        await app.StartAsync();

        var client = app.GetTestClient();

        using var healthResponse = await client.GetAsync("/health");
        using var aliveResponse = await client.GetAsync("/alive");
        using var metricsResponse = await client.GetAsync("/metrics");

        Assert.Multiple(() =>
        {
            Assert.That(healthResponse.IsSuccessStatusCode, Is.True);
            Assert.That(aliveResponse.IsSuccessStatusCode, Is.True);
            Assert.That(metricsResponse.StatusCode, Is.Not.EqualTo(System.Net.HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task MapDefaultEndpoints_InProduction_DoesNotMapHealthEndpointsButKeepsMetrics()
    {
        var builder = CreateBuilder(Environments.Production);
        builder.AddServiceDefaults();

        await using var app = builder.Build();
        app.MapDefaultEndpoints();
        await app.StartAsync();

        var client = app.GetTestClient();

        using var healthResponse = await client.GetAsync("/health");
        using var aliveResponse = await client.GetAsync("/alive");
        using var metricsResponse = await client.GetAsync("/metrics");

        Assert.Multiple(() =>
        {
            Assert.That(healthResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            Assert.That(aliveResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            Assert.That(metricsResponse.StatusCode, Is.Not.EqualTo(System.Net.HttpStatusCode.NotFound));
        });
    }

    [Test]
    public void AddServiceDefaults_WithTelemetryExporterConfiguration_BuildsSuccessfully()
    {
        var builder = CreateBuilder(
            Environments.Development,
            new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://localhost:4317",
                ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
            });

        Assert.DoesNotThrow(() =>
        {
            builder.AddServiceDefaults();
            using var app = builder.Build();
        });
    }

    private static WebApplicationBuilder CreateBuilder(string environmentName, IDictionary<string, string?>? configuration = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = environmentName,
            ApplicationName = typeof(ExtensionsTests).Assembly.GetName().Name,
        });

        builder.WebHost.UseTestServer();

        if (configuration is not null)
        {
            builder.Configuration.AddInMemoryCollection(configuration);
        }

        return builder;
    }
}
