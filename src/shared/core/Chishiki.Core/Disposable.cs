// -----------------------------------------------------------------------------
// File:        Disposable.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class implementing IDisposable with structured logging and error handling.
// Created:     2024-04-15
// Modified:    2026-05-03
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki;

/// <summary>Provides a disposable base class with structured logging for managed and unmanaged resource cleanup.</summary>
/// <param name="logger">Optional logger used to emit disposal diagnostics. Defaults to <see cref="NullLogger.Instance"/> when not provided.</param>
/// <param name="loggerFactory">Optional logger factory used to create loggers for derived types. Defaults to <c>null</c> when not provided.</param>
public abstract partial class Disposable(ILogger? logger = null, ILoggerFactory? loggerFactory = null) : Loggable(logger, loggerFactory), IDisposable
{
    /// <summary>Gets a value indicating whether the object has already been disposed. This flag prevents multiple disposal attempts and ensures that disposal logic is executed only once. .</summary>
    protected bool DisposedValue { get; private set; }

    /// <summary>Releases managed resources. Override to dispose owned <see cref="IDisposable"/> members.</summary>
    protected virtual void DisposeManaged() { }

    /// <summary>Releases unmanaged resources. Override only when holding raw OS handles.</summary>
    protected virtual void DisposeUnmanaged() { }

    /// <summary>Core disposal logic that invokes either managed or unmanaged cleanup based on <paramref name="disposing"/>.</summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="IDisposable.Dispose"/>; <c>false</c> when called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (DisposedValue)
        {
            return;
        }

        try
        {
            if (disposing)
            {
                LogDisposingManaged(Logger, GetType().Name);
                DisposeManaged();
            }
            else
            {
                LogDisposingUnmanaged(Logger, GetType().Name);
                DisposeUnmanaged();
            }
        }
        catch (Exception ex)
        {
            LogDisposeFailed(Logger, GetType().Name, ex);
        }
        finally
        {
            DisposedValue = true;
        }
    }

    /// <summary>
    /// Throws an exception if this object has been disposed.
    /// </summary>
    protected void CheckDisposed() => ObjectDisposedException.ThrowIf(DisposedValue, this);

    /// <summary>Finalizer that invokes unmanaged resource cleanup.</summary>
    ~Disposable() => Dispose(disposing: false);

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Emits a debug log entry when managed resources are being disposed.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposing managed resources for {TypeName}")]
    private static partial void LogDisposingManaged(ILogger logger, string typeName);

    /// <summary>Emits a debug log entry when unmanaged resources are being disposed.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposing unmanaged resources for {TypeName}")]
    private static partial void LogDisposingUnmanaged(ILogger logger, string typeName);

    /// <summary>Emits an error log entry when an exception occurs during disposal.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while disposing {TypeName}")]
    private static partial void LogDisposeFailed(ILogger logger, string typeName, Exception ex);
}
