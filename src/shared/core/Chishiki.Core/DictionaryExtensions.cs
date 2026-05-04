// -----------------------------------------------------------------------------
// File:        DictionaryExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for IDictionary providing safe value retrieval with default fallback.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Diagnostics;

namespace Chishiki.Core;

/// <summary>Extension methods for <see cref="IDictionary{TKey, TValue}"/> providing safe value retrieval.</summary>
public static class DictionaryExtensions
{
    /// <summary>Retrieves the value associated with the specified key, or returns a default value if the key is not found.</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="defaultValue">The value to return if the key is not found.</param>
    /// <returns>The value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if not found.</returns>
    [DebuggerStepThrough]
    public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
        TValue defaultValue) =>
        dictionary.TryGetValue(key, out var value) ? value : defaultValue;

    /// <summary>Retrieves the string value associated with the specified key, or returns an empty string if the key is not found.</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="defaultValue">The value to return if the key is not found. Defaults to empty string.</param>
    /// <returns>The string value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if not found.</returns>
    [DebuggerStepThrough]
    public static string
        GetValue<TKey>(this IDictionary<TKey, string> dictionary, TKey key, string defaultValue = "") =>
        dictionary.GetValue<TKey, string>(key, defaultValue);
}
