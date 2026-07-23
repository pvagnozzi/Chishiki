// -----------------------------------------------------------------------------
// File:        PluginAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Attribute used to declare plugin metadata directly on a plugin class.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Plugins.Sdk.Attributes;

/// <summary>
/// Declares plugin metadata on a class that implements <see cref="Chishiki.Agent.Abstractions.Interfaces.IPlugin"/>.
/// When applied, <see cref="Base.PluginBase"/> reads this attribute to populate <c>Id</c>, <c>DisplayName</c>,
/// <c>Version</c>, and <c>Description</c> automatically.
/// </summary>
/// <remarks>Apply this attribute to the concrete plugin class, not to base classes or interfaces.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="PluginAttribute"/> class.
/// </remarks>
/// <param name="id">The unique plugin identifier.</param>
/// <param name="displayName">The human-readable display name.</param>
/// <param name="version">The semantic version string.</param>
/// <param name="description">A brief description of the plugin.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PluginAttribute(string id, string displayName, string version, string description) : Attribute
{
    #region Properties

    /// <summary>Gets the unique identifier for the plugin, e.g. <c>code-analysis</c>.</summary>
    public string Id { get; } = id;

    /// <summary>Gets the human-readable display name of the plugin.</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>Gets the semantic version string for the plugin, e.g. <c>1.0.0</c>.</summary>
    public string Version { get; } = version;

    /// <summary>Gets a brief description of what the plugin does.</summary>
    public string Description { get; } = description;

    #endregion
}
