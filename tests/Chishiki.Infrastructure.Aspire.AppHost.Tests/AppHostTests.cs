// -----------------------------------------------------------------------------
// File:        AppHostTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers Aspire AppHost resource registration with and without the optional security profile.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Aspire.Hosting.Testing;
using NUnit.Framework;

namespace Chishiki.Infrastructure.Aspire.AppHost.Tests;

/// <summary>Provides focused tests for AppHost resource registration and the optional security profile switch.</summary>
[TestFixture]
[NonParallelizable]
public sealed class AppHostTests
{
    private static readonly SemaphoreSlim EnvironmentLock = new(1, 1);

    private static readonly string[] DefaultResourceNames =
    [
        "chishiki-mcp",
        "grafana",
        "keycloak",
        "ollama",
        "postgresql",
        "prometheus",
        "qdrant",
        "redis",
    ];

    [Test]
    public async Task DefaultConfigurationRegistersCoreInfrastructureResourcesAndMcpHost()
    {
        var resources = await GetResourceNamesAsync(enableSecurityProfile: false);

        Assert.That(resources, Is.EquivalentTo(DefaultResourceNames));
    }

    [Test]
    public async Task SecurityProfileEnabledRegistersOptionalSecurityScanningResources()
    {
        var resources = await GetResourceNamesAsync(enableSecurityProfile: true);

        Assert.Multiple(() =>
        {
            Assert.That(resources, Does.Contain("semgrep"));
            Assert.That(resources, Does.Contain("sonarqube"));
            Assert.That(resources, Does.Contain("dependency-check"));
            Assert.That(resources, Does.Contain("trivy"));
            Assert.That(resources, Does.Contain("gitleaks"));
            Assert.That(resources, Does.Contain("syft"));
            Assert.That(resources, Does.Contain("grype"));
            Assert.That(resources, Does.Contain("binskim"));
            Assert.That(resources, Does.Contain("nuclei"));
            Assert.That(resources, Does.Contain("sqlmap"));
            Assert.That(resources, Does.Contain("zap"));
            Assert.That(resources, Does.Contain("ffuf"));
            Assert.That(resources, Does.Contain("chishiki-mcp"));
        });
    }

    private static async Task<string[]> GetResourceNamesAsync(bool enableSecurityProfile)
    {
        await EnvironmentLock.WaitAsync();
        var originalValue = Environment.GetEnvironmentVariable("CHISHIKI_SECURITY_PROFILE");

        try
        {
            Environment.SetEnvironmentVariable("CHISHIKI_SECURITY_PROFILE", enableSecurityProfile ? "true" : null);

            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Chishiki_Infrastructure_Aspire_AppHost>();

            return builder.Resources
                .Select(resource => resource.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }
        finally
        {
            Environment.SetEnvironmentVariable("CHISHIKI_SECURITY_PROFILE", originalValue);
            EnvironmentLock.Release();
        }
    }
}
