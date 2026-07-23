// -----------------------------------------------------------------------------
// File:        StreamExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for streams and byte arrays providing string/byte encoding conversions.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Text;

namespace Chishiki;

/// <summary>Extension methods for streams and byte arrays providing string/byte encoding conversions.</summary>
public static class StreamExtensions
{
    /// <summary>Converts a byte array to a string using the specified encoding.</summary>
    /// <param name="data">The byte array to convert.</param>
    /// <param name="encoding">The encoding to use for conversion. Defaults to the system default encoding if not specified.</param>
    /// <returns>A string representation of the byte array.</returns>
    public static string AsString(this byte[] data, Encoding? encoding = null) =>
        new((encoding ?? Encoding.Default).GetChars(data));

    /// <summary>Converts a string to a byte array using the specified encoding.</summary>
    /// <param name="data">The string to convert.</param>
    /// <param name="encoding">The encoding to use for conversion. Defaults to the system default encoding if not specified.</param>
    /// <returns>A byte array representation of the string.</returns>
    public static byte[] AsBytes(this string data, Encoding? encoding = null) =>
        (encoding ?? Encoding.Default).GetBytes(data);

    /// <summary>Converts a memory stream to a string using the specified encoding.</summary>
    /// <param name="stream">The memory stream to convert.</param>
    /// <param name="encoding">The encoding to use for conversion. Defaults to the system default encoding if not specified.</param>
    /// <returns>A string representation of the memory stream contents.</returns>
    public static string AsString(this MemoryStream stream, Encoding? encoding = null) =>
        stream.ToArray().AsString(encoding);

}
