// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Core.Domain.Entities;

namespace Chishiki.Core.Domain.Interfaces;

/// <summary>
/// Generic repository contract for aggregate root persistence.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type managed by this repository.</typeparam>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>Retrieves an aggregate by its identifier, or <see langword="null"/> if not found.</summary>
    /// <param name="id">The identifier to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The matching aggregate, or <see langword="null"/>.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all aggregates managed by this repository.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A read-only list of all stored aggregates.</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a new aggregate to the store.</summary>
    /// <param name="entity">The aggregate to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing aggregate in the store.</summary>
    /// <param name="entity">The aggregate with updated state.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Removes the aggregate with the given identifier from the store.</summary>
    /// <param name="id">The identifier of the aggregate to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
}
