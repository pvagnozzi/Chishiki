// -----------------------------------------------------------------------------
// File:        FilterConditionExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Record representing a filter condition expression in query specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>
/// Represents a filter condition expression in query specifications.
/// </summary>
/// <remarks>
/// This record combines property-level filtering with specific operators and values,
/// enabling flexible query filtering with support for case-sensitive/insensitive matching.
/// </remarks>
/// <seealso cref="PropertyExpression" />
/// <seealso cref="IFilterConditionExpression" />
public record FilterConditionExpression(
    /// <summary>
    /// The name of the property to filter on.
    /// </summary>
    string PropertyName,
    /// <summary>
    /// The operator to use for comparison.
    /// </summary>
    FilterConditionOperator ConditionOperator,
    /// <summary>
    /// The value to compare against (may be null for null checks).
    /// </summary>
    object? Value = null,
    /// <summary>
    /// Indicates whether to ignore casing during string comparisons.
    /// </summary>
    bool IgnoreCasing = false)
    : PropertyExpression(PropertyName), IFilterConditionExpression;


