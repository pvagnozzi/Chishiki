// -----------------------------------------------------------------------------
// File:        SpecificationHelper.cs
// Author:      Piergiorgio Vagnozzi
// Description: Helper methods for mapping and converting specification query operators.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Globalization;

using Chishiki.Data.Query;

namespace Chishiki.Data.Specifications;

/// <summary>Specification helper class.</summary>
public static class SpecificationHelper
{
    /// <summary>Array separator. .</summary>
    private const string ArraySeparator = ",";

    /// <summary>Gets the filter condition operator from the query operator. .</summary>
    /// <param name="operator">Query operator.</param>
    /// <returns>Filter condition operator.</returns>
    /// <exception cref="NotImplementedException">Thrown when the operator is not implemented.</exception>
    public static FilterConditionOperator GetFilterConditionOperator(this QueryOperator @operator) =>
        @operator switch
        {
            QueryOperator.IsNull => FilterConditionOperator.IsNull,
            QueryOperator.IsNotNull => FilterConditionOperator.IsNotNull,
            QueryOperator.Equal => FilterConditionOperator.Equal,
            QueryOperator.NotEqual => FilterConditionOperator.NotEqual,
            QueryOperator.Lesser => FilterConditionOperator.Lesser,
            QueryOperator.LesserEqual => FilterConditionOperator.LesserEqual,
            QueryOperator.Greater => FilterConditionOperator.Greater,
            QueryOperator.GreaterEqual => FilterConditionOperator.GreaterEqual,
            QueryOperator.StartsWith => FilterConditionOperator.StartsWith,
            QueryOperator.EndsWith => FilterConditionOperator.EndsWith,
            QueryOperator.Contains => FilterConditionOperator.Contains,
            QueryOperator.NotContains => FilterConditionOperator.NotContains,
            QueryOperator.NotStartsWith => FilterConditionOperator.NotStartsWith,
            QueryOperator.NotEndsWith => FilterConditionOperator.NotEndsWith,
            QueryOperator.In => FilterConditionOperator.In,
            QueryOperator.NotIn => FilterConditionOperator.NotIn,
            _ => throw new NotImplementedException(),
        };

    /// <summary>Gets value and cast it to the correct type. .</summary>
    /// <param name="item">Item value.</param>
    /// <returns>Converted value.</returns>
    public static object? GetAndCastType(this IPropertyFilter item)
    {
        var value = item.GetValue();
        if (value is null) return null;

        var isArray = value.Contains(ArraySeparator) || (item.FilterOperator == QueryOperator.In) ||
                      (item.FilterOperator == QueryOperator.NotIn);

        return !isArray
            ? ConvertType(value, item.ValueType, item.TypeValueEnum)
            : value.Split(ArraySeparator).Select(x => ConvertType(x, item.ValueType, item.TypeValueEnum)).ToArray();
    }

    /// <summary>Converts the value to the specified type. .</summary>
    /// <param name="value">Source value.</param>
    /// <param name="type">Destination type.</param>
    /// <param name="itemTypeValue">Enum type value; mandatory if type is <see cref="QueryValueType.Enum"/>.</param>
    /// <returns>Converted type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when type is not recognized.</exception>
    private static object? ConvertType(string? value, QueryValueType type, string? itemTypeValue)
    {
        if (value is null) return null;
        return type switch
        {
            QueryValueType.String => value,
            QueryValueType.Integer => int.Parse(value, CultureInfo.InvariantCulture),
            QueryValueType.Double => double.Parse(value, CultureInfo.InvariantCulture),
            QueryValueType.Decimal => decimal.Parse(value, CultureInfo.InvariantCulture),
            QueryValueType.DateTimeOffset => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture),
            QueryValueType.Boolean => bool.Parse(value),
            QueryValueType.Guid => Guid.Parse(value),
            QueryValueType.Enum => ConvertEnumType(value, itemTypeValue!),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    /// <summary>Converts a string value to an enum type. .</summary>
    /// <param name="value">String representation of the enum value.</param>
    /// <param name="itemTypeValue">Fully qualified enum type name.</param>
    /// <returns>Parsed enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when itemTypeValue is null.</exception>
    /// <exception cref="ArgumentException">Thrown when enum type cannot be found.</exception>
    private static object ConvertEnumType(string value, string itemTypeValue)
    {
        ArgumentNullException.ThrowIfNull(itemTypeValue);

        // Try to get the type directly
        // If not found, search in all loaded assemblies
        var enumType = Type.GetType(itemTypeValue) ??
                       AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(a => a.GetTypes())
                           .FirstOrDefault(t => t.FullName == itemTypeValue || t.Name == itemTypeValue);

        if (enumType == null)
            throw new ArgumentException($"Invalid enum type: {itemTypeValue}");

        return Enum.Parse(enumType, value);
    }
}



