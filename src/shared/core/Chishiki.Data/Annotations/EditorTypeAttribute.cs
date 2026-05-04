// -----------------------------------------------------------------------------
// File:        EditorTypeAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Attribute for specifying the editor type to use when rendering a property.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Annotations;

/// <summary>
/// Attribute for specifying the editor type to use when rendering a property.
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public class EditorTypeAttribute(EditorType editorType) : Attribute
{
    /// <summary>
    /// Gets the editor type.
    /// </summary>
    public EditorType EditorType { get; init; } = editorType;
}


