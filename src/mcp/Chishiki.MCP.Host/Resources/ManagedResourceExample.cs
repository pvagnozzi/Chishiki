// -----------------------------------------------------------------------------
// File:        ManagedResourceExample.cs
// Author:      Piergiorgio Vagnozzi
// Description: Example demonstrating DisposableAsync usage for managing async resources.
// Created:     2026-04-26
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Chishiki;

namespace Chishiki.MCP.Host.Resources;

/// <summary>Example resource manager demonstrating DisposableAsync usage for async resource cleanup.</summary>
/// <remarks>
/// This is a demonstration class showing best practices for managing resources
/// that require async cleanup (e.g., database connections, stream flushing, API graceful shutdown).
/// </remarks>
internal sealed partial class ManagedResourceExample : AsyncDisposable
{
    private string? _resourceName;

    /// <summary>Initializes a new instance of the <see cref="ManagedResourceExample"/> class.</summary>
    /// <param name="resourceName">The name of the resource being managed.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ManagedResourceExample(string resourceName, ILogger<ManagedResourceExample>? logger = null)
        : base(logger)
    {
        _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        LogResourceInitialized(Logger, _resourceName);
    }

    /// <summary>Gets a value indicating whether this resource is currently allocated.</summary>
    public bool IsAllocated => !string.IsNullOrEmpty(_resourceName);

    /// <summary>Acquires the underlying resource (e.g., opens a connection).</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous acquire operation.</returns>
    public async Task AcquireAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogResourceAcquiring(Logger, _resourceName!);

        // Simulate async acquire (e.g., network call, database open)
        await Task.Delay(100, cancellationToken).ConfigureAwait(false);

        LogResourceAcquired(Logger, _resourceName!);
    }

    /// <summary>Performs an operation on the resource.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UseResourceAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsAllocated)
            throw new InvalidOperationException("Resource is not allocated.");

        LogResourceUsing(Logger, _resourceName!);

        // Simulate async operation
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);

        LogResourceUsed(Logger, _resourceName!);
    }

    /// <summary>Releases managed async resources (e.g., flushes buffers, closes connections gracefully).</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous cleanup.</returns>
    protected override async ValueTask DisposeManagedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(_resourceName))
            return;

        LogResourceClosing(Logger, _resourceName);

        // Simulate async cleanup (e.g., flush pending data, graceful shutdown)
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);

        _resourceName = null;
        LogResourceClosed(Logger, _resourceName ?? "<disposed>");
    }

    /// <summary>Emits an information log entry when the resource is initialized.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Resource '{ResourceName}' initialized")]
    private static partial void LogResourceInitialized(ILogger logger, string resourceName);

    /// <summary>Emits a debug log entry when resource acquisition begins.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Acquiring resource '{ResourceName}'")]
    private static partial void LogResourceAcquiring(ILogger logger, string resourceName);

    /// <summary>Emits an information log entry when resource acquisition completes.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Resource '{ResourceName}' acquired")]
    private static partial void LogResourceAcquired(ILogger logger, string resourceName);

    /// <summary>Emits a debug log entry when resource usage begins.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Using resource '{ResourceName}'")]
    private static partial void LogResourceUsing(ILogger logger, string resourceName);

    /// <summary>Emits a debug log entry when resource usage completes.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Resource '{ResourceName}' usage completed")]
    private static partial void LogResourceUsed(ILogger logger, string resourceName);

    /// <summary>Emits an information log entry when resource cleanup begins.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Closing resource '{ResourceName}'")]
    private static partial void LogResourceClosing(ILogger logger, string resourceName);

    /// <summary>Emits an information log entry when resource cleanup completes.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Resource '{ResourceName}' closed")]
    private static partial void LogResourceClosed(ILogger logger, string resourceName);
}
