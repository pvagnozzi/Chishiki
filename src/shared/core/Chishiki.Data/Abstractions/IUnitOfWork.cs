// -----------------------------------------------------------------------------
// File:        IUnitOfWork.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for Unit of Work pattern coordination.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Audit;

using Chishiki.Data.Models;

namespace Chishiki.Data.Abstractions;

/// <summary>
/// Unit of work interface.
/// </summary>
/// <seealso cref="IDisposable" />
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the read only repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    IReadOnlyRepository<TKey, TEntity> GetReadOnlyRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>;

    /// <summary>
    /// Gets the repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    IRepository<TKey, TEntity> GetRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>;

    /// <summary>
    /// Saves all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a list of audit entries asynchronously.
    /// </summary>
    /// <typeparam name="T">The audit entity type.</typeparam>
    /// <param name="audits">The audit entries to save.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAuditsAsync<T>(IList<T> audits, CancellationToken cancellationToken = default)
        where T : class, IEntityAudit, new();

    /// <summary>
    /// Gets audit entries for a specific authenticated user asynchronously.
    /// </summary>
    /// <typeparam name="T">The audit entity type.</typeparam>
    /// <param name="authenticatedUserId">The user ID to retrieve audits for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>List of audit entries for the user.</returns>
    Task<IList<T>> GetAuditsAsync<T>(Guid authenticatedUserId, CancellationToken cancellationToken = default)
        where T : class, IEntityAudit, new();

}


