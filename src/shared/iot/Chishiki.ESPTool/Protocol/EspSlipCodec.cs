// -----------------------------------------------------------------------------
// File:        EspSlipCodec.cs
// Author:      Piergiorgio Vagnozzi
// Description: Encodes and decodes SLIP-framed ESP ROM bootloader packets.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Protocol;

/// <summary>Encodes and decodes SLIP-framed ESP ROM bootloader packets.</summary>
internal static class EspSlipCodec
{
    /// <summary>Gets the SLIP frame delimiter.</summary>
    public const byte FrameDelimiter = 0xC0;

    /// <summary>Gets the SLIP escape byte.</summary>
    public const byte Escape = 0xDB;

    private const byte EscapedFrameDelimiter = 0xDC;
    private const byte EscapedEscape = 0xDD;

    /// <summary>Encodes a raw payload into a SLIP frame.</summary>
    /// <param name="payload">The raw payload to encode.</param>
    /// <returns>The encoded SLIP frame.</returns>
    public static byte[] Encode(ReadOnlySpan<byte> payload)
    {
        var frame = new List<byte>(payload.Length + 2)
        {
            FrameDelimiter
        };

        foreach (var value in payload)
        {
            switch (value)
            {
                case FrameDelimiter:
                    frame.Add(Escape);
                    frame.Add(EscapedFrameDelimiter);
                    break;
                case Escape:
                    frame.Add(Escape);
                    frame.Add(EscapedEscape);
                    break;
                default:
                    frame.Add(value);
                    break;
            }
        }

        frame.Add(FrameDelimiter);
        return [.. frame];
    }

    /// <summary>Translates an escaped SLIP byte into its decoded value.</summary>
    /// <param name="value">The escaped SLIP byte.</param>
    /// <returns>The decoded byte value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the escape sequence is invalid.</exception>
    public static byte DecodeEscapedByte(byte value) => value switch
    {
        EscapedFrameDelimiter => FrameDelimiter,
        EscapedEscape => Escape,
        _ => throw new InvalidOperationException("Invalid SLIP escape sequence.")
    };
}
