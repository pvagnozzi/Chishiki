// -----------------------------------------------------------------------------
// File:        AnnotationExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for reading annotation attributes and building property settings metadata.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Reflection;
using Chishiki.Data.Query;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Chishiki.Data.Annotations;

/// <summary>Annotation extensions.</summary>
public static class AnnotationExtensions
{
    /// <summary>Gets the type properties settings. .</summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static PropertySettings[] GetTypePropertiesSettings(this Type type)
    {
        var settings = new List<PropertySettings>();

        for (var currentType = type; currentType is not null && currentType != typeof(object); currentType = currentType.BaseType)
        {
            var properties =
                currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            settings.InsertRange(0, properties.Select(GetPropertySettings).Where(x => !x.IsScaffold));
        }
        return [.. settings];
    }

    /// <summary>Gets the value. .</summary>
    /// <param name="property">The property.</param>
    /// <param name="instance">The instance.</param>
    /// <returns></returns>
    public static object? GetValue(this PropertySettings property, object? instance) =>
        property.PropertyInfo.GetValue(instance);

    /// <summary>Sets the value. .</summary>
    /// <param name="property">The property.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="value">The value.</param>
    public static void SetValue(this PropertySettings property, object? instance, object? value) =>
        property.PropertyInfo.SetValue(instance, value);

    /// <summary>Gets the display value. .</summary>
    /// <param name="property">The property.</param>
    /// <param name="instance">The instance.</param>
    /// <returns></returns>
    public static string GetDisplayValue(this PropertySettings property, object? instance)
    {
        var value = property.GetValue(instance);

        return value is null
            ? property.NullDisplayText ?? string.Empty
            : !string.IsNullOrEmpty(property.DataFormatString)
            ? string.Format(CultureInfo.InvariantCulture, property.DataFormatString, value)
            : value.ToString() ?? string.Empty;
    }

    /// <summary>Gets the type of the editor. .</summary>
    /// <param name="property">The property.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static EditorType GetEditorType(this PropertyInfo property)
    {
        var attr = property.GetCustomAttribute<EditorTypeAttribute>();
        if (attr is not null)
        {
            return attr.EditorType;
        }

        var propertyType = property.PropertyType;
        return propertyType.IsBoolean()
            ? EditorType.Checkbox
            : propertyType.IsNumeric() ? EditorType.Number : propertyType.IsDateTime() ? EditorType.DateTime : EditorType.Text;
    }

    /// <summary>Gets the property settings. .</summary>
    /// <param name="property">The property.</param>
    /// <returns></returns>
    public static PropertySettings GetPropertySettings(this PropertyInfo property)
    {
        var (DataFormatString, NullDisplayText) = property.GetDisplayFormat();
        var sortable = property.GetSortable();
        var filterable = property.GetFilterable();
        var rangeAttribute = property.GetCustomAttribute<RangeAttribute>();
        return new(property)
        {
            Name = property.Name,
            DisplayName = property.GetDisplayName(),
            DataFormatString = DataFormatString,
            DefaultValue = property.GetDefaultValue(),
            EditorType = property.GetEditorType(),
            IsKey = property.IsKey(),
            IsScaffold = property.IsScaffold(),
            MinValue = rangeAttribute?.Minimum,
            MaxValue = rangeAttribute?.Maximum,
            NullDisplayText = NullDisplayText,
            Required = property.IsRequired(),
            Sortable = sortable?.Sortable ?? false,
            SortExpression = sortable?.SortExpression ?? property.Name,
            Filterable = filterable?.Filterable ?? false,
            FilterField = filterable?.FilterField ?? property.Name,
            FilterQueryOperator = filterable?.FilterQueryOperator ?? QueryOperator.Contains,
            FilterQueryValueType = filterable?.FilterQueryValueType ?? QueryValueType.String,
            FilterIgnoreCasing = filterable?.FilterIgnoreCasing ?? false,
            FilterQueryTypeEnum = filterable?.FilterQueryTypeEnum
        };
    }

    /// <summary>Gets the sortable. .</summary>
    /// <param name="property">The property.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static SortableAttribute? GetSortable(this PropertyInfo property) =>
        property.GetCustomAttribute<SortableAttribute>();

    /// <summary>Gets the filterable attribute. .</summary>
    /// <param name="property">The property.</param>
    /// <returns>The filterable attribute, or null if not present.</returns>
    [DebuggerStepThrough]
    public static FilterableAttribute? GetFilterable(this PropertyInfo property) =>
        property.GetCustomAttribute<FilterableAttribute>();

    /// <summary>Gets the display name. .</summary>
    /// <param name="property">The property.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string GetDisplayName(this PropertyInfo property) =>
        property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;

    /// <summary>Gets the display format. .</summary>
    /// <param name="property">The property.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static (string? DataFormatString, string? NullDisplayText) GetDisplayFormat(this PropertyInfo property)
    {
        var display = property.GetCustomAttribute<DisplayFormatAttribute>();
        return (display?.DataFormatString, display?.NullDisplayText);
    }

    /// <summary>Determines whether this instance is scaffold. .</summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns>
    ///   <c>true</c> if the specified property information is scaffold; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsScaffold(this PropertyInfo propertyInfo) =>
        propertyInfo.GetCustomAttribute<ScaffoldColumnAttribute>()?.Scaffold ?? false;

    /// <summary>Determines whether this instance is required. .</summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns>
    ///   <c>true</c> if the specified property information is required; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsRequired(this PropertyInfo propertyInfo) =>
        propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;

    /// <summary>Determines whether this instance is key. .</summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns>
    ///   <c>true</c> if the specified property information is key; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsKey(this PropertyInfo propertyInfo) => propertyInfo.GetCustomAttribute<KeyAttribute>() != null;

    /// <summary>Determines whether this instance is visible. .</summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns>
    ///   <c>true</c> if the specified property information is visible; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsVisible(this PropertyInfo propertyInfo) => !propertyInfo.IsScaffold();

    /// <summary>Gets the default value. .</summary>
    /// <param name="propertyInfo">The property information.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static object? GetDefaultValue(this PropertyInfo propertyInfo) =>
        propertyInfo.GetCustomAttribute<DefaultValueAttribute>()?.Value;

    /// <summary>Gets the visible properties. .</summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static IEnumerable<PropertyInfo> GetVisibleProperties(this Type type) =>
        type.GetProperties().Where(p => p.IsVisible());
}



