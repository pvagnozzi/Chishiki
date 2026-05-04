// -----------------------------------------------------------------------------
// File:        EFCoreRepository.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core implementation of a full CRUD repository using DbContext.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chishiki.Data.EFCore;

/// <summary>EF Core full CRUD repository that tracks entity changes within the current <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.</summary>
/// <typeparam name="TKey">The type of the entity primary key.</typeparam>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>All write operations register changes in the EF Core change tracker; the Unit of Work must call
/// <c>SaveChangesAsync</c> to persist them to the database.</remarks>
internal sealed partial class EFCoreRepository<TKey, TEntity>(DbContext context, ILogger logger)
    : EFCoreReadOnlyRepository<TKey, TEntity>(context, logger), IRepository<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
{
    /// <inheritdoc/>
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        LogAdd(Logger, typeof(TEntity));
        _ = Context.Set<TEntity>().Add(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        LogUpdate(Logger, typeof(TEntity));
        _ = Context.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        LogDelete(Logger, typeof(TEntity));
        _ = Context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        LogDeleteById(Logger, typeof(TEntity));
        var entity = await Context.Set<TEntity>().FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _ = Context.Set<TEntity>().Remove(entity);
        }
    }

    /// <inheritdoc/>
    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        LogAddRange(Logger, typeof(TEntity));
        Context.Set<TEntity>().AddRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        LogUpdateRange(Logger, typeof(TEntity));
        Context.Set<TEntity>().UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        LogDeleteRange(Logger, typeof(TEntity));
        Context.Set<TEntity>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteRangeByIdAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
    {
        LogDeleteRangeById(Logger, typeof(TEntity));
        var idList = ids.ToList();
        var entities = await Context.Set<TEntity>()
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(cancellationToken);
        Context.Set<TEntity>().RemoveRange(entities);
    }

    /// <inheritdoc/>
    public void Add(TEntity entity) => Context.Set<TEntity>().Add(entity);

    /// <inheritdoc/>
    public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

    /// <inheritdoc/>
    public void Delete(TEntity entity) => Context.Set<TEntity>().Remove(entity);

    /// <inheritdoc/>
    public void DeleteById(TKey id)
    {
        var entity = Context.Set<TEntity>().Find(id);
        if (entity is not null)
        {
            _ = Context.Set<TEntity>().Remove(entity);
        }
    }

    /// <inheritdoc/>
    public void AddRange(IEnumerable<TEntity> entities) => Context.Set<TEntity>().AddRange(entities);

    /// <inheritdoc/>
    public void UpdateRange(IEnumerable<TEntity> entities) => Context.Set<TEntity>().UpdateRange(entities);

    /// <inheritdoc/>
    public void DeleteRange(IEnumerable<TEntity> entities) => Context.Set<TEntity>().RemoveRange(entities);

    /// <inheritdoc/>
    public void DeleteRangeById(IEnumerable<TKey> ids)
    {
        var idList = ids.ToList();
        var entities = Context.Set<TEntity>().Where(e => idList.Contains(e.Id)).ToList();
        Context.Set<TEntity>().RemoveRange(entities);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Add entity '{EntityType}'")]
    private static partial void LogAdd(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Update entity '{EntityType}'")]
    private static partial void LogUpdate(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Delete entity '{EntityType}'")]
    private static partial void LogDelete(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DeleteById entity '{EntityType}'")]
    private static partial void LogDeleteById(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AddRange entity '{EntityType}'")]
    private static partial void LogAddRange(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "UpdateRange entity '{EntityType}'")]
    private static partial void LogUpdateRange(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DeleteRange entity '{EntityType}'")]
    private static partial void LogDeleteRange(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DeleteRangeById entity '{EntityType}'")]
    private static partial void LogDeleteRangeById(ILogger logger, Type entityType);
}
