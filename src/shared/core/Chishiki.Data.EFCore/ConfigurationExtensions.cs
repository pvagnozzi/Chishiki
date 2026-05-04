// -----------------------------------------------------------------------------
// File:        ConfigurationExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core extensions for DbContext, UoW factory, and PostgreSQL database configuration.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------


using Chishiki.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Chishiki.Data.EFCore;

/// <summary>
/// Configuration and dependency injection extensions for EF Core DbContext, Unit of Work factory, and PostgreSQL database provider.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Registers the EF Core Unit of Work factory with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The DbContext type to use for the Unit of Work.</typeparam>
    /// <param name="serviceCollection">The service collection to register the factory with.</param>
    /// <returns>The service collection for method chaining.</returns>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection AddEFUnitOfWorkFactory<T>(
        this IServiceCollection serviceCollection)
        where T : DbContext =>
        serviceCollection
            .AddScoped<IUnitOfWorkFactory, EFCoreUnitOfWorkFactory<T>>(ctx => new EFCoreUnitOfWorkFactory<T>(ctx))
            .AddScoped(ctx => ctx.GetRequiredService<IUnitOfWorkFactory>().CreateUnitOfWork());

    /// <summary>
    /// Registers the EF Core repository mapper with custom repository implementations from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to register the repository mapper with.</param>
    /// <param name="assemblies">The assemblies containing custom repository type implementations.</param>
    /// <returns>The service collection for method chaining.</returns>
    // ReSharper disable once InconsistentNaming
    public static IServiceCollection AddEFRepositoryMapper(this IServiceCollection services, Assembly[] assemblies) =>
        services.AddSingleton<IRepositoryMapper, RepositoryMapper>(ctx => new RepositoryMapper(assemblies));

    /// <summary>
    /// Configures PostgreSQL database provider with connection string, retry policy, and migrations assembly.
    /// </summary>
    /// <param name="builder">The DbContext options builder.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="migrationsAssembly">The assembly containing EF Core migrations.</param>
    /// <param name="maxRetryCount">Maximum number of retry attempts. Default is 15.</param>
    /// <param name="retryDelaySeconds">Delay in seconds between retries. Default is 30.</param>
    /// <returns>The configured DbContext options builder.</returns>
    private static DbContextOptionsBuilder ConfigurePostgres(
        this DbContextOptionsBuilder builder,
        string connectionString,
        Assembly migrationsAssembly,
        int maxRetryCount = 15,
        int retryDelaySeconds = 30) =>
        builder.UseNpgsql(
            connectionString,
            opts => opts
                .MigrationsAssembly(migrationsAssembly.GetName().Name!)
                .EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(retryDelaySeconds), null));

    /// <summary>
    /// Configures PostgreSQL database provider with connection string, retry policy, and DbContext type.
    /// </summary>
    /// <param name="builder">The DbContext options builder.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="dbContextType">The DbContext type used to locate migrations assembly.</param>
    /// <param name="maxRetryCount">Maximum number of retry attempts. Default is 15.</param>
    /// <param name="retryDelaySeconds">Delay in seconds between retries. Default is 30.</param>
    /// <returns>The configured DbContext options builder.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static DbContextOptionsBuilder ConfigurePostgres(
        this DbContextOptionsBuilder builder,
        string connectionString,
        Type dbContextType,
        int maxRetryCount = 15,
        int retryDelaySeconds = 30) =>
        builder.ConfigurePostgres(connectionString, dbContextType.Assembly, maxRetryCount, retryDelaySeconds);

    /// <summary>
    /// Configures PostgreSQL database provider with connection string and retry policy using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The DbContext type used to locate migrations assembly.</typeparam>
    /// <param name="builder">The DbContext options builder.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="maxRetryCount">Maximum number of retry attempts. Default is 15.</param>
    /// <param name="retryDelaySeconds">Delay in seconds between retries. Default is 30.</param>
    /// <returns>The configured DbContext options builder.</returns>
    public static DbContextOptionsBuilder ConfigurePostgres<T>(
        this DbContextOptionsBuilder builder,
        string connectionString,
        int maxRetryCount = 15,
        int retryDelaySeconds = 30)
        where T : DbContext =>
        builder.ConfigurePostgres(connectionString, typeof(T), maxRetryCount, retryDelaySeconds);

    /// <summary>
    /// Adds a PostgreSQL DbContext to the dependency injection container with transient lifetime.
    /// </summary>
    /// <typeparam name="T">The DbContext type to add.</typeparam>
    /// <param name="services">The service collection to add the DbContext to.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>The service collection for method chaining.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection AddPostgresDbContext<T>(
        this IServiceCollection services,
        string connectionString)
        where T : DbContext =>
        services.AddDbContext<T>((serviceProvider, options) =>
                options
                    .ConfigurePostgres<T>(connectionString)
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>()),
            ServiceLifetime.Transient);

    /// <summary>
    /// Adds a PostgreSQL DbContext and EF Core Unit of Work factory to the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The DbContext type to add.</typeparam>
    /// <param name="services">The service collection to add the DbContext and Unit of Work to.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="repositoryAssembly">Assemblies containing custom repository implementations.</param>
    /// <returns>The service collection for method chaining.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection AddPostgresDbContextWithUnitOfWork<T>(
        this IServiceCollection services,
        string connectionString,
        Assembly[] repositoryAssembly)
        where T : DbContext
    {
        var result = services
            .AddPostgresDbContext<T>(connectionString)
            .AddEFUnitOfWorkFactory<T>();
        return (repositoryAssembly.Length > 0) ? result.AddEFRepositoryMapper(repositoryAssembly) : result;
    }

    /// <summary>
    /// Adds a PostgreSQL DbContext and EF Core Unit of Work factory to the dependency injection container using the DbContext's own assembly for custom repositories.
    /// </summary>
    /// <typeparam name="T">The DbContext type to add.</typeparam>
    /// <param name="services">The service collection to add the DbContext and Unit of Work to.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPostgresDbContextWithUnitOfWork<T>(
        this IServiceCollection services, string connectionString)
        where T : DbContext => services.AddPostgresDbContextWithUnitOfWork<T>(connectionString, [typeof(T).Assembly]);

    /// <summary>
    /// Applies any pending EF Core migrations to the database synchronously.
    /// </summary>
    /// <typeparam name="T">The DbContext type.</typeparam>
    /// <param name="dbContext">The DbContext instance.</param>
    /// <param name="logger">Logger instance for migration diagnostics.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public static void ApplyMigrations<T>(this T dbContext, ILogger logger)
        where T : DbContext
    {
        var dbContextName = dbContext.GetType().FullName;

        try
        {
            var database = dbContext.Database;
            logger.LogTrace("Migration '{DbContext}': pending migrations", dbContextName);
            var pendingMigrations = database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                logger.LogTrace("Migration '{DbContext}': apply {PendingMigrations}", dbContextName,
                    pendingMigrations);
                dbContext.Database.Migrate();
                logger.LogTrace("Migration '{DbContext}': completed", dbContextName);
            }
            else
            {
                logger.LogTrace("Migration '{DbContext}': no migrations", dbContextName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration '{DbContext}': {Ex}", dbContextName, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the database asynchronously.
    /// </summary>
    /// <typeparam name="T">The DbContext type.</typeparam>
    /// <param name="dbContext">The DbContext instance.</param>
    /// <param name="logger">Logger instance for migration diagnostics.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static async Task ApplyMigrationsAsync<T>(this T dbContext, ILogger logger, CancellationToken cancellationToken = default)
        where T : DbContext
    {
        var dbContextName = dbContext.GetType().FullName;

        try
        {
            var database = dbContext.Database;
            logger.LogTrace("Migration '{DbContext}': pending migrations", dbContextName);
            var pendingMigrations = await database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                logger.LogTrace("Migration '{DbContext}': apply {PendingMigrations}", dbContextName,
                    pendingMigrations);
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogTrace("Migration '{DbContext}': completed", dbContextName);
            }
            else
            {
                logger.LogTrace("Migration '{DbContext}': no migrations", dbContextName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration '{DbContext}': {Ex}", dbContextName, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the database by retrieving the DbContext from the service provider.
    /// </summary>
    /// <typeparam name="T">The DbContext type.</typeparam>
    /// <param name="serviceProvider">The service provider to retrieve the DbContext and logger from.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public static void ApplyMigrations<T>(this IServiceProvider serviceProvider)
        where T : DbContext
    {
        var dbContext = serviceProvider.GetRequiredService<T>();
        var logger = serviceProvider.GetRequiredService<ILogger<T>>();
        dbContext.ApplyMigrations(logger);
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the database asynchronously by retrieving the DbContext from the service provider.
    /// </summary>
    /// <typeparam name="T">The DbContext type.</typeparam>
    /// <param name="serviceProvider">The service provider to retrieve the DbContext and logger from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if migration fails.</exception>
    public static async Task ApplyMigrationsAsync<T>(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        where T : DbContext
    {
        var migrationLogger = serviceProvider.GetRequiredService<ILogger<T>>();
        try
        {
            migrationLogger.LogTrace("Migration '{DbContext}': started", typeof(T).FullName);
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            var logger = serviceProvider.GetRequiredService<ILogger<T>>();
            await dbContext.ApplyMigrationsAsync(logger, cancellationToken);
            migrationLogger.LogTrace("Migration '{DbContext}': completed", typeof(T).FullName);
        }
        catch (Exception ex)
        {
            migrationLogger.LogError(ex, "Migration '{DbContext}': {Ex}", typeof(T).FullName, ex.Message);
            throw new InvalidOperationException($"Migration '{typeof(T).FullName}' failed", ex);
        }
    }

    /// <summary>
    /// Seeds a database table with entities if the table is empty.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="dbSet">The DbSet representing the table to seed.</param>
    /// <param name="entities">The entities to seed the table with.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedTableAsync<T>(this DbContext context, DbSet<T> dbSet, IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (await dbSet.AnyAsync(cancellationToken))
        {
            return;
        }

        await dbSet.AddRangeAsync(entities, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a required connection string from the configuration, throwing an exception if not found.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="name">The name of the connection string.</param>
    /// <returns>The connection string value.</returns>
    /// <exception cref="InvalidDataException">Thrown if the connection string is not found in configuration.</exception>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) ?? throw new InvalidDataException($"Connection string '{name}' not found.");
}
