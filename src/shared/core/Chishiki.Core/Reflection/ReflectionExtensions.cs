// -----------------------------------------------------------------------------
// File:        ReflectionExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for reflection-based type inspection, type classification, and method invocation.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Chishiki.Reflection;

/// <summary>Extension methods for reflection-based type inspection, type classification, and method invocation.</summary>
public static class ReflectionExtensions
{
    /// <summary>Determines whether the specified type implements the interface <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The interface type to check for implementation.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type implements the interface; otherwise, false.</returns>
    [DebuggerStepThrough]
    public static bool Implements<T>(this Type type) => type.IsAssignableTo(typeof(T));

    /// <summary>Determines whether the specified type implements the given interface type.</summary>
    /// <param name="type">The type to check.</param>
    /// <param name="implementingType">The interface type to check for implementation.</param>
    /// <returns>True if the type implements the interface; otherwise, false.</returns>
    [DebuggerStepThrough]
    public static bool Implements(this Type type, Type implementingType) => type.IsAssignableTo(implementingType);

    /// <summary>Gets all types in the assembly that implement the specified interface.</summary>
    /// <typeparam name="T">The interface type to search for.</typeparam>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>An array of types that implement the interface.</returns>
    [DebuggerStepThrough]
    public static Type[] GetTypesImplementing<T>(this Assembly assembly) =>
        assembly.GetTypes().Where(t => t.Implements<T>()).ToArray();

    /// <summary>Gets all types in the assembly that implement the specified interface.</summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="type">The interface type to search for.</param>
    /// <returns>An array of types that implement the interface.</returns>
    [DebuggerStepThrough]
    public static Type[] GetTypesImplementing(this Assembly assembly, Type type) =>
        assembly.GetTypes().Where(t => t.Implements(type)).ToArray();

    /// <summary>Determines whether the specified type is a boolean type (bool or bool?).</summary>
    /// <param name="propertyType">The type to check.</param>
    /// <returns>True if the type is bool or bool?; otherwise, false.</returns>
    [DebuggerStepThrough]
    public static bool IsBoolean(this Type propertyType) =>
        propertyType == typeof(bool) || propertyType == typeof(bool?);

    /// <summary>Determines whether the specified type is a binary type (byte[] or Stream).</summary>
    /// <param name="propertyType">The type to check.</param>
    /// <returns>True if the type is byte[] or Stream; otherwise, false.</returns>
    [DebuggerStepThrough]
    public static bool IsBinary(this Type propertyType) =>
        propertyType == typeof(byte[]) || propertyType == typeof(Stream);

    /// <summary>Determines whether [is date time]. .</summary>
    /// <param name="propertyType">Type of the property.</param>
    /// <returns>
    ///   <c>true</c> if [is date time] [the specified property type]; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsDateTime(this Type propertyType) => propertyType == typeof(DateTime) ||
                                                             propertyType == typeof(DateTime?) ||
                                                             propertyType == typeof(DateTimeOffset) ||
                                                             propertyType == typeof(DateTimeOffset?);

    /// <summary>Determines whether the specified type is a numeric type.</summary>
    /// <param name="propertyType">The type to check.</param>
    /// <returns>True if the type is a numeric type; otherwise, false.</returns>
    [DebuggerStepThrough]
    public static bool IsNumeric(this Type propertyType) =>
        propertyType == typeof(int) || propertyType == typeof(int?) ||
        propertyType == typeof(decimal) || propertyType == typeof(decimal?) ||
        propertyType == typeof(double) || propertyType == typeof(double?) ||
        propertyType == typeof(float) || propertyType == typeof(float?) ||
        propertyType == typeof(long) || propertyType == typeof(long?) ||
        propertyType == typeof(short) || propertyType == typeof(short?) ||
        propertyType == typeof(byte) || propertyType == typeof(byte?) ||
        propertyType == typeof(sbyte) || propertyType == typeof(sbyte?) ||
        propertyType == typeof(uint) || propertyType == typeof(uint?) ||
        propertyType == typeof(ulong) || propertyType == typeof(ulong?) ||
        propertyType == typeof(ushort) || propertyType == typeof(ushort?);

    /// <summary>Determines whether this instance is scalar. .</summary>
    /// <param name="type">The type.</param>
    /// <returns>
    ///   <c>true</c> if the specified type is scalar; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsScalar(this Type type) => type == typeof(string) || type.IsNumeric() || type.IsDateTime() ||
                                                   type.IsBoolean() || type.IsBinary();

    /// <summary>Determines whether the specified type is enumerable (implements <see cref="IEnumerable"/>).</summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is enumerable; otherwise, false.</returns>
    [DebuggerStepThrough]
    public static bool IsEnumerable(this Type type) => typeof(IEnumerable).IsAssignableFrom(type);

    /// <summary>Invokes a static method on the specified type with the provided parameter and value.</summary>
    /// <param name="type">The type containing the static method.</param>
    /// <param name="methodName">The name of the static method to invoke.</param>
    /// <param name="parameterType">The type of the parameter that the method accepts.</param>
    /// <param name="value">The value to pass to the method.</param>
    /// <exception cref="InvalidCastException">Thrown when the method is not found on the type.</exception>
    [DebuggerStepThrough]
    public static void InvokeStaticMethod(this Type type, string methodName, Type parameterType, object value)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, [parameterType]);
        if (method is null)
        {
            throw new InvalidCastException($"{type} has no method {methodName}({parameterType})");
        }

        method.Invoke(null, [value]);
    }
}
