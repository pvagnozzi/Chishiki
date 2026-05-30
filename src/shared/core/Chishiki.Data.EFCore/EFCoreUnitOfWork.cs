// -----------------------------------------------------------------------------
// File:        EFCoreUnitOfWork.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core implementation of the Unit of Work pattern wrapping a DbContext.
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

/// <summary>EF Core Unit of Work that coordinates repository instances over a shared <see cref="DbContext"/> and flushes all pending changes in a single <c>SaveChangesAsync</c> call.</summary>
internal sealed partial class EFCoreUnitOfWork(
    DbContext context,
    IRepositoryMapper repositoryMapper,
    ILoggerFactory loggerFactory)
    : UnitOfWork(repositoryMapper, loggerFactory)
{
    /// <summary>Builds the <see cref="EfCoreRepositoryFactory"/> bound to the underlying <see cref="DbContext"/>.</summary>
    /// <param name="repositoryMapper">The repository mapper.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>An <see cref="IRepositoryFactory"/> backed by EF Core.</returns>
    protected override IRepositoryFactory BuildRepositoryFactory(
        IRepositoryMapper repositoryMapper,
        ILoggerFactory loggerFactory) =>
        new EfCoreRepositoryFactory(repositoryMapper, loggerFactory, context);

    /// <summary>Disposes the underlying <see cref="DbContext"/>.</summary>
    protected override void DisposeUnitOfWork() => context.Dispose();

    /// <summary>Saves all pending changes tracked by the <see cref="DbContext"/>.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    protected override Task SaveUnitOfWorkAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
