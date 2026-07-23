// -----------------------------------------------------------------------------
// File:        ParameterAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Annotates a parameter of a capability method with its name, description, and requirement.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Plugins.Sdk.Attributes;

/// <summary>
/// Annotates a parameter of a capability method with metadata used to build the
/// <see cref="Abstractions.Interfaces.IPluginCapability.ParameterSchema"/>.
/// </summary>
/// <remarks>
/// Apply this attribute to individual parameters of methods decorated with <see cref="CapabilityAttribute"/>
/// to provide schema information that callers can inspect at runtime.
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="ParameterAttribute"/> class.</remarks>
/// <param name="name">The logical parameter name.</param>
/// <param name="description">A brief description of the parameter.</param>
/// <param name="required">Whether the parameter is required. Defaults to <c>true</c>.</param>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class ParameterAttribute(string name, string description, bool required = true) : Attribute
{
    #region Properties

    /// <summary>Gets the logical parameter name as it appears in <see cref="Abstractions.Models.CapabilityRequest.Parameters"/>.</summary>
    public string Name { get; } = name;

    /// <summary>Gets a brief description of the parameter and its expected value.</summary>
    public string Description { get; } = description;

    /// <summary>Gets a value indicating whether this parameter is required. Defaults to <c>true</c>.</summary>
    public bool Required { get; } = required;

    #endregion
}
