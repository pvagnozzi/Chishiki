// -----------------------------------------------------------------------------
// File:        PropertySettings.cs
// Author:      Piergiorgio Vagnozzi
// Description: Record representing resolved property metadata including display, sort, and filter settings.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Query;
using System.Reflection;

namespace Chishiki.Data.Annotations;

/// <summary>Property settings.</summary>
public record PropertySettings(PropertyInfo PropertyInfo)
{
    /// <summary>Gets the name. .</summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; init; } = PropertyInfo.Name;

    /// <summary>Gets the type of the property. .</summary>
    /// <value>
    /// The type of the property.
    /// </value>
    public Type PropertyType { get; init; } = PropertyInfo.PropertyType;

    /// <summary>Gets the type of the editor. .</summary>
    /// <value>
    /// The type of the editor.
    /// </value>
    public EditorType EditorType { get; init; }

    /// <summary>Gets the display name. .</summary>
    /// <value>
    /// The display name.
    /// </value>
    public string DisplayName { get; init; } = PropertyInfo.Name;

    /// <summary>Gets the data format string. .</summary>
    /// <value>
    /// The data format string.
    /// </value>
    public string? DataFormatString { get; init; }

    /// <summary>Gets the null display text. .</summary>
    /// <value>
    /// The null display text.
    /// </value>
    public string? NullDisplayText { get; init; }

    /// <summary>Gets a value indicating whether this <see cref="PropertySettings"/> is sortable. .</summary>
    /// <value>
    ///   <c>true</c> if sortable; otherwise, <c>false</c>.
    /// </value>
    public bool Sortable { get; init; }

    /// <summary>Gets the sort expression. .</summary>
    /// <value>
    /// The sort expression.
    /// </value>
    public string SortExpression { get; init; } = PropertyInfo.Name;

    /// <summary>Gets a value indicating whether this property is filterable. .</summary>
    /// <value>
    ///   <c>true</c> if filterable; otherwise, <c>false</c>.
    /// </value>
    public bool Filterable { get; init; }

    /// <summary>Gets the name of the field to filter on. .</summary>
    /// <value>
    /// The filter field name.
    /// </value>
    public string FilterField { get; init; } = PropertyInfo.Name;

    /// <summary>Gets the query operator to use for filtering. .</summary>
    /// <value>
    /// The filter query operator.
    /// </value>
    public QueryOperator FilterQueryOperator { get; init; } = QueryOperator.Contains;

    /// <summary>Gets the query value type for filter conversion. .</summary>
    /// <value>
    /// The filter query value type.
    /// </value>
    public QueryValueType FilterQueryValueType { get; init; } = QueryValueType.String;

    /// <summary>Gets a value indicating whether to ignore casing during filtering. .</summary>
    /// <value>
    ///   <c>true</c> if ignore casing; otherwise, <c>false</c>.
    /// </value>
    public bool FilterIgnoreCasing { get; init; }

    /// <summary>Gets the fully-qualified enum type name when <see cref="FilterQueryValueType"/> is <see cref="QueryValueType.Enum"/>. .</summary>
    /// <value>
    /// The fully-qualified enum type name, or null if not an enum value type.
    /// </value>
    public Type? FilterQueryTypeEnum { get; init; }

    /// <summary>Gets the default value. .</summary>
    /// <value>
    /// The default value.
    /// </value>
    public object? DefaultValue { get; init; }

    /// <summary>Gets a value indicating whether this instance is key. .</summary>
    /// <value>
    ///   <c>true</c> if this instance is key; otherwise, <c>false</c>.
    /// </value>
    public bool IsKey { get; init; }

    /// <summary>Gets a value indicating whether this instance is scaffold. .</summary>
    /// <value>
    ///   <c>true</c> if this instance is scaffold; otherwise, <c>false</c>.
    /// </value>
    public bool IsScaffold { get; init; }

    /// <summary>Gets a value indicating whether this <see cref="PropertySettings"/> is required. .</summary>
    /// <value>
    ///   <c>true</c> if required; otherwise, <c>false</c>.
    /// </value>
    public bool Required { get; init; }

    /// <summary>Gets the minimum value. .</summary>
    /// <value>
    /// The minimum value.
    /// </value>
    public object? MinValue { get; init; }

    /// <summary>Gets the maximum value. .</summary>
    /// <value>
    /// The maximum value.
    /// </value>
    public object? MaxValue { get; init; }
}



