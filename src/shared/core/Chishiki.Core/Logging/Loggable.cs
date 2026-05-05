// -----------------------------------------------------------------------------
// File:        Loggable.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class implementing ILoggable with lazy logger creation via ILoggerFactory.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki.Logging;

/// <summary>Abstract base class implementing <see cref="ILoggable"/> with lazy logger creation via <see cref="ILoggerFactory"/>.</summary>
/// <param name="logger">Optional logger used to record diagnostic and operational messages. Defaults to <see cref="NullLogger.Instance"/> when not provided.</param>
/// <param name="loggerFactory">Optional logger factory used to create loggers for the containing component and its dependencies.</param>
public abstract class Loggable(ILogger? logger, ILoggerFactory? loggerFactory = null) : ILoggable
{
    /// <summary>Gets the logger factory used to create loggers for the containing component and its dependencies.</summary>
    private readonly ILoggerFactory? _loggerFactory = loggerFactory;

    /// <summary>Gets the logger used to record diagnostic and operational messages for the containing component.</summary>
    public ILogger Logger { get => field ??= CreateLogger(GetType()); } = logger;

    /// <summary>Creates an <see cref="ILogger"/> instance for the specified type.</summary>
    /// <param name="type">The type for which to create a logger. This is typically the class or component that will use the logger.</param>
    /// <returns>An <see cref="ILogger"/> instance associated with the specified type. If a logger cannot be created, a no-op logger is returned.</returns>
    protected ILogger CreateLogger(Type type) => _loggerFactory?.CreateLogger(type) ?? NullLogger.Instance;
}
