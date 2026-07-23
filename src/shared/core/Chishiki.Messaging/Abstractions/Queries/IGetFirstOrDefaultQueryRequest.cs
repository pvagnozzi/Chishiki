// -----------------------------------------------------------------------------
// File:        IGetFirstOrDefaultQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Query request interface for retrieving the first entity matching a specification.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Models;
using Chishiki.Data.Specifications;

namespace Chishiki.Messaging.Abstractions.Queries;

/// <summary>Get first or default query request interface.</summary>
/// <typeparam name="TKey">Entity key type.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IGetFirstOrDefaultQueryRequest<TKey, TEntity> : IQueryRequest<TEntity?>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>Gets the specification for filtering entities. .</summary>
    ISpecification<TKey, TEntity> Specification { get; }
}


