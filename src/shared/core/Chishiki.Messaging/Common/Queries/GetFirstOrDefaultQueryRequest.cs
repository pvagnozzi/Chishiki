// -----------------------------------------------------------------------------
// File:        GetFirstOrDefaultQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Get first or default query request implementation.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Chishiki.Messaging.Abstractions.Queries;

namespace Chishiki.Messaging.Common.Queries;

/// <summary>Get first or default query request implementation.</summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract record GetFirstOrDefaultQueryRequest<TKey, TEntity> : QueryRequest<TEntity?>, IGetFirstOrDefaultQueryRequest<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>Initializes a new instance of the <see cref="GetFirstOrDefaultQueryRequest{TKey, TEntity}"/> class. .</summary>
    /// <param name="specification">The specification.</param>
    /// <param name="correlationId">The correlation ID.</param>
    protected GetFirstOrDefaultQueryRequest(ISpecification<TKey, TEntity> specification, Guid correlationId) : base(correlationId)
    {
        Specification = specification;
    }

    /// <summary>Gets the specification for filtering entities. .</summary>
    public ISpecification<TKey, TEntity> Specification { get; }
}

