// -----------------------------------------------------------------------------
// File:        IPagedSpecification.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for paged query specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Models;

namespace Chishiki.Data.Specifications;

/// <summary>
/// Paged specification interface.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <seealso cref="Daikin.Data.Abstractions.Specifications.ISpecification&lt;TKey, TEntity&gt;" />
public interface IPagedSpecification<TKey, TEntity> : ISpecification<TKey, TEntity>
    where TEntity : IEntity<TKey>
{
    /// <summary>
    /// Gets the index of the page.
    /// </summary>
    /// <value>
    /// The index of the page.
    /// </value>
    int PageIndex { get; }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    /// <value>
    /// The size of the page.
    /// </value>
    int PageSize { get; }
}


