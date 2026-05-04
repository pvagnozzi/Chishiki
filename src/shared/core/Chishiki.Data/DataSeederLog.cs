// -----------------------------------------------------------------------------
// File:        DataSeederLog.cs
// Author:      Piergiorgio Vagnozzi
// Description: Structured logging definitions for data seeding operations.
// Created:     2026-04-26
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace Chishiki.Data;

/// <summary>Provides compile-time structured log definitions for data seeding operations.</summary>
internal static partial class Log
{
    /// <summary>Emits an information log entry when seeding starts.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting data seeding process")]
    public static partial void SeedDataStarting(ILogger logger);

    /// <summary>Emits an information log entry when seeding completes successfully.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Data seeding completed successfully")]
    public static partial void SeedDataCompleted(ILogger logger);

    /// <summary>Emits an error log entry when seeding fails.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Data seeding failed with an exception")]
    public static partial void SeedDataFailed(ILogger logger, Exception ex);

    /// <summary>Emits a debug log entry when beginning to seed a specific entity type.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Seeding entities of type {EntityType}")]
    public static partial void SeedingEntityType(ILogger logger, string entityType);

    /// <summary>Emits a debug log entry when seeding an individual entity.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Seeding entity of type {EntityType}")]
    public static partial void SeedingEntity(ILogger logger, string entityType);

    /// <summary>Emits a debug log entry when an entity is successfully inserted.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Entity of type {EntityType} inserted successfully")]
    public static partial void EntityInserted(ILogger logger, string entityType);

    /// <summary>Emits a debug log entry when an entity is found during seeding.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Entity of type {EntityType} found in database")]
    public static partial void EntityFound(ILogger logger, string entityType);

    /// <summary>Emits an error log entry when seeding an individual entity fails.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to seed entity of type {EntityType}")]
    public static partial void SeedingEntityFailed(ILogger logger, string entityType, Exception ex);

    /// <summary>Emits a debug log entry when seeding a specific entity type completes.</summary>
    [LoggerMessage(Level = LogLevel.Debug, Message = "Completed seeding entities of type {EntityType}")]
    public static partial void SeedingEntityTypeCompleted(ILogger logger, string entityType);

    /// <summary>Emits an error log entry when seeding a specific entity type fails.</summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to complete seeding entities of type {EntityType}")]
    public static partial void SeedingEntityTypeFailed(ILogger logger, string entityType, Exception ex);
}
