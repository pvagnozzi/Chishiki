// -----------------------------------------------------------------------------
// File:        DesignTimeDbContextFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core implementation of a design-time DbContext factory.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chishiki.Data.EFCore;

/// <summary>
/// Design-time DbContext factory for EF Core. This class provides a base implementation of the IDesignTimeDbContextFactory interface, which is used by EF Core tools to create instances of the DbContext at design time (e.g., for migrations). The factory takes a connection string and a default connection string as parameters, and uses them to configure the DbContext options. The BuildDbContextOptionsBuilder method can be overridden to customize the options builder (e.g., to use a different database provider or additional options). The CreateDbContext method must be implemented by derived classes to create an instance of the DbContext using the configured options.
/// </summary>
/// <typeparam name="T">The type of the DbContext.</typeparam>
/// <param name="connectionString">The connection string to use for the DbContext.</param>
/// <param name="defaultConnectionString">The default connection string to use if the primary connection string is not provided.</param>
public abstract class DesignTimeDbContextFactory<T>(string connectionString, string defaultConnectionString)
    : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    /// <summary>
    /// Creates a new instance of the database context using the specified command-line arguments.
    /// </summary>
    /// <remarks>If the connection string is not set, the default connection string is used. The provided
    /// arguments can be used to customize the context configuration as needed.</remarks>
    /// <param name="args">An array of command-line arguments that may influence the configuration of the database context.</param>
    /// <returns>A new instance of the database context of type T configured with the appropriate options.</returns>
    public T CreateDbContext(string[] args)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = defaultConnectionString;
        }

        var dbContextOptionsBuilder = BuildDbContextOptionsBuilder(new DbContextOptionsBuilder<T>(), connectionString);
        return CreateDbContext(dbContextOptionsBuilder.Options);
    }

    /// <summary>
    /// Builds the DbContext options builder using the provided connection string. This method can be overridden by derived classes to customize the options builder (e.g., to use a different database provider or additional options). The default implementation simply returns the provided builder without modification, but it can be extended to configure the builder as needed.
    /// </summary>
    /// <param name="builder">The DbContext options builder to configure.</param>
    /// <param name="connectionString">The connection string to use for the DbContext.</param>
    /// <returns>The configured DbContext options builder.</returns>
    protected abstract DbContextOptionsBuilder<T> BuildDbContextOptionsBuilder(DbContextOptionsBuilder<T> builder,string connectionString);

    /// <summary>
    /// Creates an instance of the DbContext using the provided options. This method must be implemented by derived classes to create an instance of the DbContext using the configured options. The implementation should return a new instance of the DbContext, passing the options to its constructor.
    /// </summary>
    /// <param name="options">The DbContext options to use for the DbContext.</param>
    /// <returns>An instance of the DbContext.</returns>
    protected abstract T CreateDbContext(DbContextOptions<T> options);
}
