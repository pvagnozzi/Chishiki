// -----------------------------------------------------------------------------
// File:        ResourceExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for Assembly resource management and embedded text retrieval.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Reflection;

namespace Chishiki;

/// <summary>Extension methods for Assembly resource management and embedded text retrieval.</summary>
public static class ResourceExtensions
{
    /// <summary>Lists all resource names in the assembly, optionally filtered by folder name.</summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="folderName">Optional folder name to filter resources by.</param>
    /// <returns>An array of resource names in the assembly, optionally filtered by folder.</returns>
    public static string[] ListResources(this Assembly assembly, string? folderName = null)
    {
        string[] items = assembly.GetManifestResourceNames();
        if (!string.IsNullOrEmpty(folderName))
        {
            items = items.Where(x => x.StartsWith(folderName, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        return items;
    }

    /// <summary>Retrieves the text content of an embedded resource in the assembly.</summary>
    /// <param name="assembly">The assembly containing the resource.</param>
    /// <param name="resourceName">The full name of the resource to retrieve.</param>
    /// <returns>The text content of the resource.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified resource is not found in the assembly.</exception>
    public static string GetResourceText(this Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly}'");
        }

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
