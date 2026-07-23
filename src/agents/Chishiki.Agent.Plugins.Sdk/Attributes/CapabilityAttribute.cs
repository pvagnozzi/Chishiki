// -----------------------------------------------------------------------------
// File:        CapabilityAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Marks a public method on a PluginBase subclass as a discoverable plugin capability.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Plugins.Sdk.Attributes;

/// <summary>
/// Marks a public instance method on a <see cref="Base.PluginBase"/> subclass as a discoverable plugin capability.
/// </summary>
/// <remarks>
/// The decorated method must return <see cref="Task{TResult}"/> where
/// <c>TResult</c> is <see cref="Abstractions.Models.CapabilityResult"/> and accept
/// <c>(CapabilityRequest request, CancellationToken cancellationToken)</c> as parameters.
/// <see cref="Base.PluginBase"/> discovers and registers these methods automatically during initialization.
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="CapabilityAttribute"/> class.</remarks>
/// <param name="name">The unique name of the capability.</param>
/// <param name="description">A brief description of what the capability does.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CapabilityAttribute(string name, string description) : Attribute
{
    #region Properties

    /// <summary>Gets the unique name of the capability within the plugin, e.g. <c>analyze</c>.</summary>
    public string Name { get; } = name;

    /// <summary>Gets a brief description of what the capability does.</summary>
    public string Description { get; } = description;

    #endregion
}
