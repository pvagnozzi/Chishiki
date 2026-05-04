// -----------------------------------------------------------------------------
// File:        IRepositoryFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for repository factory abstraction.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Models;

namespace Chishiki.Data.Abstractions;

/// <summary>
/// Interface for a repository factory.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>
    /// Gets the repository mapper.
    /// </summary>
    /// <value>
    /// The repository mapper.
    /// </value>
    IRepositoryMapper RepositoryMapper { get; }

    /// <summary>
    /// Creates the read only repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    IReadOnlyRepository<TKey, TEntity> CreateReadOnlyRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>;

    /// <summary>
    /// Creates the repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns></returns>
    IRepository<TKey, TEntity> CreateRepository<TKey, TEntity>()
        where TEntity : class, IEntity<TKey>;
}

