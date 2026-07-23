// -----------------------------------------------------------------------------
// File:        DbContextExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core extensions for DbContext.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Data;
using System.Diagnostics;
using System.Transactions;
using Chishiki.Data.Audit;
using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Chishiki.Data.EFCore;

/// <summary>DbContext extensions for executing actions within a transaction scope and tracking entity changes. This class provides extension methods for the DbContext class, allowing you to execute actions within a transaction and to retrieve information about the changes made to the entities tracked by the DbContext. The ExecuteInTransactionAsync method allows you to execute an asynchronous action within a transaction scope, ensuring that the transaction is properly committed or rolled back based on the success of the action. The GetChanges methods allow you to retrieve information about the changes made to the entities, including the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values). These methods can be used for auditing purposes or to track changes in the application.</summary>
public static class DbContextExtensions
{
    /// <summary>Executes the specified action within a transaction scope. The transaction will be committed if the action completes successfully, or rolled back if an exception is thrown. .</summary>
    /// <param name="db">The DbContext instance.</param>
    /// <param name="action">The action to execute within the transaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [DebuggerStepThrough]
    public static async Task ExecuteInTransactionAsync(this DbContext db, Func<DbContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        using TransactionScope transaction =
            new(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        await action(db, cancellationToken);
        transaction.Complete();
    }

    /// <summary>Gets the changes made to the entities tracked by the DbContext. This method returns an enumerable of EntityChange objects, which contain information about the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values). .</summary>
    /// <param name="entities">The collection of entity entries to analyze for changes.</param>
    /// <returns>An enumerable of EntityChange objects representing the changes.</returns>
    [DebuggerStepThrough]
    public static IEnumerable<EntityChange> GetChanges(this IEnumerable<EntityEntry> entities) =>
        entities.Select(ToEntityChange).Where(x => x is not null)
            .Cast<EntityChange>();

    /// <summary>Gets the changes made to the entities tracked by the DbContext. This method returns an enumerable of EntityChange objects, which contain information about the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values). .</summary>
    /// <param name="dbContext">The DbContext instance.</param>
    /// <returns>An enumerable of EntityChange objects representing the changes.</returns>
    [DebuggerStepThrough]
    public static IEnumerable<EntityChange> GetChanges(this DbContext dbContext) =>
        dbContext.ChangeTracker.Entries().GetChanges();

    /// <summary>Converts an EntityEntry to an EntityChange object, which contains information about the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values). If the entity state is unchanged or detached, this method returns null. .</summary>
    /// <param name="entityEntry">The EntityEntry to convert.</param>
    /// <returns>An EntityChange object representing the changes, or null if the entity state is unchanged or detached.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the entity has no primary key.</exception>
    [DebuggerStepThrough]
    private static EntityChange? ToEntityChange(EntityEntry entityEntry)
    {
        var entity = entityEntry.Entity;

        ChangeAction changeAction;
        switch (entityEntry.State)
        {
            case EntityState.Added:
                changeAction = ChangeAction.Inserted;
                break;

            case EntityState.Modified:
                changeAction = ChangeAction.Updated;
                break;

            case EntityState.Deleted:
                changeAction = ChangeAction.Deleted;
                break;

            case EntityState.Detached:
            case EntityState.Unchanged:
            default:
                return null;
        }

        var entityName = entity.GetType().FullName!;
        var idProperty = entityEntry.Properties.SingleOrDefault(p => p.Metadata.IsPrimaryKey());
        if (idProperty?.CurrentValue is null)
        {
            throw new InvalidOperationException($"{entityName} has no primary key");
        }

        var entityId = idProperty.CurrentValue.ToString()!;
        var changes = entityEntry.Properties.Select(ToPropertyChange).ToArray();
        return new(entityName, entityId, changeAction, changes);
    }

    /// <summary>Creates a new PropertyChange instance representing the change in value for the specified property entry. .</summary>
    /// <param name="propertyEntry">The property entry containing metadata and value information for the property to be tracked. Cannot be null.</param>
    /// <returns>A PropertyChange object containing the property name, current value, and original value as strings.</returns>
    [DebuggerStepThrough]
    private static PropertyChange ToPropertyChange(this PropertyEntry propertyEntry) =>
        new(propertyEntry.Metadata.Name, propertyEntry.CurrentValue?.ToString(),
            propertyEntry.OriginalValue?.ToString());

    /// <summary>Converts to PagedList async. .</summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static async Task<PagedList<TEntity>> ToPagedListAsync<TKey, TEntity>(
        this IQueryable<TEntity> source,
        int pageSize = 10,
        int pageNumber = 0,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        var count = await source.CountAsync(cancellationToken);
        var orderedQueryable = source.OrderMethodExists() ? source : source.OrderBy(s => s.Id);
        var items = await orderedQueryable
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<TEntity>(items, count, pageNumber, pageSize);
    }

    /// <summary>Converts to paged list with select expression. .</summary>
    /// <param name="source">Source data.</param>
    /// <param name="selectExpression">Select expression.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Paged list.</returns>
    public static async Task<PagedList<TResult>> ToPagedListWithSelectAsync<TKey, TEntity, TResult>(
        this IQueryable<TEntity> source,
        Func<TEntity, int, TResult> selectExpression,
        int pageSize = 10,
        int pageNumber = 0,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        var res = await source.ToPagedListAsync<TKey, TEntity>(
            pageSize,
            pageNumber,
            cancellationToken);

        List<TResult> mappedItems = [.. res.Items.Select(selectExpression)];
        return new PagedList<TResult>(mappedItems, res.TotalCount, pageNumber, pageSize);
    }

    /// <summary>Includes the specified include expression. .</summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="includeExpression">The include expression.</param>
    /// <returns>Expresion with include.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IQueryable<T> Include<T>(this IQueryable<T> source, IIncludeExpression includeExpression)
        where T : class
    {
        _ = source.Include(includeExpression.PropertyName);

        foreach (var childExpression in includeExpression.NestedExpressions)
        {
            _ = source.Include(childExpression);
        }

        return source;
    }

    /// <summary>Includes the specified include expressions. .</summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="includeExpressions">The include expressions.</param>
    /// <returns>Filtered result.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IQueryable<T> Include<T>(this IQueryable<T> source,
        IEnumerable<IIncludeExpression> includeExpressions)
        where T : class
    {
        foreach (var includeExpression in includeExpressions)
        {
            _ = source.Include(includeExpression);
        }

        return source;
    }

    /// <summary>Applies the specification to the query source. .</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The filtered and sorted queryable.</returns>
    public static IQueryable<TEntity> ApplySpecification<TKey, TEntity>(
        this IQueryable<TEntity> source,
        ISpecification<TKey, TEntity> specification)
        where TEntity : class, IEntity<TKey>
    {
        _ = source.Include(specification.Includes);

        if (specification.Where is not null)
        {
            source = source.Where(specification.Where);
        }

        source = source.Where(specification.Filters);
        source = source.Sort(specification.Sort);

        return source;
    }
}
