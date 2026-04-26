// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki.Core;

public abstract partial class Disposable(ILogger? logger = null) : IDisposable
{
    private bool _disposedValue;

    protected ILogger Logger { get; } = logger ?? NullLogger.Instance;

    protected virtual void DisposeManaged()
    {
    }

    protected virtual void DisposeUnmanaged()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
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
            _disposedValue = true;
        }
    }

    ~Disposable() => Dispose(disposing: false);

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposing managed resources for {TypeName}")]
    private static partial void LogDisposingManaged(ILogger logger, string typeName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposing unmanaged resources for {TypeName}")]
    private static partial void LogDisposingUnmanaged(ILogger logger, string typeName);

    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while disposing {TypeName}")]
    private static partial void LogDisposeFailed(ILogger logger, string typeName, Exception ex);
}
