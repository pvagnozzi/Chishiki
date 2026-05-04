// -----------------------------------------------------------------------------
// File:        IService.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for services with structured logging and asynchronous disposal.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Core;

/// <summary>Service interface providing structured logging and asynchronous resource disposal for application components.</summary>
/// <remarks>
/// Services implementing this interface can leverage structured logging through <see cref="ILoggable"/> for better diagnostics and monitoring.
/// The <see cref="IAsyncDisposable"/> implementation allows services to perform asynchronous cleanup of resources when they are no longer needed,
/// ensuring efficient resource management and preventing memory leaks.
/// </remarks>
public interface IService : ILoggable, IAsyncDisposable
{
}
