// -----------------------------------------------------------------------------
// File:        DeleteByIdCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract delete by ID command request implementation.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Chishiki.Messaging.Abstractions.Commands;

namespace Chishiki.Messaging.Common.Commands;

/// <summary>Delete by ID command request implementation.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public abstract record DeleteByIdCommandRequest<TKey, TEntity> : IdCommandRequest<TKey>, IDeleteByIdCommandRequest<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>Initializes a new instance of the <see cref="DeleteByIdCommandRequest{TKey, TEntity}"/> class. .</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    protected DeleteByIdCommandRequest(TKey id, Guid correlationId) : base(id, correlationId)
    {
    }
}

