// -----------------------------------------------------------------------------
// File:        IReadOnlyRepository.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for read-only repository operations.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Core;
using Chishiki.Data.Specifications;
using Chishiki.Data.Models;

namespace Chishiki.Data.Abstractions;

/// <summary>
/// Represents a read-only repository.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <seealso cref="IDisposable" />
public interface IReadOnlyRepository<TKey, TEntity> : IDisposable
    where TEntity : class, IEntity<TKey>
{
    #region Query

    /// <summary>
    /// Gets the query provider for the entity.
    /// </summary>
    /// <returns>Queryable collection of entities.</returns>
    IQueryable<TEntity> AsQuery();

    #endregion

    #region Async Methods

    /// <summary>
    /// Gets an item by identifier asynchronously.
    /// </summary>
    /// <param name="key">The primary key.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first element matching the specification or null asynchronously.
    /// </summary>
    /// <param name="specification">The specification query.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The first matching entity or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TKey, TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single element matching the specification or null asynchronously. Throws if more than one match is found.
    /// </summary>
    /// <param name="specification">The specification query.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The single matching entity or null.</returns>
    Task<TEntity?> SingleOrDefaultAsync(ISpecification<TKey, TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all items matching the specification asynchronously.
    /// </summary>
    /// <param name="specification">The optional specification query.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>List of matching entities.</returns>
    Task<IList<TEntity>> ListAsync(ISpecification<TKey, TEntity>? specification = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists a page of items matching the specification asynchronously.
    /// </summary>
    /// <param name="specification">The paged specification query.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>Paged list of matching entities.</returns>
    Task<IPagedList<TEntity>> ListPagedAsync(IPagedSpecification<TKey, TEntity> specification,
        CancellationToken cancellationToken = default);

    #endregion

    #region Sync Methods

    /// <summary>
    /// Gets an item by identifier synchronously.
    /// </summary>
    /// <param name="key">The primary key.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    TEntity? GetById(TKey key);

    /// <summary>
    /// Gets the first element matching the specification or null synchronously.
    /// </summary>
    /// <param name="specification">The specification query.</param>
    /// <returns>The first matching entity or null.</returns>
    TEntity? FirstOrDefault(ISpecification<TKey, TEntity> specification);

    /// <summary>
    /// Gets a single element matching the specification or null synchronously. Throws if more than one match is found.
    /// </summary>
    /// <param name="specification">The specification query.</param>
    /// <returns>The single matching entity or null.</returns>
    TEntity? SingleOrDefault(ISpecification<TKey, TEntity> specification);

    /// <summary>
    /// Lists all items matching the specification synchronously.
    /// </summary>
    /// <param name="specification">The optional specification query.</param>
    /// <returns>List of matching entities.</returns>
    IList<TEntity> List(ISpecification<TKey, TEntity>? specification = null);

    /// <summary>
    /// Lists a page of items matching the specification synchronously.
    /// </summary>
    /// <param name="specification">The paged specification query.</param>
    /// <returns>Paged list of matching entities.</returns>
    IPagedList<TEntity> ListPaged(IPagedSpecification<TKey, TEntity> specification);

    #endregion
}


