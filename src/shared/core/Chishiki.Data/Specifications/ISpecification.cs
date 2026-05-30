// -----------------------------------------------------------------------------
// File:        ISpecification.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for domain query specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Linq.Expressions;

using Chishiki.Data.Models;

namespace Chishiki.Data.Specifications;

/// <summary>Specification interface.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface ISpecification<TKey, TEntity>
    where TEntity : IEntity<TKey>
{
    /// <summary>Gets the where filter. .</summary>
    /// <value>
    /// The where.
    /// </value>
    Expression<Func<TEntity, bool>>? Where { get; }

    /// <summary>Gets the includes. .</summary>
    /// <value>
    /// The includes.
    /// </value>
    IIncludeExpression[] Includes { get; }

    /// <summary>Gets the filters. .</summary>
    /// <value>
    /// The filters.
    /// </value>
    IFilterConditionExpression[] Filters { get; }

    /// <summary>Gets the sort. .</summary>
    /// <value>
    /// The sort.
    /// </value>
    ISortExpression[] Sort { get; }
}


