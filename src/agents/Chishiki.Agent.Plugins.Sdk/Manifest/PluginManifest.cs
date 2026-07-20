// -----------------------------------------------------------------------------
// File:        PluginManifest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Serializable manifest record representing the plugin.json descriptor file.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace Chishiki.Agent.Plugins.Sdk.Manifest;

/// <summary>
/// Represents the serializable manifest for a Chishiki plugin, corresponding to the
/// <c>plugin.json</c> descriptor file placed alongside the plugin assembly.
/// </summary>
/// <remarks>
/// Example <c>plugin.json</c>:
/// <code>
/// {
///   "id": "code-analysis",
///   "displayName": "Code Analysis",
///   "version": "1.0.0",
///   "description": "Provides static code analysis capabilities.",
///   "entryAssembly": "Chishiki.Agent.Plugins.CodeAnalysis.dll",
///   "entryType": "Chishiki.Agent.Plugins.CodeAnalysis.CodeAnalysisPlugin"
/// }
/// </code>
/// </remarks>
public sealed record PluginManifest
{
    #region Properties

    /// <summary>Gets the unique plugin identifier, e.g. <c>code-analysis</c>.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Gets the human-readable display name of the plugin.</summary>
    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    /// <summary>Gets the semantic version string for the plugin, e.g. <c>1.0.0</c>.</summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>Gets a brief description of what the plugin does.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Gets the filename of the assembly that contains the plugin entry type, e.g. <c>Chishiki.Agent.Plugins.CodeAnalysis.dll</c>.</summary>
    [JsonPropertyName("entryAssembly")]
    public required string EntryAssembly { get; init; }

    /// <summary>Gets the fully qualified type name of the class that implements the plugin entry point.</summary>
    [JsonPropertyName("entryType")]
    public required string EntryType { get; init; }

    #endregion
}
