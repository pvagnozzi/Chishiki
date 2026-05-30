// -----------------------------------------------------------------------------
// File:        IdCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract command request with generic key support.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Commands;

namespace Chishiki.Messaging.Common.Commands;

/// <summary>Generic command with an identifier.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract record IdCommandRequest<TKey> : CommandRequest, IIdCommandRequest<TKey>
{
    /// <summary>Initializes a new instance of the <see cref="IdCommandRequest{TKey}"/> class. .</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    protected IdCommandRequest(TKey id, Guid correlationId) : base(correlationId)
    {
        Id = id;
    }

    /// <summary>Gets the identifier. .</summary>
    /// <value>
    /// The identifier.
    /// </value>
    public TKey Id { get; init; }
}

