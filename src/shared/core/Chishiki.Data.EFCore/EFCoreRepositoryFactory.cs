// -----------------------------------------------------------------------------
// File:        EFCoreRepositoryFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core implementation of the repository factory that creates repository instances from a DbContext.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chishiki.Data.EFCore;

/// <summary>EF Core repository factory that creates <see cref="EfCoreReadOnlyRepository{TKey,TEntity}"/>
/// and <see cref="EfCoreRepository{TKey,TEntity}"/> instances bound to the provided <see cref="DbContext"/>.</summary>
/// <remarks>
/// Custom repository types registered via <see cref="IRepositoryMapper"/> must expose a constructor with the
/// signature <c>(DbContext context, ILogger logger)</c> to be instantiated by this factory.
/// </remarks>
internal sealed partial class EfCoreRepositoryFactory(
    IRepositoryMapper repositoryMapper,
    ILoggerFactory loggerFactory,
    DbContext context)
    : RepositoryFactory(repositoryMapper, loggerFactory)
{
    /// <summary>Creates an instance of a custom repository type registered via <see cref="IRepositoryMapper"/>.</summary>
    /// <param name="type">The concrete repository type to instantiate.</param>
    /// <param name="logger">The logger to pass to the constructor.</param>
    /// <returns>The repository instance, or <c>null</c> if instantiation fails.</returns>
    protected override object? CreateInstance(Type type, ILogger logger)
    {
        try
        {
            return Activator.CreateInstance(type, context, logger);
        }
        catch (Exception ex)
        {
            LogCreateInstanceFailed(LoggerFactory.CreateLogger<EfCoreRepositoryFactory>(), type, ex);
            return null;
        }
    }

    /// <inheritdoc/>
    protected override IReadOnlyRepository<TKey, TEntity> CreateReadOnlyRepositoryInstance<TKey, TEntity>(
        ILogger logger) =>
        new EFCoreReadOnlyRepository<TKey, TEntity>(context, logger);

    /// <inheritdoc/>
    protected override IRepository<TKey, TEntity> CreateRepositoryInstance<TKey, TEntity>(ILogger logger) =>
        new EFCoreRepository<TKey, TEntity>(context, logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to create custom repository instance of type '{RepositoryType}'")]
    private static partial void LogCreateInstanceFailed(ILogger logger, Type repositoryType, Exception ex);
}
