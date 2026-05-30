// -----------------------------------------------------------------------------
// File:        PagedSpecification.cs
// Author:      Piergiorgio Vagnozzi
// Description: Specification class for paged query results.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Linq.Expressions;
using Chishiki.Data.Models;

namespace Chishiki.Data.Specifications;

/// <summary>Paged specification.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <seealso cref="Specification&lt;TKey, TEntity&gt;" />
public record PagedSpecification<TKey, TEntity> : Specification<TKey, TEntity>, IPagedSpecification<TKey, TEntity>
    where TEntity : IEntity<TKey>
{
    /// <summary>Initializes a new instance of the <see cref="PagedSpecification{TKey, TEntity}"/> class. .</summary>
    /// <param name="where">The where.</param>
    /// <param name="filterExpressions">The filter expressions.</param>
    /// <param name="includeExpressions">The include expressions.</param>
    /// <param name="sortExpressions">The sort expressions.</param>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="pageSize">Size of the page.</param>
    public PagedSpecification(
        Expression<Func<TEntity, bool>>? where = null,
        IEnumerable<IFilterConditionExpression>? filterExpressions = null,
        IEnumerable<IIncludeExpression>? includeExpressions = null,
        IEnumerable<ISortExpression>? sortExpressions = null,
        int pageIndex = 0,
        int pageSize = 10) : base(where, filterExpressions, includeExpressions, sortExpressions)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>Gets the index of the page. .</summary>
    /// <value>
    /// The index of the page.
    /// </value>
    public int PageIndex { get; }

    /// <summary>Gets the size of the page. .</summary>
    /// <value>
    /// The size of the page.
    /// </value>
    public int PageSize { get; }
}

