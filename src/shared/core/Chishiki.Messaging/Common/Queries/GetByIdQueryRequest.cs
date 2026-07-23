// -----------------------------------------------------------------------------
// File:        GetByIdQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Get by ID query request implementation.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Queries;

namespace Chishiki.Messaging.Common.Queries;

/// <summary>Get by id query to be implemented by all get by id query messages.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public abstract record GetByIdQueryRequest<TKey, TEntity> : QueryRequest<TEntity?>, IGetByIdQueryRequest<TKey, TEntity?>
{
    /// <summary>Initializes a new instance of the <see cref="GetByIdQueryRequest{TKey,TEntity}"/> class. .</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    protected GetByIdQueryRequest(TKey id, Guid correlationId) : base(correlationId)
    {
        Id = id;
    }

    /// <summary>Gets the identifier. .</summary>
    /// <value>
    /// The identifier.
    /// </value>
    public TKey Id { get; }
}

