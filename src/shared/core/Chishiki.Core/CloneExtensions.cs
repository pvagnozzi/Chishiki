// -----------------------------------------------------------------------------
// File:        CloneExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension method for deep-cloning objects via JSON serialization.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Text.Json;

namespace Chishiki;

/// <summary>Extension methods for deep-cloning objects via JSON serialization.</summary>
public static class ExtensionMethods
{
    /// <summary>Creates a deep copy of the specified object using JSON serialization.</summary>
    /// <typeparam name="T">The type of object to clone.</typeparam>
    /// <param name="self">The object to clone.</param>
    /// <returns>A deep copy of the specified object.</returns>
    public static T DeepCopy<T>(this T self)
    {
        var serialized = JsonSerializer.Serialize(self);
        var result = JsonSerializer.Deserialize<T>(serialized) ?? default!;
        return result;
    }
}
