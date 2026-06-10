// -----------------------------------------------------------------------------
// File:        EspToolStubLoader.cs
// Author:      Piergiorgio Vagnozzi
// Description: Loads upstream esptool stub-loader JSON artifacts into Chishiki models.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Text.Json;

namespace Chishiki.ESPTool;

/// <summary>Parses upstream esptool stub-loader JSON artifacts into <see cref="EspStubImage"/> instances.</summary>
public static class EspToolStubLoader
{
    /// <summary>Parses an upstream esptool stub-loader JSON payload.</summary>
    /// <param name="json">The JSON payload to parse.</param>
    /// <returns>A fully populated <see cref="EspStubImage"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the JSON payload is missing required properties.</exception>
    public static EspStubImage FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var entryPoint = root.GetProperty("entry").GetUInt32();
        var textStart = root.GetProperty("text_start").GetUInt32();
        var textData = Convert.FromBase64String(root.GetProperty("text").GetString() ?? throw new InvalidOperationException("The stub JSON does not contain a valid text segment."));
        var dataStart = root.GetProperty("data_start").GetUInt32();
        var dataData = Convert.FromBase64String(root.GetProperty("data").GetString() ?? throw new InvalidOperationException("The stub JSON does not contain a valid data segment."));

        var segments = new List<EspStubSegment>
        {
            new()
            {
                LoadAddress = textStart,
                Data = textData
            }
        };

        var bssSize = root.TryGetProperty("bss_size", out var bssSizeProperty) ? bssSizeProperty.GetUInt32() : 0u;
        if (bssSize > 0)
        {
            Array.Resize(ref dataData, checked(dataData.Length + (int)bssSize));
        }

        segments.Add(new EspStubSegment
        {
            LoadAddress = root.TryGetProperty("bss_start", out var bssStartProperty) ? bssStartProperty.GetUInt32() : dataStart,
            Data = dataData
        });

        return new EspStubImage
        {
            EntryPoint = entryPoint,
            Segments = segments
        };
    }

    /// <summary>Parses an upstream esptool stub-loader JSON file from disk.</summary>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A fully populated <see cref="EspStubImage"/> instance.</returns>
    public static async Task<EspStubImage> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return FromJson(json);
    }
}
