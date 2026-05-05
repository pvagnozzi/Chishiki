// -----------------------------------------------------------------------------
// File:        UnitOfWorkFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract factory for creating Unit of Work instances.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Chishiki;
using Chishiki.Data.Abstractions;

namespace Chishiki.Data;

/// <summary>
/// Unit of work factory abstract class.
/// </summary>
/// <seealso cref="Disposable" />
/// <seealso cref="IUnitOfWorkFactory" />
/// <remarks>
/// Initializes a new instance of the <see cref="UnitOfWorkFactory"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider.</param>
public abstract class UnitOfWorkFactory(IServiceProvider serviceProvider) : Disposable(null, serviceProvider.GetRequiredService<ILoggerFactory>()), IUnitOfWorkFactory
{
    /// <summary>
    /// Gets the service provider.
    /// </summary>
    /// <value>
    /// The service provider.
    /// </value>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// The logger factory
    /// </summary>
    private readonly ILoggerFactory _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    /// <summary>
    /// The repository mapper
    /// </summary>
    private readonly IRepositoryMapper _repositoryMapper = serviceProvider.GetService<IRepositoryMapper>() ?? new RepositoryMapper();

    /// <summary>
    /// Creates a new Unit of Work instance.
    /// </summary>
    /// <returns>A new IUnitOfWork instance configured with registered repositories and logging.</returns>
    public IUnitOfWork CreateUnitOfWork() => CreateUnitOfWorkInstance(_repositoryMapper, _loggerFactory);

    /// <summary>
    /// Creates a new Unit of Work instance with the specified repository mapper and logger factory.
    /// </summary>
    /// <param name="repositoryMapper">The repository mapper to use for repository registration.</param>
    /// <param name="loggerFactory">The logger factory to use for creating loggers.</param>
    /// <returns>A new IUnitOfWork instance configured with the provided dependencies.</returns>
    protected abstract IUnitOfWork CreateUnitOfWorkInstance(IRepositoryMapper repositoryMapper,
        ILoggerFactory loggerFactory);
}



