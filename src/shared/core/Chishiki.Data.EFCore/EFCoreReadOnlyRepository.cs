// -----------------------------------------------------------------------------
// File:        EFCoreReadOnlyRepository.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core implementation of a read-only repository using DbContext.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chishiki.Data.EFCore;

/// <summary>EF Core read-only repository that executes all queries against a <see cref="DbContext"/>.</summary>
/// <typeparam name="TKey">The type of the entity primary key.</typeparam>
/// <typeparam name="TEntity">The entity type.</typeparam>
internal partial class EFCoreReadOnlyRepository<TKey, TEntity>(DbContext context, ILogger logger)
    : IReadOnlyRepository<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>Gets the underlying <see cref="DbContext"/>.</summary>
    protected DbContext Context { get; } = context;

    /// <summary>Gets the logger for this repository.</summary>
    protected ILogger Logger { get; } = logger;

    /// <inheritdoc/>
    public IQueryable<TEntity> AsQuery() => Context.Set<TEntity>().AsNoTracking();

    /// <inheritdoc/>
    public async Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken = default)
    {
        LogGetById(Logger, typeof(TEntity));
        return await Context.Set<TEntity>().FindAsync([key], cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TKey, TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        LogFirstOrDefault(Logger, typeof(TEntity));
        return AsQuery().ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TEntity?> SingleOrDefaultAsync(ISpecification<TKey, TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        LogSingleOrDefault(Logger, typeof(TEntity));
        return AsQuery().ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IList<TEntity>> ListAsync(ISpecification<TKey, TEntity>? specification = null,
        CancellationToken cancellationToken = default)
    {
        LogList(Logger, typeof(TEntity));
        var query = AsQuery();
        if (specification is not null)
            query = query.ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IPagedList<TEntity>> ListPagedAsync(IPagedSpecification<TKey, TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        LogListPaged(Logger, typeof(TEntity));
        return await AsQuery()
            .ApplySpecification(specification)
            .ToPagedListAsync<TKey, TEntity>(specification.PageSize, specification.PageIndex, cancellationToken);
    }

    /// <inheritdoc/>
    public TEntity? GetById(TKey key)
    {
        LogGetById(Logger, typeof(TEntity));
        return Context.Set<TEntity>().Find(key);
    }

    /// <inheritdoc/>
    public TEntity? FirstOrDefault(ISpecification<TKey, TEntity> specification)
    {
        LogFirstOrDefault(Logger, typeof(TEntity));
        return AsQuery().ApplySpecification(specification).FirstOrDefault();
    }

    /// <inheritdoc/>
    public TEntity? SingleOrDefault(ISpecification<TKey, TEntity> specification)
    {
        LogSingleOrDefault(Logger, typeof(TEntity));
        return AsQuery().ApplySpecification(specification).SingleOrDefault();
    }

    /// <inheritdoc/>
    public IList<TEntity> List(ISpecification<TKey, TEntity>? specification = null)
    {
        LogList(Logger, typeof(TEntity));
        var query = AsQuery();
        if (specification is not null)
            query = query.ApplySpecification(specification);
        return [.. query];
    }

    /// <inheritdoc/>
    public IPagedList<TEntity> ListPaged(IPagedSpecification<TKey, TEntity> specification)
    {
        LogListPaged(Logger, typeof(TEntity));
        return AsQuery()
            .ApplySpecification(specification)
            .ToPagedList<TKey, TEntity>(specification.PageSize, specification.PageIndex);
    }

    /// <inheritdoc/>
    public void Dispose() =>
        // The DbContext lifetime is managed by the Unit of Work; repositories must not dispose it.
        LogDisposed(Logger, typeof(TEntity));

    [LoggerMessage(Level = LogLevel.Debug, Message = "GetById for entity '{EntityType}'")]
    private static partial void LogGetById(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "FirstOrDefault for entity '{EntityType}'")]
    private static partial void LogFirstOrDefault(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "SingleOrDefault for entity '{EntityType}'")]
    private static partial void LogSingleOrDefault(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "List for entity '{EntityType}'")]
    private static partial void LogList(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ListPaged for entity '{EntityType}'")]
    private static partial void LogListPaged(ILogger logger, Type entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Disposed read-only repository for entity '{EntityType}'")]
    private static partial void LogDisposed(ILogger logger, Type entityType);
}
