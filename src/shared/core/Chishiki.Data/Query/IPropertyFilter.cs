// -----------------------------------------------------------------------------
// File:        IPropertyFilter.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for property filter descriptors used in query specification helpers.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Query;

/// <summary>
/// Represents a property filter used to build query conditions.
/// </summary>
public interface IPropertyFilter
{
    /// <summary>
    /// Gets the raw string value of the filter.
    /// </summary>
    /// <returns>The raw string value, or null if no value is set.</returns>
    string? GetValue();

    /// <summary>
    /// Gets the comparison operator.
    /// </summary>
    /// <value>
    /// The query operator to use for comparison.
    /// </value>
    QueryOperator FilterOperator { get; }

    /// <summary>
    /// Gets the value type used for conversion.
    /// </summary>
    /// <value>
    /// The query value type.
    /// </value>
    QueryValueType ValueType { get; }

    /// <summary>
    /// Gets the fully-qualified enum type name when <see cref="ValueType"/> is <see cref="QueryValueType.Enum"/>.
    /// </summary>
    /// <value>
    /// The fully-qualified enum type name, or null if not an enum value type.
    /// </value>
    string? TypeValueEnum { get; }
}
