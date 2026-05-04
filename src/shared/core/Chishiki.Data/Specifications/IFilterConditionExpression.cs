// -----------------------------------------------------------------------------
// File:        IFilterConditionExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for filter condition expressions in specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>
/// Filter Condition Expression.
/// </summary>
/// <seealso cref="IPropertyExpression" />
public interface IFilterConditionExpression : IPropertyExpression
{
    /// <summary>
    /// Gets the condition operator.
    /// </summary>
    /// <value>
    /// The condition operator.
    /// </value>
    FilterConditionOperator ConditionOperator { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    object? Value { get; }

    /// <summary>
    /// Gets a value indicating whether to ignore casing during string comparisons.
    /// </summary>
    /// <value>
    ///   <c>true</c> if casing should be ignored; otherwise, <c>false</c>.
    /// </value>
    bool IgnoreCasing { get; }
}


