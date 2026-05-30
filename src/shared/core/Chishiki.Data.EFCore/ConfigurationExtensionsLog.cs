// -----------------------------------------------------------------------------
// File:        ConfigurationExtensionsLog.cs
// Author:      Piergiorgio Vagnozzi
// Description: Structured logging definitions for EF Core migrations and database configuration.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace Chishiki.Data.EFCore;

/// <summary>Provides compile-time structured log definitions for EF Core database configuration and migrations.</summary>
internal static partial class Log
{
    /// <summary>Emits a trace log entry when checking for pending migrations.</summary>
    /// <param name="logger">Logger instance for recording the message.</param>
    /// <param name="dbContextName">Fully qualified name of the DbContext type.</param>
    [LoggerMessage(Level = LogLevel.Trace, Message = "Migration '{dbContextName}': pending migrations")]
    public static partial void MigrationCheckingPending(ILogger logger, string? dbContextName);

    /// <summary>Emits a trace log entry when applying pending migrations.</summary>
    /// <param name="logger">Logger instance for recording the message.</param>
    /// <param name="dbContextName">Fully qualified name of the DbContext type.</param>
    /// <param name="pendingMigrations">Collection of pending migration names.</param>
    [LoggerMessage(Level = LogLevel.Trace, Message = "Migration '{dbContextName}': apply {pendingMigrations}")]
    public static partial void MigrationApplying(ILogger logger, string? dbContextName, IEnumerable<string> pendingMigrations);

    /// <summary>Emits a trace log entry when migrations complete successfully.</summary>
    /// <param name="logger">Logger instance for recording the message.</param>
    /// <param name="dbContextName">Fully qualified name of the DbContext type.</param>
    [LoggerMessage(Level = LogLevel.Trace, Message = "Migration '{dbContextName}': completed")]
    public static partial void MigrationCompleted(ILogger logger, string? dbContextName);

    /// <summary>Emits a trace log entry when no pending migrations are found.</summary>
    /// <param name="logger">Logger instance for recording the message.</param>
    /// <param name="dbContextName">Fully qualified name of the DbContext type.</param>
    [LoggerMessage(Level = LogLevel.Trace, Message = "Migration '{dbContextName}': no migrations")]
    public static partial void MigrationNone(ILogger logger, string? dbContextName);

    /// <summary>Emits an error log entry when migration fails with an exception.</summary>
    /// <param name="logger">Logger instance for recording the message.</param>
    /// <param name="dbContextName">Fully qualified name of the DbContext type.</param>
    /// <param name="exception">The exception that caused the migration failure.</param>
    [LoggerMessage(Level = LogLevel.Error, Message = "Migration '{dbContextName}': failed")]
    public static partial void MigrationFailed(ILogger logger, string? dbContextName, Exception exception);

    /// <summary>Emits a trace log entry when migration is started.</summary>
    /// <param name="logger">Logger instance for recording the message.</param>
    /// <param name="dbContextName">Fully qualified name of the DbContext type.</param>
    [LoggerMessage(Level = LogLevel.Trace, Message = "Migration '{dbContextName}': started")]
    public static partial void MigrationStarted(ILogger logger, string? dbContextName);
}
