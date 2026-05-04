// -----------------------------------------------------------------------------
// File:        RepositoryFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract factory for creating typed repository instances.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;

namespace Chishiki.Data;

/// <summary>Abstract factory for creating repository instances of a specific type bound to a repository mapper.</summary>
/// <remarks>
/// This abstract class provides the template for factory implementations that create both read-only and mutable repositories.
/// Implementations must provide type-specific instantiation logic via the abstract methods.
/// </remarks>
/// <seealso cref="Chishiki.Data.Abstractions.IRepositoryFactory" />
public abstract class RepositoryFactory(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory)
    : IRepositoryFactory
{
    /// <summary>Gets the logger factory used to create loggers for repository instances.</summary>
    /// <value>The logger factory.</value>
    protected ILoggerFactory LoggerFactory { get; } = loggerFactory;

    /// <summary>Gets the repository type mapper that determines the concrete repository type for each entity.</summary>
    /// <value>The repository mapper.</value>
    public IRepositoryMapper RepositoryMapper { get; } = repositoryMapper;

    /// <summary>Creates a read-only repository for the specified entity type and key type.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A read-only repository instance, or the default factory-created instance if no custom type is mapped.</returns>
    public IReadOnlyRepository<TKey, TEntity> CreateReadOnlyRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>
    {
        var type = RepositoryMapper.GetRepositoryType(typeof(TEntity));
        ILogger logger = LoggerFactory.CreateLogger<IReadOnlyRepository<TKey, TEntity>>();

        var result = type is not null
            ? (IReadOnlyRepository<TKey, TEntity>?)CreateInstance(type, logger)
            : null;
        return result ?? CreateReadOnlyRepositoryInstance<TKey, TEntity>(logger);
    }

    /// <summary>Creates a mutable repository for the specified entity type and key type.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A repository instance that supports insert, update, and delete operations.</returns>
    public virtual IRepository<TKey, TEntity> CreateRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>
    {
        var type = RepositoryMapper.GetRepositoryType(typeof(TEntity));
        ILogger logger = LoggerFactory.CreateLogger<IReadOnlyRepository<TKey, TEntity>>();

        var result = type is not null
            ? (IRepository<TKey, TEntity>?)CreateInstance(type, logger)
            : null;
        return result ?? CreateRepositoryInstance<TKey, TEntity>(logger);
    }

    /// <summary>Creates an instance of the repository from the specified type using the provided logger.</summary>
    /// <param name="type">The concrete repository type to instantiate.</param>
    /// <param name="logger">The logger to inject into the repository.</param>
    /// <returns>An instance of the repository type, or null if instantiation fails.</returns>
    protected abstract object? CreateInstance(Type type, ILogger logger);

    /// <summary>Creates a read-only repository instance for the specified entity when no custom type is mapped.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="logger">The logger to inject into the repository.</param>
    /// <returns>A newly created read-only repository instance.</returns>
    protected abstract IReadOnlyRepository<TKey, TEntity> CreateReadOnlyRepositoryInstance<TKey, TEntity>(
        ILogger logger)
        where TEntity : class, IEntity<TKey>;

    /// <summary>Creates a mutable repository instance for the specified entity when no custom type is mapped.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="logger">The logger to inject into the repository.</param>
    /// <returns>A newly created repository instance.</returns>
    protected abstract IRepository<TKey, TEntity> CreateRepositoryInstance<TKey, TEntity>(ILogger logger)
        where TEntity : class, IEntity<TKey>;
}


