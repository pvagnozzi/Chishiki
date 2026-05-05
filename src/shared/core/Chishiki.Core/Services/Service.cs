// -----------------------------------------------------------------------------
// File:        Service.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base service class with structured logging and asynchronous disposal.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace Chishiki.Services;

/// <summary>Concrete implementation of a service providing structured logging and asynchronous resource cleanup.</summary>
/// <remarks>
/// This is the standard implementation of <see cref="IService"/> for components that need logging and async disposal capabilities.
/// Inherits from <see cref="DisposableAsync"/> which provides the async disposal pattern.
/// </remarks>
/// <param name="logger">Optional logger for diagnostics. Defaults to null if not provided.</param>
/// <param name="loggerFactory">Optional logger factory for creating additional loggers. Defaults to null if not provided.</param>
public class Service(ILogger? logger = null, ILoggerFactory? loggerFactory = null) : DisposableAsync(logger, loggerFactory), IService
{
}
