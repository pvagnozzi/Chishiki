// -----------------------------------------------------------------------------
// File:        SerialConnection.Logging.cs
// Author:      Piergiorgio Vagnozzi
// Description: Logging methods for SerialConnection using LoggerMessage source generation.
// Created:     2026-04-26
// Modified:    2026-04-26
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Chishiki.Serial.Common;

/// <summary>Logging partial class for SerialConnection.</summary>
internal static partial class SerialConnectionLogging
{
    // Constructor Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "SerialConnection constructor: initializing port '{PortName}' with default settings (9600 baud, 8 data bits, 1 stop bit, no parity)")]
    public static partial void LogConstructorInitializing(ILogger logger, string portName);

    [LoggerMessage(Level = LogLevel.Information, Message = "SerialConnection constructor: port '{PortName}' initialized successfully")]
    public static partial void LogConstructorCompleted(ILogger logger, string portName);

    // Open Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "Open: port opening requested")]
    public static partial void LogOpenRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Open: port opened successfully")]
    public static partial void LogOpenSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Open: failed to open port")]
    public static partial void LogOpenFailed(ILogger logger, Exception ex);

    // OpenAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "OpenAsync: port opening requested asynchronously")]
    public static partial void LogOpenAsyncRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "OpenAsync: port opened successfully")]
    public static partial void LogOpenAsyncSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "OpenAsync: failed to open port")]
    public static partial void LogOpenAsyncFailed(ILogger logger, Exception ex);

    // Close Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "Close: port closing requested")]
    public static partial void LogCloseRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Close: port closed successfully")]
    public static partial void LogCloseSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Close: failed to close port")]
    public static partial void LogCloseFailed(ILogger logger, Exception ex);

    // CloseAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "CloseAsync: port closing requested asynchronously")]
    public static partial void LogCloseAsyncRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "CloseAsync: port closed successfully")]
    public static partial void LogCloseAsyncSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "CloseAsync: failed to close port")]
    public static partial void LogCloseAsyncFailed(ILogger logger, Exception ex);

    // Read Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "Read: reading {Count} bytes from offset {Offset}")]
    public static partial void LogReadRequested(ILogger logger, int offset, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Read: successfully read {BytesRead} bytes")]
    public static partial void LogReadSuccess(ILogger logger, int bytesRead);

    [LoggerMessage(Level = LogLevel.Error, Message = "Read: failed to read data")]
    public static partial void LogReadFailed(ILogger logger, Exception ex);

    // ReadAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadAsync: reading {Count} bytes from offset {Offset} asynchronously")]
    public static partial void LogReadAsyncRequested(ILogger logger, int offset, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadAsync: successfully read {BytesRead} bytes")]
    public static partial void LogReadAsyncSuccess(ILogger logger, int bytesRead);

    [LoggerMessage(Level = LogLevel.Error, Message = "ReadAsync: failed to read data")]
    public static partial void LogReadAsyncFailed(ILogger logger, Exception ex);

    // ReadExistingText Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadExistingText: reading all available text")]
    public static partial void LogReadExistingTextRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadExistingText: successfully read {Length} characters")]
    public static partial void LogReadExistingTextSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "ReadExistingText: failed to read text")]
    public static partial void LogReadExistingTextFailed(ILogger logger, Exception ex);

    // ReadExistingTextAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadExistingTextAsync: reading all available text asynchronously")]
    public static partial void LogReadExistingTextAsyncRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadExistingTextAsync: successfully read {Length} characters")]
    public static partial void LogReadExistingTextAsyncSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "ReadExistingTextAsync: failed to read text")]
    public static partial void LogReadExistingTextAsyncFailed(ILogger logger, Exception ex);

    // ReadLine Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadLine: reading a line of text")]
    public static partial void LogReadLineRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadLine: successfully read {Length} characters")]
    public static partial void LogReadLineSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "ReadLine: failed to read line")]
    public static partial void LogReadLineFailed(ILogger logger, Exception ex);

    // ReadLineAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadLineAsync: reading a line of text asynchronously")]
    public static partial void LogReadLineAsyncRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ReadLineAsync: successfully read {Length} characters")]
    public static partial void LogReadLineAsyncSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "ReadLineAsync: failed to read line")]
    public static partial void LogReadLineAsyncFailed(ILogger logger, Exception ex);

    // Write Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "Write: writing {Count} bytes")]
    public static partial void LogWriteRequested(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Write: successfully wrote {Count} bytes")]
    public static partial void LogWriteSuccess(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Write: failed to write data")]
    public static partial void LogWriteFailed(ILogger logger, Exception ex);

    // WriteAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteAsync: writing {Count} bytes asynchronously")]
    public static partial void LogWriteAsyncRequested(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteAsync: successfully wrote {Count} bytes")]
    public static partial void LogWriteAsyncSuccess(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "WriteAsync: failed to write data")]
    public static partial void LogWriteAsyncFailed(ILogger logger, Exception ex);

    // WriteText Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteText: writing {Length} characters")]
    public static partial void LogWriteTextRequested(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteText: successfully wrote {Length} characters")]
    public static partial void LogWriteTextSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "WriteText: failed to write text")]
    public static partial void LogWriteTextFailed(ILogger logger, Exception ex);

    // WriteTextAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteTextAsync: writing {Length} characters asynchronously")]
    public static partial void LogWriteTextAsyncRequested(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteTextAsync: successfully wrote {Length} characters")]
    public static partial void LogWriteTextAsyncSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "WriteTextAsync: failed to write text")]
    public static partial void LogWriteTextAsyncFailed(ILogger logger, Exception ex);

    // WriteLine Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteLine: writing {Length} characters with line terminator")]
    public static partial void LogWriteLineRequested(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteLine: successfully wrote {Length} characters with line terminator")]
    public static partial void LogWriteLineSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "WriteLine: failed to write line")]
    public static partial void LogWriteLineFailed(ILogger logger, Exception ex);

    // WriteLineAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteLineAsync: writing {Length} characters with line terminator asynchronously")]
    public static partial void LogWriteLineAsyncRequested(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WriteLineAsync: successfully wrote {Length} characters with line terminator")]
    public static partial void LogWriteLineAsyncSuccess(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "WriteLineAsync: failed to write line")]
    public static partial void LogWriteLineAsyncFailed(ILogger logger, Exception ex);

    // Flush Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "Flush: flushing all buffered data")]
    public static partial void LogFlushRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Flush: buffer flushed successfully")]
    public static partial void LogFlushSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Flush: failed to flush buffer")]
    public static partial void LogFlushFailed(ILogger logger, Exception ex);

    // FlushAsync Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "FlushAsync: flushing all buffered data asynchronously")]
    public static partial void LogFlushAsyncRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "FlushAsync: buffer flushed successfully")]
    public static partial void LogFlushAsyncSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "FlushAsync: failed to flush buffer")]
    public static partial void LogFlushAsyncFailed(ILogger logger, Exception ex);

    // DiscardInBuffer Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "DiscardInBuffer: clearing input buffer")]
    public static partial void LogDiscardInBufferRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DiscardInBuffer: input buffer cleared successfully")]
    public static partial void LogDiscardInBufferSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "DiscardInBuffer: failed to clear input buffer")]
    public static partial void LogDiscardInBufferFailed(ILogger logger, Exception ex);

    // DiscardOutBuffer Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "DiscardOutBuffer: clearing output buffer")]
    public static partial void LogDiscardOutBufferRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DiscardOutBuffer: output buffer cleared successfully")]
    public static partial void LogDiscardOutBufferSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "DiscardOutBuffer: failed to clear output buffer")]
    public static partial void LogDiscardOutBufferFailed(ILogger logger, Exception ex);

    // CancelPendingOperations Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "CancelPendingOperations: cancelling all pending operations")]
    public static partial void LogCancelPendingOperationsRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "CancelPendingOperations: all pending operations cancelled successfully")]
    public static partial void LogCancelPendingOperationsSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "CancelPendingOperations: failed to cancel operations")]
    public static partial void LogCancelPendingOperationsFailed(ILogger logger, Exception ex);

    // Dispose Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "Dispose: disposing serial connection")]
    public static partial void LogDisposeRequested(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispose: serial connection disposed successfully")]
    public static partial void LogDisposeSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Dispose: failed to dispose serial connection")]
    public static partial void LogDisposeFailed(ILogger logger, Exception ex);

    // Event Handler Logging
    [LoggerMessage(Level = LogLevel.Debug, Message = "DataReceived event: {EventType}")]
    public static partial void LogDataReceivedEventFired(ILogger logger, string eventType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DataReceived event: EOF received")]
    public static partial void LogDataReceivedEof(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DataReceived event: {BytesRead} bytes read")]
    public static partial void LogDataReceivedBytesRead(ILogger logger, int bytesRead);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DataReceived event: {Length} characters decoded as UTF-8")]
    public static partial void LogDataReceivedTextDecoded(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Warning, Message = "DataReceived event: failed to decode received data as UTF-8")]
    public static partial void LogDataReceivedDecodingFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "DataReceived event: error handling event")]
    public static partial void LogDataReceivedEventError(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ErrorReceived event: {EventType}")]
    public static partial void LogErrorReceivedEventFired(ILogger logger, string eventType);

    [LoggerMessage(Level = LogLevel.Error, Message = "ErrorReceived event: error handling event")]
    public static partial void LogErrorReceivedEventError(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "PinChanged event: {EventType}")]
    public static partial void LogPinChangedEventFired(ILogger logger, string eventType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "PinChanged event: signals updated - CTS: {CtsHolding}, DSR: {DsrHolding}, CD: {CdHolding}")]
    public static partial void LogPinChangedSignalsUpdated(ILogger logger, bool ctsHolding, bool dsrHolding, bool cdHolding);

    [LoggerMessage(Level = LogLevel.Error, Message = "PinChanged event: error handling event")]
    public static partial void LogPinChangedEventError(ILogger logger, Exception ex);
}
