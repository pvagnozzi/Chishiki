// -----------------------------------------------------------------------------
// File:        EspToolEmbeddedStubCatalog.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides access to embedded upstream esptool stub-loader artifacts.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Reflection;
using Chishiki.ESPTool.Exceptions;

namespace Chishiki.ESPTool;

/// <summary>Loads embedded upstream esptool stub-loader artifacts for supported chip targets.</summary>
public static class EspToolEmbeddedStubCatalog
{
    private static readonly IReadOnlyDictionary<EspChipTarget, string> ResourceNames = new Dictionary<EspChipTarget, string>
    {
        [EspChipTarget.Esp32] = "Chishiki.ESPTool.Resources.Stubs.esp32.json",
        [EspChipTarget.Esp32S2] = "Chishiki.ESPTool.Resources.Stubs.esp32s2.json",
        [EspChipTarget.Esp32S3] = "Chishiki.ESPTool.Resources.Stubs.esp32s3.json",
        [EspChipTarget.Esp32C3] = "Chishiki.ESPTool.Resources.Stubs.esp32c3.json",
        [EspChipTarget.Esp32C2] = "Chishiki.ESPTool.Resources.Stubs.esp32c2.json",
        [EspChipTarget.Esp32C6] = "Chishiki.ESPTool.Resources.Stubs.esp32c6.json",
        [EspChipTarget.Esp32H2] = "Chishiki.ESPTool.Resources.Stubs.esp32h2.json"
    };

    /// <summary>Gets the set of chip targets that currently ship with an embedded stub artifact.</summary>
    public static IReadOnlyCollection<EspChipTarget> SupportedTargets => ResourceNames.Keys.ToArray();

    /// <summary>Loads an embedded stub artifact for the specified target.</summary>
    /// <param name="target">The target whose embedded stub should be loaded.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A parsed <see cref="EspStubImage"/> instance.</returns>
    /// <exception cref="EspToolException">Thrown when no embedded stub is available for the requested target.</exception>
    public static async Task<EspStubImage> LoadAsync(EspChipTarget target, CancellationToken cancellationToken = default)
    {
        if (!ResourceNames.TryGetValue(target, out var resourceName))
        {
            throw new EspToolException($"No embedded stub artifact is available for chip target '{target}'.");
        }

        var assembly = typeof(EspToolEmbeddedStubCatalog).Assembly;
        await using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new EspToolException($"Embedded stub resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return EspToolStubLoader.FromJson(json);
    }
}
