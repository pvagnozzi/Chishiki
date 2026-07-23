// -----------------------------------------------------------------------------
// File:        DataSeeder.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class for seeding initial data into repositories.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Services;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Microsoft.Extensions.Logging;

namespace Chishiki.Data;

/// <summary>Abstract base class for seeding initial data into repositories with structured logging.</summary>
/// <seealso cref="IDataSeeder" />
public abstract partial class DataSeeder(IUnitOfWork unitOfWork, ILogger<DataSeeder> logger) : Service(logger), IDataSeeder
{
    /// <summary>Gets the unit of work for repository coordination.</summary>
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    /// <summary>Seeds all data asynchronously and handles exceptions with structured logging.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Log.SeedDataStarting(Logger);
            await SeedEntitiesAsync(cancellationToken);
            Log.SeedDataCompleted(Logger);
        }
        catch (Exception ex)
        {
            Log.SeedDataFailed(Logger, ex);
        }
    }

    /// <summary>Seeds all entities asynchronously. Must be implemented by derived classes.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task SeedEntitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>Seeds entities of a specific type into the database asynchronously.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity to seed.</typeparam>
    /// <param name="entities">The collection of entities to seed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    protected async Task SeedEntityAsync<TKey, TEntity>(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        try
        {
            var entityTypeName = typeof(TEntity).Name;
            Log.SeedingEntityType(Logger, entityTypeName);
            var repository = UnitOfWork.GetRepository<TKey, TEntity>();
            foreach (var entity in entities)
            {
                try
                {
                    Log.SeedingEntity(Logger, entityTypeName);
                    await repository.AddAsync(entity, cancellationToken);
                    Log.EntityInserted(Logger, entityTypeName);
                }
                catch (Exception ex)
                {
                    Log.SeedingEntityFailed(Logger, entityTypeName, ex);
                }
            }

            await UnitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            Log.SeedingEntityTypeCompleted(Logger, entityTypeName);
        }
        catch (Exception ex)
        {
            var entityTypeName = typeof(TEntity).Name;
            Log.SeedingEntityTypeFailed(Logger, entityTypeName, ex);
        }
    }

    /// <summary>Seeds entities with a custom finder predicate to avoid duplicates asynchronously.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity to seed.</typeparam>
    /// <param name="entities">The collection of entities to seed.</param>
    /// <param name="finder">Predicate to find existing entities and avoid duplicates.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>List of all seeded and existing entities.</returns>
    protected async Task<IList<TEntity>> SeedEntityAsync<TKey, TEntity>(IEnumerable<TEntity> entities,
        Func<TEntity, TEntity, bool> finder, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        var result = new List<TEntity>();
        var entityTypeName = typeof(TEntity).Name;

        try
        {
            Log.SeedingEntityType(Logger, entityTypeName);
            var repository = UnitOfWork.GetRepository<TKey, TEntity>();
            var currentItems = await repository.ListAsync(null, cancellationToken);

            foreach (var entity in entities)
            {
                try
                {
                    Log.SeedingEntity(Logger, entityTypeName);
                    var currentItem = currentItems.FirstOrDefault(x => finder(x, entity));
                    if (currentItem is not null)
                    {
                        Log.EntityFound(Logger, entityTypeName);
                        result.Add(currentItem);
                        continue;
                    }

                    await repository.AddAsync(entity, cancellationToken);
                    result.Add(entity);
                    Log.EntityInserted(Logger, entityTypeName);
                }
                catch (Exception ex)
                {
                    Log.SeedingEntityFailed(Logger, entityTypeName, ex);
                }
            }

            await UnitOfWork.SaveChangesAsync(cancellationToken);
            Log.SeedingEntityTypeCompleted(Logger, entityTypeName);
        }
        catch (Exception ex)
        {
            Log.SeedingEntityTypeFailed(Logger, entityTypeName, ex);
        }

        return result;
    }
}



