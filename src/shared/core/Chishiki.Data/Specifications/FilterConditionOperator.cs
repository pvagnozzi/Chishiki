// -----------------------------------------------------------------------------
// File:        FilterConditionOperator.cs
// Author:      Piergiorgio Vagnozzi
// Description: Enumeration of filter condition operators for query specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>
/// Enum Filter Condition Operator
/// </summary>
public enum FilterConditionOperator
{
    /// <summary>
    /// Is null (IS NULL).
    /// </summary>
    IsNull,

    /// <summary>
    /// Not is null (NOT IS NULL).
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Not equal (<>).
    /// </summary>
    Equal,

    /// <summary>
    /// Equal (=).
    /// </summary>
    NotEqual,

    /// <summary>
    /// Lesser (<).
    /// </summary>
    Lesser,

    /// <summary>
    /// Lesser or equal (<=).
    /// </summary>
    LesserEqual,

    /// <summary>
    /// Greater (>).
    /// </summary>
    Greater,

    /// <summary>
    /// Greater or equal (>=)
    /// </summary>
    GreaterEqual,

    /// <summary>
    /// Starts with (only for strings).
    /// </summary>
    StartsWith,

    /// <summary>
    /// Ends with (only for strings).
    /// </summary>
    EndsWith,

    /// <summary>
    /// Contains (only for strings).
    /// </summary>
    Contains,

    /// <summary>
    /// Not contains (only for strings).
    /// </summary>
    NotContains,

    /// <summary>
    /// Not starts with (only for strings).
    /// </summary>
    NotStartsWith,

    /// <summary>
    /// Not ends with (only for strings).
    /// </summary>
    NotEndsWith,

    /// <summary>
    /// In.
    /// </summary>
    In,

    /// <summary>
    /// Not in.
    /// </summary>
    NotIn,
}


