// -----------------------------------------------------------------------------
// File:        RepositoryExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for repository query operations.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Specifications;
using Chishiki.Data.Models;
using System.Diagnostics;
using System.Linq.Expressions;
using Chishiki.Exceptions;

namespace Chishiki.Data;

/// <summary>Repository extensions.</summary>
public static class RepositoryExtensions
{
    /// <summary>Finds the first entity matching the filter expression or returns null asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The read-only repository.</param>
    /// <param name="expression">The filter expression to match entities.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The first matching entity, or null if no entity matches the filter.</returns>
    [DebuggerStepThrough]
    public static Task<TEntity?> FirstOrDefaultAsync<TKey, TEntity>(this IReadOnlyRepository<TKey, TEntity> repository,
        Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey> =>
        repository.FirstOrDefaultAsync(new Specification<TKey, TEntity>(expression), cancellationToken);

    /// <summary>Finds the first entity matching the filter expression from the unit of work or returns null asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="expression">The filter expression to match entities.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The first matching entity, or null if no entity matches the filter.</returns>
    [DebuggerStepThrough]
    public static async Task<TEntity?> FirstOrDefaultAsync<TKey, TEntity>(this IUnitOfWork unitOfWork,
        Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        using var repository = unitOfWork.GetReadOnlyRepository<TKey, TEntity>();
        return await repository.FirstOrDefaultAsync(expression, cancellationToken);
    }

    /// <summary>Retrieves all entities matching the filter expression asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The read-only repository.</param>
    /// <param name="expression">The filter expression to match entities.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A list of all entities matching the filter expression.</returns>
    [DebuggerStepThrough]
    public static Task<IList<TEntity>> ListAsync<TKey, TEntity>(this IReadOnlyRepository<TKey, TEntity> repository,
        Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey> =>
        repository.ListAsync(new Specification<TKey, TEntity>(expression), cancellationToken);

    /// <summary>Retrieves all entities matching the filter expression from the unit of work asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="expression">The filter expression to match entities.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A list of all entities matching the filter expression.</returns>
    [DebuggerStepThrough]
    public static async Task<IList<TEntity>> ListAsync<TKey, TEntity>(this IUnitOfWork unitOfWork,
        Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        using var repository = unitOfWork.GetReadOnlyRepository<TKey, TEntity>();
        return await repository.ListAsync(expression, cancellationToken);
    }

    /// <summary>Retrieves a paginated list of entities matching the filter expression asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The read-only repository.</param>
    /// <param name="expression">The filter expression to match entities.</param>
    /// <param name="pageIndex">The zero-based index of the page.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A paginated list of entities matching the filter.</returns>
    [DebuggerStepThrough]
    public static Task<IPagedList<TEntity>> ListAsync<TKey, TEntity>(this IReadOnlyRepository<TKey, TEntity> repository,
        Expression<Func<TEntity, bool>> expression, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey> =>
        repository.ListPagedAsync(new PagedSpecification<TKey, TEntity>(expression, pageIndex: pageIndex, pageSize: pageSize), cancellationToken);

    /// <summary>Inserts a master entity and its detail records asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="entity">The master entity to insert.</param>
    /// <param name="deleteDetail">Action to delete detail records after insertion.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public static async Task InsertMasterDetailAsync<TKey, TEntity>(this IUnitOfWork unitOfWork, TEntity entity,
        Func<TEntity, CancellationToken, Task> deleteDetail, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        using var masterRepository = unitOfWork.GetRepository<TKey, TEntity>();
        await masterRepository.AddAsync(entity, cancellationToken);
        await deleteDetail(entity, cancellationToken);
    }

    /// <summary>Inserts multiple entities asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public static async Task InsertAsync<TKey, TEntity>(this IUnitOfWork unitOfWork, IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        using var repository = unitOfWork.GetRepository<TKey, TEntity>();
        await repository.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>Deletes a master entity and its related detail records asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="id">The unique identifier of the master entity to delete.</param>
    /// <param name="deleteDetail">Action to handle deletion of detail records associated with the master.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="NotFoundDomainException">Thrown if the entity with the specified id is not found.</exception>
    public static async Task DeleteMasterDetailAsync<TKey, TEntity>(this IUnitOfWork unitOfWork, TKey id,
        Func<TEntity, CancellationToken, Task> deleteDetail, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        using var masterRepository = unitOfWork.GetRepository<TKey, TEntity>();
        var master = await masterRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundDomainException($"{typeof(TEntity).Name} with id {id} not found");
        await deleteDetail(master, cancellationToken);
        await masterRepository.DeleteByIdAsync(id, cancellationToken);
    }

    /// <summary>Deletes multiple entities asynchronously. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public static async Task DeleteAsync<TKey, TEntity>(this IUnitOfWork unitOfWork, IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        using var repository = unitOfWork.GetRepository<TKey, TEntity>();
        await repository.DeleteRangeAsync(entities, cancellationToken);
    }

    /// <summary>Updates entities by comparing new entities with existing entities asynchronously. Automatically inserts new entities, updates changed entities, and deletes removed entities. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="entities">The new entities to update with.</param>
    /// <param name="existing">The existing entities to compare against.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public static async Task UpdateAsync<TKey, TEntity>(this IUnitOfWork unitOfWork, IEnumerable<TEntity> entities,
        IEnumerable<TEntity> existing, CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        var ent = entities.ToList();
        var ext = existing.ToList();

        var updated = new List<TEntity>();
        var inserted = new List<TEntity>();

        foreach (var entity in ent)
        {
            var existingEntity = ext.Find(e => e.Id!.Equals(entity.Id));

            if (existingEntity is null)
            {
                inserted.Add(entity);
            }
            else
            {
                updated.Add(entity);
            }
        }

        var deleted = ext.Where(entity => Enumerable.All(ent, e => !e.Id!.Equals(entity.Id))).ToList();
        using var repository = unitOfWork.GetRepository<TKey, TEntity>();

        await repository.AddRangeAsync(inserted, cancellationToken);
        await repository.UpdateRangeAsync(updated, cancellationToken);
        await repository.DeleteRangeAsync(deleted, cancellationToken);
    }
}



