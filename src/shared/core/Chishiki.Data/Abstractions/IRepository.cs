// -----------------------------------------------------------------------------
// File:        IRepository.cs
// Author:      Piergiorgio Vagnozzi
// Description: Generic repository interface for full CRUD operations on entities.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Models;

namespace Chishiki.Data.Abstractions;

/// <summary>Generic repository interface providing full CRUD operations on entities.</summary>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IRepository<TKey, TEntity> : IReadOnlyRepository<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
{
    #region Async Methods

    /// <summary>Adds an entity asynchronously. .</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an entity asynchronously. .</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity asynchronously. .</summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity by identifier asynchronously. .</summary>
    /// <param name="id">The entity's primary key.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>Adds a range of entities asynchronously. .</summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Updates a range of entities asynchronously. .</summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Deletes a range of entities asynchronously. .</summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Deletes a range of entities by identifier asynchronously. .</summary>
    /// <param name="ids">The primary keys of entities to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteRangeByIdAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

    #endregion

    #region Sync Methods

    /// <summary>Adds an entity synchronously. .</summary>
    /// <param name="entity">The entity to add.</param>
    void Add(TEntity entity);

    /// <summary>Updates an entity synchronously. .</summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>Deletes an entity synchronously. .</summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>Deletes an entity by identifier synchronously. .</summary>
    /// <param name="id">The entity's primary key.</param>
    void DeleteById(TKey id);

    /// <summary>Adds a range of entities synchronously. .</summary>
    /// <param name="entities">The entities to add.</param>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>Updates a range of entities synchronously. .</summary>
    /// <param name="entities">The entities to update.</param>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>Deletes a range of entities synchronously. .</summary>
    /// <param name="entities">The entities to delete.</param>
    void DeleteRange(IEnumerable<TEntity> entities);

    /// <summary>Deletes a range of entities by identifier synchronously. .</summary>
    /// <param name="ids">The primary keys of entities to delete.</param>
    void DeleteRangeById(IEnumerable<TKey> ids);

    #endregion
}

