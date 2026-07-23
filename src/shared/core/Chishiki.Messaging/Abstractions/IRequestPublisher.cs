// -----------------------------------------------------------------------------
// File:        IRequestPublisher.cs
// Author:      Piergiorgio Vagnozzi
// Description: Request publisher interface for sending query and command requests through the messaging system.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Commands;
using Chishiki.Messaging.Abstractions.Queries;

namespace Chishiki.Messaging.Abstractions;

/// <summary>Request publisher interface.</summary>
public interface IRequestPublisher
{
    /// <summary>Sends a query request. .</summary>
    /// <param name="query">Query to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TQuery">Query type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Query result.</returns>
    Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : class, IQueryRequest<TResult>
        where TResult : class?;

    /// <summary>Sends a command request. .</summary>
    /// <param name="command">Command to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TCommand">Command type.</typeparam>
    Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommandRequest;
}
