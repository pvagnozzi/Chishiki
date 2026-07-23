// ------------- ---------------------------------------------------------------
// File:        SerialConnection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Wrapper for System.IO.Ports.SerialPort implementing ISerialConnection.
// Created:     2026-04-26
// Modified:    2026-04-26
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.IO.Ports;
using System.Text;
using Chishiki.Serial.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chishiki.Serial.Common;

/// <summary>Wrapper implementation of <see cref="ISerialConnection"/> using <see cref="SerialPort"/>.</summary>
public sealed partial class SerialConnection : Disposable, ISerialConnection
{
    /// <summary>
    /// Serial port instance used for communication.
    /// </summary>
    private readonly SerialPort _serialPort;

    /// <summary>Initializes a new instance of the <see cref="SerialConnection"/> class.</summary>
    /// <param name="portName">The name of the serial port (e.g., "COM1", "/dev/ttyUSB0").</param>
    /// <param name="options">Options for configuring the serial connection.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="portName"/> or <paramref name="logger"/> is null.</exception>
    internal SerialConnection(string portName, SerialConnectionOptions options, ILogger<SerialConnection> logger) : base(logger)
    {
        ArgumentNullException.ThrowIfNull(portName, nameof(portName));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        SerialConnectionLogging.LogConstructorInitializing(logger, portName);

        Options = options;
        _serialPort = CreateAndConfigureSerialPort(portName, options);
        _serialPort.DataReceived += OnSystemDataReceived;
        _serialPort.ErrorReceived += OnSystemErrorReceived;
        _serialPort.PinChanged += OnSystemPinChanged;

        SerialConnectionLogging.LogConstructorCompleted(logger, portName);
    }

    /// <inheritdoc />
    public event EventHandler<Abstractions.SerialDataReceivedEventArgs>? DataReceived;

    /// <inheritdoc />
    public event EventHandler<SerialTextReceivedEventArgs>? TextReceived;

    /// <inheritdoc />
    public event EventHandler<SerialErrorEventArgs>? ErrorReceived;

    /// <inheritdoc />
    public event EventHandler<SerialSignalChangedEventArgs>? SignalChanged;

    /// <inheritdoc />
    public SerialConnectionOptions Options { get; }

    /// <inheritdoc />
    public bool IsOpen => _serialPort.IsOpen;

    /// <inheritdoc />
    public bool DtrEnable
    {
        get => _serialPort.DtrEnable;
        set => _serialPort.DtrEnable = value;
    }

    /// <inheritdoc />
    public bool RtsEnable
    {
        get => _serialPort.RtsEnable;
        set => _serialPort.RtsEnable = value;
    }

    /// <inheritdoc />
    public bool CtsHolding => _serialPort.CtsHolding;

    /// <inheritdoc />
    public bool DsrHolding => _serialPort.DsrHolding;

    /// <inheritdoc />
    public bool CdHolding => _serialPort.CDHolding;

    /// <inheritdoc />
    public int BytesToRead => _serialPort.BytesToRead;

    /// <inheritdoc />
    public int BytesToWrite => _serialPort.BytesToWrite;

