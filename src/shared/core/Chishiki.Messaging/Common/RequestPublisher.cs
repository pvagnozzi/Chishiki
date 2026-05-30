// -----------------------------------------------------------------------------
// File:        RequestPublisher.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base publisher for sending queries and commands.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions;
using Chishiki.Messaging.Abstractions.Commands;
using Chishiki.Messaging.Abstractions.Queries;
using Microsoft.Extensions.Logging;

namespace Chishiki.Messaging.Common;

/// <summary>Request publisher base class.</summary>
public abstract class RequestPublisher : IRequestPublisher
{
    /// <summary>Initializes a new instance of the <see cref="RequestPublisher"/> class. .</summary>
    /// <param name="logger">The logger.</param>
    protected RequestPublisher(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>Gets the logger. .</summary>
    protected ILogger Logger { get; }

    /// <summary>Sends the query asynchronously. .</summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query result.</returns>
    public abstract Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : class, IQueryRequest<TResult>
        where TResult : class?;

    /// <summary>Sends the command asynchronously. .</summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public abstract Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommandRequest;
}
