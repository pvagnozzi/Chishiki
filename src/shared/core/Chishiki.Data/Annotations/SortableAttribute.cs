// -----------------------------------------------------------------------------
// File:        SortableAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Attribute marking properties as sortable in query specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Annotations;

/// <summary>Sortable attribute.</summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public class SortableAttribute(bool sortable = false, string? sortExpression = null) : Attribute
{
    /// <summary>Gets or sets a value indicating whether this <see cref="SortableAttribute"/> is sortable. .</summary>
    /// <value>
    ///   <c>true</c> if sortable; otherwise, <c>false</c>.
    /// </value>
    public bool Sortable { get; set; } = sortable;

    /// <summary>Gets or sets the sort expression. .</summary>
    /// <value>
    /// The sort expression.
    /// </value>
    public string? SortExpression { get; set; } = sortExpression;
}


