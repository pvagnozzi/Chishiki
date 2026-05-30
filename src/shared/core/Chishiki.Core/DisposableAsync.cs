// -----------------------------------------------------------------------------
// File:        DisposableAsync.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class implementing IAsyncDisposable with structured logging for async resource cleanup.
// Created:     2026-04-26
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki;

/// <summary>Abstract base class implementing IAsyncDisposable with structured logging for async resource cleanup.</summary>
/// <param name="logger">Optional logger used to emit disposal diagnostics. Defaults to <see cref="NullLogger.Instance"/> when not provided.</param>
/// <param name="loggerFactory">Optional logger factory used to create loggers for derived types. Defaults to <c>null</c> when not provided.</param>
public abstract partial class DisposableAsync(ILogger? logger = null, ILoggerFactory? loggerFactory = null) : Loggable(logger, loggerFactory), IAsyncDisposable
{
    /// <summary>Gets a value indicating whether the object has already been disposed. This flag prevents multiple disposal attempts and ensures that disposal logic is executed only once. .</summary>
    private bool _disposedValue;

    /// <summary>Releases managed resources asynchronously. Override to dispose owned <see cref="IAsyncDisposable"/> or <see cref="IDisposable"/> members.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous managed resource cleanup.</returns>
    protected virtual ValueTask DisposeManagedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    /// <summary>Releases unmanaged resources. Override only when holding raw OS handles that require synchronous cleanup.</summary>
    protected virtual void DisposeUnmanaged() { }

    /// <summary>Core async disposal logic that invokes either managed or unmanaged cleanup based on <paramref name="disposing"/>.</summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="IAsyncDisposable.DisposeAsync"/>; <c>false</c> when called from the finalizer.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous disposal.</returns>
    protected virtual async ValueTask DisposeAsync(bool disposing, CancellationToken cancellationToken = default)
    {
        if (_disposedValue)
        {
            return;
        }

        try
        {
            if (disposing)
            {
                LogDisposingManagedAsync(Logger, GetType().Name);
                await DisposeManagedAsync(cancellationToken).ConfigureAwait(false);
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
            _disposedValue = true;
        }
    }

    /// <summary>Finalizer that invokes unmanaged resource cleanup synchronously.</summary>
    ~DisposableAsync() => DisposeUnmanaged();

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>Emits a debug log entry when managed resources are being disposed asynchronously.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposing managed resources asynchronously for {TypeName}")]
    private static partial void LogDisposingManagedAsync(ILogger logger, string typeName);

    /// <summary>Emits a debug log entry when unmanaged resources are being disposed.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposing unmanaged resources for {TypeName}")]
    private static partial void LogDisposingUnmanaged(ILogger logger, string typeName);

    /// <summary>Emits an error log entry when an exception occurs during disposal.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while disposing {TypeName}")]
    private static partial void LogDisposeFailed(ILogger logger, string typeName, Exception ex);
}
