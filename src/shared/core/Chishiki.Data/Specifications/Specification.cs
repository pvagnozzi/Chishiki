// -----------------------------------------------------------------------------
// File:        Specification.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base specification class for domain queries with filter, include, and sort expressions.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Linq.Expressions;

using Chishiki.Data.Models;

namespace Chishiki.Data.Specifications;

/// <summary>
/// Specification class.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <seealso cref="ISpecification&lt;TKey, TEntity&gt;" />
/// <seealso cref="IEquatable&lt;Specification&lt;TKey, TEntity&gt;&gt;" />
public record Specification<TKey, TEntity> : ISpecification<TKey, TEntity>
    where TEntity : IEntity<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TKey, TEntity}"/> class.
    /// </summary>
    /// <param name="where">The where.</param>
    /// <param name="filterExpressions">The filter expressions.</param>
    /// <param name="includeExpressions">The include expressions.</param>
    /// <param name="sortExpressions">The sort expressions.</param>
    public Specification(
        Expression<Func<TEntity, bool>>? where = null,
        IEnumerable<IFilterConditionExpression>? filterExpressions = null,
        IEnumerable<IIncludeExpression>? includeExpressions = null,
        IEnumerable<ISortExpression>? sortExpressions = null)
    {
        Where = where;
        Filters = filterExpressions?.ToArray() ?? [];
        Includes = includeExpressions?.ToArray() ?? [];
        Sort = sortExpressions?.ToArray() ?? [];
    }

    /// <summary>
    /// Gets the where.
    /// </summary>
    /// <value>
    /// The where.
    /// </value>
    public Expression<Func<TEntity, bool>>? Where { get; }

    /// <summary>
    /// Gets the filters.
    /// </summary>
    /// <value>
    /// The filters.
    /// </value>
    public IFilterConditionExpression[] Filters { get; }

    /// <summary>
    /// Gets the includes.
    /// </summary>
    /// <value>
    /// The includes.
    /// </value>
    public IIncludeExpression[] Includes { get; }

    /// <summary>
    /// Gets the sort.
    /// </summary>
    /// <value>
    /// The sort.
    /// </value>
    public ISortExpression[] Sort { get; }
}


