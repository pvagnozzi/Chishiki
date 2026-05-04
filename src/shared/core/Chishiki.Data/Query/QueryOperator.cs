// -----------------------------------------------------------------------------
// File:        QueryOperator.cs
// Author:      Piergiorgio Vagnozzi
// Description: Enumeration of query filter operators for property-based filtering.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Query;

/// <summary>
/// Query operator enumeration.
/// </summary>
public enum QueryOperator
{
    /// <summary>Is null.</summary>
    IsNull,
    /// <summary>Is not null.</summary>
    IsNotNull,
    /// <summary>Equal (=).</summary>
    Equal,
    /// <summary>Not equal (&lt;&gt;).</summary>
    NotEqual,
    /// <summary>Lesser (&lt;).</summary>
    Lesser,
    /// <summary>Lesser or equal (&lt;=).</summary>
    LesserEqual,
    /// <summary>Greater (&gt;).</summary>
    Greater,
    /// <summary>Greater or equal (&gt;=).</summary>
    GreaterEqual,
    /// <summary>Starts with.</summary>
    StartsWith,
    /// <summary>Ends with.</summary>
    EndsWith,
    /// <summary>Contains.</summary>
    Contains,
    /// <summary>Not contains.</summary>
    NotContains,
    /// <summary>Not starts with.</summary>
    NotStartsWith,
    /// <summary>Not ends with.</summary>
    NotEndsWith,
    /// <summary>In list.</summary>
    In,
    /// <summary>Not in list.</summary>
    NotIn,
}
