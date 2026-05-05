// -----------------------------------------------------------------------------
// File:        UnitOfWork.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class implementing the Unit of Work pattern with async disposal.
// Created:     2024-04-15
// Modified:    2026-05-03
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Audit;
using Chishiki.Data.Models;
using Microsoft.Extensions.Logging;

namespace Chishiki.Data;

/// <summary>
/// Unit of work abstract class.
/// </summary>
/// <seealso cref="Disposable" />
/// <seealso cref="IUnitOfWork" />
/// <remarks>
/// Initializes a new instance of the <see cref="UnitOfWork"/> class.
/// </remarks>
/// <param name="repositoryMapper">The repository mapper.</param>
/// <param name="loggerFactory">The logger factory.</param>
public abstract partial class UnitOfWork(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory) : Disposable(null, loggerFactory), IUnitOfWork
{
    /// <summary>
    /// The logger factory
    /// </summary>
    protected ILoggerFactory LoggerFactory { get; } = loggerFactory;

    /// <summary>
    /// The repository mapper
    /// </summary>
    private readonly IRepositoryMapper _repositoryMapper = repositoryMapper;

    /// <summary>
    /// Gets the repository factory.
    /// </summary>
    /// <value>
    /// The repository factory.
    /// </value>
    private IRepositoryFactory RepositoryFactory =>
        field ??= BuildRepositoryFactory(_repositoryMapper, LoggerFactory);

    /// <summary>
    /// Gets the read only repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    public IReadOnlyRepository<TKey, TEntity> GetReadOnlyRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>
    {
        LogGetReadOnlyRepository(Logger, typeof(TEntity));
        var result = RepositoryFactory.CreateReadOnlyRepository<TKey, TEntity>();
        return result;
    }

    /// <summary>
    /// Gets the repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    public IRepository<TKey, TEntity> GetRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>
    {
        LogGetRepository(Logger, typeof(TEntity));
        var result = RepositoryFactory.CreateRepository<TKey, TEntity>();
        return result;
    }

    /// <summary>
    /// Saves the changes asynchronous.
    /// </summary>
    /// <param name="saveAudit"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        LogSaveChangesAsync(Logger);
        await SaveUnitOfWorkAsync(cancellationToken);
        LogSaveChangesAsyncCompleted(Logger);
    }

    public virtual Task SaveAuditsAsync<T>(IList<T> audits,
        CancellationToken cancellationToken = default)
        where T : class, IEntityAudit, new() => Task.CompletedTask;

    public virtual Task<IList<T>> GetAuditsAsync<T>(Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
        where T : class, IEntityAudit, new() => Task.FromResult<IList<T>>([]);

    /// <summary>
    /// Disposes the resources.
    /// </summary>
    protected override void DisposeManaged()
    {
        LogDisposeResources(Logger);
        DisposeUnitOfWork();
        base.DisposeManaged();
    }

    /// <summary>
    /// Builds the repository factory.
    /// </summary>
    /// <param name="repositoryMapper">The repository mapper.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns></returns>
    protected abstract IRepositoryFactory BuildRepositoryFactory(IRepositoryMapper repositoryMapper,
        ILoggerFactory loggerFactory);

    /// <summary>
    /// Disposes the unit of work.
    /// </summary>
    protected abstract void DisposeUnitOfWork();

    /// <summary>Saves the unit of work asynchronous.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns></returns>
    protected abstract Task SaveUnitOfWorkAsync(CancellationToken cancellationToken = default);

    [LoggerMessage(Level = LogLevel.Debug, Message = "GetReadOnlyRepository '{Entity}'")]
    private static partial void LogGetReadOnlyRepository(ILogger logger, Type entity);

    [LoggerMessage(Level = LogLevel.Debug, Message = "GetRepository '{Entity}'")]
    private static partial void LogGetRepository(ILogger logger, Type entity);

    [LoggerMessage(Level = LogLevel.Debug, Message = "SaveChangesAsync")]
    private static partial void LogSaveChangesAsync(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "SaveChangesAsync: completed")]
    private static partial void LogSaveChangesAsyncCompleted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "DisposeResources")]
    private static partial void LogDisposeResources(ILogger logger);
}