    /// <inheritdoc />
    public void Open()
    {
        SerialConnectionLogging.LogOpenRequested(Logger);
        try
        {
            _serialPort.Open();
            SerialConnectionLogging.LogOpenSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogOpenFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        SerialConnectionLogging.LogOpenAsyncRequested(Logger);
        try
        {
            await Task.Run(_serialPort.Open, cancellationToken);
            SerialConnectionLogging.LogOpenAsyncSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogOpenAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void Close()
    {
        SerialConnectionLogging.LogCloseRequested(Logger);
        try
        {
            _serialPort.Close();
            SerialConnectionLogging.LogCloseSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogCloseFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        SerialConnectionLogging.LogCloseAsyncRequested(Logger);
        try
        {
            await Task.Run(_serialPort.Close, cancellationToken);
            SerialConnectionLogging.LogCloseAsyncSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogCloseAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        SerialConnectionLogging.LogReadRequested(Logger, offset, count);
        try
        {
            var bytesRead = _serialPort.Read(buffer, offset, count);
            SerialConnectionLogging.LogReadSuccess(Logger, bytesRead);
            return bytesRead;
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogReadFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        SerialConnectionLogging.LogReadAsyncRequested(Logger, offset, count);
        try
        {
            var bytesRead = await Task.Run(() => _serialPort.Read(buffer, offset, count), cancellationToken);
            SerialConnectionLogging.LogReadAsyncSuccess(Logger, bytesRead);
            return bytesRead;
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogReadAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public string ReadExistingText()
    {
        SerialConnectionLogging.LogReadExistingTextRequested(Logger);
        try
        {
            var text = _serialPort.ReadExisting();
            SerialConnectionLogging.LogReadExistingTextSuccess(Logger, text.Length);
            return text;
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogReadExistingTextFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> ReadExistingTextAsync(CancellationToken cancellationToken = default)
    {
        SerialConnectionLogging.LogReadExistingTextAsyncRequested(Logger);
        try
        {
            var text = await Task.Run(() => _serialPort.ReadExisting(), cancellationToken);
            SerialConnectionLogging.LogReadExistingTextAsyncSuccess(Logger, text.Length);
            return text;
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogReadExistingTextAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public string ReadLine()
    {
        SerialConnectionLogging.LogReadLineRequested(Logger);
        try
        {
            var line = _serialPort.ReadLine();
            SerialConnectionLogging.LogReadLineSuccess(Logger, line.Length);
            return line;
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogReadLineFailed(Logger!, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        SerialConnectionLogging.LogReadLineAsyncRequested(Logger);
        try
        {
            var line = await Task.Run(() => _serialPort.ReadLine(), cancellationToken);
            SerialConnectionLogging.LogReadLineAsyncSuccess(Logger, line.Length);
            return line;
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogReadLineAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        SerialConnectionLogging.LogWriteRequested(Logger, count);
        try
        {
            _serialPort.Write(buffer, offset, count);
            SerialConnectionLogging.LogWriteSuccess(Logger, count);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogWriteFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        SerialConnectionLogging.LogWriteAsyncRequested(Logger, count);
        try
        {
            await Task.Run(() => _serialPort.Write(buffer, offset, count), cancellationToken);
            SerialConnectionLogging.LogWriteAsyncSuccess(Logger, count);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogWriteAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void WriteText(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        SerialConnectionLogging.LogWriteTextRequested(Logger, text.Length);
        try
        {
            _serialPort.Write(text);
            SerialConnectionLogging.LogWriteTextSuccess(Logger, text.Length);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogWriteTextFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        SerialConnectionLogging.LogWriteTextAsyncRequested(Logger, text.Length);
        try
        {
            await Task.Run(() => _serialPort.Write(text), cancellationToken);
            SerialConnectionLogging.LogWriteTextAsyncSuccess(Logger, text.Length);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogWriteTextAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void WriteLine(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        SerialConnectionLogging.LogWriteLineRequested(Logger, text.Length);
        try
        {
            _serialPort.WriteLine(text);
            SerialConnectionLogging.LogWriteLineSuccess(Logger, text.Length);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogWriteLineFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        SerialConnectionLogging.LogWriteLineAsyncRequested(Logger, text.Length);
        try
        {
            await Task.Run(() => _serialPort.WriteLine(text), cancellationToken);
            SerialConnectionLogging.LogWriteLineAsyncSuccess(Logger, text.Length);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogWriteLineAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void Flush()
    {
        SerialConnectionLogging.LogFlushRequested(Logger);
        try
        {
            _serialPort.BaseStream.Flush();
            SerialConnectionLogging.LogFlushSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogFlushFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        SerialConnectionLogging.LogFlushAsyncRequested(Logger);
        try
        {
            await Task.Run(_serialPort.BaseStream.Flush, cancellationToken);
            SerialConnectionLogging.LogFlushAsyncSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogFlushAsyncFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void DiscardInBuffer()
    {
        SerialConnectionLogging.LogDiscardInBufferRequested(Logger);
        try
        {
            _serialPort.DiscardInBuffer();
            SerialConnectionLogging.LogDiscardInBufferSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogDiscardInBufferFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void DiscardOutBuffer()
    {
        SerialConnectionLogging.LogDiscardOutBufferRequested(Logger);
        try
        {
            _serialPort.DiscardOutBuffer();
            SerialConnectionLogging.LogDiscardOutBufferSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogDiscardOutBufferFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public void CancelPendingOperations()
    {
        SerialConnectionLogging.LogCancelPendingOperationsRequested(Logger);
        try
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            SerialConnectionLogging.LogCancelPendingOperationsSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogCancelPendingOperationsFailed(Logger, ex);
            throw;
        }
    }

    /// <inheritdoc />
    protected override void DisposeManaged()
    {
        SerialConnectionLogging.LogDisposeRequested(Logger);
        try
        {
            _serialPort.DataReceived -= OnSystemDataReceived;
            _serialPort.ErrorReceived -= OnSystemErrorReceived;
            _serialPort.PinChanged -= OnSystemPinChanged;
            _serialPort.Dispose();
            SerialConnectionLogging.LogDisposeSuccess(Logger);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogDisposeFailed(Logger, ex);
            throw;
        }
    }

    /// <summary>Creates and configures a <see cref="SerialPort"/> from the provided port name and options.</summary>
    /// <param name="portName">The serial port name.</param>
    /// <param name="options">The serial connection options.</param>
    /// <returns>A configured <see cref="SerialPort"/> instance.</returns>
    private static SerialPort CreateAndConfigureSerialPort(string portName, SerialConnectionOptions options) =>
        new(portName)
        {
            BaudRate = options.BaudRate,
            DataBits = options.DataBits,
            Parity = options.Parity switch
            {
                SerialParity.None => Parity.None,
                SerialParity.Odd => Parity.Odd,
                SerialParity.Even => Parity.Even,
                SerialParity.Mark => Parity.Mark,
                SerialParity.Space => Parity.Space,
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.Parity, "Unsupported parity value.")
            },
            StopBits = options.StopBits switch
            {
                SerialStopBits.None => StopBits.None,
                SerialStopBits.One => StopBits.One,
                SerialStopBits.Two => StopBits.Two,
                SerialStopBits.OnePointFive => StopBits.OnePointFive,
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.StopBits, "Unsupported stop bits value.")
            },
            Handshake = options.Handshake switch
            {
                SerialHandshake.None => Handshake.None,
                SerialHandshake.XOnXOff => Handshake.XOnXOff,
                SerialHandshake.RequestToSend => Handshake.RequestToSend,
                SerialHandshake.RequestToSendXOnXOff => Handshake.RequestToSendXOnXOff,
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.Handshake, "Unsupported handshake value.")
            },
            ReadTimeout = options.ReadTimeout,
            WriteTimeout = options.WriteTimeout,
            DtrEnable = options.DtrEnable,
            RtsEnable = options.RtsEnable,
            Encoding = options.TextEncoding,
            NewLine = options.NewLine
        };

    /// <summary>
    /// Gets the list of available serial ports on the system.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SerialDataReceivedEventArgs that contains the event data.</param>
    private void OnSystemDataReceived(object? sender, System.IO.Ports.SerialDataReceivedEventArgs e)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            var eventType = e.EventType.ToString();
            SerialConnectionLogging.LogDataReceivedEventFired(Logger, eventType);
        }

        try
        {
            if (e.EventType == SerialData.Eof)
            {
                SerialConnectionLogging.LogDataReceivedEof(Logger);
                return;
            }

            var buffer = new byte[_serialPort.BytesToRead];
            var bytesRead = _serialPort.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                SerialConnectionLogging.LogDataReceivedBytesRead(Logger, bytesRead);
                var readBuffer = buffer.Take(bytesRead).ToArray();
                DataReceived?.Invoke(this, new Abstractions.SerialDataReceivedEventArgs(readBuffer));

                try
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    SerialConnectionLogging.LogDataReceivedTextDecoded(Logger, text.Length);
                    TextReceived?.Invoke(this, new SerialTextReceivedEventArgs(text));
                }
                catch (DecoderFallbackException ex)
                {
                    SerialConnectionLogging.LogDataReceivedDecodingFailed(Logger, ex);
                }
            }
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogDataReceivedEventError(Logger, ex);
        }
    }

    /// <summary>
    /// OnSystemErrorReceived is the event handler for the SerialPort.ErrorReceived event. It logs the error and raises the ErrorReceived event with the appropriate arguments.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SerialErrorReceivedEventArgs that contains the event data.</param>
    private void OnSystemErrorReceived(object? sender, SerialErrorReceivedEventArgs e)
    {
        if (Logger.IsEnabled(LogLevel.Error)) // or appropriate level
        {
            var eventType = e.EventType.ToString();
            SerialConnectionLogging.LogErrorReceivedEventFired(Logger, eventType);
        }

        try
        {
            var errorMessage = e.EventType.ToString();
            ErrorReceived?.Invoke(this, new SerialErrorEventArgs(errorMessage));
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogErrorReceivedEventError(Logger, ex);
        }
    }

    /// <summary>
    /// Handles the SerialPort.PinChanged event and raises the PinChanged event with the appropriate information.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SerialPinChangedEventArgs that contains the event data.</param>
    private void OnSystemPinChanged(object? sender, SerialPinChangedEventArgs e)
    {
        if (Logger.IsEnabled(LogLevel.Debug)) // or appropriate level
        {
            var eventType = e.EventType.ToString();
            SerialConnectionLogging.LogPinChangedEventFired(Logger, eventType);
        }

        try
        {
            var signals = new SerialSignalChangedEventArgs(
                _serialPort.CtsHolding,
                _serialPort.DsrHolding,
                _serialPort.CDHolding);

            SerialConnectionLogging.LogPinChangedSignalsUpdated(Logger, _serialPort.CtsHolding, _serialPort.DsrHolding, _serialPort.CDHolding);
            SignalChanged?.Invoke(this, signals);
        }
        catch (Exception ex)
        {
            SerialConnectionLogging.LogPinChangedEventError(Logger, ex);
        }
    }
}
