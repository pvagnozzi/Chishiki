// -----------------------------------------------------------------------------
// File:        EFCoreUnitOfWorkFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core Unit of Work factory that creates UoW instances backed by a typed DbContext.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chishiki.Data.EFCore;

/// <summary>EF Core Unit of Work factory that creates a fresh <see cref="EfCoreUnitOfWork"/> for each
/// call to <see cref="UnitOfWorkFactory.CreateUnitOfWork"/>, using an
/// <see cref="IDbContextFactory{TContext}"/> to obtain isolated <see cref="DbContext"/> instances.</summary>
/// <typeparam name="TContext">The concrete <see cref="DbContext"/> type registered with the DI container.</typeparam>
public sealed class EFCoreUnitOfWorkFactory<TContext>(IServiceProvider serviceProvider)
    : UnitOfWorkFactory(serviceProvider)
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _contextFactory =
        serviceProvider.GetRequiredService<IDbContextFactory<TContext>>();

    /// <summary>Creates a new <see cref="EfCoreUnitOfWork"/> backed by a fresh <typeparamref name="TContext"/> instance.</summary>
    /// <param name="repositoryMapper">The repository mapper.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>A new <see cref="IUnitOfWork"/> ready for use.</returns>
    protected override IUnitOfWork CreateUnitOfWorkInstance(
        IRepositoryMapper repositoryMapper,
        ILoggerFactory loggerFactory) =>
        new EFCoreUnitOfWork(_contextFactory.CreateDbContext(), repositoryMapper, loggerFactory);
}
