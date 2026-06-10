// -----------------------------------------------------------------------------
// File:        EspProtocolTransport.cs
// Author:      Piergiorgio Vagnozzi
// Description: Implements framed ROM bootloader command transport over ISerialConnection.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Buffers.Binary;
using Chishiki.ESPTool.Exceptions;
using Chishiki.Serial.Abstractions;

namespace Chishiki.ESPTool.Protocol;

/// <summary>Provides framed ROM bootloader command transport over <see cref="ISerialConnection"/>.</summary>
internal sealed class EspProtocolTransport(ISerialConnection connection, int statusLength)
{
    private const byte RequestDirection = 0x00;
    private const byte ResponseDirection = 0x01;

    private readonly ISerialConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly int _statusLength = statusLength > 0 ? statusLength : throw new ArgumentOutOfRangeException(nameof(statusLength));

    /// <summary>Sends a command and waits for the matching response.</summary>
    /// <param name="command">The command to send.</param>
    /// <param name="payload">The serialized command payload.</param>
    /// <param name="checksum">The command checksum.</param>
    /// <param name="timeout">The overall timeout for the command.</param>
    /// <param name="attempts">The maximum number of attempts.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The parsed command response.</returns>
    /// <exception cref="EspToolTimeoutException">Thrown when a response is not received in time.</exception>
    public async Task<EspCommandResponse> ExecuteCommandAsync(EspCommandCode command, ReadOnlyMemory<byte> payload, uint checksum, TimeSpan timeout, int attempts, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(attempts);

        EspToolException? lastException = null;

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _connection.DiscardInBuffer();
            await WriteCommandAsync(command, payload, checksum, cancellationToken).ConfigureAwait(false);

            try
            {
                return await ReadResponseAsync(command, timeout, cancellationToken).ConfigureAwait(false);
            }
            catch (EspToolTimeoutException ex) when (attempt < attempts - 1)
            {
                lastException = ex;
            }
        }

        throw lastException ?? new EspToolTimeoutException($"Timed out waiting for a response to command '{command}'.");
    }

    #region Low-Level SLIP

    /// <summary>Writes a raw SLIP packet to the serial connection.</summary>
    /// <param name="payload">The unescaped payload to encode and write.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task WriteSlipPacketAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        var framedPacket = EspSlipCodec.Encode(payload.Span);
        await _connection.WriteAsync(framedPacket, 0, framedPacket.Length, cancellationToken).ConfigureAwait(false);
        await _connection.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads and decodes the next raw SLIP packet from the serial connection.</summary>
    /// <param name="timeout">The overall timeout to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The decoded SLIP payload.</returns>
    public async Task<byte[]> ReadSlipPacketAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        return await ReadFrameAsync(deadline, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Private Helpers

    /// <summary>Writes a framed command request to the serial connection.</summary>
    /// <param name="command">The command to send.</param>
    /// <param name="payload">The command payload.</param>
    /// <param name="checksum">The request checksum.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task WriteCommandAsync(EspCommandCode command, ReadOnlyMemory<byte> payload, uint checksum, CancellationToken cancellationToken)
    {
        var rawPacket = new byte[8 + payload.Length];
        rawPacket[0] = RequestDirection;
        rawPacket[1] = (byte)command;
        BinaryPrimitives.WriteUInt16LittleEndian(rawPacket.AsSpan(2, 2), (ushort)payload.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(rawPacket.AsSpan(4, 4), checksum);
        payload.Span.CopyTo(rawPacket.AsSpan(8));

        await WriteSlipPacketAsync(rawPacket, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads a response frame and parses it into a command response.</summary>
    /// <param name="expectedCommand">The expected response command.</param>
    /// <param name="timeout">The overall timeout to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The parsed response.</returns>
    private async Task<EspCommandResponse> ReadResponseAsync(EspCommandCode expectedCommand, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var frame = await ReadFrameAsync(deadline, cancellationToken).ConfigureAwait(false);
            var response = ParseResponse(frame);

            if (response.Command == expectedCommand)
            {
                return response;
            }
        }

        throw new EspToolTimeoutException($"Timed out waiting for command '{expectedCommand}' response.");
    }

    /// <summary>Reads and decodes the next SLIP frame from the serial connection.</summary>
    /// <param name="deadline">The absolute deadline for the read operation.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The decoded frame payload.</returns>
    private async Task<byte[]> ReadFrameAsync(DateTime deadline, CancellationToken cancellationToken)
    {
        var frame = new List<byte>(256);
        var insideFrame = false;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await ReadByteAsync(deadline, cancellationToken).ConfigureAwait(false);

            if (value == EspSlipCodec.FrameDelimiter)
            {
                if (!insideFrame)
                {
                    insideFrame = true;
                    frame.Clear();
                    continue;
                }

                if (frame.Count == 0)
                {
                    continue;
                }

                return [.. frame];
            }

            if (!insideFrame)
            {
                continue;
            }

            if (value == EspSlipCodec.Escape)
            {
                var escapedValue = await ReadByteAsync(deadline, cancellationToken).ConfigureAwait(false);
                frame.Add(EspSlipCodec.DecodeEscapedByte(escapedValue));
                continue;
            }

            frame.Add(value);
        }

        throw new EspToolTimeoutException("Timed out waiting for a SLIP frame.");
    }

    /// <summary>Reads a single byte from the serial connection before the specified deadline.</summary>
    /// <param name="deadline">The absolute deadline for the read operation.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The next byte value.</returns>
    private async Task<byte> ReadByteAsync(DateTime deadline, CancellationToken cancellationToken)
    {
        var buffer = new byte[1];

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var bytesRead = await _connection.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 1)
                {
                    return buffer[0];
                }
            }
            catch (TimeoutException)
            {
                // Short serial timeouts are expected while polling for the next protocol byte.
            }
        }

        throw new EspToolTimeoutException("Timed out waiting for serial data.");
    }

    /// <summary>Parses a raw response frame.</summary>
    /// <param name="frame">The decoded response frame.</param>
    /// <returns>The parsed response object.</returns>
    private EspCommandResponse ParseResponse(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 8)
        {
            throw new EspToolException("Received an incomplete ROM bootloader response header.");
        }

        if (frame[0] != ResponseDirection)
        {
            throw new EspToolException($"Received an unexpected ROM bootloader response direction '{frame[0]}'.");
        }

        var command = (EspCommandCode)frame[1];
        var payloadLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.Slice(2, 2));
        var value = BinaryPrimitives.ReadUInt32LittleEndian(frame.Slice(4, 4));
        var payloadWithStatus = frame.Slice(8);

        if (payloadWithStatus.Length != payloadLength)
        {
            throw new EspToolException($"Received a ROM bootloader response with payload length {payloadWithStatus.Length}, expected {payloadLength}.");
        }

        if (payloadWithStatus.Length < _statusLength)
        {
            throw new EspToolException("Received a ROM bootloader response without the required status trailer.");
        }

        var status = payloadWithStatus[^_statusLength..];
        var payload = payloadWithStatus[..^_statusLength].ToArray();
        var statusCode = status[0];
        var errorCode = status.Length > 1 ? status[1] : (byte)0;

        return statusCode != 0
            ? throw new EspToolProtocolException($"ROM bootloader command '{command}' failed with status {statusCode} and error {errorCode}.", statusCode, errorCode)
            : new EspCommandResponse(command, value, payload);
    }

    #endregion
}
