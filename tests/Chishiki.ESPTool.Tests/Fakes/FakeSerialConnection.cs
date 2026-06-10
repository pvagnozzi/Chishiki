// -----------------------------------------------------------------------------
// File:        FakeSerialConnection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides fake serial abstractions for focused ESPTool tests.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Buffers.Binary;
#pragma warning disable CS0067
using Chishiki.ESPTool.Protocol;
using Chishiki.Serial.Abstractions;

namespace Chishiki.ESPTool.Tests.Fakes;

/// <summary>Provides an in-memory implementation of <see cref="ISerialConnection"/> for deterministic protocol tests.</summary>
internal sealed class FakeSerialConnection(SerialConnectionOptions options) : ISerialConnection
{
    private readonly Queue<byte> _readBuffer = new();

    /// <inheritdoc/>
    public event EventHandler<SerialDataReceivedEventArgs>? DataReceived;

    /// <inheritdoc/>
    public event EventHandler<SerialTextReceivedEventArgs>? TextReceived;

    /// <inheritdoc/>
    public event EventHandler<SerialErrorEventArgs>? ErrorReceived;

    /// <inheritdoc/>
    public event EventHandler<SerialSignalChangedEventArgs>? SignalChanged;

    /// <summary>Gets the captured binary writes made through the connection.</summary>
    public List<byte[]> WrittenBuffers { get; } = [];

    /// <summary>Gets the number of times the connection was opened.</summary>
    public int OpenCount { get; private set; }

    /// <summary>Gets the number of times the connection was closed.</summary>
    public int CloseCount { get; private set; }

    /// <inheritdoc/>
    public SerialConnectionOptions Options { get; } = options;

    /// <inheritdoc/>
    public bool IsOpen { get; private set; }

    /// <inheritdoc/>
    public bool DtrEnable { get; set; }

    /// <inheritdoc/>
    public bool RtsEnable { get; set; }

    /// <inheritdoc/>
    public bool CtsHolding => true;

    /// <inheritdoc/>
    public bool DsrHolding => true;

    /// <inheritdoc/>
    public bool CdHolding => true;

    /// <inheritdoc/>
    public int BytesToRead => _readBuffer.Count;

    /// <inheritdoc/>
    public int BytesToWrite => 0;

    /// <summary>Queues a successful protocol response frame.</summary>
    /// <param name="command">The command associated with the response.</param>
    /// <param name="value">The 32-bit response value.</param>
    /// <param name="payload">The optional payload without the status trailer.</param>
    /// <param name="statusLength">The status trailer length to append.</param>
    /// <param name="status">The response status byte.</param>
    /// <param name="errorCode">The optional error byte.</param>
    public void QueueResponse(EspCommandCode command, uint value = 0, byte[]? payload = null, int statusLength = 4, byte status = 0, byte errorCode = 0)
    {
        payload ??= [];
        var payloadWithStatus = new byte[payload.Length + statusLength];
        payload.CopyTo(payloadWithStatus, 0);

        if (statusLength > 0)
        {
            payloadWithStatus[payload.Length] = status;
        }

        if (statusLength > 1)
        {
            payloadWithStatus[payload.Length + 1] = errorCode;
        }

        var rawPacket = new byte[8 + payloadWithStatus.Length];
        rawPacket[0] = 0x01;
        rawPacket[1] = (byte)command;
        BinaryPrimitives.WriteUInt16LittleEndian(rawPacket.AsSpan(2, 2), (ushort)payloadWithStatus.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(rawPacket.AsSpan(4, 4), value);
        payloadWithStatus.CopyTo(rawPacket, 8);

        QueueSlipPacket(rawPacket);
    }

    /// <summary>Queues a raw SLIP packet payload for low-level stub-loader flows.</summary>
    /// <param name="payload">The unescaped payload to queue.</param>
    public void QueueSlipPacket(byte[] payload)
    {
        var frame = EspSlipCodec.Encode(payload);
        foreach (var valueByte in frame)
        {
            _readBuffer.Enqueue(valueByte);
        }
    }

    /// <inheritdoc/>
    public void Open()
    {
        IsOpen = true;
        OpenCount++;
    }

    /// <inheritdoc/>
    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Open();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Close()
    {
        IsOpen = false;
        CloseCount++;
    }

    /// <inheritdoc/>
    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Close();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsOpen)
        {
            throw new InvalidOperationException("The fake serial connection is closed.");
        }

        if (_readBuffer.Count == 0)
        {
            throw new TimeoutException("No fake serial data is available.");
        }

        var bytesRead = 0;
        while (bytesRead < count && _readBuffer.Count > 0)
        {
            buffer[offset + bytesRead] = _readBuffer.Dequeue();
            bytesRead++;
        }

        return Task.FromResult(bytesRead);
    }

    /// <inheritdoc/>
    public string ReadExistingText() => string.Empty;

    /// <inheritdoc/>
    public Task<string> ReadExistingTextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(string.Empty);
    }

    /// <inheritdoc/>
    public string ReadLine() => string.Empty;

    /// <inheritdoc/>
    public Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(string.Empty);
    }

    /// <inheritdoc/>
    public void Write(byte[] buffer, int offset, int count)
    {
        var copy = new byte[count];
        Array.Copy(buffer, offset, copy, 0, count);
        WrittenBuffers.Add(copy);
    }

    /// <inheritdoc/>
    public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Write(buffer, offset, count);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void WriteText(string text)
    {
    }

    /// <inheritdoc/>
    public Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void WriteLine(string text)
    {
    }

    /// <inheritdoc/>
    public Task WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Flush()
    {
    }

    /// <inheritdoc/>
    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void DiscardInBuffer()
    {
    }

    /// <inheritdoc/>
    public void DiscardOutBuffer()
    {
    }

    /// <inheritdoc/>
    public void CancelPendingOperations()
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        IsOpen = false;
    }
}

/// <summary>Provides a queue-backed factory for fake serial connections.</summary>
#pragma warning restore CS0067

internal sealed class FakeSerialConnectionFactory : ISerialConnectionFactory
{
    private readonly Queue<FakeSerialConnection> _connections = new();

    /// <summary>Gets the captured connection-creation options.</summary>
    public List<SerialConnectionOptions> CreatedOptions { get; } = [];

    /// <summary>Queues a fake connection that will be returned by the next create call.</summary>
    /// <param name="connection">The connection to queue.</param>
    public void EnqueueConnection(FakeSerialConnection connection) => _connections.Enqueue(connection);

    /// <inheritdoc/>
    public IReadOnlyList<string> GetAvailablePorts() => ["COM1"];

    /// <inheritdoc/>
    public ISerialConnection CreateConnection(string portName, SerialConnectionOptions options)
    {
        CreatedOptions.Add(options);

        if (_connections.Count == 0)
        {
            return new FakeSerialConnection(options);
        }

        return _connections.Dequeue();
    }
}
