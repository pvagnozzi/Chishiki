// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Chishiki.Infrastructure.Aspire.ServiceDefaults.UnitTests.Extensions;

[TestFixture, Category("Unit")]
public class ServiceDefaultsExtensionsTests
{
    [Test]
    public void AddDefaultHealthChecks_ReturnsSameBuilderForChaining()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.AddDefaultHealthChecks();

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void AddDefaultHealthChecks_RegistersHealthCheckService()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddDefaultHealthChecks();

        using var host = builder.Build();

        var service = host.Services.GetService<HealthCheckService>();
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void AddServiceDefaults_ReturnsSameBuilderForChaining()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.AddServiceDefaults();

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void AddServiceDefaults_RegistersHealthCheckService()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddServiceDefaults();

        using var host = builder.Build();

        var service = host.Services.GetService<HealthCheckService>();
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void ConfigureOpenTelemetry_ReturnsSameBuilderForChaining()
    {
        var builder = Host.CreateApplicationBuilder();

        var result = builder.ConfigureOpenTelemetry();

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void AddDefaultHealthChecks_IsGenericMethod_AcceptsAnyHostApplicationBuilder()
    {
        var method = typeof(Microsoft.Extensions.Hosting.Extensions)
            .GetMethod(nameof(Microsoft.Extensions.Hosting.Extensions.AddDefaultHealthChecks));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.IsGenericMethod, Is.True);
    }

    [Test]
    public void AddServiceDefaults_IsGenericMethod_AcceptsAnyHostApplicationBuilder()
    {
        var method = typeof(Microsoft.Extensions.Hosting.Extensions)
            .GetMethod(nameof(Microsoft.Extensions.Hosting.Extensions.AddServiceDefaults));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.IsGenericMethod, Is.True);
    }

    [Test]
    public void MapDefaultEndpoints_MethodExists_OnExtensionsClass()
    {
        var method = typeof(Microsoft.Extensions.Hosting.Extensions)
            .GetMethod(nameof(Microsoft.Extensions.Hosting.Extensions.MapDefaultEndpoints));

        Assert.That(method, Is.Not.Null);
    }
}
