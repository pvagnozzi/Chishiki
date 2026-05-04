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

using Chishiki.Data.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;
using System.Diagnostics;
using System.Transactions;

namespace Chishiki.Data.EFCore;

/// <summary>
/// DbContext extensions for executing actions within a transaction scope and tracking entity changes. This class provides extension methods for the DbContext class, allowing you to execute actions within a transaction and to retrieve information about the changes made to the entities tracked by the DbContext. The ExecuteInTransactionAsync method allows you to execute an asynchronous action within a transaction scope, ensuring that the transaction is properly committed or rolled back based on the success of the action. The GetChanges methods allow you to retrieve information about the changes made to the entities, including the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values). These methods can be used for auditing purposes or to track changes in the application.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Executes the specified action within a transaction scope. The transaction will be committed if the action completes successfully, or rolled back if an exception is thrown.
    /// </summary>
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

    /// <summary>
    /// Gets the changes made to the entities tracked by the DbContext. This method returns an enumerable of EntityChange objects, which contain information about the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values).   
    /// </summary>
    /// <param name="entities">The collection of entity entries to analyze for changes.</param>
    /// <returns>An enumerable of EntityChange objects representing the changes.</returns>
    [DebuggerStepThrough]
    public static IEnumerable<EntityChange> GetChanges(this IEnumerable<EntityEntry> entities) =>
        entities.Select(ToEntityChange).Where(x => x is not null)
            .Cast<EntityChange>();

    /// <summary>
    /// Gets the changes made to the entities tracked by the DbContext. This method returns an enumerable of EntityChange objects, which contain information about the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values).
    /// </summary>
    /// <param name="dbContext">The DbContext instance.</param>
    /// <returns>An enumerable of EntityChange objects representing the changes.</returns>
    [DebuggerStepThrough]
    public static IEnumerable<EntityChange> GetChanges(this DbContext dbContext) =>
        dbContext.ChangeTracker.Entries().GetChanges();

    /// <summary>
    /// Converts an EntityEntry to an EntityChange object, which contains information about the entity name, entity ID, change action (inserted, updated, deleted), and the properties that were changed (with their current and original values). If the entity state is unchanged or detached, this method returns null.
    /// </summary>
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

    /// <summary>
    /// Creates a new PropertyChange instance representing the change in value for the specified property entry.    
    /// </summary>
    /// <param name="propertyEntry">The property entry containing metadata and value information for the property to be tracked. Cannot be null.</param>
    /// <returns>A PropertyChange object containing the property name, current value, and original value as strings.</returns>
    [DebuggerStepThrough]
    private static PropertyChange ToPropertyChange(this PropertyEntry propertyEntry) =>
        new(propertyEntry.Metadata.Name, propertyEntry.CurrentValue?.ToString(),
            propertyEntry.OriginalValue?.ToString());
}
