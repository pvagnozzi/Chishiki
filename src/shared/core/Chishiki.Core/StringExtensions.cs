// -----------------------------------------------------------------------------
// File:        StringExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for string manipulation including capitalization and GUID conversion.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Core;

/// <summary>Extension methods for string manipulation including capitalization and GUID conversion.</summary>
public static class StringExtensions
{
    /// <summary>Capitalizes the first character of the input string.</summary>
    /// <param name="input">The input string to capitalize.</param>
    /// <returns>The string with the first character capitalized.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the input is an empty string.</exception>
    public static string Capitalize(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(char.ToUpperInvariant(input[0]).ToString(), input.AsSpan(1))
        };

    /// <summary>Converts a string to a <see cref="Guid"/>, returning <see cref="Guid.Empty"/> if parsing fails.</summary>
    /// <param name="input">The string to parse as a GUID.</param>
    /// <returns>The parsed <see cref="Guid"/>, or <see cref="Guid.Empty"/> if the string cannot be parsed.</returns>
    public static Guid ToGuid(this string input) =>
        Guid.TryParse(input, out var result) ? result : Guid.Empty;

    /// <summary>Converts a string to a <see cref="Guid"/>, generating a new GUID if parsing fails.</summary>
    /// <param name="input">The string to parse as a GUID.</param>
    /// <returns>The parsed <see cref="Guid"/>, or a newly generated <see cref="Guid"/> if the string cannot be parsed.</returns>
    public static Guid ToCorrelationId(this string input) =>
        Guid.TryParse(input, out var result) ? result : Guid.NewGuid();
}
