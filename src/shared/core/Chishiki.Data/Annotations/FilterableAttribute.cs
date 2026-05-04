// -----------------------------------------------------------------------------
// File:        FilterableAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Attribute marking properties as filterable in query specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Query;

namespace Chishiki.Data.Annotations;

/// <summary>
/// Attribute marking properties as filterable in query specifications.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterableAttribute(
    bool filterable = false,
    string? filterField = null,
    QueryOperator filterQueryOperator = QueryOperator.Contains,
    QueryValueType filterQueryValueType = QueryValueType.String,
    bool filterIgnoreCasing = false,
    Type? filterQueryTypeEnum = null) : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether the property is filterable.
    /// </summary>
    public bool Filterable { get; set; } = filterable;

    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    public string? FilterField { get; set; } = filterField;

    /// <summary>
    /// Gets or sets the query operator to use for filtering.
    /// </summary>
    public QueryOperator FilterQueryOperator { get; set; } = filterQueryOperator;

    /// <summary>
    /// Gets or sets the query value type for conversion.
    /// </summary>
    public QueryValueType FilterQueryValueType { get; set; } = filterQueryValueType;

    /// <summary>
    /// Gets or sets a value indicating whether to ignore casing during filtering.
    /// </summary>
    public bool FilterIgnoreCasing { get; set; } = filterIgnoreCasing;

    /// <summary>
    /// Gets or sets the fully-qualified enum type name when <see cref="FilterQueryValueType"/> is <see cref="QueryValueType.Enum"/>.
    /// </summary>
    public Type? FilterQueryTypeEnum { get; set; } = filterQueryTypeEnum;
}


